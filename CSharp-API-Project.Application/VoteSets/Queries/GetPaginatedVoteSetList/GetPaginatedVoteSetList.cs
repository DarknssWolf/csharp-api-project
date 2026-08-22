using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.VoteSets.Queries.GetPaginatedVoteSetList;


[Authorize(Policy = Policies.CanView_VoteSet)]
public record GetPaginatedVoteSetListQuery(
	int PageNumber,
	int PageSize,
	string QueryFilter,
    bool archived = false
) : IRequest<PaginatedList<VoteSetDto>>;

public class GetPaginatedVoteSetListQueryValidator : AbstractValidator<GetPaginatedVoteSetListQuery>
{
	public GetPaginatedVoteSetListQueryValidator()
	{
		RuleFor(x => x.PageNumber).NotEmpty();
		RuleFor(x => x.PageSize).NotEmpty().LessThan(1000);
	}
}

public class GetPaginatedVoteSetListQueryHandler(
	IApplicationDbContext context,
	IMapper mapper,
	ITenantResolver tenantResolver,
    IAuthHelper authHelper,
    IUser user) : IRequestHandler<GetPaginatedVoteSetListQuery, PaginatedList<VoteSetDto>>
{
	public async Task<PaginatedList<VoteSetDto>> Handle(GetPaginatedVoteSetListQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        bool isClientAdmin = await authHelper.LoggedIn_IsVendorAdmin();

        var query = context.VoteSets
            .Where(s => s.VendorId == vendorId)
            .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
            .Select(s => new VoteSet
            {
                VoteSetId = s.VoteSetId,
                VendorId = s.VendorId,
                Title = s.Title,
                Code =s.Code,
                StartOn = s.StartOn,
                EndOn = s.EndOn,
                CanEarnPoints = s.CanEarnPoints,
                AllowMultipleTries = s.AllowMultipleTries,
                AllowMultipleItems = s.AllowMultipleItems,
                EmailNotification = (isPlatformAdmin || isClientAdmin) ? s.EmailNotification : "",//exclude email list if not platform or vendor admin
                VoteSetData = s.VoteSetData,
                WhoCanCreateVoteItemFlag = s.WhoCanCreateVoteItemFlag,
                VisibilityConfigurationId = s.VisibilityConfigurationId,
                VoteItems = s.VoteItems
                    .Where(vi => isPlatformAdmin || isClientAdmin || vi.Status == VoteItemStatus.Approve)
                    .Select(vi => new VoteItem
                    {
                        VoteItemId = vi.VoteItemId,
                        VoteSetId = vi.VoteSetId,
                        Title = vi.Title,
                        Description = vi.Description,
                        Status = vi.Status,
                        VoteItemStatusReason = vi.VoteItemStatusReason,
                        VoteItemData = vi.VoteItemData,
                        CreatedBy = vi.CreatedBy,
                        VoteSubmissions = vi.VoteSubmissions != null ? vi.VoteSubmissions.ToList() : new List<VoteSubmission>(),
                        VisibilityConfigurationId = vi.VisibilityConfigurationId
                    })
                    .ToList()
            });

        if (string.IsNullOrWhiteSpace(request.QueryFilter) == false)
		{
			query = query
				.Where(s => s.Title.Contains(request.QueryFilter) || s.Code.Contains(request.QueryFilter));
		}

        //admins should be able to see archived results through the admin portal
        if (!await authHelper.LoggedIn_IsPlatformAdmin())
        {
            query = query
                .Where(s => (request.archived) ? s.EndOn < DateTime.Now : s.EndOn >= DateTime.Now);
        }

        var voteSets = await query.ToListAsync(cancellationToken);

		List<VoteSetDto> voteSetsDto = new List<VoteSetDto>();

		foreach (var voteSet in voteSets)
		{
			var voteSetDto = mapper.Map<VoteSetDto>(voteSet);

			voteSetDto.VoteSetData = FileDataHelper.AddFileUri(voteSetDto.VoteSetData);
			foreach (var voteItemDto in voteSetDto.VoteItems ?? [])
				voteItemDto.VoteItemData = FileDataHelper.AddFileUri(voteItemDto.VoteItemData);

            //get ANY vote items created by the user for this vote set
            var hasUserUploaded = await context.VoteItems
                .Where(vi => vi.VoteSetId == voteSet.VoteSetId && vi.CreatedBy == user.Id)
                .FirstOrDefaultAsync(cancellationToken);

            voteSetDto.userHasUploaded = hasUserUploaded != null;

            if (voteSetDto.VoteItems != null)
            {
                foreach (var voteItem in voteSetDto.VoteItems)
                {
                    var voteItemEntity = voteSet.VoteItems.FirstOrDefault(vi => vi.VoteItemId == voteItem.VoteItemId);
                    if (voteItemEntity != null)
                    {
                        voteItem.VoteSubmissionsCount = voteItemEntity.VoteSubmissions != null ? voteItemEntity.VoteSubmissions.Count : 0;
                        voteItem.UserSubmission = voteItemEntity.VoteSubmissions != null ? voteItemEntity.VoteSubmissions
                            .Where(vs => vs.CreatedBy == user.Id)
                            .FirstOrDefault() != null ? true : false //check if the use voted on this item
                        : false;
                        voteItem.IsAuthor = voteItemEntity.CreatedBy == user.Id;
                    }
                }

                voteSetDto.VoteItems = voteSetDto.VoteItems.OrderByDescending(q => q.VoteItemId).ToList();
                voteSetsDto.Add(voteSetDto);
            }
		}

		// Paginate
		var paginatedResult = voteSetsDto.OrderByDescending(q => q.VoteSetId)
            .Skip((request.PageNumber - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToList();

		// Return results
		return new PaginatedList<VoteSetDto>(paginatedResult, voteSetsDto.Count(), request.PageNumber, request.PageSize);
	}
}