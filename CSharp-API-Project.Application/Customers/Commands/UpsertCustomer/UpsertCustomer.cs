using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Commands.CreateCustomer;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.ValueObjects;
using FluentValidation.Results;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Customers.Commands.UpsertCustomer;

[Authorize(Policy = Policies.CanUpdate_Customer)]
public class UpsertCustomerCommand : IRequest<CustomerCommandResponse>
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string IDNumber { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public string AddressLine1 { get; set; }
	public string AddressLine2 { get; set; }
	public string City { get; set; }
	public string PostalCode { get; set; }
	public string Province { get; set; }
	public string Country { get; set; }
	public string CustomerData { get; set; }
    public string UserGroup { get; set; }
    public string? UserSubGroup { get; set; }
	public string CustomerPassword { get; set; } // Only works for Create
}

public class CustomerCommandResponse
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string IDNumber { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public string AddressLine1 { get; set; }
	public string AddressLine2 { get; set; }
	public string City { get; set; }
	public string PostalCode { get; set; }
	public string Province { get; set; }
	public string Country { get; set; }
    public string UserGroup { get; set; }
    public string? UserSubGroup { get; set; }
	public string CustomerData { get; set; }
}

public class UpsertCustomerCommandValidator : AbstractValidator<UpsertCustomerCommand>
{
    public UpsertCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.IDNumber).NotEmpty();
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.AddressLine1).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.PostalCode).NotEmpty();
        RuleFor(x => x.Province).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        //RuleFor(x => x.UserGroup).NotEmpty();
        RuleFor(x => x.CustomerData).NotEmpty();
    }
}

public class UpsertCustomerCommandHandler(
	IApplicationDbContext context,
	IUser user,
	IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    ITenantResolver tenantResolver,
	ISender sender,
    IAuthHelper authHelper
) : IRequestHandler<UpsertCustomerCommand, CustomerCommandResponse>
{
	public async Task<CustomerCommandResponse> Handle(UpsertCustomerCommand request, CancellationToken cancellationToken)
	{
		var response = new CustomerCommandResponse
		{
			FirstName = request.FirstName,
			LastName = request.LastName,
			IDNumber = request.IDNumber,
			Email = request.Email,
			Phone = request.Phone,
			AddressLine1 = request.AddressLine1,
			AddressLine2 = request.AddressLine2,
			City = request.City,
			PostalCode = request.PostalCode,
			Province = request.Province,
			Country = request.Country,
            UserGroup = request.UserGroup,
            UserSubGroup = request.UserSubGroup,
			CustomerData = request.CustomerData,
        };

		int tenantId = tenantResolver.ResolveTenantId();

		if (user.VendorId != 0 && user.VendorId != tenantId)
			throw new VendorMisMatchException(tenantId);

		// Future Queries
		var queryVendor = context.Vendors.FirstOrDefault(x => x.VendorId == tenantId);
		var queryCustomerEmail = context.Customers.FirstOrDefault(x => x.Email == request.Email && x.VendorId == tenantId);

		// Execute queries
		var vendor =  queryVendor;
		var customer = queryCustomerEmail;

		// Check that the vendor Exists
		if (vendor == null)
		{
			throw new VendorNotFoundException(tenantId);
		}

		Address customerAddress = new Address(request.AddressLine1, request.AddressLine2, request.City, request.PostalCode, request.Country, request.Province);

        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        var isClientUser = await authHelper.LoggedIn_IsVendorAdmin();

        if (customer == null)
		{
			if (!string.IsNullOrWhiteSpace(request.CustomerPassword))
			{
				if (request.CustomerPassword.Length < 6)
					throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("CustomerPassword", "Password must be at least 6 characters long.") });
				else if (!request.CustomerPassword.Any(char.IsDigit))
					throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("CustomerPassword", "Password must contain at least 1 numeric value.") });
			}
			else
				throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("CustomerPassword", "Password cannot be empty.") });

            var createCommand = new CreateCustomerCommand(
                request.FirstName,
                request.LastName,
                request.IDNumber,
                request.Email,
                request.Phone,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.PostalCode,
                request.Province,
                request.Country,
                request.CustomerData,
                request.CustomerPassword,
                request.UserGroup,
                request.UserSubGroup
            );
            await sender.Send(createCommand, cancellationToken);
        }
		else
		{
            if (!isPlatformAdmin && !isClientUser)
			{
				var customerEmail = await identityService.GetUserEmailAsync(user.Id);
				if (customer.Email != customerEmail)
					throw new CustomerWithEmailNotFoundException(customer.Email, tenantId);
			}

			customer.FirstName = request.FirstName;
			customer.LastName = request.LastName;
			customer.IDNumber = request.IDNumber;
			customer.Email = request.Email;
			customer.Phone = request.Phone;
			customer.Address = customerAddress;
			customer.CustomerData = request.CustomerData;
        }

		string? userId = null;
        
        if (isPlatformAdmin) userId = await globalIdentityService.GetUserIdAsync(customer.Email, tenantId);
        else userId = await identityService.GetUserIdAsync(customer.Email, tenantId);

        if (userId == null) throw new CustomerUserNotFoundException(customer.Email);

		await context.SaveChangesAsync(cancellationToken);

        if (isPlatformAdmin)
        {
            // Change the phone number of the user
            await globalIdentityService.ChangeUserPhoneNumberAsync(userId, request.Phone);

            // Update the first and last name of the user in the claims
            await globalIdentityService.SetClaims(userId, ClaimTypes.GivenName.ToString(), request.FirstName);
            await globalIdentityService.SetClaims(userId, ClaimTypes.Surname.ToString(), request.LastName);
            await globalIdentityService.SetClaims(userId, "UserGroupId", request.UserGroup ?? "");
            await globalIdentityService.SetClaims(userId, "UserSubGroupId", request.UserSubGroup ?? "");
        }
        else
        {
            // Change the phone number of the user
            await identityService.ChangeUserPhoneNumberAsync(userId, request.Phone);

            // Update the first and last name of the user in the claims
            await identityService.SetClaims(userId, ClaimTypes.GivenName.ToString(), request.FirstName);
            await identityService.SetClaims(userId, ClaimTypes.Surname.ToString(), request.LastName);
            await identityService.SetClaims(userId, "UserGroupId", request.UserGroup ?? "");
            await identityService.SetClaims(userId, "UserSubGroupId", request.UserSubGroup ?? "");
        }

        return response;
	}

}
