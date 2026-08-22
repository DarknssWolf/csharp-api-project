using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.VoteItems.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteItems.Queries.GetPaginatedVoteItemList;

[Authorize(Policy = Policies.CanView_VoteItem)]
public record GetPaginatedVoteItemListQuery(
    int PageNumber,
    int PageSize,
    string? QueryFilter,
    DateTime? FromDate,
    DateTime? ToDate
) : IRequest<PaginatedList<VoteItemDto>>;

public class GetPaginatedVoteItemListQueryValidator : AbstractValidator<GetPaginatedVoteItemListQuery>
{
    public GetPaginatedVoteItemListQueryValidator()
    {
        RuleFor(x => x.PageNumber).NotEmpty();
        RuleFor(x => x.PageSize).NotEmpty().LessThan(1000);
    }
}

public class GetPaginatedVoteItemListQueryHandler(
    IApplicationDbContext context,
    ITenantResolver tenantResolver,
    IAuthHelper authHelper,
    IUser user) : IRequestHandler<GetPaginatedVoteItemListQuery, PaginatedList<VoteItemDto>>
{
    public async Task<PaginatedList<VoteItemDto>> Handle(GetPaginatedVoteItemListQuery request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveTenantId();
        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        bool isClientAdmin = await authHelper.LoggedIn_IsVendorAdmin();

        var voteItem = context.VoteItems
            .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
            .Where(v => v.CreatedBy == user.Id) // filter based on logged in user and visible config ids
            .Select(v => new VoteItemDto //pre map so that we can include the VoteSet Title, without creating nested objects
            {
                VoteItemId = v.VoteItemId,
                VoteSetId = v.VoteSetId,
                VoteSetTitle = v.VoteSet.Title,
                Title = v.Title,
                Description = v.Description,
                Status = v.Status,
                VoteItemStatusReason = v.VoteItemStatusReason,
                VoteItemData = v.VoteItemData,
                CreatedDate = v.CreatedOn,
                IsAuthor = v.CreatedBy == user.Id,
                VoteSubmissionsCount = v.VoteSubmissions != null ? v.VoteSubmissions.Count : 0,
                UserSubmission = v.VoteSubmissions != null ? v.VoteSubmissions.Any(vs => vs.CreatedBy == user.Id) : false,
                VisibilityConfigurationId = v.VisibilityConfigurationId ?? Guid.Empty // Ensure we have a default value for visibility configuration ID
            });

        //filter based on query
        if (string.IsNullOrWhiteSpace(request.QueryFilter) == false)
        {
            voteItem = voteItem
                .Where(vi => vi.Title.Contains(request.QueryFilter));
        }

        //filter based on date range
        if (request.FromDate != null)
        {
            voteItem = voteItem
                .Where(vi => vi.CreatedDate >= request.FromDate);
        }

        if ( request.ToDate != null)
        {
            voteItem = voteItem
                .Where(vi => vi.CreatedDate <= request.ToDate);
        }


        var VoteItems = await voteItem.ToListAsync(cancellationToken);

        // Add download links for any stored files referenced in the data
        foreach (var item in VoteItems)
            item.VoteItemData = FileDataHelper.AddFileUri(item.VoteItemData);

        //order by date created
        VoteItems = VoteItems.OrderByDescending(q => q.CreatedDate).ToList();

        // Paginate
        var paginatedResult = VoteItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Return results
        return new PaginatedList<VoteItemDto>(paginatedResult, VoteItems.Count(), request.PageNumber, request.PageSize);
    }
}