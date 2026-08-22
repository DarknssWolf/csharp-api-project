using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.UserGroups.Exceptions;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.UserGroups.Commands.DeleteUserGroup;

[Authorize(Policy = Policies.CanDelete_UserGroup)]
public record DeleteUserGroupCommand : IRequest
{
    public Guid UserGroupId { get; set; }
}

public class DeleteUserGroupCommandValidator : AbstractValidator<DeleteUserGroupCommand>
{
    public DeleteUserGroupCommandValidator()
    {
        RuleFor(v => v.UserGroupId).NotEmpty();
    }
}
public class DeleteUserGroupCommandHandler(
    IApplicationDbContext context,
    IAuthHelper authHelper,
    ITenantResolver tenantResolver) 
    : IRequestHandler<DeleteUserGroupCommand>
{
    public async Task Handle(DeleteUserGroupCommand request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();
        bool isTicAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

        UserGroup? userGroup;

        if(isTicAdmin)
            userGroup = await context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == request.UserGroupId && ug.VendorId == vendorId, cancellationToken);
        else
            userGroup = await context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == request.UserGroupId, cancellationToken);

        if (userGroup == null)
        {
            throw new UserGroupNotFoundException(request.UserGroupId);
        }

        List<UserSubGroup>? userSubGroupsToRemove;
        if(isTicAdmin)
            userSubGroupsToRemove = await context.UserSubGroups.Where(usg => usg.GroupId == request.UserGroupId && usg.VendorId == vendorId).ToListAsync(cancellationToken);
        else
            userSubGroupsToRemove = await context.UserSubGroups.Where(usg => usg.GroupId == request.UserGroupId).ToListAsync(cancellationToken);

        if (userSubGroupsToRemove != null)
        {
            for (int i = 0; i < userSubGroupsToRemove.Count; i++)
            {
                var userSubGroup = userSubGroupsToRemove[i];
                if (userSubGroup.StatusFlag.Status == RecordStatus.Active)
                {
                    userSubGroup.StatusFlag = new EntityStatus(RecordStatus.Deleted, userGroup.StatusFlag.StatusUpdatedBy, DateTimeOffset.UtcNow);
                }
            }
            context.UserSubGroups.UpdateRange(userSubGroupsToRemove);
        }

        context.UserGroups.Update(userGroup);

        await context.SaveChangesAsync(cancellationToken);
    }
}
