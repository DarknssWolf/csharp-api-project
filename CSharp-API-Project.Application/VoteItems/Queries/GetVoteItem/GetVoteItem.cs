using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Application.VoteItems.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

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
	IMapper mapper,
	IUser user,
    ITenantResolver tenantResolver) : IRequestHandler<GetVoteItemQuery, VoteItemDto>
{
	public async Task<VoteItemDto> Handle(GetVoteItemQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

        var voteItem = await context.VoteItems
            .Where(ug => ug.StatusFlag.Status == RecordStatus.Active)
            .Include(v => v.VoteSubmissions)
            .Where(s => s.VoteItemId == request.VoteItemId)
            .FirstOrDefaultAsync(v => v.VoteItemId == request.VoteItemId, cancellationToken);

		// Check if the VoteItem exists
		if (voteItem == null)
		{
			throw new VoteItemWithVendorAndVoteItemIdNotFoundException(request.VoteItemId, vendorId);
		}

		//Map sto DTo and recursively map children
		var voteItemDto = mapper.Map<VoteItemDto>(voteItem);

		// Add a download link if the data references a stored file
		voteItemDto.VoteItemData = FileDataHelper.AddFileUri(voteItemDto.VoteItemData);

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
