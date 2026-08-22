
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Surveys.Helpers;

	public static class SurveyHelper
	{
	public static void ValidateQuestion(SurveyQuestion question)
	{
		//because we reuse the question Types, prevent open text questions from being created in Surveys
		//if (question.Type == QuestionType.OpenText)
		//	throw new OpenTextNotSupportedException(question.QuestionId);

		//if (question.ExpectedAnswers == null || question.ExpectedAnswers == "")
		//	throw new ExpectedAnswersCannotBeEmptyException(question.QuestionId);

		if (question.Type != QuestionType.OpenText && (question.Options == null || question.Options == ""))
			throw new OptionsCannotBeEmptyException(question.QuestionId);

		//string[] options = question.Options.Split(",");
        string[] options = question.Options.Split(",").Select(x => x.Trim()).ToArray();
        string[] expectedAnswers = question.ExpectedAnswers.Split(",").Select(x => x.Trim()).ToArray();

        //foreach (var expectedAnswer in expectedAnswers)
        //    if (!options.Contains(expectedAnswer))
        //        throw new ExpectedAnswerNotInOptionsExceptions(expectedAnswer, question.QuestionId);

        // Additional validation for parent/child questions
        if (question.ParentQuestionId == null && !string.IsNullOrEmpty(question.ParentCondition))
		{
			throw new QuestionWithConditionButNoParentException(question.SurveyId, question.QuestionId, question.ParentQuestionId);
		}
	}
}
