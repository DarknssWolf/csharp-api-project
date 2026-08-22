using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.GalleryImages.Helpers.GalleryImageSaveFromTemp;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Application.VoteItems.Models;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.VoteItems.Helpers;

public static class VoteItemHelper
{
    public static void ValidateVoteItem(VoteItem voteItem)
	{
		if(voteItem.Description == null || voteItem.Description == String.Empty)
			throw new VoteItemMustHaveADescriptionException(voteItem.VoteItemId);

		//Check for No No Words
		string[] bannedWords = new string[] { "fuck", "bitch", "poes", "fucked" };
		string[] wordsInDescription = voteItem.Description.Split(" ").Select(x => x.Trim().ToLower()).ToArray();
		foreach (string word in wordsInDescription)
			if (bannedWords.Contains(word))
				throw new BannedWordException(voteItem.VoteItemId, word);
	}
	public static void ValidateVoteSubmission(VoteItem voteItem)
	{
		if (voteItem.Status != VoteItemStatus.Approve)
			throw new Exception("VoteItem must be approved before it can be submitted");
		
	}

	public static async Task<List<VoteItem>> SaveGalleryImagesForVoteItems(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		string VoteItemCode,
		int vendorId,
		string vendorName,
		List<VoteItem>? existingVoteItems,
		List<VoteItem> allVoteItems,
		CancellationToken cancellationToken)
	{
		int galleryId = -1; //placeholder for if we need to get a gallery for the questions
		foreach (var voteItem in allVoteItems)
		{
			//check if the question has an image upload ID
			if (voteItem.VoteItemData?.Length > 0)
			{
				var existingVoteItem = existingVoteItems?
					.FirstOrDefault(q => q.VoteItemId == voteItem.VoteItemId && q.VoteItemId == voteItem.VoteItemId);

				//if we havent already got a gallery id, get or create one
				if (galleryId < 0)
					galleryId = await GetOrCreateVoteGallery(cloudFileStorage, context, tenantResolver, $"VoteItem-{VoteItemCode}", vendorId, vendorName, VoteItemCode, cancellationToken);

				VoteItemDataDto? voteItemDataDto = GetVoteItemData(voteItem);

				Guid newGalleryImageId = Guid.Empty;
				if (voteItemDataDto != null)
				{
					newGalleryImageId = await SaveGalleryImages(configuration, cloudFileStorage, context, galleryId, vendorName, voteItemDataDto.UploadId, null, $"VoteItem-{VoteItemCode}", voteItemDataDto?.FileName ?? "", voteItemDataDto.FileType ?? "", voteItemDataDto.FileDescription ?? "", cancellationToken);
				}

				//update the question with the new gallery image id
				if (newGalleryImageId != Guid.Empty)
					voteItem.VoteItemData = JsonSerializer.Serialize(new { galleryImageId = newGalleryImageId });
				else
					voteItem.VoteItemData = null;//empty updated data because there was nothing to upload
			}
		}

		return allVoteItems;
	}

	public static async Task<string?> SaveGalleryImageForVoteItem(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		IConfiguration configuration,
		ITenantResolver tenantResolver,
		int vendorId,
		string vendorName,
		int? VoteItemId,
		string VoteItemCode,
		string VoteItemData,
		CancellationToken cancellationToken)
	{
		int galleryId = -1; //placeholder for if we need to get a gallery for the questions

		//check if the poll has an image upload ID
		if (VoteItemData != null && VoteItemData != String.Empty)
		{
			Guid? ExistingImageGuid = Guid.Empty;
			if (VoteItemId != null) { }
			var existingVoteItem = await context.VoteItems
				.Where(p => p.VoteItemId == VoteItemId).FirstOrDefaultAsync(cancellationToken);

			ExistingImageGuid = GetGalleryImageIdFromString(existingVoteItem?.VoteItemData);//get an existing image

			//if we havent already got a gallery id, get or create one
			if (galleryId < 0)
				galleryId = await GetOrCreateVoteGallery(cloudFileStorage, context, tenantResolver, $"VoteItem-{VoteItemCode}", vendorId, vendorName, VoteItemCode, cancellationToken);

			VoteItemDataDto? VoteItemDataDto = GetVoteItemData(VoteItemData);

			Guid newGalleryImageId = Guid.Empty;
			if (VoteItemDataDto != null)
			{
				newGalleryImageId = await SaveGalleryImages(configuration, cloudFileStorage, context, galleryId, vendorName, VoteItemDataDto.UploadId, ExistingImageGuid, $"VoteItem-{VoteItemCode}", VoteItemDataDto?.FileName ?? "", VoteItemDataDto.FileType ?? "", VoteItemDataDto.FileDescription ?? "", cancellationToken);
			}

			//return statements
			if (newGalleryImageId != Guid.Empty)
				return JsonSerializer.Serialize(new { galleryImageId = newGalleryImageId });
			else
				return null;//empty updated data because there was nothing to upload
		}
		else
		{
			return null;
		}
	}
	public static Guid GetGalleryImageIdFromString(string? data)
	{
		if (data == null)
			return Guid.Empty;

		Guid imgGuid = Guid.Empty;
		var dataObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data);
		if (dataObj != null && dataObj.TryGetValue("galleryImageId", out JsonElement jsonElement))
		{

			string x = jsonElement.GetString() ?? "";
			if (Guid.TryParse(x, out Guid guid))
				imgGuid = guid;
		}

		return imgGuid;
	}

    /// <summary>
    /// Updates the gallery image to be the full URI for the image.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="context"></param>
    /// <param name="VoteItem"></param>
    /// <param name="ct"></param>
    /// <returns>VoteItemDto</returns>
	public static async Task<VoteItemDto> GetGalleryImages(IConfiguration configuration, IApplicationDbContext context, VoteItemDto VoteItem, CancellationToken ct)
	{
		string cloudStorageBaseUrl = configuration.GetSection("CloudStorage")["BaseUrl"] ?? "";

		//check if the poll has any image data
		if (VoteItem.VoteItemData != null && VoteItem.VoteItemData != String.Empty)
		{
			Guid imgGuid = GetGalleryImageIdFromString(VoteItem.VoteItemData);
			if (imgGuid != Guid.Empty)
			{
				var imgData = await context.GalleryImages
					.Where(gi => gi.GalleryImageId == imgGuid).FirstOrDefaultAsync(ct);
				Dictionary<string, string> newDataObj = new Dictionary<string, string>
				{
					{ "galleryImageUri", cloudStorageBaseUrl + imgData?.ImagePath }
				};
				VoteItem.VoteItemData = JsonSerializer.Serialize(newDataObj);
			}
		}

		return VoteItem;
	}

	public static async Task<Guid> SaveGalleryImages(
			IConfiguration configuration,
			ICloudFileStorage cloudFileStorage,
			IApplicationDbContext context,
			int galleryId,
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

	public static VoteItemDataDto? GetVoteItemData(string VoteItemData)
	{
		if (TryDeserialize(VoteItemData, out Dictionary<string, object>? result))
		{
			var obj = JsonSerializer.Deserialize<VoteItemDataDto>(
				VoteItemData,
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

	public static VoteItemDataDto? GetVoteItemData(VoteItem voteItem)
	{
		if (TryDeserialize(voteItem.VoteItemData, out Dictionary<string, object>? result))
		{
			var obj = JsonSerializer.Deserialize<VoteItemDataDto>(
				voteItem?.VoteItemData,
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

	public static async Task<int> GetOrCreateVoteGallery(
		ICloudFileStorage cloudFileStorage,
		IApplicationDbContext context,
		ITenantResolver tenantResolver,
		string name,
		int vendorId,
		string vendorName,
		string voteCode,
		CancellationToken ct)
	{
		var newGalleryCode = cloudFileStorage.GetNormalizedContainerName(name);
		var existingCheck = context.Galleries.Where(g => g.Code == newGalleryCode && g.VendorId == vendorId).FirstOrDefault();

		if (existingCheck != null)
		{
			tenantResolver.ResetTenantId();
			return existingCheck.GalleryId;//if gallery already exists for Quiz return
		}

		//create new gallery if not exists
		Gallery newGallery = new Gallery()
		{
			Name = name,
			Code = newGalleryCode,
			Description = $"Gallery for VoteItems and VoteItems with code {voteCode}",
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

	public static async Task<VoteItem> GetGalleryImagesForVoteItem(IConfiguration configuration, IApplicationDbContext context, VoteItem VoteItem, CancellationToken ct)
	{ 
		if (VoteItem.VoteItemData != null && VoteItem.VoteItemData != string.Empty)
		{
			VoteItem.VoteItemData = await GetImageURI(context, configuration, VoteItem.VoteItemData, ct);
		}
		return VoteItem;
	}

	private static async Task<string?> GetImageURI(IApplicationDbContext context, IConfiguration configuration, string dataString, CancellationToken ct)
	{
		string cloudStorageBaseUrl = configuration.GetSection("CloudStorage")["BaseUrl"] ?? "";

		var dataObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dataString);
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

				return JsonSerializer.Serialize(newDataObj);
			}
			else
			{
				return null;
			}
		}
		else
		{
			return null;
		}
	}

	public static async Task GetImageDataAndRemove(
	IApplicationDbContext context,
	ICloudFileStorage cloudFileStorage,
    IUser user,
    string vendorName,
	VoteItem VoteItem)
	{
		//get poll image Data
		string? existingGalleryImageData = VoteItem.VoteItemData;

		if (existingGalleryImageData != null && existingGalleryImageData.Length > 0)
		{
			await RemoveGalleryImages(context, cloudFileStorage, user, existingGalleryImageData, vendorName);
		}
	}

    /// <summary>
    /// Validates if the text exceeds the maximum number of words.
    /// </summary>
    /// <param name="wordLimit"></param>
    /// <param name="text"></param>
    /// <returns>'true' if within limit, 'false if exceeds limit'</returns>
    public static bool ValidateMaxXWords(int wordLimit, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        var wordCount = text
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return wordCount <= wordLimit;
    }

    public static async Task RemoveGalleryImages(
		IApplicationDbContext context,
		ICloudFileStorage cloudFileStorage,
        IUser user,
        string imageDataObj,
		string vendorName)
	{
		var jsonObject = await Task.Run(() => JsonSerializer.Deserialize<JsonElement>(imageDataObj));
		// Access a key in the object
		if (jsonObject.TryGetProperty("galleryImageId", out var dataImageId))
		{
			var galleryImage = context.GalleryImages.Where(x => x.GalleryImageId == Guid.Parse(dataImageId.GetString())).FirstOrDefault();
            //await DeleteGalleryImage.DeleteGalleryImageFromContainer(cloudFileStorage, vendorName, galleryImage.ImagePath);
            galleryImage.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
            context.GalleryImages.Update(galleryImage);
		}
	}
}
