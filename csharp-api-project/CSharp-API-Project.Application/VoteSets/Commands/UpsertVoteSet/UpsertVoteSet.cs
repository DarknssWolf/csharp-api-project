using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Common;
using Microsoft.Extensions.Configuration;
using CSharpAPIProject.Application.VoteSets.Exceptions;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Application.VoteSets.Helpers;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Application.Interfaces.Entities;

namespace CSharpAPIProject.Application.VoteSets.Commands.UpsertVoteSet;

[Authorize(Policy = Policies.CanCreate_VoteSet)]
[Authorize(Policy = Policies.CanUpdate_VoteSet)]
public record UpsertVoteSetCommand(
	int? VoteSetId,
	string Title,
	string Code,
	DateTime StartOn,
	DateTime EndOn,
	bool CanEarnPoints,
	bool AllowMultipleTries,
	bool AllowMultipleItems,
	string? EmailNotification,
	string? VoteSetData,
	WhoCanCreateVoteItemFlag CanUsersUploadItems,
    Guid visibilityConfigurationId,
	IEnumerable<VoteSetItemDto>? Items
) : IRequest;

public class UpsertVoteSetCommandValidator : AbstractValidator<UpsertVoteSetCommand>
{
	public UpsertVoteSetCommandValidator()
	{
		RuleFor(x => x.Title).NotEmpty();
		RuleFor(x => x.Code).NotEmpty();
		RuleFor(x => x.StartOn).NotEmpty();
		RuleFor(x => x.EndOn).NotEmpty();
		RuleFor(x => x.CanUsersUploadItems).IsInEnum(); // Ensures the value is a valid enum
	}
}

public class UpsertVoteSetCommandHandler(
	IConfiguration _configuration,
	IApplicationDbContext _context,
	ITenantResolver _tenantResolver,
	ICloudFileStorage _cloudFileStorage,
	IMapper _mapper,
    IUser user
) : IRequestHandler<UpsertVoteSetCommand>
{
	public async Task Handle(UpsertVoteSetCommand request, CancellationToken cancellationToken)
	{
		// use tenant resolver
		var vendorId = _tenantResolver.ResolveTenantId();
		var vendor = _context.Vendors.Where(v => v.VendorId == vendorId).FutureValue();
		if (vendor.Value == null)
			throw new VendorNotFoundException();

        if(!VoteSetHelper.ValidateMaxXWords(200, request.Title))
            throw new TextExceedsWordCountException(50, "VoteSet Title");

        var voteSet = await _context.VoteSets
			.Include(x => x.VoteItems)
			.FirstOrDefaultAsync(x =>
				x.VendorId == vendorId &&
				((request.VoteSetId > 0 && x.VoteSetId == request.VoteSetId)),
			cancellationToken);

		var allItems = new List<VoteItem>();
		//Loop to validate Items before adding them to the list
        if (request.Items != null)
            foreach (var voteItemDto in request.Items)
		    {
			    var voteItem = _mapper.Map<VoteItem>(voteItemDto);
                VoteSetHelper.ValidateVoteItem(voteItem);
			    allItems.Add(voteItem);
		    }

		if (voteSet == null)
		{
			// Create new VoteSet if not found
			voteSet = new VoteSet
			{
				Title = request.Title,
				Code = request.Code,
				StartOn = request.StartOn,
				EndOn = request.EndOn,
				VendorId = vendorId,
				CanEarnPoints = request.CanEarnPoints,
				AllowMultipleTries = request.AllowMultipleTries,
				AllowMultipleItems = request.AllowMultipleItems,
				EmailNotification = request.EmailNotification,
				VoteSetData = await VoteSetHelper.SaveGalleryImageForVoteSet(_cloudFileStorage,_context,_configuration,_tenantResolver,vendorId,vendor.Value.Name,null,request.Code, request.VoteSetData, cancellationToken),
				WhoCanCreateVoteItemFlag = request.CanUsersUploadItems,
                VisibilityConfigurationId = request.visibilityConfigurationId,
            };

			_context.VoteSets.Add(voteSet);
			await _context.SaveChangesAsync(cancellationToken);

			// Save gallery images for Items and update thier dataobjects with the new galleryImageId
			allItems = await VoteSetHelper.SaveGalleryImagesForVoteItems(_cloudFileStorage, _context, _configuration, _tenantResolver, voteSet.Code, vendorId, vendor.Value.Name, null, allItems, cancellationToken);

			// Add new Items for the new VoteSet
			voteSet.VoteItems = allItems;
		}
		else
		{
            //cannot update VoteSet if it has ended
            if (voteSet.EndOn < DateTime.UtcNow)
                throw new VoteSetExpiredException(voteSet.VoteSetId, voteSet.EndOn);

            // Update existing VoteSet's details
            voteSet.Title = request.Title;
			voteSet.StartOn = request.StartOn;
			voteSet.EndOn = request.EndOn;
			voteSet.CanEarnPoints = request.CanEarnPoints;
			voteSet.AllowMultipleTries = request.AllowMultipleTries;
			voteSet.AllowMultipleItems = request.AllowMultipleItems;
			voteSet.EmailNotification = request.EmailNotification;
			voteSet.WhoCanCreateVoteItemFlag = request.CanUsersUploadItems;
            voteSet.VisibilityConfigurationId = request.visibilityConfigurationId;

			//Only update if there is new data else itll overwrite with null
			if (request.VoteSetData != null && request.VoteSetData.Length > 0)
				voteSet.VoteSetData = await VoteSetHelper.SaveGalleryImageForVoteSet(_cloudFileStorage, _context, _configuration, _tenantResolver, vendorId, vendor.Value.Name, voteSet.VoteSetId, request.Code, request.VoteSetData, cancellationToken);

			// Get existing Items from the database
			var existingItems = _context.VoteItems
				.Where(q => q.VoteSetId == voteSet.VoteSetId)
				.ToList();

			// Save gallery images for Items and update thier dataobjects with the new galleryImageId
			allItems = await VoteSetHelper.SaveGalleryImagesForVoteItems(_cloudFileStorage, _context, _configuration, _tenantResolver, voteSet.Code, vendorId, vendor.Value.Name, existingItems, allItems, cancellationToken);

			// Update or add Items
			foreach (var item in allItems)
			{
				var existingItem = existingItems
					.FirstOrDefault(i => i.VoteItemId == item.VoteItemId);

				if (existingItem == null)
				{
					//vote items ALWAYS start in pending state
					item.Status = VoteItemStatus.Pending; 
					item.VoteItemStatusReason = "New vote item has been created";

					// Save gallery images for Items
					voteSet.VoteItems.Add(item);
					continue;
				}

				// Update existing question
				existingItem.Description = item.Description;

				existingItem.Status = item.Status;
				existingItem.VoteItemStatusReason = item.VoteItemStatusReason;

                //check if there is a new config id
                bool newVisibilityConfigId = existingItem.VisibilityConfigurationId == item.VisibilityConfigurationId;

                existingItem.VisibilityConfigurationId = item.VisibilityConfigurationId;

                var voteSubmissions = await _context.VoteSubmissions
                    .Where(vs => vs.VoteItemId == existingItem.VoteItemId).ToListAsync();

                // Update VoteSubmissions' VisibilityConfigurationId to match the VoteItem IF new visibility Config ID
                if (newVisibilityConfigId && voteSubmissions != null && voteSubmissions.Count > 0)
                { 
                    foreach (var submission in voteSubmissions)
                    {
                        submission.VisibilityConfigurationId = item.VisibilityConfigurationId;
                    }
                }

                // question data is null if nothing was changed, so only update if data is populated 
                if (item.VoteItemData != null && item.VoteItemData != String.Empty)
					existingItem.VoteItemData = item.VoteItemData;
			}

			// Remove deleted Items
			var itemsToDelete = existingItems
				.Where(eq => !allItems.Any(i => i.VoteItemId == eq.VoteItemId))
				.ToList();
			//Remove any data images
			await VoteSetHelper.GetQuestionDataAndRemove(_context, _cloudFileStorage, user, vendor.Value.Name, itemsToDelete);
			_context.VoteItems.RemoveRange(itemsToDelete);
		}

		await _context.SaveChangesAsync(cancellationToken);
	}
}