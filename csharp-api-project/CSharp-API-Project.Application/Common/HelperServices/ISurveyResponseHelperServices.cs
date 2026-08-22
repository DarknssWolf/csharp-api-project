using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace CSharpAPIProject.Application.Common.Interfaces.HelperServices;
public interface ISurveyResponseHelperService
{
	Task<List<SurveyQuestionDto>> getGalleryImages(IConfiguration configuration, IApplicationDbContext context, List<SurveyQuestionDto> questions, CancellationToken ct);
	IEnumerable<SurveyQuestionDto> GetChildQuestions(int questionId, int? parentQuestionId,ICollection<SurveyQuestion> allQuestions, HashSet<string> visited = null);
	SurveyQuestionDto MapQuestionToDto(SurveyQuestion entity, ICollection<SurveyQuestion> allQuestions, HashSet<string> visited);
}