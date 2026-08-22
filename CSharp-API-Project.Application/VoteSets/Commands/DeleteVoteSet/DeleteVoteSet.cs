using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.VoteSets.Exceptions;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.VoteSets.Commands.DeleteVoteSet;


[Authorize(Policy = Policies.CanDelete_VoteSet)]
public record DeleteVoteSetCommand : IRequest
{
	public int VoteSetId { get; set; }
}

public class DeleteVoteSetCommandValidator : AbstractValidator<DeleteVoteSetCommand>
{
	public DeleteVoteSetCommandValidator()
	{
		RuleFor(v => v.VoteSetId).GreaterThan(0);
	}
}

public class DeleteVoteSetCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver, IUser user) : IRequestHandler<DeleteVoteSetCommand>
{
	public async Task Handle(DeleteVoteSetCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = await context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
		if (vendor == null)
			throw new VendorNotFoundException();

		var voteSet = await context.VoteSets
			.Include(x => x.VoteItems)
			.FirstOrDefaultAsync(s => s.VoteSetId == request.VoteSetId && s.VendorId == vendorId, cancellationToken);

		if (voteSet == null)
		{
			throw new VoteSetNotFoundException(request.VoteSetId);
		}

		var VoteSetQuestionsToRemove = voteSet.VoteItems;

		// Soft delete any files attached to the vote set and its items
		var storedFileIds = voteSet.VoteItems
			.Select(vi => FileDataHelper.GetStoredFileId(vi.VoteItemData))
			.Append(FileDataHelper.GetStoredFileId(voteSet.VoteSetData))
			.Where(id => id != null)
			.Select(id => id.Value)
			.ToList();

		if (storedFileIds.Any())
		{
			var storedFiles = await context.StoredFiles
				.Where(f => f.VendorId == vendorId && storedFileIds.Contains(f.StoredFileId))
				.ToListAsync(cancellationToken);

			foreach (var storedFile in storedFiles)
				storedFile.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
		}

		context.VoteItems.RemoveRange(VoteSetQuestionsToRemove);
		context.VoteSets.Remove(voteSet);

		await context.SaveChangesAsync(cancellationToken);
	}
}
