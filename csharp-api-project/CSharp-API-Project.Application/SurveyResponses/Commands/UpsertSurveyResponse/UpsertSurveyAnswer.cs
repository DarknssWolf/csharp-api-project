using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponses.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using Z.EntityFramework.Plus;

namespace CSharpAPIProject.Application.SurveyResponses.Commands.UpsertSurveyResponse
{
	[Authorize(Policy = Policies.CanCreate_SurveyResponse)]
	public record UpsertSurveyResponseCommand : IRequest
	{
		public int SurveyId { get; set; }
		public int CustomerId { get; set; }
		public int? SurveyResponseSetId { get; set; }
		public List<SurveyQuestionResponse> Responses { get; set; } = new List<SurveyQuestionResponse>();
	}

	public record SurveyQuestionResponse : IRequest
	{
		public int SurveyQuestionId { get; set; }
		public string Response { get; set; }
	}

	public class UpsertSurveyResponseCommandValidator : AbstractValidator<UpsertSurveyResponseCommand>
	{
		public UpsertSurveyResponseCommandValidator()
		{
			RuleFor(v => v.SurveyId).GreaterThan(0);
			RuleFor(v => v.CustomerId).GreaterThan(0);
		}
	}	
	public class SurveyResponseCommandValidator : AbstractValidator<SurveyResponse>
	{
		public SurveyResponseCommandValidator()
		{
			RuleFor(v => v.SurveyQuestionId).GreaterThan(0);
			RuleFor(v => v.Response).NotEmpty();
		}
	}

	public class UpsertSurveyResponseCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver) : IRequestHandler<UpsertSurveyResponseCommand>
	{
		public async Task Handle(UpsertSurveyResponseCommand request, CancellationToken cancellationToken)
		{
			var vendorId = tenantResolver.ResolveTenantId();
			var survey = await context.Surveys
				.Include(s => s.SurveyQuestions)
				.Where(x => x.SurveyId == request.SurveyId)
				.FirstOrDefaultAsync(cancellationToken);
			//Loop through each response
			foreach (var response in request.Responses)
			{
				//Get survey question
				var surveyQuestion = context.SurveyQuestions
					.Where(x => x.SurveyId == request.SurveyId
					&& x.SurveyQuestionId == response.SurveyQuestionId).FirstOrDefault();

				// Validate based on question type
				switch (surveyQuestion.Type)
				{
					case Domain.Enums.QuestionType.OpenText:
						// Any non-empty text is acceptable; optional additional length checks could go here
						if (string.IsNullOrWhiteSpace(response.Response))
							throw new ResponseNotValidException(response.SurveyQuestionId);
						break;

					case Domain.Enums.QuestionType.MultipleChoice:
						{
							var options = (surveyQuestion.Options ?? string.Empty)
								.Split(',', StringSplitOptions.RemoveEmptyEntries)
								.Select(o => o.Trim())
								.ToHashSet(StringComparer.OrdinalIgnoreCase);
							var selected = (response.Response ?? string.Empty).Trim();
							if (string.IsNullOrWhiteSpace(selected) || !options.Contains(selected))
								throw new ResponseNotValidException(response.SurveyQuestionId);
							break;
						}

					case Domain.Enums.QuestionType.MultiSelect:
						{
							var options = (surveyQuestion.Options ?? string.Empty)
								.Split(',', StringSplitOptions.RemoveEmptyEntries)
								.Select(o => o.Trim())
								.ToHashSet(StringComparer.OrdinalIgnoreCase);
							var answers = (response.Response ?? string.Empty)
								.Split(',', StringSplitOptions.RemoveEmptyEntries)
								.Select(a => a.Trim())
								.ToArray();
							if (answers.Length == 0 || answers.Any(a => !options.Contains(a)))
								throw new ResponseNotValidException(response.SurveyQuestionId);
							break;
						}
				}

				var surveyResponse = context.SurveyResponses.FirstOrDefault(
					s => s.SurveyQuestionId == response.SurveyQuestionId
					&& s.CustomerId == request.CustomerId
					&& s.VendorId == vendorId
                    && s.SurveyResponseSetId == request.SurveyResponseSetId
				);

				if (surveyResponse == null)
				{
					surveyResponse = new SurveyResponse()
					{
						VendorId = vendorId,
						SurveyId = request.SurveyId,
						SurveyQuestionId = response.SurveyQuestionId,
						QuestionId = surveyQuestion.QuestionId,
						CustomerId = request.CustomerId,
						SurveyResponseSetId = request.SurveyResponseSetId,
						Response = response.Response,
					};

					context.SurveyResponses.Add(surveyResponse);
				}
				else
				{
					//check if the user is allowed to answer again
					if (!survey.AllowMultipleTries) 
						throw new NoRetriesAllowedException();
					surveyResponse.Response = response.Response;
					surveyResponse.SurveyResponseSetId = request.SurveyResponseSetId;
				}
			}

			await context.SaveChangesAsync(cancellationToken);

			// Update SurveyResponseSet progress similarly to QuizResponseSet
			if (request.SurveyResponseSetId != null)
			{
				var surveyResponseSet = await context.SurveyResponseSets
					.Where(x => x.SurveyId == request.SurveyId 
						&& x.CustomerId == request.CustomerId 
						&& x.SurveyResponseSetId == request.SurveyResponseSetId)
					.FirstOrDefaultAsync(cancellationToken);

				if (surveyResponseSet != null)
				{
					var responses = await (from sr in context.SurveyResponses
													join sq in context.SurveyQuestions on sr.SurveyQuestionId equals sq.SurveyQuestionId
													where sq.SurveyId == request.SurveyId
														&& sr.CustomerId == request.CustomerId
														&& sr.VendorId == vendorId
														&& sr.SurveyResponseSetId == request.SurveyResponseSetId
													select sr).ToListAsync(cancellationToken);

					// total ordered questions for this survey
					var totalQuestions = survey?.SurveyQuestions?.Count 
						?? await context.SurveyQuestions.Where(q => q.SurveyId == request.SurveyId).CountAsync(cancellationToken);

					surveyResponseSet.TotalQuestions = totalQuestions;
					surveyResponseSet.CompletedQuestions = responses.Count;
					// advance index while not at the last question
					if (surveyResponseSet.LastQuestionIndex < Math.Max(0, totalQuestions - 1))
					{
						surveyResponseSet.LastQuestionIndex++;
					}
					// if we've reached the last question index, mark the set as completed
					if (surveyResponseSet.LastQuestionIndex >= Math.Max(0, totalQuestions - 1))
					{
						surveyResponseSet.ResponseSetStatus = Domain.Enums.ResponseSetStatus.Completed;
						surveyResponseSet.EndDate = DateTime.UtcNow;
					}

					await context.SaveChangesAsync(cancellationToken);
				}
			}
		}
	}
}
