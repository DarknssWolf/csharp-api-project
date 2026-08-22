using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Surveys.Helpers;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using System.Text.Json;
using Z.EntityFramework.Plus;

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

public class DeleteSurveyCommandHandler(IApplicationDbContext context, ICloudFileStorage cloudFileStorage, ITenantResolver tenantResolver) : IRequestHandler<DeleteSurveyCommand>
{
	public async Task Handle(DeleteSurveyCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = context.Vendors.Where(v => v.VendorId == vendorId).FutureValue();
		if (vendor.Value == null)
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
        //var surveyQuestionsToRemove = survey.SurveyQuestions;
        //await SurveyHelper.RemoveGalleryImages(context, cloudFileStorage, vendor.Value.Name, surveyQuestionsToRemove.ToList());
        //context.SurveyQuestions.RemoveRange(surveyQuestionsToRemove);

        context.Surveys.Update(survey);

		await context.SaveChangesAsync(cancellationToken);
	}


}
