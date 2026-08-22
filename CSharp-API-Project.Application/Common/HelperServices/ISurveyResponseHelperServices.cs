using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Common.Interfaces.HelperServices;
public interface ISurveyResponseHelperService
{
	IEnumerable<SurveyQuestionDto> GetChildQuestions(int questionId, int? parentQuestionId,ICollection<SurveyQuestion> allQuestions, HashSet<string> visited = null);
	SurveyQuestionDto MapQuestionToDto(SurveyQuestion entity, ICollection<SurveyQuestion> allQuestions, HashSet<string> visited);
}
