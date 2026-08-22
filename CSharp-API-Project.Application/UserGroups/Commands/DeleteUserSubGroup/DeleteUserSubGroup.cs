using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.UserGroups.Exceptions;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.UserGroups.Commands.DeleteUserSubGroup;

[Authorize(Policy = Policies.CanDelete_UserGroup)]
public record DeleteUserSubGroupCommand : IRequest
{
    public Guid UserSubGroupId { get; set; }
}

public class DeleteUserSubGroupCommandValidator : AbstractValidator<DeleteUserSubGroupCommand>
{
    public DeleteUserSubGroupCommandValidator()
    {
        RuleFor(v => v.UserSubGroupId).NotEmpty();
    }
}
public class DeleteUserSubGroupCommandHandler(
    IApplicationDbContext context,
    IAuthHelper authHelper,
    IUser user,
    ITenantResolver tenantResolver) 
    : IRequestHandler<DeleteUserSubGroupCommand>
{
    public async Task Handle(DeleteUserSubGroupCommand request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();
        bool isTicAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

        UserSubGroup? UserSubGroup;

        if(isTicAdmin)
            UserSubGroup = await context.UserSubGroups
                .FirstOrDefaultAsync(ug => ug.SubGroupId == request.UserSubGroupId && ug.VendorId == vendorId, cancellationToken);
        else
            UserSubGroup = await context.UserSubGroups
                .FirstOrDefaultAsync(ug => ug.SubGroupId == request.UserSubGroupId, cancellationToken);

        if (UserSubGroup == null)
        {
            throw new UserSubGroupNotFoundException(request.UserSubGroupId);
        }

        UserSubGroup.StatusFlag = new EntityStatus(RecordStatus.Deleted,
            user.Id,
            statusUpdatedOn: DateTimeOffset.UtcNow);
        context.UserSubGroups.Update(UserSubGroup);

        await context.SaveChangesAsync(cancellationToken);
    }
}
