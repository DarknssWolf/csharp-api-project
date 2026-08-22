using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CSharpAPIProject.Application.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandResponse() { 
	public bool Success = false;
	public string Message = string.Empty;
}

[Authorize(Policy = Policies.CanDelete_Customer)]
public record DeleteCustomerCommand(int CustomerId) : IRequest<DeleteCustomerCommandResponse>;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
	public DeleteCustomerCommandValidator()
	{
		RuleFor(x => x.CustomerId).NotEmpty();
	}
}

public class DeleteCustomerCommandHandler(
    IApplicationDbContext context, 
    IUser user, 
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    ILogger<DeleteCustomerCommand> logger,
    IAuthHelper authHelper
) :
	IRequestHandler<DeleteCustomerCommand, DeleteCustomerCommandResponse>
{
	public async Task<DeleteCustomerCommandResponse> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
	{
		var customerQuery = context.Customers.Where(c => c.CustomerId == request.CustomerId).FirstOrDefault();

		var customer = customerQuery;

		if (customer == null)
			throw new CustomerNotFoundException(request.CustomerId);

		var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        var isClientUser = await authHelper.LoggedIn_IsVendorAdmin();

        if (!isPlatformAdmin && !isClientUser)
			throw new CustomerNotFoundException(request.CustomerId);
		else if (isClientUser)
		{
			var userTenantId = await identityService.GetTenantId(user.Id);
			if(userTenantId != customer.VendorId)
				throw new CantDeleteOtherVendorCustomerException();
		}

        //we dont delete, the user claim is set to deleted
        //context.Customers.Remove(customer);
        var successMessage = "Customer deleted successfully.";

		string? userToDelete;
        IList<string?> userToDeleteRoles;

        if (isPlatformAdmin) { 
            userToDelete = await globalIdentityService.GetUserIdAsync(customer.Email, customer.VendorId);
            userToDeleteRoles = await globalIdentityService.GetRolesAsync(Guid.Parse(userToDelete));

            bool hasOtherRoles = userToDeleteRoles.Any(r => r != Roles.Customer);

            // Only delete the user if they are not assigned to any other roles.
            if (!hasOtherRoles)
            {
                await globalIdentityService.SetClaims(userToDelete, "UserStatus", UserStatus.Deleted.ToString());
                //await globalIdentityService.DeleteUserAsync(userToDelete);
                successMessage += "\nUser deleted successfully.";
            }
        }
        else { 
            userToDelete = await identityService.GetUserIdAsync(customer.Email, customer.VendorId);
            userToDeleteRoles = await identityService.GetRolesAsync(Guid.Parse(userToDelete));

            bool hasOtherRoles = userToDeleteRoles.Any(r => r != Roles.Customer);

            // Only delete the user if they are not assigned to any other roles.
            if (!hasOtherRoles)
            {
                await identityService.SetClaims(userToDelete, "UserStatus", UserStatus.Deleted.ToString());
                //await identityService.DeleteUserAsync(userToDelete);
                successMessage += "\nUser deleted successfully.";
            }
        }

		try
		{
			await context.SaveChangesAsync(cancellationToken);
		} catch(Exception e)
		{
			logger.LogError($"Failed to delete customer from the database. {0}", e.Message);
			return new DeleteCustomerCommandResponse()
			{
				Success = false,
				Message = e.Message
			};
		}

		return new DeleteCustomerCommandResponse() { Success = true };
	}
}