using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Domain.ValueObjects;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Vendors.Commands.DeleteVendor;
[Authorize (Roles = Roles.Administrator)]
public class DeleteVendorCommand : IRequest
{
    public int VendorId { get; set; }
}

public class DeleteVendorCommandValidator : AbstractValidator<DeleteVendorCommand>
{
    public DeleteVendorCommandValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
    }
}

public class DeleteVendorCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<DeleteVendorCommand>
{
    public async Task Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await context.Vendors.Where(v => v.VendorId == request.VendorId).SingleOrDefaultAsync(cancellationToken);
        if (vendor == null)
            throw new VendorNotFoundException(request.VendorId);

        vendor.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
        context.Vendors.Update(vendor);
        await context.SaveChangesAsync(cancellationToken);
    }
}
