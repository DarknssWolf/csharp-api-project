using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Customers.Commands.UpsertCustomer;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Application.Customers.Helpers.CustomerUserHelper;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.ValueObjects;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Customers.Commands.CreateCustomer;

[Authorize(Policy = Policies.CanCreate_Customer)]
public record CreateCustomerCommand(
		string FirstName,
		string LastName,
		string IDNumber,
		string Email,
		string Phone,
		string AddressLine1,
		string AddressLine2,
		string City,
		string PostalCode,
		string Province,
		string Country,
		string CustomerData,
		string CustomerPassword,
        string UserGroup,
        string? UserSubGroup,
        Guid? BranchId,
        Guid? CompanyId,
        Guid? DepartmentId,
        Guid? DesignationId,
        Guid? ProvinceId,
        Guid? ResidenceCountryId,
        Guid? ServiceCountryId,
        Guid? TeamId
        ) : IRequest;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
	public CreateCustomerCommandValidator()
	{
		RuleFor(x => x.FirstName).NotEmpty();
		RuleFor(x => x.LastName).NotEmpty();
		RuleFor(x => x.IDNumber).NotEmpty();
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Phone).NotEmpty();
		RuleFor(x => x.AddressLine1).NotEmpty();
		RuleFor(x => x.City).NotEmpty();
		RuleFor(x => x.PostalCode).NotEmpty();
        RuleFor(x => x.Province).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        //RuleFor(x => x.UserGroup).NotEmpty();
        RuleFor(x => x.CustomerPassword)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
			.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");
	}
}

public class CreateCustomerCommandHandler(
	IApplicationDbContext context, 
	ITenantResolver tenantResolver,
	IMediator mediator,
	IUser user,
	IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IAuthHelper authHelper
) :
	IRequestHandler<CreateCustomerCommand>
{
	public async Task Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
	{
		var tenantId = tenantResolver.ResolveTenantId();

		// Check that the vendor Exists
		var vendor = context.Vendors.Where(x => x.VendorId == tenantId).FirstOrDefault();
		if (vendor == null)
			throw new VendorNotFoundException(tenantId);

        // Check that the default region exists
        var region = await context.Regions.Where(x => x.VendorId == tenantId && x.RegionCode == "DEFAULT").FirstOrDefaultAsync(cancellationToken);

        if (region == null)
        {
            region = await context.Regions.Where(x => x.VendorId == tenantId).FirstOrDefaultAsync(cancellationToken);

            if (region == null)
            {
                region = new Region
                {
                    RegionCode = "DEFAULT",
                    RegionName = "Default Region",
                    VendorId = tenantId
                };
                context.Regions.Add(region);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        //Check if a customers already exists with the same Email Address or ID Number
        var checkCustomer = await context.Customers.Where(x => x.Email == request.Email || x.IDNumber == request.IDNumber).FirstOrDefaultAsync();
		if (checkCustomer != null) {
			if (checkCustomer.Email == request.Email)
				throw new CustomerWithEmailAlreadyExistsException(request.Email);
			if (checkCustomer.IDNumber == request.IDNumber)
				throw new CustomerWithIDNumberAlreadyExistsException(request.IDNumber);
		}

		Address CustomerAddress = new Address(request.AddressLine1, request.AddressLine2, request.City, request.PostalCode, request.Country, request.Province);

		Customer customer = new Customer
		{
			VendorId = tenantId,
            VendorRegionId = region.RegionId,
            FirstName = request.FirstName,
			LastName = request.LastName,
			IDNumber = request.IDNumber,
			Email = request.Email,
			Phone = request.Phone,
			Address = new Address(request.AddressLine1, request.AddressLine2, request.City, request.Province, request.Country, request.PostalCode),
			CustomerData = request.CustomerData,
		};

		context.Customers.Add(customer);
		await context.SaveChangesAsync(cancellationToken);

		// Check if the customer already has a user
		var userId = await identityService.GetUserIdAsync(request.Email, tenantId);
		// Create User
		if (userId == null)
		{
			var customerHelper = new CustomerUserHelper(identityService, globalIdentityService, user, authHelper);
			var userResult = await customerHelper.CreateCustomerUser(customer.Email, tenantResolver.ResolveTenantId(), customer.CustomerId, request.CustomerPassword, request.FirstName, request.LastName, request.Email);

			if (!userResult)
				throw new Exception("Something went wrong");

			userId = await globalIdentityService.GetUserIdAsync(request.Email, tenantId);
			if (string.IsNullOrEmpty(userId))
				throw new Exception("Something went wrong");
		} else
		{
			// Add user to the Customer Role
			var addedToRole = await identityService.AddToRoleAsync(userId, Roles.Customer);
			if (!addedToRole)
				throw new Exception("Something went wrong");
		}

        bool isTicUser = await authHelper.LoggedIn_IsTICUser();

        if (isTicUser)
        {
            await globalIdentityService.ChangeUserPhoneNumberAsync(userId, customer.Phone);

            await globalIdentityService.SetClaims(userId, ClaimTypes.GivenName.ToString(), request.FirstName);
            await globalIdentityService.SetClaims(userId, ClaimTypes.Surname.ToString(), request.LastName);
            await globalIdentityService.SetClaims(userId, "UserGroupId", request.UserGroup ?? "");
            await globalIdentityService.SetClaims(userId, "UserSubGroupId", request.UserSubGroup ?? "");
        }
        else
        {
            await identityService.ChangeUserPhoneNumberAsync(userId, customer.Phone);

            await identityService.SetClaims(userId, ClaimTypes.GivenName.ToString(), request.FirstName);
            await identityService.SetClaims(userId, ClaimTypes.Surname.ToString(), request.LastName);
            await identityService.SetClaims(userId, "UserGroupId", request.UserGroup);
            await identityService.SetClaims(userId, "UserSubGroupId", request.UserSubGroup ?? "");
        }

        // Update ApplicationUser FK fields
        await globalIdentityService.UpdateUserFieldsAsync(
            request.Email,
            tenantId,
            request.CompanyId,
            request.DesignationId,
            request.TeamId,
            request.DepartmentId,
            request.BranchId,
            request.ProvinceId,
            request.ResidenceCountryId,
            request.ServiceCountryId
        );
	}
}