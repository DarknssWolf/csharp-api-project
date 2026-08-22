using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
namespace CSharpAPIProject.Application.Common.Interfaces.HelperServices;

public interface ISurveyHelperService
{
	List<SurveyQuestionDto> ReAssignQuestionIds(int? newParentId, List<SurveyQuestionDto> questions);

	SurveyQuestion MapSurveyQuestion(SurveyQuestionDto x, int surveyId);
	
	Task<List<SurveyQuestion>> SaveGalleryImagesForQuestions(ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		string surveyCode,
		int vendorId,
		string vendorName,
		List<SurveyQuestion>? existingSurveyQuestions,
		List<SurveyQuestion> allQuestions,
		CancellationToken cancellationToken);
		
	void CollectChildQuestions(SurveyQuestion parentQuestion, IEnumerable<SurveyQuestionDto>? childQuestions, List<SurveyQuestion> allQuestions);
	
	void IsValidQuestion(SurveyQuestionDto question, List<SurveyQuestion> allQuestions);
	
	SurveyQuestionDataDto? GetQuestionData(SurveyQuestion question);
	
	Task<int> GetOrCreateSurveyGallery(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		ITenantResolver tenantResolver,
		string name,
		int vendorId,
		string vendorName,
		string surveyCode,
		CancellationToken ct);
		
	Task<Guid> SaveGalleryImages(
		IConfiguration configuration,
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		int galleryId,
		int? surveyQuestionId, //can be null if its a new question
		string vendorName,
		Guid? uploadId,
		Guid? galleryImageId,
		string galleryCode,
		string fileName,
		string fileType,
		string fileDescription,
		CancellationToken ct);
		

		
	Task<string?> SaveGalleryImagesForSurvey(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		int vendorId,
		string vendorName,
		string surveyCode,
		int? surveyId,
		string? surveyData,
		CancellationToken cancellationToken);
		
	SurveyDataDto? GetSurveyData(string surveyData);
	
	Guid GetGalleryImageIdFromString(string? data);
	

	
	Task<SurveyDto> GetGalleryImagesForSurvey(IConfiguration configuration, IApplicationDbContext context, SurveyDto survey, CancellationToken ct);
	
	Task RemoveGalleryImages(
		IApplicationDbContext context,
		ICloudFileStorage cloudFileStorage,
		string vendorName,
		List<SurveyQuestion> listOfQuestions);
}
