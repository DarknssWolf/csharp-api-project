using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Infrastructure.HelperServices.SurveyResponseHelperService;

public class SurveyResponseHelperService : ISurveyResponseHelperService
{
	public IEnumerable<SurveyQuestionDto> GetChildQuestions(int questionId, int? parentQuestionId,
	ICollection<SurveyQuestion> allQuestions, HashSet<string> visited = null)
	{
		if (visited == null)
			visited = new HashSet<string>();

		// Unique key to track visited questions, handling null safely
		string uniqueKey = $"{questionId}-{(parentQuestionId.HasValue ? parentQuestionId.Value.ToString() : "null")}";

		if (visited.Contains(uniqueKey))
			return Enumerable.Empty<SurveyQuestionDto>(); // Prevent infinite recursion

		visited.Add(uniqueKey);

		// Find children where ParentQuestionId matches QuestionId
		var childQuestions = allQuestions
			.Where(q => q.ParentQuestionId == questionId) // Correct filtering
			.OrderBy(q => q.OrderNumber)
			.ToList();

		return childQuestions.Select(q => MapQuestionToDto(q, allQuestions, visited)).ToList();
	}

	public SurveyQuestionDto MapQuestionToDto(SurveyQuestion entity,
		ICollection<SurveyQuestion> allQuestions, HashSet<string> visited)
	{
		return new SurveyQuestionDto
		{
			SurveyQuestionId = entity.SurveyQuestionId,
			QuestionId = entity.QuestionId,
			ParentQuestionId = entity.ParentQuestionId,
			SurveyId = entity.SurveyId,
			Prompt = entity.Prompt,
			Type = entity.Type,
			//DefaultResponse = entity.DefaultResponse,
			Options = entity.Options,
			AllowMultipleResponses = entity.AllowMultipleResponses,
			ParentCondition = entity.ParentCondition,
			OrderNumber = entity.OrderNumber,
			IsRequired = entity.IsRequired,
			SurveyQuestionData = entity.SurveyQuestionData,
			ChildQuestions = GetChildQuestions(entity.QuestionId, entity.ParentQuestionId, allQuestions, visited)
		};
	}
}
