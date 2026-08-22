using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.VoteSets.Exceptions;
using CSharpAPIProject.Application.VoteSets.Helpers;
using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.Services;

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
	IConfiguration configuration,
	IMapper mapper,
	IUser user,
    IAuthHelper authHelper,
    IVisibilityService visibilityService,
    ITenantResolver tenantResolver) : IRequestHandler<GetVoteSetQuery, VoteSetDto>
{
	public async Task<VoteSetDto> Handle(GetVoteSetQuery request, CancellationToken cancellationToken)
	{
        var vendorId = tenantResolver.ResolveTenantId();
        bool isTICUser = await authHelper.LoggedIn_IsTICUser();
        bool isClientAdmin = await authHelper.LoggedIn_IsClientAdmin();

        var visibleConfigIds = await visibilityService.GetVisibleConfigIdsAsync(Guid.Parse(user.Id), vendorId);

        var voteSet = await context.VoteSets
            .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
            .Where(s => s.VendorId == vendorId && s.VoteSetId == request.VoteSetId )
            .Where(vs => vs.VisibilityConfigurationId != null && visibleConfigIds.Contains(vs.VisibilityConfigurationId.Value))
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
                EmailNotification = (isTICUser || isClientAdmin) ? s.EmailNotification : "",//exclude email list if not TIC or Client Admin
                VoteSetData = s.VoteSetData,
                WhoCanCreateVoteItemFlag = s.WhoCanCreateVoteItemFlag,
                VisibilityConfigurationId = s.VisibilityConfigurationId,
                VoteItems = s.VoteItems
                    .Where(vi => isTICUser || isClientAdmin || vi.Status == VoteItemStatus.Approve)
                    .Where(vs => vs.VisibilityConfigurationId != null && visibleConfigIds.Contains(vs.VisibilityConfigurationId.Value))
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

		//update question data to get galleryUri
		voteSetDto = await VoteSetHelper.GetGalleryImages(configuration, context, voteSetDto, cancellationToken);

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
