using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Infrastructure.HelperServices.SurveyHelperService;

public class SurveyHelperService : ISurveyHelperService
{
	private  int _questionCounter = 1; // Use a class-level counter
	public  List<SurveyQuestionDto> ReAssignQuestionIds(int? newParentId, List<SurveyQuestionDto> questions)
	{
		foreach (var question in questions)
		{
			question.QuestionId = _questionCounter++;
			question.ParentQuestionId = newParentId;

			if (question.ChildQuestions != null && question.ChildQuestions.Count() > 0)
			{
				question.ChildQuestions = ReAssignQuestionIds(question.QuestionId, question.ChildQuestions.ToList());
			}
		}
		return questions;
	}

	public  SurveyQuestion MapSurveyQuestion(SurveyQuestionDto x, int surveyId)
	{
		return new SurveyQuestion
		{
			SurveyId = surveyId,
			QuestionId = x.QuestionId,
			ParentQuestionId = x.ParentQuestionId,
			Prompt = x.Prompt,
			Type = x.Type,
			ExpectedAnswers = x.ExpectedAnswers,
			Options = x.Options,
			AllowMultipleResponses = x.AllowMultipleResponses,
			ParentCondition = x.ParentCondition,
			OrderNumber = x.OrderNumber,
			IsRequired = x.IsRequired,
			SurveyQuestionData = x.SurveyQuestionData
		};
	}

	public  void CollectChildQuestions(SurveyQuestion parentQuestion, IEnumerable<SurveyQuestionDto>? childQuestions, List<SurveyQuestion> allQuestions)
	{
		if (childQuestions == null || childQuestions.Count() == 0)
			return;

		foreach (var childQuestionDto in childQuestions)
		{
			//check if question is valid
			IsValidQuestion(childQuestionDto, allQuestions);

			//check if parent question ID is correct.
			if (childQuestionDto.ParentQuestionId != parentQuestion.QuestionId)
				throw new QuestionWithParentIdNotFoundException(childQuestionDto.SurveyId, childQuestionDto.QuestionId, parentQuestion.ParentQuestionId);

			var childQuestion = MapSurveyQuestion(childQuestionDto, parentQuestion.SurveyId);
			childQuestion.ParentQuestionId = parentQuestion.QuestionId;
			allQuestions.Add(childQuestion);

			// Recursively collect child questions
			CollectChildQuestions(childQuestion, childQuestionDto?.ChildQuestions, allQuestions);
		}
	}

	public  void IsValidQuestion(SurveyQuestionDto question, List<SurveyQuestion> allQuestions)
	{
		if (question.ParentQuestionId == null && question.ParentCondition != null && question.ParentCondition != "")
		{
			throw new QuestionWithConditionButNoParentException(question.SurveyId, question.QuestionId, question.ParentQuestionId);
		}
		else if (question.ParentQuestionId != null && question.ParentCondition != null)
		{
			var parentQuestion = allQuestions.FirstOrDefault(x => x.QuestionId == question.ParentQuestionId);

			if (parentQuestion?.Options == null)
				throw new QuestionWithConditionButNoOptionsFoundException(question.SurveyId, question.QuestionId, question.ParentQuestionId);

			string[] allowedValues = parentQuestion.Options.Split(",");

			if (allowedValues.Length > 0)
			{
				var conditionalValues = question.ParentCondition.Split(",");
				foreach (var conditionalValue in conditionalValues)
				{
					if (conditionalValue != String.Empty)
						if (!allowedValues.Contains(conditionalValue))
							throw new QuestionConditionDoesNotMatchParentOptionException(question.SurveyId, question.QuestionId, question.ParentQuestionId);
				}
			}
			else
			{
				throw new QuestionWithConditionButNoOptionsFoundException(question.SurveyId, question.QuestionId, question.ParentQuestionId);
			}
		}
	}
}
