using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponseSets.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.SurveyResponseSets.Commands.DeleteSurveyResponseSet;

[Authorize(Policy = Policies.CanDelete_SurveyResponseSet)]
public record DeleteSurveyResponseSetCommand(int SurveyResponseSetId) : IRequest;

public class DeleteSurveyResponseSetCommandValidator : AbstractValidator<DeleteSurveyResponseSetCommand>
{
	public DeleteSurveyResponseSetCommandValidator()
	{
		RuleFor(x => x.SurveyResponseSetId).GreaterThan(0);
	}
}

public class DeleteSurveyResponseSetCommandHandler(
	IApplicationDbContext _context,
	ITenantResolver _tenantResolver) : IRequestHandler<DeleteSurveyResponseSetCommand>
{
	public async Task Handle(DeleteSurveyResponseSetCommand request, CancellationToken cancellationToken)
	{
		var vendorId = _tenantResolver.ResolveTenantId();

		var surveyResponseSet = await _context.SurveyResponseSets
			.FirstOrDefaultAsync(srs => srs.SurveyResponseSetId == request.SurveyResponseSetId && srs.VendorId == vendorId, cancellationToken);

		if (surveyResponseSet == null)
			throw new SurveyResponseSetNotFoundException(request.SurveyResponseSetId);

        surveyResponseSet.StatusFlag.Status = RecordStatus.Deleted;
        _context.SurveyResponseSets.Update(surveyResponseSet);
		await _context.SaveChangesAsync(cancellationToken);
	}
}
