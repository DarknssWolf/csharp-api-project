using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponseSets.Exceptions;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.SurveyResponseSets.Commands.UpsertSurveyResponseSet;

[Authorize(Policy = Policies.CanCreate_SurveyResponseSet)]
[Authorize(Policy = Policies.CanUpdate_SurveyResponseSet)]
public record UpsertSurveyResponseSetCommand(
	int SurveyResponseSetId,
	int SurveyId,
	int CustomerId,
	DateTime StartDate,
	DateTime? EndDate,
	int CompletedQuestions,
	int TotalQuestions,
	int LastQuestionIndex,
	ResponseSetStatus ResponseSetStatus
) : IRequest;

public class UpsertSurveyResponseSetCommandValidator : AbstractValidator<UpsertSurveyResponseSetCommand>
{
	public UpsertSurveyResponseSetCommandValidator()
	{
		RuleFor(x => x.SurveyId).GreaterThan(0);
		RuleFor(x => x.CustomerId).GreaterThan(0);
		RuleFor(x => x.StartDate).NotEmpty();
		RuleFor(x => x.CompletedQuestions).GreaterThanOrEqualTo(0);
		RuleFor(x => x.TotalQuestions).GreaterThan(0);
		RuleFor(x => x.LastQuestionIndex).GreaterThanOrEqualTo(0);
	}
}

public class UpsertSurveyResponseSetCommandHandler(
	IApplicationDbContext _context,
    IAuthHelper authHelper,
    ITenantResolver _tenantResolver,
    IUser user,
    IMapper _mapper,
    ISettingsService settingsService,
	IPendingPointsService pendingPointsService
) : IRequestHandler<UpsertSurveyResponseSetCommand>
{
	public async Task Handle(UpsertSurveyResponseSetCommand request, CancellationToken cancellationToken)
	{
        bool hasTicRole = await authHelper.LoggedIn_IsTICUser();
        // use tenant resolver
        var vendorId = _tenantResolver.ResolveTenantId();
        VendorSettings? vendorSettings;

        vendorSettings = settingsService.LoadSetting<VendorSettings>();

        var vendor = await _context.Vendors
			.FirstOrDefaultAsync(v => v.VendorId == vendorId, cancellationToken);
		if (vendor == null)
			throw new CSharpAPIProject.Application.Vendors.Exceptions.VendorNotFoundException(vendorId);

		// Validate Survey exists
		var survey = await _context.Surveys
            .Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
            .FirstOrDefaultAsync(s => s.SurveyId == request.SurveyId && s.VendorId == vendorId, cancellationToken);
		if (survey == null)
			throw new SurveyNotFoundException(request.SurveyId);

		// Validate Customer exists
		var customer = await _context.Customers
			.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && c.VendorId == vendorId, cancellationToken);
		if (customer == null)
			throw new CustomerNotFoundException(request.CustomerId);

		var surveyResponseSet = await _context.SurveyResponseSets
			.FirstOrDefaultAsync(srs =>
				srs.VendorId == vendorId &&
				((request.SurveyResponseSetId > 0 && srs.SurveyResponseSetId == request.SurveyResponseSetId)),
			cancellationToken);

		if (surveyResponseSet == null)
		{
			// Get active survey questions for this survey
			var activeQuestions = await _context.SurveyQuestions
				.Where(sq => sq.SurveyId == request.SurveyId && sq.StatusFlag.Status == RecordStatus.Active)
				.OrderBy(sq => sq.OrderNumber)
				.ToListAsync(cancellationToken);

			var totalQuestions = activeQuestions.Count;

			// Create new SurveyResponseSet if not found
			surveyResponseSet = new SurveyResponseSet
			{
				SurveyId = request.SurveyId,
				CustomerId = request.CustomerId,
				VendorId = vendorId,
				StartDate = request.StartDate,
				EndDate = request.EndDate,
				CompletedQuestions = 0, // Start with 0 completed questions
				TotalQuestions = totalQuestions,
				LastQuestionIndex = 0, // Start at the first question
				ResponseSetStatus = ResponseSetStatus.InProgress // Set to InProgress for new survey attempt
			};

			_context.SurveyResponseSets.Add(surveyResponseSet);
		}
		else
		{
            // Update existing SurveyResponseSet's details
            surveyResponseSet.SurveyId = request.SurveyId;
			surveyResponseSet.CustomerId = request.CustomerId;
			surveyResponseSet.StartDate = request.StartDate;
			surveyResponseSet.EndDate = request.ResponseSetStatus == ResponseSetStatus.Completed ? request.EndDate : null;
			surveyResponseSet.ResponseSetStatus = request.ResponseSetStatus;

			_context.SurveyResponseSets.Update(surveyResponseSet);

			// If completed and survey allows points, create pending points entry
			bool isCompleted = surveyResponseSet.ResponseSetStatus == ResponseSetStatus.Completed;
			if (isCompleted && survey.CanEarnPoints)
			{
				var userId = user.Id;
				var batchNumber = ++vendorSettings.LastSettingsBatch;
				var source = CSharpAPIProject.Domain.Enums.LedgerEnums.PendingPointsSource.POINTS_ENTRY;
				var description = string.Format("Survey - {0}, {1}", survey.Code, survey.Title);
				var sourceReference = "Survey Completed";
				var publicReference = string.Format("[{0}] {1} {2}", DateTime.UtcNow, survey.Code, survey.Title);
				var success = await pendingPointsService.CreatePendingPointsEntryAsync(
					_context,
					userId,
					batchNumber,
					survey.Points,
					source,
					sourceReference,
					publicReference,
					description,
					vendorId,
					cancellationToken
				);

				if (!success)
					throw new Exception("Failed to create pending points entry for survey completed.");

				settingsService.SaveSetting(vendorSettings);
			}
		}

		await _context.SaveChangesAsync(cancellationToken);
	}
}
