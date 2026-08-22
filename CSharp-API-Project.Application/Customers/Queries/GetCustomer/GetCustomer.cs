using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Application.Customers.Models;
using CSharpAPIProject.Domain.Common;

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
    IAuthHelper authHelper
) : IRequestHandler<GetCustomerQuery, CustomerDto>
{
	public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
	{
        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        var isClientUser = await authHelper.LoggedIn_IsVendorAdmin();

        if (!isPlatformAdmin && !isClientUser)
		{
			var email = await identityService.GetUserEmailAsync(user.Id);
			if (email != request.CustomerEmail)
			{
				throw new UnauthorizedAccessWithEmailException(request.CustomerEmail);
			}
		}

        var vendorId = tenantResolver.ResolveTenantId();

        // 1. Fetch the customer directly from the database
        var customer = await context.Customers
            .AsNoTracking()
            .Where(x => x.Email.Trim().ToLower() == request.CustomerEmail.Trim().ToLower() && x.VendorId == vendorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
            throw new CustomerWithEmailNotFoundException(request.CustomerEmail, vendorId);

        var customerDto = mapper.Map<CustomerDto>(customer);

        // 2. Get claims
        string? userId;
        if (isPlatformAdmin)
            userId = await globalIdentityService.GetUserIdAsync(customer.Email, vendorId);
        else
            userId = await identityService.GetUserIdAsync(customer.Email, vendorId);

        var customerClaims = isPlatformAdmin ? await globalIdentityService.GetClaimsAsync(userId) : await identityService.GetClaimsAsync(userId);

        if (customerClaims != null)
        {
            customerDto.UserGroup = customerClaims.FirstOrDefault(c => c.Type == "UserGroupId")?.Value ?? string.Empty;
            customerDto.UserSubGroup = customerClaims.FirstOrDefault(c => c.Type == "UserSubGroupId")?.Value ?? string.Empty;
        }

        return customerDto;
	}
}
