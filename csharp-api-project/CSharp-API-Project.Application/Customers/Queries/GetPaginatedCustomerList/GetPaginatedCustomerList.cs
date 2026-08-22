using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Application.Customers.Models;
using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CSharpAPIProject.Application.Customers.Queries.GetPaginatedCustomerList;

[Authorize(Policy = Policies.CanView_Customer)]
public class GetPaginatedCustomerListQuery : IRequest<PaginatedList<CustomerDto>>
{
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public string QueryFilter { get; set; }
}

public class GetPaginatedCustomerListQueryValidator : AbstractValidator<GetPaginatedCustomerListQuery>
{
	public GetPaginatedCustomerListQueryValidator()
	{
		RuleFor(x => x.PageNumber).GreaterThan(0);
		RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(1000);
	}
}

public class GetPaginatedCustomerListQueryHandler(
    IApplicationDbContext context, 
    IUser user, 
    IMapper mapper, 
    IIdentityService identityService, 
    IGlobalIdentityService globalIdentityService, 
    ITenantResolver tenantResolver,
    IAuthHelper authHelper,
    ISettingsService settingsService
) : IRequestHandler<GetPaginatedCustomerListQuery, PaginatedList<CustomerDto>>
{

    public async Task<PaginatedList<CustomerDto>> Handle(GetPaginatedCustomerListQuery request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();

        var vendorSettings = settingsService.LoadSetting<VendorSettings>();
        var ptsAbbr = vendorSettings.PointsAbbreviation;
        if (string.IsNullOrEmpty(ptsAbbr)) ptsAbbr = "PTS";

        // Build customer query with filters
        var customerQuery = context.Customers
            .Where(c => c.VendorId == vendorId);

        if (!string.IsNullOrWhiteSpace(request.QueryFilter))
        {
            customerQuery = customerQuery.Where(c =>
                c.FirstName.Contains(request.QueryFilter) ||
                c.LastName.Contains(request.QueryFilter) ||
                c.Email.Contains(request.QueryFilter));
        }

        var isTICUser = await authHelper.LoggedIn_IsTICUser();
        var isClientUser = await authHelper.LoggedIn_IsClientAdmin();

        if (!isTICUser && !isClientUser)
        {
            var customerEmail = await identityService.GetUserEmailAsync(user.Id);
            customerQuery = customerQuery.Where(c => c.Email == customerEmail && c.VendorId == vendorId);
        }

        if (isClientUser)
            customerQuery = customerQuery.Where(c => c.VendorId == vendorId);

        // Get total count for pagination
        var totalCount = await customerQuery.CountAsync(cancellationToken);

        // Paginate in the database
        var customerList = await customerQuery
            .AsNoTracking()
            .OrderBy(c => c.CustomerId) // or another order if needed
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var customerIds = customerList.Select(c => c.CustomerId).ToList();
        var customerEmails = customerList.Select(c => c.Email.Trim().ToLower()).ToList();

        // Fetch ledgers for paginated customers
        var ledgers = await context.CustomerLedgers
            .Where(l => customerIds.Contains(l.CustomerId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Fetch users for paginated customers
        var users = globalIdentityService.ApplicationUsers()
            .Where(u => u.TenantId == vendorId && customerEmails.Contains(u.Email.Trim().ToLower()))
            .ToList();

        // Map to DTOs
        List<CustomerDto> customerDtoList = mapper.Map<List<CustomerDto>>(customerList);

        // Use dictionaries for fast lookup
        var userDict = users.ToDictionary(u => u.Email.Trim().ToLower());
        var ledgerDict = ledgers
            .GroupBy(l => l.CustomerId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.TransactionDate).FirstOrDefault()); // Get latest ledger by date

        // Collect all unique IDs for related entities from users
        var branchIds = users.Where(u => u.BranchId.HasValue).Select(u => u.BranchId.Value).Distinct().ToList();
        var companyIds = users.Where(u => u.CompanyId.HasValue).Select(u => u.CompanyId.Value).Distinct().ToList();
        var departmentIds = users.Where(u => u.DepartmentId.HasValue).Select(u => u.DepartmentId.Value).Distinct().ToList();
        var designationIds = users.Where(u => u.DesignationId.HasValue).Select(u => u.DesignationId.Value).Distinct().ToList();
        var provinceIds = users.Where(u => u.ProvinceId.HasValue).Select(u => u.ProvinceId.Value).Distinct().ToList();
        var residenceCountryIds = users.Where(u => u.ResidenceCountryId.HasValue).Select(u => u.ResidenceCountryId.Value).Distinct().ToList();
        var serviceCountryIds = users.Where(u => u.ServiceCountryId.HasValue).Select(u => u.ServiceCountryId.Value).Distinct().ToList();
        var teamIds = users.Where(u => u.TeamId.HasValue).Select(u => u.TeamId.Value).Distinct().ToList();

        // Batch fetch all related entities
        var branches = await context.Branches.Where(x => branchIds.Contains(x.BranchId)).AsNoTracking().ToListAsync(cancellationToken);
        var companies = await context.Companies.Where(x => companyIds.Contains(x.CompanyId)).AsNoTracking().ToListAsync(cancellationToken);
        var departments = await context.Departments.Where(x => departmentIds.Contains(x.DepartmentId)).AsNoTracking().ToListAsync(cancellationToken);
        var designations = await context.Designations.Where(x => designationIds.Contains(x.DesignationId)).AsNoTracking().ToListAsync(cancellationToken);
        var provinces = await context.Provinces.Where(x => provinceIds.Contains(x.ProvinceId)).AsNoTracking().ToListAsync(cancellationToken);
        var residenceCountries = await context.Countries.Where(x => residenceCountryIds.Contains(x.CountryId)).AsNoTracking().ToListAsync(cancellationToken);
        var serviceCountries = await context.Countries.Where(x => serviceCountryIds.Contains(x.CountryId)).AsNoTracking().ToListAsync(cancellationToken);
        var teams = await context.Teams.Where(x => teamIds.Contains(x.TeamId)).AsNoTracking().ToListAsync(cancellationToken);

        // Build dictionaries for fast lookup
        var branchDict = branches.ToDictionary(x => x.BranchId, x => x.Name);
        var companyDict = companies.ToDictionary(x => x.CompanyId, x => x.Name);
        var departmentDict = departments.ToDictionary(x => x.DepartmentId, x => x.Name);
        var designationDict = designations.ToDictionary(x => x.DesignationId, x => x.Name);
        var provinceDict = provinces.ToDictionary(x => x.ProvinceId, x => x.Name);
        var residenceCountryDict = residenceCountries.ToDictionary(x => x.CountryId, x => x.Name);
        var serviceCountryDict = serviceCountries.ToDictionary(x => x.CountryId, x => x.Name);
        var teamDict = teams.ToDictionary(x => x.TeamId, x => x.Name);

        foreach (var customer in customerDtoList)
        {
            // Assign user
            var emailKey = customer.Email.Trim().ToLower();
            if (userDict.TryGetValue(emailKey, out var appUser))
            {
                customer.BranchId = appUser.BranchId;
                customer.CompanyId = appUser.CompanyId;
                customer.DepartmentId = appUser.DepartmentId;
                customer.DesignationId = appUser.DesignationId;
                customer.ProvinceId = appUser.ProvinceId;
                customer.ResidenceCountryId = appUser.ResidenceCountryId;
                customer.ServiceCountryId = appUser.ServiceCountryId;
                customer.TeamId = appUser.TeamId;

                // Assign related entity names using dictionaries
                customer.BranchName = appUser.BranchId.HasValue && branchDict.TryGetValue(appUser.BranchId.Value, out var branchName) ? branchName : string.Empty;
                customer.CompanyName = appUser.CompanyId.HasValue && companyDict.TryGetValue(appUser.CompanyId.Value, out var companyName) ? companyName : string.Empty;
                customer.DepartmentName = appUser.DepartmentId.HasValue && departmentDict.TryGetValue(appUser.DepartmentId.Value, out var departmentName) ? departmentName : string.Empty;
                customer.DesignationName = appUser.DesignationId.HasValue && designationDict.TryGetValue(appUser.DesignationId.Value, out var designationName) ? designationName : string.Empty;
                customer.ProvinceName = appUser.ProvinceId.HasValue && provinceDict.TryGetValue(appUser.ProvinceId.Value, out var provinceName) ? provinceName : string.Empty;
                customer.ResidenceCountryName = appUser.ResidenceCountryId.HasValue && residenceCountryDict.TryGetValue(appUser.ResidenceCountryId.Value, out var residenceCountryName) ? residenceCountryName : string.Empty;
                customer.ServiceCountryName = appUser.ServiceCountryId.HasValue && serviceCountryDict.TryGetValue(appUser.ServiceCountryId.Value, out var serviceCountryName) ? serviceCountryName : string.Empty;
                customer.TeamName = appUser.TeamId.HasValue && teamDict.TryGetValue(appUser.TeamId.Value, out var teamName) ? teamName : string.Empty;

                customer.PtsAbbr = ptsAbbr;
            }

            // Assign latest ledger balance
            if (ledgerDict.TryGetValue(customer.CustomerId, out var ledger) && ledger != null)
            {
                customer.Balance = ledger.Balance;
            }
        }

        // Return paginated result
        return new PaginatedList<CustomerDto>(customerDtoList, totalCount, request.PageNumber, request.PageSize);
    }
}