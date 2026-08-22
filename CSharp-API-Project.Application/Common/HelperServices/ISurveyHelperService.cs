using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;
namespace CSharpAPIProject.Application.Common.Interfaces.HelperServices;

public interface ISurveyHelperService
{
	List<SurveyQuestionDto> ReAssignQuestionIds(int? newParentId, List<SurveyQuestionDto> questions);

	SurveyQuestion MapSurveyQuestion(SurveyQuestionDto x, int surveyId);

	void CollectChildQuestions(SurveyQuestion parentQuestion, IEnumerable<SurveyQuestionDto>? childQuestions, List<SurveyQuestion> allQuestions);

	void IsValidQuestion(SurveyQuestionDto question, List<SurveyQuestion> allQuestions);
}
