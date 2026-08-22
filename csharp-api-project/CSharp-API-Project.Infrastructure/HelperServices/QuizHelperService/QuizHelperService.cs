using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.GalleryImages.Helpers.GalleryImageSaveFromTemp;
using CSharpAPIProject.Application.Quizzes.Exceptions;
using CSharpAPIProject.Application.Quizzes.Models;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CSharpAPIProject.Application.Interfaces.Data;

namespace CSharpAPIProject.Infrastructure.HelperServices.QuizHelperService;

public class QuizHelperService : IQuizHelperService
{
	private int _questionCounter = 1; // Use a class-level counter

	public List<QuizQuestionDto> ReAssignQuestionIds(int? newParentId, List<QuizQuestionDto> questions)
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

	public QuizQuestion MapQuizQuestion(QuizQuestionDto x, int quizId)
	{
		return new QuizQuestion
		{
			QuizId = quizId,
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
			QuizQuestionData = x.QuizQuestionData
		};
	}

	public async Task<List<QuizQuestion>> SaveGalleryImagesForQuestions(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		string quizCode,
		int vendorId,
		string vendorName,
		List<QuizQuestion>? existingQuizQuestions,
		List<QuizQuestion> allQuestions,
		CancellationToken cancellationToken)
	{
		int galleryId = -1; //placeholder for if we need to get a gallery for the questions
		foreach (var question in allQuestions)
		{
			//check if the question has an image upload ID
			if (question.QuizQuestionData?.Length > 0)
			{
				var existingQuestion = existingQuizQuestions?
					.FirstOrDefault(q => q.Prompt == question.Prompt && q.Type == question.Type);

				//if we havent already got a gallery id, get or create one
				if (galleryId < 0)
					galleryId = await GetOrCreateQuizGallery(cloudFileStorage, context, tenantResolver, $"quiz-{quizCode}", vendorId, vendorName, quizCode, cancellationToken);

				QuizQuestionDataDto? quizQuestionDataDto = GetQuestionData(question);
				Guid newGalleryImageId = Guid.Empty;
				if (quizQuestionDataDto != null)
				{
					newGalleryImageId = await SaveGalleryImages(configuration, cloudFileStorage, context, galleryId, existingQuestion?.QuizQuestionId, vendorName, quizQuestionDataDto.UploadId, null, $"quiz-{quizCode}", quizQuestionDataDto?.FileName ?? "", quizQuestionDataDto.FileType ?? "", quizQuestionDataDto.FileDescription ?? "", cancellationToken);
				}

				//update the question with the new gallery image id
				if (newGalleryImageId != Guid.Empty)
					question.QuizQuestionData = JsonSerializer.Serialize(new { galleryImageId = newGalleryImageId });
				else
					question.QuizQuestionData = null;//empty updated data because there was nothing to upload
			}
		}

		return allQuestions;
	}

	public void CollectChildQuestions(QuizQuestion parentQuestion, IEnumerable<QuizQuestionDto>? childQuestions, List<QuizQuestion> allQuestions)
	{
		if (childQuestions == null || childQuestions.Count() == 0)
			return;

		foreach (var childQuestionDto in childQuestions)
		{
			//check if question is valid
			IsValidQuestion(childQuestionDto, allQuestions);

			//check if parent question ID is correct.
			if (childQuestionDto.ParentQuestionId != parentQuestion.QuestionId)
				throw new QuestionWithParentIdNotFoundException(childQuestionDto.QuizId, childQuestionDto.QuestionId, parentQuestion.ParentQuestionId);

			var childQuestion = MapQuizQuestion(childQuestionDto, parentQuestion.QuizId);
			childQuestion.ParentQuestionId = parentQuestion.QuestionId;
			allQuestions.Add(childQuestion);

			// Recursively collect child questions
			CollectChildQuestions(childQuestion, childQuestionDto?.ChildQuestions, allQuestions);
		}
	}

	public void IsValidQuestion(QuizQuestionDto question, List<QuizQuestion> allQuestions)
	{
		if (question.ParentQuestionId == null && question.ParentCondition != null && question.ParentCondition != "")
		{
			throw new QuestionWithConditionButNoParentException(question.QuizId, question.QuestionId, question.ParentQuestionId);
		}
		else if (question.ParentQuestionId != null && question.ParentCondition != null)
		{
			var parentQuestion = allQuestions.FirstOrDefault(x => x.QuestionId == question.ParentQuestionId);

			if (parentQuestion?.Options == null)
				throw new QuestionWithConditionButNoOptionsFoundException(question.QuizId, question.QuestionId, question.ParentQuestionId);

			string[] allowedValues = parentQuestion.Options.Split(",");

			if (allowedValues.Length > 0)
			{
				var conditionalValues = question.ParentCondition.Split(",");
				foreach (var conditionalValue in conditionalValues)
				{
					if (conditionalValue != String.Empty)
						if (!allowedValues.Contains(conditionalValue))
							throw new QuestionConditionDoesNotMatchParentOptionException(question.QuizId, question.QuestionId, question.ParentQuestionId);
				}
			}
			else
			{
				throw new QuestionWithConditionButNoOptionsFoundException(question.QuizId, question.QuestionId, question.ParentQuestionId);
			}
		}
	}

	public QuizQuestionDataDto? GetQuestionData(QuizQuestion question)
	{
		if (TryDeserialize(question.QuizQuestionData, out Dictionary<string, object>? result))
		{
			var obj = JsonSerializer.Deserialize<QuizQuestionDataDto>(
				question?.QuizQuestionData,
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

	bool TryDeserialize(string json, out Dictionary<string, object>? result)
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

	public async Task<int> GetOrCreateQuizGallery(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		ITenantResolver tenantResolver,
		string name,
		int vendorId,
		string vendorName,
		string quizCode,
		CancellationToken ct)
	{
		var newGalleryCode = cloudFileStorage.GetNormalizedContainerName(name);
		var existingCheck = context.Galleries.Where(g => g.Code == newGalleryCode && g.VendorId == vendorId).FirstOrDefault();

		if (existingCheck != null)
		{
			tenantResolver.ResetTenantId();
			return existingCheck.GalleryId;//if gallery already exists for quiz return
		}

		//create new gallery if not exists
		Gallery newGallery = new Gallery()
		{
			Name = name,
			Code = newGalleryCode,
			Description = $"Gallery for quiz with code {quizCode}",
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

	public async Task<Guid> SaveGalleryImages(
		IConfiguration configuration,
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		int galleryId,
		int? quizQuestionId, //can be null if its a new question
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
		string DestUploadPath = $"{ContainerFriendlyVendorName}/{galleryCode}/{uploadId}.{fileType}";
		bool UploadNewFile = true;

		//If there is nothing to upload, return
		if (uploadId == Guid.Empty)
		{
			return Guid.Empty;
		}

		string ContainerName = cloudFileStorage.GetNormalizedContainerName(defaultUploadContainer);

		//check if the question has an existing image but was not provided
		if (quizQuestionId != null && galleryImageId == null || galleryImageId == Guid.Empty)
		{
			var quizQuestion = await context.QuizQuestions
					.Where(gi => gi.QuizQuestionId == quizQuestionId).FirstOrDefaultAsync(ct);

			string? existingGalleryImageData = quizQuestion?.QuizQuestionData;

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

	public async Task RemoveGalleryImages(
		IApplicationDbContext context,
		ICloudFileStorage cloudFileStorage,
		string vendorName,
		List<QuizQuestion> listOfQuestions)
	{
		foreach (var question in listOfQuestions)
		{
			string? existingGalleryImageData = question.QuizQuestionData;

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