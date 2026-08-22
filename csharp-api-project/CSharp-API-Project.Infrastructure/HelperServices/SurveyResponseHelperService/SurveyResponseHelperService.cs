using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CSharpAPIProject.Infrastructure.HelperServices.SurveyResponseHelperService;

public class SurveyResponseHelperService : ISurveyResponseHelperService
{
	public async Task<List<SurveyQuestionDto>> getGalleryImages(IConfiguration configuration, IApplicationDbContext context, List<SurveyQuestionDto> questions, CancellationToken ct)
	{
		string cloudStorageBaseUrl = configuration.GetSection("CloudStorage")["BaseUrl"] ?? "";

		foreach (var question in questions)
		{
			if (question.SurveyQuestionData != null && question.SurveyQuestionData != string.Empty)
			{
				var dataObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(question.SurveyQuestionData);
				if (dataObj != null && dataObj.TryGetValue("galleryImageId", out JsonElement jsonElement))
				{
					Guid imgGuid = Guid.Empty;
					string x = jsonElement.GetString() ?? "";
					if (Guid.TryParse(x, out Guid guid))
						imgGuid = guid;

					if (imgGuid != Guid.Empty)
					{
						var imgData = await context.GalleryImages
							.Where(gi => gi.GalleryImageId == imgGuid).FirstOrDefaultAsync(ct);

						Dictionary<string, string> newDataObj = new Dictionary<string, string>
						{
							{ "galleryImageUri", cloudStorageBaseUrl + imgData?.ImagePath }
						};

						question.SurveyQuestionData = JsonSerializer.Serialize(newDataObj);
					}
				}
			}

			if (question.ChildQuestions?.Count() > 0)
			{
				question.ChildQuestions = await getGalleryImages(configuration, context, question.ChildQuestions.ToList(), ct);
			}
		}

		return questions;
	}
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