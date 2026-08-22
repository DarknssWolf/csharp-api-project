using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Surveys.Helpers;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.Surveys.Commands.UpsertSurvey;

[Authorize(Policy = Policies.CanCreate_Survey)]
[Authorize(Policy = Policies.CanUpdate_Survey)]
	public record UpsertSurveyCommand(
	int SurveyId,
	string Title,
	string Code,
	DateTime StartOn,
	DateTime EndOn,
	bool CanEarnPoints,
	bool AllowMultipleTries,
	int MultipleTriesLimit,
	int Points,
	string? SurveyData,
	Guid? VisibilityConfigurationId,
	IEnumerable<SurveyQuestionDto> Questions
) : IRequest;

public class UpsertSurveyCommandValidator : AbstractValidator<UpsertSurveyCommand>
{
	public UpsertSurveyCommandValidator()
	{
		RuleFor(x => x.Title).NotEmpty();
		RuleFor(x => x.Code).NotEmpty();
		RuleFor(x => x.StartOn).NotEmpty();
		RuleFor(x => x.EndOn).NotEmpty();
		RuleFor(x => x.Questions).NotEmpty();
	}
}

public class UpsertSurveyCommandHandler(
	IApplicationDbContext _context,
	ITenantResolver _tenantResolver,
    IMapper _mapper,
    IUser _user
) : IRequestHandler<UpsertSurveyCommand>
{
    public async Task Handle(UpsertSurveyCommand request, CancellationToken cancellationToken)
    {
        // use tenant resolver
        var vendorId = _tenantResolver.ResolveTenantId();
        var vendor = await _context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
        if (vendor == null)
            throw new VendorNotFoundException();

        var survey = await _context.Surveys
            .Include(x => x.SurveyQuestions)
            .FirstOrDefaultAsync(x =>
                x.VendorId == vendorId &&
                ((request.SurveyId > 0 && x.SurveyId == request.SurveyId)),
            cancellationToken);

        var allQuestions = new List<SurveyQuestion>();

        foreach (var questionDto in request.Questions)
        {
            var SurveyQuestion = _mapper.Map<SurveyQuestion>(questionDto);
            SurveyHelper.ValidateQuestion(SurveyQuestion);
            allQuestions.Add(SurveyQuestion);
        }

        if (survey == null)
        {
            // Create new survey if not found
            survey = new Survey
            {
                Title = request.Title,
                Code = request.Code,
                StartOn = request.StartOn,
                EndOn = request.EndOn,
                CanEarnPoints = request.CanEarnPoints,
                AllowMultipleTries = request.AllowMultipleTries,
                MultipleTriesLimit = request.MultipleTriesLimit,
                Points = request.Points,
                VisibilityConfigurationId = request.VisibilityConfigurationId,
                VendorId = vendorId,
                SurveyData = request.SurveyData
            };

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync(cancellationToken);

            int cnt = 0;
            allQuestions.ForEach(q =>
            {
                cnt++;
                q.OrderNumber = cnt;
            });
            // Add new questions for the new survey
            survey.SurveyQuestions = allQuestions;
        }
        else
        {
            // Update existing survey's details
            survey.Title = request.Title;
            survey.StartOn = request.StartOn;
            survey.EndOn = request.EndOn;
            survey.CanEarnPoints = request.CanEarnPoints;
            survey.AllowMultipleTries = request.AllowMultipleTries;
            survey.MultipleTriesLimit = request.MultipleTriesLimit;
            survey.Points = request.Points;
            survey.VisibilityConfigurationId = request.VisibilityConfigurationId;

            //Only update if there is new data else itll overwrite with null
            if (request.SurveyData != null && request.SurveyData.Length > 0)
            {
                survey.SurveyData = request.SurveyData;
            }

            // Get existing questions from the database
            var existingQuestions = _context.SurveyQuestions
                .AsNoTracking()
                .Where(q => q.SurveyId == request.SurveyId)
                .ToList();

            // Update or add questions
            int cnt = 0;
            foreach (var question in allQuestions)
            {
                cnt++;
                // ids are different every update so now match on prompt and type
                // because this should never be duplicated in a single survey
                var existingQuestion = existingQuestions
                    .FirstOrDefault(q => q.Prompt == question.Prompt && q.Type == question.Type);

                if (existingQuestion == null)
                {
                    question.SurveyQuestionId = 0;
                    _context.SurveyQuestions.Add(question);
                    continue;
                }

                // Update existing question - get the tracked entity first
                var trackedQuestion = _context.SurveyQuestions
                    .FirstOrDefault(q => q.SurveyQuestionId == existingQuestion.SurveyQuestionId);

                if (trackedQuestion != null)
                {
                    // Update existing question
                    trackedQuestion.Prompt = question.Prompt;
                    trackedQuestion.Type = question.Type;
                    trackedQuestion.ExpectedAnswers = question.ExpectedAnswers;
                    trackedQuestion.Options = question.Options;
                    trackedQuestion.AllowMultipleResponses = question.AllowMultipleResponses;
                    trackedQuestion.ParentCondition = question.ParentCondition;
                    trackedQuestion.OrderNumber = question.OrderNumber;
                    trackedQuestion.IsRequired = question.IsRequired;
                    trackedQuestion.ParentQuestionId = question.ParentQuestionId;
                    // ensure persisted order follows QuestionId ordering
                    trackedQuestion.OrderNumber = cnt;

                    // question data is null if nothing was changed, so only update if data is populated 
                    if (question.SurveyQuestionData != null && question.SurveyQuestionData.Length > 0)
                        trackedQuestion.SurveyQuestionData = question.SurveyQuestionData;
                }
                else
                {
                    // If we can't find the tracked entity, attach and update
                    question.SurveyQuestionId = existingQuestion.SurveyQuestionId;
                    _context.SurveyQuestions.Update(question);
                }
            }


            // Remove deleted questions
            var questionsToDelete = existingQuestions
                .Where(eq => !allQuestions.Any(nq => nq.Prompt == eq.Prompt && nq.Type == eq.Type))
                .ToList();

            if (questionsToDelete.Any())
            {
                // Soft delete any files attached to the removed questions
                var storedFileIdsToDelete = questionsToDelete
                    .Select(q => FileDataHelper.GetStoredFileId(q.SurveyQuestionData))
                    .Where(id => id != null)
                    .Select(id => id.Value)
                    .ToList();

                if (storedFileIdsToDelete.Any())
                {
                    var storedFilesToDelete = await _context.StoredFiles
                        .Where(f => f.VendorId == vendorId && storedFileIdsToDelete.Contains(f.StoredFileId))
                        .ToListAsync(cancellationToken);

                    foreach (var storedFile in storedFilesToDelete)
                        storedFile.StatusFlag = new EntityStatus(RecordStatus.Deleted, _user.Id, DateTimeOffset.UtcNow);
                }

                // Get the IDs of questions to delete
                var questionIdsToDelete = questionsToDelete.Select(q => q.SurveyQuestionId).ToList();

                // Find and remove the tracked entities
                var trackedQuestionsToDelete = _context.SurveyQuestions
                    .Where(q => questionIdsToDelete.Contains(q.SurveyQuestionId))
                    .ToList();

                _context.SurveyQuestions.RemoveRange(trackedQuestionsToDelete);
                
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
