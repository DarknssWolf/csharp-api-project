using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Vendors.Exceptions;
using Microsoft.Extensions.Logging;
using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.Vendors.Commands.DeleteVendorApiUser;

[Authorize(Policy = Policies.CanDelete_VendorApiUser)]
public class DeleteVendorApiUserCommand : IRequest<bool>
{
    public int VendorId { get; set; }
}

public class DeleteVendorApiUserHandler(
    IGlobalIdentityService identityService,
    ILogger<DeleteVendorApiUserHandler> logger)
    : IRequestHandler<DeleteVendorApiUserCommand, bool>
{
    public async Task<bool> Handle(DeleteVendorApiUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await identityService.DeleteVendorApiUserAndToken(request.VendorId);
            
            if (!result)
            {
                logger.LogWarning("Failed to delete vendor API user for VendorId: {VendorId}", request.VendorId);
                throw new VendorApiUserNotFoundException(request.VendorId);
            }

            logger.LogInformation("Successfully deleted vendor API user for VendorId: {VendorId}", request.VendorId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting vendor API user for VendorId: {VendorId}", request.VendorId);
            throw;
        }
    }
} 