using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Services.Identity_Framework;
using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Application.Customers.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CSharpAPIProject.Application.Customers.Queries.GetCustomer;

[Authorize(Policy = Policies.CanView_Customer)]
public class GetCustomerQuery : IRequest<CustomerDto>
{
	public string CustomerEmail { get; set; }
}

public class GetCustomerQueryValidator : AbstractValidator<GetCustomerQuery>
{
	public GetCustomerQueryValidator()
	{
		RuleFor(x => x.CustomerEmail).NotEmpty();
	}
}

public class GetCustomerQueryHandler(
    IApplicationDbContext context, 
    IUser user, 
    IMapper mapper, 
    IIdentityService identityService, 
    IGlobalIdentityService globalIdentityService, 
    ITenantResolver tenantResolver,
    IAuthHelper authHelper,
    ISettingsService settingsService,
    IUserDataService userDataService
) : IRequestHandler<GetCustomerQuery, CustomerDto>
{
	public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
	{
        var isTICUser = await authHelper.LoggedIn_IsTICUser();
        var isClientUser = await authHelper.LoggedIn_IsClientAdmin();

        if (!isTICUser && !isClientUser)
		{
			var email = await identityService.GetUserEmailAsync(user.Id);
			if (email != request.CustomerEmail)
			{
				throw new UnauthorizedAccessWithEmailException(request.CustomerEmail);
			}
		}

        var vendorSettings = settingsService.LoadSetting<VendorSettings>();
        var ptsAbbr = vendorSettings.PointsAbbreviation;
        if (string.IsNullOrEmpty(ptsAbbr)) ptsAbbr = "PTS";

        var vendorId = tenantResolver.ResolveTenantId();

        // 1. Fetch the customer directly from the database
        var customer = await context.Customers
            .AsNoTracking()
            .Where(x => x.Email.Trim().ToLower() == request.CustomerEmail.Trim().ToLower() && x.VendorId == vendorId)
            .Include(x => x.PointsLedger)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
            throw new CustomerWithEmailNotFoundException(request.CustomerEmail, vendorId);

        var customerDto = mapper.Map<CustomerDto>(customer);

        // 2. Get claims
        string? userId;
        if (isTICUser)
            userId = await globalIdentityService.GetUserIdAsync(customer.Email, vendorId);
        else
            userId = await identityService.GetUserIdAsync(customer.Email, vendorId);

        var customerClaims = isTICUser ? await globalIdentityService.GetClaimsAsync(userId) : await identityService.GetClaimsAsync(userId);

        if (customerClaims != null)
        {
            customerDto.UserGroup = customerClaims.FirstOrDefault(c => c.Type == "UserGroupId")?.Value ?? string.Empty;
            customerDto.UserSubGroup = customerClaims.FirstOrDefault(c => c.Type == "UserSubGroupId")?.Value ?? string.Empty;
        }

        // 3. Attach Application user Data
        var userData = await userDataService.GetUserData(userId);
        if(userData != null)
        {
            customerDto.BranchId = userData.BranchId;
            customerDto.CompanyId = userData.CompanyId;
            customerDto.DepartmentId = userData.DepartmentId;
            customerDto.DesignationId = userData.DesignationId;
            customerDto.ProvinceId = userData.ProvinceId;
            customerDto.ResidenceCountryId = userData.ResidenceCountryId;
            customerDto.ServiceCountryId = userData.ServiceCountryId;
            customerDto.TeamId = userData.TeamId;
            customerDto.BranchName = userData.BranchName;
            customerDto.CompanyName = userData.CompanyName;
            customerDto.DepartmentName = userData.DepartmentName;
            customerDto.DesignationName = userData.DesignationName;
            customerDto.ProvinceName = userData.ProvinceName;
            customerDto.ResidenceCountryName = userData.ResidenceCountryName;
            customerDto.ServiceCountryName = userData.ServiceCountryName;
            customerDto.TeamName = userData.TeamName;
        }

        customerDto.PtsAbbr = ptsAbbr;

        return customerDto;
	}
}

