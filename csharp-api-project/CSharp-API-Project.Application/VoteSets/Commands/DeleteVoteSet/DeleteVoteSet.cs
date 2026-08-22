using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Application.VoteSets.Exceptions;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Application.VoteSets.Helpers;

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

public class DeleteVoteSetCommandHandler(IApplicationDbContext context, ICloudFileStorage cloudFileStorage, ITenantResolver tenantResolver, IUser user) : IRequestHandler<DeleteVoteSetCommand>
{
	public async Task Handle(DeleteVoteSetCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = context.Vendors.Where(v => v.VendorId == vendorId).FutureValue();
		if (vendor.Value == null)
			throw new VendorNotFoundException();

		var voteSet = await context.VoteSets
			.Include(x => x.VoteItems)
			.FirstOrDefaultAsync(s => s.VoteSetId == request.VoteSetId && s.VendorId == vendorId, cancellationToken);

		if (voteSet == null)
		{
			throw new VoteSetNotFoundException(request.VoteSetId);
		}

		var VoteSetQuestionsToRemove = voteSet.VoteItems;
		await VoteSetHelper.GetImageDataAndRemove(context, cloudFileStorage,user,vendor.Value.Name, voteSet);
		context.VoteItems.RemoveRange(VoteSetQuestionsToRemove);
		context.VoteSets.Remove(voteSet);

		await context.SaveChangesAsync(cancellationToken);
	}
}