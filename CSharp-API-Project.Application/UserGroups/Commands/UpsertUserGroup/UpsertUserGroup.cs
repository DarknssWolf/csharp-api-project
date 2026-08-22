using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.UserGroups.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Application.Interfaces.Entities;

namespace CSharpAPIProject.Application.UserGroups.Commands.UpsertUserGroup;

[Authorize(Policy = Policies.CanCreate_UserGroup)]
[Authorize(Policy = Policies.CanUpdate_UserGroup)]
public record UpsertUserGroupCommand(
    Guid? GroupId,
    string Name,
    string? Description,
    int OrderNumber,
    IEnumerable<UserSubGroupDto>? SubGroups
) : IRequest;

public class UpsertUserGroupCommandValidator : AbstractValidator<UpsertUserGroupCommand>
{
    public UpsertUserGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.OrderNumber).NotEmpty();
    }
}
public class UpsertUserGroupCommandHandler(
    IApplicationDbContext _context,
    ITenantResolver _tenantResolver,
    IMapper _mapper,
    IAuthHelper _authHelper
) : IRequestHandler<UpsertUserGroupCommand>
{
    public async Task Handle(UpsertUserGroupCommand request, CancellationToken cancellationToken)
    {
        // use tenant resolver
        int vendorId = _tenantResolver.ResolveTenantId();
        bool isTicAdmin = await _authHelper.LoggedIn_IsPlatformAdmin();
        bool isClientAdmin = await _authHelper.LoggedIn_IsVendorAdmin();

        UserGroup? userGroup;
        if(!isTicAdmin)
            userGroup = await _context.UserGroups
                .Where(ug => ug.GroupId == request.GroupId)
                .FirstOrDefaultAsync(cancellationToken);
        else
            userGroup = await _context.UserGroups.IgnoreQueryFilters()
                .Where(ug => ug.GroupId == request.GroupId && ug.VendorId == vendorId)
                .FirstOrDefaultAsync(cancellationToken);

        if(userGroup == null)
        {
            userGroup = new UserGroup
            {
                GroupId = request.GroupId ?? Guid.NewGuid(),
                VendorId = vendorId,
                Name = request.Name,
                Description = request.Description,
                OrderNumber = request.OrderNumber
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.SubGroups != null && request.SubGroups.Count() > 0)
            {
                foreach(var subGroup in request.SubGroups)
                {
                    var userSubGroup = new UserSubGroup
                    {
                        SubGroupId = subGroup.SubGroupId,
                        VendorId = vendorId,
                        GroupId = userGroup.GroupId,
                        Name = subGroup.Name,
                        Description = subGroup.Description,
                        OrderNumber = subGroup.OrderNumber
                    };
                    _context.UserSubGroups.Add(userSubGroup);
                }
            }
        }
        else
        {
            userGroup.Name = request.Name;
            userGroup.Description = request.Description;
            userGroup.OrderNumber = request.OrderNumber;

            List<UserSubGroup>? existingSubGroups = await _context.UserSubGroups
                .Where(usg => usg.GroupId == userGroup.GroupId)
                .ToListAsync();

            if (request.SubGroups != null && request.SubGroups.Count() > 0)
            {
                foreach (var subGroup in request.SubGroups)
                {
                    var existingSubGroup = existingSubGroups
                        .FirstOrDefault(usg => usg.SubGroupId == subGroup.SubGroupId);

                    if (existingSubGroup == null)
                    {
                        var userSubGroup = new UserSubGroup
                        {
                            SubGroupId = subGroup.SubGroupId,
                            VendorId = vendorId,
                            GroupId = userGroup.GroupId,
                            Name = subGroup.Name,
                            Description = subGroup.Description,
                            OrderNumber = subGroup.OrderNumber
                        };
                        _context.UserSubGroups.Add(userSubGroup);
                    }
                    else
                    {
                        existingSubGroup.Name = subGroup.Name;
                        existingSubGroup.Description = subGroup.Description;
                        existingSubGroup.OrderNumber = subGroup.OrderNumber;
                    }
                }
            }

            if(request.SubGroups == null || !request.SubGroups.Any())
            {
                // If no subgroups are provided, remove all existing subgroups for this group
                _context.UserSubGroups.RemoveRange(existingSubGroups);
            }
            else
            {
                // Remove any subgroups that are not in the request
                var subGroupsToRemove = existingSubGroups
                    .Where(usg => !request.SubGroups.Any(rsg => rsg.SubGroupId == usg.SubGroupId))
                    .ToList();
             
                _context.UserSubGroups.RemoveRange(subGroupsToRemove);
            }
        }

        //save context
        await _context.SaveChangesAsync(cancellationToken);
    }
}
