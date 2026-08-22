using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.UserGroups.Exceptions;
using CSharpAPIProject.Application.UserGroups.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.UserGroups.Queries.GetUserGroup;

[Authorize(Policy = Policies.CanView_UserGroup)]
public record GetUserGroupQuery(
    Guid UserGroupId
) : IRequest<UserGroupDto>;

public class GetUserGroupsQueryValidator : AbstractValidator<GetUserGroupQuery>
{
    public GetUserGroupsQueryValidator()
    {
        RuleFor(x => x.UserGroupId).NotEmpty();
    }
}
public class GetUserGroup(
    IApplicationDbContext context,
    IMapper mapper,
    ITenantResolver tenantResolver,
    IAuthHelper authHelper
    ) : IRequestHandler<GetUserGroupQuery, UserGroupDto>
{
    public async Task<UserGroupDto> Handle(GetUserGroupQuery request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();
        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

        UserGroup? groupQuery;

        if(isPlatformAdmin)
            groupQuery = await context.UserGroups
                .IgnoreQueryFilters()
                .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .Where(ug => ug.GroupId == request.UserGroupId && ug.VendorId == vendorId)
                .FirstOrDefaultAsync(cancellationToken);
        else
            groupQuery = await context.UserGroups.Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .Where(ug => ug.GroupId == request.UserGroupId)
                .FirstOrDefaultAsync(cancellationToken);

        if(groupQuery == null)
            throw new UserGroupNotFoundException(request.UserGroupId);

        List<UserSubGroup>? subGroupQuery;

        if (isPlatformAdmin)
            subGroupQuery = await context.UserSubGroups
                .IgnoreQueryFilters()
                .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .Where(ug => ug.GroupId == request.UserGroupId && ug.VendorId == vendorId)
                .ToListAsync(cancellationToken);
        else
            subGroupQuery = await context.UserSubGroups.Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .Where(ug => ug.GroupId == request.UserGroupId)
                .ToListAsync(cancellationToken);

        var userGroupDto = mapper.Map<UserGroupDto>(groupQuery);

        if(subGroupQuery != null && subGroupQuery.Count() > 0)
        {
            userGroupDto.UserSubGroups = subGroupQuery
                .Select(ug => mapper.Map<UserSubGroupDto>(ug)).OrderBy(x => x.OrderNumber)
                .ToList();
        }

        return userGroupDto;
    }
}
