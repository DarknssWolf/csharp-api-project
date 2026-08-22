using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Models;
using CSharpAPIProject.Domain.Common;

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
    ITenantResolver tenantResolver,
    IAuthHelper authHelper
) : IRequestHandler<GetPaginatedCustomerListQuery, PaginatedList<CustomerDto>>
{

    public async Task<PaginatedList<CustomerDto>> Handle(GetPaginatedCustomerListQuery request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();

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

        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        var isClientUser = await authHelper.LoggedIn_IsVendorAdmin();

        if (!isPlatformAdmin && !isClientUser)
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

        // Map to DTOs
        List<CustomerDto> customerDtoList = mapper.Map<List<CustomerDto>>(customerList);

        // Return paginated result
        return new PaginatedList<CustomerDto>(customerDtoList, totalCount, request.PageNumber, request.PageSize);
    }
}
