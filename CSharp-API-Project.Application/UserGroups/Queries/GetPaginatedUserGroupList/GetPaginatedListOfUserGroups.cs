using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.UserGroups.Models;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.UserGroups.Queries.GetPaginatedUserGroupList;

[Authorize(Policy = Policies.CanView_UserGroup)]
public record GetPaginatedUserGroupListQuery(
    int PageNumber,
    int PageSize,
    string? QueryFilter
) : IRequest<PaginatedList<UserGroupDto>>;

public class GetPaginatedListOfUserGroupsQueryValidator : AbstractValidator<GetPaginatedUserGroupListQuery>
{
    public GetPaginatedListOfUserGroupsQueryValidator()
    {
        RuleFor(x => x.PageNumber).NotEmpty();
        RuleFor(x => x.PageSize).NotEmpty().LessThan(1000);
    }
}
public class GetPaginatedListOfUserGroups(
    IApplicationDbContext context,
    IMapper mapper,
    ITenantResolver tenantResolver,
    IAuthHelper authHelper
    ) : IRequestHandler<GetPaginatedUserGroupListQuery, PaginatedList<UserGroupDto>>
{
    public async Task<PaginatedList<UserGroupDto>> Handle(GetPaginatedUserGroupListQuery request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();
        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

        List<UserGroup>? groupQuery;

        if (isPlatformAdmin)
            groupQuery = await context.UserGroups
                .IgnoreQueryFilters()
                .Where(ug => ug.VendorId == vendorId && ug.StatusFlag.Status == RecordStatus.Active)
                .ToListAsync(cancellationToken);
        else
            groupQuery = await context.UserGroups
                .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .ToListAsync(cancellationToken);

        List<UserSubGroup>? subGroupQuery;

        if (isPlatformAdmin)
            subGroupQuery = await context.UserSubGroups
                .IgnoreQueryFilters()
                .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .Where(ug => ug.VendorId == vendorId && groupQuery.Select(g => g.GroupId).Contains(ug.GroupId.Value))
                .ToListAsync(cancellationToken);
        else
            subGroupQuery = await context.UserSubGroups
                .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
                .Where(ug => groupQuery.Select(g => g.GroupId).Contains(ug.GroupId.Value))
                .ToListAsync(cancellationToken);

        List<UserGroupDto> userGroups = new List<UserGroupDto>();
        foreach (var group in context.UserGroups)
        {
            var userGroupDto = mapper.Map<UserGroupDto>(group);

            if(subGroupQuery != null && subGroupQuery.Count() > 0)
            {
                userGroupDto.UserSubGroups = subGroupQuery
                    .Select(ug => mapper.Map<UserSubGroupDto>(ug)).OrderBy(x => x.OrderNumber)
                    .Where(x => x.GroupId == group.GroupId)
                    .ToList();
            }

            userGroups.Add(userGroupDto);
        }

        // Paginate
        var paginatedResult = userGroups.OrderBy(x => x.OrderNumber)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Return results
        return new PaginatedList<UserGroupDto>(paginatedResult, userGroups.Count(), request.PageNumber, request.PageSize);

    }
}
