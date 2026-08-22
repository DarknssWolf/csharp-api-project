using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Interfaces.Services;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Application.VoteItems.Helpers;
using CSharpAPIProject.Application.VoteItems.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace CSharpAPIProject.Application.VoteItems.Queries.GetVoteItem;

[Authorize(Policy = Policies.CanView_VoteItem)]
public record GetVoteItemQuery(
	int VoteItemId
) : IRequest<VoteItemDto>;

public class GetVoteItemQueryValidator : AbstractValidator<GetVoteItemQuery>
{
	public GetVoteItemQueryValidator()
	{
		RuleFor(x => x.VoteItemId).GreaterThan(0);
	}
}

public class GetVoteItemQueryHandler(
	IApplicationDbContext context,
	IConfiguration configuration,
	IMapper mapper,
	IUser user,
    IVisibilityService visibilityService,
    ITenantResolver tenantResolver) : IRequestHandler<GetVoteItemQuery, VoteItemDto>
{
	public async Task<VoteItemDto> Handle(GetVoteItemQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

        var visibleConfigIds = await visibilityService.GetVisibleConfigIdsAsync(Guid.Parse(user.Id), vendorId);

        var voteItem = await context.VoteItems
            .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
            .Include(v => v.VoteSubmissions)
            .Where(s => s.VoteItemId == request.VoteItemId)
            .Where(vi => vi.VisibilityConfigurationId != null && visibleConfigIds.Contains(vi.VisibilityConfigurationId.Value))
            .FirstOrDefaultAsync(v => v.VoteItemId == request.VoteItemId, cancellationToken);

		// Check if the VoteItem exists
		if (voteItem == null)
		{
			throw new VoteItemWithVendorAndVoteItemIdNotFoundException(request.VoteItemId, vendorId);
		}

		//Map sto DTo and recursively map children
		var voteItemDto = mapper.Map<VoteItemDto>(voteItem);

		//update question data to get galleryUri
		voteItemDto = await VoteItemHelper.GetGalleryImages(configuration, context, voteItemDto, cancellationToken);

        // Calculate vote submissions count and user vote status
        if (voteItem.VoteSubmissions != null)
        {
            voteItemDto.VoteSubmissionsCount = voteItem.VoteSubmissions.Count;
            voteItemDto.UserSubmission = voteItem.VoteSubmissions.Any(vs => vs.CreatedBy == user.Id);
        }

        voteItemDto.IsAuthor = voteItem.CreatedBy == user.Id;

		return voteItemDto;
	}

}
