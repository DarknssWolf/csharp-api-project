using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.VoteSets.Exceptions;
using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteSets.Queries.GetVoteSet;

[Authorize(Policy = Policies.CanView_VoteSet)]
public record GetVoteSetQuery(
	int VoteSetId
) : IRequest<VoteSetDto>;

public class GetVoteSetQueryValidator : AbstractValidator<GetVoteSetQuery>
{
	public GetVoteSetQueryValidator()
	{
		RuleFor(x => x.VoteSetId).GreaterThan(0);
	}
}

public class GetVoteSetQueryHandler(
	IApplicationDbContext context,
	IMapper mapper,
	IUser user,
    IAuthHelper authHelper,
    ITenantResolver tenantResolver) : IRequestHandler<GetVoteSetQuery, VoteSetDto>
{
	public async Task<VoteSetDto> Handle(GetVoteSetQuery request, CancellationToken cancellationToken)
	{
        var vendorId = tenantResolver.ResolveTenantId();
        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        bool isClientAdmin = await authHelper.LoggedIn_IsVendorAdmin();

        var voteSet = await context.VoteSets
            .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
            .Where(s => s.VendorId == vendorId && s.VoteSetId == request.VoteSetId )
            .Select(s => new VoteSet
            {
                VoteSetId = s.VoteSetId,
                VendorId = s.VendorId,
                Title = s.Title,
                Code = s.Code,
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
                        VoteSubmissions = vi.VoteSubmissions != null ? vi.VoteSubmissions.ToList() : new List<VoteSubmission>(),
                        VisibilityConfigurationId = vi.VisibilityConfigurationId,
                    })
                    .ToList()
            }).FirstOrDefaultAsync(cancellationToken);

        // Check if the VoteSet exists
        if (voteSet == null)
		{
			throw new VoteSetWithVendorAndVoteSetIdNotFoundException(request.VoteSetId, vendorId);
		}

		//Map sto DTo and recursively map children
		var voteSetDto = mapper.Map<VoteSetDto>(voteSet);

		// Add download links for any stored files referenced in the data
		voteSetDto.VoteSetData = FileDataHelper.AddFileUri(voteSetDto.VoteSetData);
		foreach (var voteItemDto in voteSetDto.VoteItems ?? [])
			voteItemDto.VoteItemData = FileDataHelper.AddFileUri(voteItemDto.VoteItemData);

		// Update each vote item with submission count and user vote status
		foreach (var voteItemDto in voteSetDto.VoteItems ?? [])
		{
			var voteItemEntity = voteSet.VoteItems.FirstOrDefault(vi => vi.VoteItemId == voteItemDto.VoteItemId);
			if (voteItemEntity != null)
			{
                if (voteItemEntity.VoteSubmissions != null)
                {
                    voteItemDto.VoteSubmissionsCount = voteItemEntity.VoteSubmissions.Count;
                    voteItemDto.UserSubmission = voteItemEntity.VoteSubmissions.Any(vs => vs.CreatedBy == user.Id);
                }
                voteItemDto.IsAuthor = voteItemEntity.CreatedBy == user.Id;
			}
		}

		return voteSetDto;
	}

}
