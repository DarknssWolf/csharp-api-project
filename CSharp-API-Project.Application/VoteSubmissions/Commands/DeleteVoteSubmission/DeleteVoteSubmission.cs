using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;

namespace CSharpAPIProject.Application.VoteSubmissions.Commands.DeleteVoteSubmission;

[Authorize(Policy = Policies.CanDelete_VoteSubmission)]
public record DeleteVoteSubmissionCommand : IRequest
{
	public int VoteItemId { get; set; }
}

public class DeleteVoteSubmissionCommandValidator : AbstractValidator<DeleteVoteSubmissionCommand>
{
	public DeleteVoteSubmissionCommandValidator()
	{
		RuleFor(v => v.VoteItemId).GreaterThan(0);
	}
}

public class DeleteVoteSubmissionCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<DeleteVoteSubmissionCommand>
{
	public async Task Handle(DeleteVoteSubmissionCommand request, CancellationToken cancellationToken)
	{
        var voteSubmission = await context.VoteItems
            .Include(x => x.VoteSubmissions)
            .Where(vi => vi.VoteItemId == request.VoteItemId)
            .SelectMany(vi => vi.VoteSubmissions)
            .FirstOrDefaultAsync(vs => vs.CreatedBy == user.Id, cancellationToken);

		if (voteSubmission == null)
		{
			return;
		}

        // vote submissions SHOULD BE REMOVED 
        //because the user can spam the vote submission button which creates multiple submissions
        context.VoteSubmissions.Remove(voteSubmission);

		await context.SaveChangesAsync(cancellationToken);
	}
}
