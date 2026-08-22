using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.Surveys.Commands.DeleteSurvey;

[Authorize(Policy = Policies.CanDelete_Survey)]
public record DeleteSurveyCommand : IRequest
{
	public int SurveyId { get; set; }
}

public class DeleteSurveyCommandValidator : AbstractValidator<DeleteSurveyCommand>
{
	public DeleteSurveyCommandValidator()
	{
		RuleFor(v => v.SurveyId).GreaterThan(0);
	}
}

public class DeleteSurveyCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver, IUser user) : IRequestHandler<DeleteSurveyCommand>
{
	public async Task Handle(DeleteSurveyCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = await context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
		if (vendor == null)
			throw new VendorNotFoundException();

		var survey = await context.Surveys
			.Include(x => x.SurveyQuestions)
			.FirstOrDefaultAsync(s => s.SurveyId == request.SurveyId && s.VendorId == vendorId, cancellationToken);

		if (survey == null)
		{
			throw new SurveyNotFoundException(request.SurveyId);
        }

        survey.RecordStatus = RecordStatus.Deleted;
        survey.RecordDeletedDate = DateTimeOffset.Now;

        // Soft Deletes will still require the data to remain so we will not remove the questions and responses.
        // Files attached to the survey and its questions are soft deleted with it.
        var storedFileIds = survey.SurveyQuestions
            .Select(q => FileDataHelper.GetStoredFileId(q.SurveyQuestionData))
            .Append(FileDataHelper.GetStoredFileId(survey.SurveyData))
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

        context.Surveys.Update(survey);

		await context.SaveChangesAsync(cancellationToken);
	}


}
