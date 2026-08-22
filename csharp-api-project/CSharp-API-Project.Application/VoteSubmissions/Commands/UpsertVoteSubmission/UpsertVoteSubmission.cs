using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.VoteSubmissions.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Application.Common.Security;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteSubmissions.Commands.UpsertVoteSubmission;

[Authorize(Policy = Policies.CanCreate_VoteSubmission)]
[Authorize(Policy = Policies.CanUpdate_VoteSubmission)]
public record UpsertVoteSubmissionCommand(
	int VoteItemId
) : IRequest;

public class UpsertVoteSubmissionCommandValidator : AbstractValidator<UpsertVoteSubmissionCommand>
{
	public UpsertVoteSubmissionCommandValidator()
	{
		RuleFor(x => x.VoteItemId).NotEmpty();
	}
}

public class UpsertVoteSubmissionCommandHandler(
	IApplicationDbContext _context,
	IUser _user
) : IRequestHandler<UpsertVoteSubmissionCommand>
{
	public async Task Handle(UpsertVoteSubmissionCommand request, CancellationToken cancellationToken)
	{
        //check if the customer has already voted
        var voteItem = await _context.VoteItems
        .Include(x => x.VoteSubmissions)
        .Where(i => i.VoteItemId == request.VoteItemId)
        .FirstOrDefaultAsync(cancellationToken);
        
        if (voteItem == null)
            throw new VoteItemNotFoundException(request.VoteItemId);

        var voteSet = await _context.VoteSets.Where(v => v.VoteSetId == voteItem.VoteSetId).FirstOrDefaultAsync(cancellationToken);

        if (voteSet == null)
            throw new VoteSetNotFoundException(voteItem.VoteSetId);

        if (voteSet.EndOn <= DateTime.UtcNow)
            throw new VoteSetExpiredException(voteItem.VoteSetId, voteSet.EndOn);

        if (voteSet.StartOn > DateTime.UtcNow)
            throw new VoteSetNotStartedException(voteItem.VoteSetId, voteSet.StartOn);

        if (voteItem.Status != VoteItemStatus.Approve)
			throw new VoteItemNotApprovedException(request.VoteItemId);

		var voteSubmission = voteItem?.VoteSubmissions?
        .Where(s => s.CreatedBy == _user.Id)
        .FirstOrDefault();

		//only create vote if the customer has not voted
		if (voteSubmission == null)
		{
            // Create new VoteSubmission if not found
            voteSubmission = new VoteSubmission
            {
                VoteItemId = request.VoteItemId,
                VisibilityConfigurationId = voteItem?.VisibilityConfigurationId //set visibility to that of the item
            };

			_context.VoteSubmissions.Add(voteSubmission);

			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}