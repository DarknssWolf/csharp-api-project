
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.GalleryImages.Helpers.GalleryImageSaveFromTemp;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
	public static async Task<List<SurveyQuestion>> SaveGalleryImagesForQuestions(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		string SurveyCode,
		int vendorId,
		string vendorName,
		List<SurveyQuestion>? existingSurveyQuestions,
		List<SurveyQuestion> allQuestions,
		CancellationToken cancellationToken)
	{
		int galleryId = -1; //placeholder for if we need to get a gallery for the questions
		foreach (var question in allQuestions)
		{
			//check if the question has an image upload ID
			if (question.SurveyQuestionData?.Length > 0)
			{
				var existingQuestion = existingSurveyQuestions?
					.FirstOrDefault(q => q.Prompt == question.Prompt && q.Type == question.Type);

				//if we havent already got a gallery id, get or create one
				if (galleryId < 0)
					galleryId = await GetOrCreateSurveyGallery(cloudFileStorage, context, tenantResolver, $"sur-{SurveyCode}", vendorId, vendorName, SurveyCode, cancellationToken);

				SurveyQuestionDataDto? SurveyQuestionDataDto = GetQuestionData(question);
				Guid newGalleryImageId = Guid.Empty;
				if (SurveyQuestionDataDto != null)
				{
					newGalleryImageId = await SaveGalleryImages(configuration, cloudFileStorage, context, galleryId, existingQuestion?.SurveyQuestionId, vendorName, SurveyQuestionDataDto.UploadId, null, $"sur-{SurveyCode}", SurveyQuestionDataDto?.FileName ?? "", SurveyQuestionDataDto.FileType ?? "", SurveyQuestionDataDto.FileDescription ?? "", cancellationToken);
				}

				//update the question with the new gallery image id
				if (newGalleryImageId != Guid.Empty)
					question.SurveyQuestionData = JsonSerializer.Serialize(new { galleryImageId = newGalleryImageId });
				else
					question.SurveyQuestionData = null;//empty updated data because there was nothing to upload
			}
		}

		return allQuestions;
	}

	public static async Task<Guid> SaveGalleryImages(
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
			CancellationToken ct)
	{
		var defaultUploadContainer = configuration["CloudStorage:DefaultStorageContainer"] ?? "WebsiteMediaStorage";

		//Gallery exists check is done in CreatGalleryFunction which must happen before this
		string ContainerFriendlyVendorName = cloudFileStorage.GetNormalizedContainerName(vendorName);

		// Set File Paths
		string TmpUploadPath = $"tmp/{uploadId}";
		// Extract file extension from MIME type (e.g., "image/jpeg" -> "jpeg") to align with QuizHelper
		string fileExtension = fileType.Contains("/") ? fileType.Split('/').Last() : fileType;
		string DestUploadPath = $"{ContainerFriendlyVendorName}/{galleryCode}/{uploadId}.{fileExtension}";
		bool UploadNewFile = true;

		//If there is nothing to upload, return
		if (uploadId == Guid.Empty)
		{
			return Guid.Empty;
		}

		string ContainerName = cloudFileStorage.GetNormalizedContainerName(defaultUploadContainer);

		//check if the question has an existing image but was not provided
		if (surveyQuestionId != null && galleryImageId == null || galleryImageId == Guid.Empty)
		{
			var surveyQuestion = await context.SurveyQuestions
					.Where(gi => gi.SurveyQuestionId == surveyQuestionId).FirstOrDefaultAsync(ct);

			string? existingGalleryImageData = surveyQuestion?.SurveyQuestionData;

			if (existingGalleryImageData != null && existingGalleryImageData.Length > 0)
			{
				var jsonObject = JsonSerializer.Deserialize<JsonElement>(existingGalleryImageData);
				// Access a key in the object
				if (jsonObject.TryGetProperty("galleryImageId", out var dataImageId))
				{
					galleryImageId = Guid.Parse(dataImageId.GetString()); //assign the existing image id
				}
			}
		}

		Guid uploadedImageId = Guid.Empty;

		string? existingGalleryImagePath = null;
		// If the Guid isnt null then we are updating an image, else we are uploading.
		if (galleryImageId != null && galleryImageId != Guid.Empty)
		{
			var galleryImage = await context.GalleryImages
				.Include(gi => gi.Gallery)
				.Where(gi =>
					gi.GalleryId == galleryId
					&& gi.GalleryImageId == galleryImageId
				).FirstOrDefaultAsync(ct);

			if (galleryImage == null)
			{
				throw new GalleryImageNotFoundException();
			}

			existingGalleryImagePath = galleryImage.ImagePath;

			galleryImage.Name = fileName;
			if (UploadNewFile)
				galleryImage.ImagePath = DestUploadPath; // We only save the image path if the file has changed.
			galleryImage.Status = GalleryImageStatus.Approved; // For any changes, if the gallery is moderated we will update the status accordingly.
			galleryImage.Description = fileDescription;

			context.GalleryImages.Update(galleryImage);
			uploadedImageId = galleryImage.GalleryImageId;
		}
		else
		{
			var galleryImage = new GalleryImage()
			{
				GalleryId = galleryId,
				Name = fileName,
				ImagePath = DestUploadPath,
				Status = GalleryImageStatus.Approved,
				Description = fileDescription
			};

			context.GalleryImages.Add(galleryImage);
			uploadedImageId = galleryImage.GalleryImageId;
		}

		if (UploadNewFile)
		{
			// Move the file from `tmp` into the Gallery Folder.
			await GalleryImageSaveFromTemp.MoveImageFromTemp(
				cloudFileStorage,
				DestUploadPath,
				ContainerName,
				vendorName,
				uploadId,
				galleryImageId,
				galleryCode,
				existingGalleryImagePath);
		}

		return uploadedImageId;
	}



	public static SurveyQuestionDataDto? GetQuestionData(SurveyQuestion question)
	{
		if (TryDeserialize(question.SurveyQuestionData, out Dictionary<string, object>? result))
		{
			var obj = JsonSerializer.Deserialize<SurveyQuestionDataDto>(
				question?.SurveyQuestionData,
				new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true //Enables case-insensitive deserialization
				});

			//check if the deserialized object has a uploadID 
			if (obj.UploadId != null)
				return obj;
		}

		return null;
	}

	static bool TryDeserialize(string json, out Dictionary<string, object>? result)
	{
		try
		{
			result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
			return result != null;
		}
		catch (Exception) // Catch invalid JSON format
		{
			result = null;
			return false;
		}
	}

	public static async Task<int> GetOrCreateSurveyGallery(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		ITenantResolver tenantResolver,
		string name,
		int vendorId,
		string vendorName,
		string SurveyCode,
		CancellationToken ct)
	{
		var newGalleryCode = cloudFileStorage.GetNormalizedContainerName(name);
		var existingCheck = context.Galleries.Where(g => g.Code == newGalleryCode && g.VendorId == vendorId).FirstOrDefault();

		if (existingCheck != null)
		{
			tenantResolver.ResetTenantId();
			return existingCheck.GalleryId;//if gallery already exists for survey return
		}

		//create new gallery if not exists
		Gallery newGallery = new Gallery()
		{
			Name = name,
			Code = newGalleryCode,
			Description = $"Gallery for survey with code {SurveyCode}",
			SendNotifications = false,
			IsModerated = false,
			IsPublic = false,
			VendorId = vendorId
		};

		context.Galleries.Add(newGallery);
		await context.SaveChangesAsync(ct);//save gallery so that images can be saved

		var containerName = cloudFileStorage.GetNormalizedContainerName(vendorName);
		await cloudFileStorage.FolderExistsAsync(containerName, newGalleryCode, true); // Will create the file if it does not exist.

		tenantResolver.ResetTenantId();

		return newGallery.GalleryId;
	}

	public static async Task<List<SurveyQuestionDto>> getGalleryImages(IConfiguration configuration, IApplicationDbContext context, List<SurveyQuestionDto> questions, CancellationToken ct)
	{
		string cloudStorageBaseUrl = configuration.GetSection("CloudStorage")["BaseUrl"] ?? "";

		foreach (var question in questions)
		{
			if (!string.IsNullOrEmpty(question.SurveyQuestionData))
			{
				var dataObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(question.SurveyQuestionData);
				if (dataObj != null && dataObj.TryGetValue("galleryImageId", out JsonElement jsonElement))
				{
					Guid imgGuid = Guid.Empty;
					string guidString = jsonElement.GetString() ?? "";
					if (Guid.TryParse(guidString, out Guid parsed))
						imgGuid = parsed;

					if (imgGuid != Guid.Empty)
					{
						var imgData = await context.GalleryImages
							.Where(gi => gi.GalleryImageId == imgGuid).FirstOrDefaultAsync(ct);

						if (imgData != null)
						{
							var newDataObj = new Dictionary<string, string>
							{
								{ "galleryImageUri", cloudStorageBaseUrl + imgData.ImagePath }
							};

							question.SurveyQuestionData = JsonSerializer.Serialize(newDataObj);
						}
					}
				}
			}
		}

		return questions;
	}

	public static async Task<string?> SaveGalleryImagesForSurvey(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		int vendorId,
		string vendorName,
		string surveyCode,
		int? surveyId,
		string? surveyData,
		CancellationToken cancellationToken)
	{
		if (!string.IsNullOrEmpty(surveyData))
		{
			Guid? existingImageGuid = Guid.Empty;
			if (surveyId != null)
			{
				var existingSurvey = await context.Surveys
					.Where(s => s.SurveyId == surveyId)
					.FirstOrDefaultAsync(cancellationToken);
				existingImageGuid = GetGalleryImageIdFromString(existingSurvey?.SurveyData);
			}

			int galleryId = await GetOrCreateSurveyGallery(cloudFileStorage, context, tenantResolver, $"sur-{surveyCode}", vendorId, vendorName, surveyCode, cancellationToken);

			SurveyDataDto? surveyDataDto = GetSurveyData(surveyData);
			Guid newGalleryImageId = Guid.Empty;

			if (surveyDataDto != null && surveyDataDto.UploadId != null && surveyDataDto.UploadId != Guid.Empty)
			{
				newGalleryImageId = await SaveGalleryImages(
					configuration,
					cloudFileStorage,
					context,
					galleryId,
					null,
					vendorName,
					surveyDataDto.UploadId,
					existingImageGuid,
					$"sur-{surveyCode}",
					surveyDataDto?.FileName ?? "",
					surveyDataDto.FileType ?? "",
					surveyDataDto.FileDescription ?? "",
					cancellationToken);
			}

			var result = new Dictionary<string, object>();
			if (newGalleryImageId != Guid.Empty)
				result["galleryImageId"] = newGalleryImageId;
			else if (existingImageGuid != Guid.Empty)
				result["galleryImageId"] = existingImageGuid;

			return result.Count > 0 ? JsonSerializer.Serialize(result) : null;
		}

		return null;
	}

	public static SurveyDataDto? GetSurveyData(string surveyData)
	{
		if (TryDeserialize(surveyData, out Dictionary<string, object>? result))
		{
			var obj = JsonSerializer.Deserialize<SurveyDataDto>(
				surveyData,
				new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true //Enables case-insensitive deserialization
				});

			//check if the deserialized object has a uploadID 
			if (obj.UploadId != null)
				return obj;
		}

		return null;
	}

	public static Guid GetGalleryImageIdFromString(string? data)
	{
		if (string.IsNullOrEmpty(data))
			return Guid.Empty;

		try
		{
			var jsonObject = JsonSerializer.Deserialize<JsonElement>(data);
			// Access a key in the object
			if (jsonObject.TryGetProperty("galleryImageId", out var dataImageId))
			{
				if (dataImageId.GetString() != null && dataImageId.GetString().Length > 0)
				{
					return Guid.Parse(dataImageId.GetString());
				}
			}
		}
		catch (Exception)
		{
			// If deserialization fails, return empty guid
		}

		return Guid.Empty;
	}



	public static async Task<SurveyDto> GetGalleryImagesForSurvey(IConfiguration configuration, IApplicationDbContext context, SurveyDto survey, CancellationToken ct)
	{
		string cloudStorageBaseUrl = configuration.GetSection("CloudStorage")["BaseUrl"] ?? "";

		if (survey.SurveyData != null)
		{
			Guid imgGuid = GetGalleryImageIdFromString(survey.SurveyData);
			if (imgGuid != Guid.Empty)
			{
				var imgData = await context.GalleryImages
					.Where(gi => gi.GalleryImageId == imgGuid).FirstOrDefaultAsync(ct);
				if (imgData != null)
				{
					var result = new Dictionary<string, string>
					{
						{ "galleryImageUri", cloudStorageBaseUrl + imgData.ImagePath }
					};
					survey.SurveyData = JsonSerializer.Serialize(result);
				}
			}
		}

		if (survey.Questions != null)
		{
			survey.Questions = await getGalleryImages(configuration, context, survey.Questions.ToList(), ct);
		}

		return survey;
	}

	public static async Task RemoveGalleryImages(
		IApplicationDbContext context,
		ICloudFileStorage cloudFileStorage,
		string vendorName,
		List<SurveyQuestion> listOfQuestions)
	{
		foreach (var question in listOfQuestions)
		{
			string? existingGalleryImageData = question.SurveyQuestionData;

			if (existingGalleryImageData != null && existingGalleryImageData.Length > 0)
			{
				var jsonObject = await Task.Run(() => JsonSerializer.Deserialize<JsonElement>(existingGalleryImageData));
				// Access a key in the object
				if (jsonObject.TryGetProperty("galleryImageId", out var dataImageId))
				{
					if (dataImageId.GetString() == null || dataImageId.GetString().Length == 0)
						return;

					var galleryImage = context.GalleryImages.Where(x => x.GalleryImageId == Guid.Parse(dataImageId.GetString())).FirstOrDefault();
					await DeleteGalleryImage.DeleteGalleryImageFromContainer(cloudFileStorage, vendorName, galleryImage.ImagePath);
					context.GalleryImages.Remove(galleryImage);
				}
			}
		}
	}
}
