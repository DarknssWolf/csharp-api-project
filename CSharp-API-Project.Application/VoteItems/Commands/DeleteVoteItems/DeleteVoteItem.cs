using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Domain.ValueObjects;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteItems.Commands.DeleteVoteItem;


[Authorize(Policy = Policies.CanDelete_VoteItem)]
public record DeleteVoteItemCommand : IRequest
{
	public int VoteSetId { get; set; }
	public int VoteItemId { get; set; }
}

public class DeleteVoteItemCommandValidator : AbstractValidator<DeleteVoteItemCommand>
{
	public DeleteVoteItemCommandValidator()
	{
		RuleFor(v => v.VoteItemId).GreaterThan(0);
	}
}

public class DeleteVoteItemCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver, IUser user) : IRequestHandler<DeleteVoteItemCommand>
{
	public async Task Handle(DeleteVoteItemCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = await context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
		if (vendor == null)
			throw new VendorNotFoundException();

        var voteItem = await context.VoteSets
            .Include(x => x.VoteItems)
            .Where(s => s.VoteSetId == request.VoteSetId && s.VendorId == vendorId)
            .SelectMany(s => s.VoteItems)
            .FirstOrDefaultAsync(v => v.VoteItemId == request.VoteItemId, cancellationToken);

		if (voteItem == null)
		{
			throw new VoteItemNotFoundException(request.VoteItemId);
		}

		// Soft delete the item's file along with it
		var storedFileId = FileDataHelper.GetStoredFileId(voteItem.VoteItemData);
		if (storedFileId != null)
		{
			var storedFile = await context.StoredFiles
				.FirstOrDefaultAsync(f => f.StoredFileId == storedFileId && f.VendorId == vendorId, cancellationToken);

			if (storedFile != null)
				storedFile.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
		}

        voteItem.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
        context.VoteItems.Update(voteItem);

		await context.SaveChangesAsync(cancellationToken);
	}
}
