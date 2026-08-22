using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Application.VoteItems.Helpers;
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

public class DeleteVoteItemCommandHandler(IApplicationDbContext context, ICloudFileStorage cloudFileStorage, ITenantResolver tenantResolver, IUser user) : IRequestHandler<DeleteVoteItemCommand>
{
	public async Task Handle(DeleteVoteItemCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = context.Vendors.Where(v => v.VendorId == vendorId).FutureValue();
		if (vendor.Value == null)
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

		await VoteItemHelper.GetImageDataAndRemove(context, cloudFileStorage, user,vendor.Value.Name, voteItem);
        voteItem.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
        context.VoteItems.Update(voteItem);

		await context.SaveChangesAsync(cancellationToken);
	}
}