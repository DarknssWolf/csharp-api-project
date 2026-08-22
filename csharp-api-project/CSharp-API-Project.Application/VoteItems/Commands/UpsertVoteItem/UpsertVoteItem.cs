using System.Security.Claims;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Mailer.Notifications.SendEmailNotification;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Application.VoteItems.Helpers;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Application.Interfaces.Entities;

namespace CSharpAPIProject.Application.VoteItems.Commands.UpsertVoteItem;

[Authorize(Policy = Policies.CanCreate_VoteItem)]
[Authorize(Policy = Policies.CanUpdate_VoteItem)]
public record UpsertVoteItemCommand(
	int? VoteItemId,
	int VoteSetId,
    string Title,
	string Description,
	VoteItemStatus? Status,
	string? VoteItemStatusReason,
	string? VoteItemData
) : IRequest;

public class UpsertVoteItemCommandValidator : AbstractValidator<UpsertVoteItemCommand>
{
	public UpsertVoteItemCommandValidator()
	{
		RuleFor(x => x.VoteSetId).NotEmpty();
		RuleFor(x => x.Description).NotEmpty();
	}
}

public class UpsertVoteItemCommandHandler(
	IConfiguration _configuration,
	IApplicationDbContext _context,
	ITenantResolver _tenantResolver,
	ICloudFileStorage _cloudFileStorage,
	IUser _user,
    IIdentityService identityService,
    IAuthHelper _authHelper,
    IMediator _mediator,
    ISettingsService _settingsService,
    ILogger<UpsertVoteItemCommand> logger
) : IRequestHandler<UpsertVoteItemCommand>
{
	public async Task Handle(UpsertVoteItemCommand request, CancellationToken cancellationToken)
	{
		// use tenant resolver
		var vendorId = _tenantResolver.ResolveTenantId();
		var vendor = _context.Vendors.Where(v => v.VendorId == vendorId).FutureValue();

		if (vendor.Value == null)
			throw new VendorNotFoundException();

        var voteSet = await _context.VoteSets
            .Include(x => x.VoteItems)
            .Where(s => s.VoteSetId == request.VoteSetId && s.VendorId == vendorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (voteSet == null)
            throw new VoteSetNotFoundException(request.VoteSetId);

        if(voteSet.EndOn < DateTime.UtcNow)
            throw new VoteSetExpiredException(voteSet.VoteSetId, voteSet.EndOn);

        if(voteSet.StartOn > DateTime.UtcNow)
            throw new VoteSetNotStartedException(voteSet.VoteSetId, voteSet.StartOn);

        //check if the user is allowed to create a vote item
        var isAdministrator = await _authHelper.LoggedIn_IsTICUser();
        var isClientAdmin = await _authHelper.LoggedIn_IsClientAdmin();

		//check permissions
		if (!isAdministrator && !isClientAdmin && voteSet.WhoCanCreateVoteItemFlag != WhoCanCreateVoteItemFlag.User)
			throw new CustomerNotAllowedToCreateItemForVoteSetException(voteSet.VoteSetId);

        if (!VoteItemHelper.ValidateMaxXWords(200, request.Title))
            throw new TextExceedsWordCountException(50, "Item Title");

        if (!VoteItemHelper.ValidateMaxXWords(200, request.Description))
            throw new TextExceedsWordCountException(200, "Item Description");

        var voteItem = voteSet.VoteItems
            .FirstOrDefault(v => v.VoteItemId == request.VoteItemId);

		if (voteItem == null)
		{
			// Create new VoteItem if not found
			voteItem = new VoteItem
			{
				VoteSetId = request.VoteSetId,
                Title = request.Title,
                Description = request.Description,
				Status = VoteItemStatus.Pending, //vote items ALWAYS start in pending state
				VoteItemStatusReason = "New vote item has been created",
                VisibilityConfigurationId = voteSet.VisibilityConfigurationId, //when creating a vote item, use the vote set configuration ID
				VoteItemData = (request.VoteItemData != null) ? await VoteItemHelper.SaveGalleryImageForVoteItem(_cloudFileStorage,_context,_configuration,_tenantResolver,vendorId,vendor.Value.Name,null,voteSet.Code, request.VoteItemData, cancellationToken) : null
			};

			//Validate Item before adding. Checks for nono words
			VoteItemHelper.ValidateVoteItem(voteItem);

			_context.VoteItems.Add(voteItem);
		}
		else
		{
            //if not allowed multiple items, check if the user has already uploaded for this item
            if (!voteSet.AllowMultipleItems)
            {
                var userMoment = await _context.VoteItems
                    .Where(vi => vi.VoteSetId == voteSet.VoteSetId && vi.CreatedBy == _user.Id).FirstOrDefaultAsync(cancellationToken);

                if (userMoment != null)
                    throw new VoteSetDoesNotAllowMultipleException(voteSet.VoteSetId);
            }

            VoteItemHelper.ValidateVoteItem(voteItem);

			// Update existing VoteItem's details
            voteItem.Title = request.Title;
            voteItem.Description = request.Description;

			//only relevent permissions can update status and reason
			if (isAdministrator || isClientAdmin)
			{
				voteItem.Status = request.Status ?? VoteItemStatus.Pending;
				voteItem.VoteItemStatusReason = request.VoteItemStatusReason;
			}

			//Only update if there is new data else itll overwrite with null
			if (request.VoteItemData != null && request.VoteItemData.Length > 0)
				voteItem.VoteItemData = await VoteItemHelper.SaveGalleryImageForVoteItem(_cloudFileStorage, _context, _configuration, _tenantResolver, vendorId, vendor.Value.Name, voteItem.VoteItemId, voteSet.Code, request.VoteItemData, cancellationToken);

		}

        if (voteSet.EmailNotification != null && voteSet.EmailNotification.Length > 0)
        {
            var vendorSettings = _settingsService.LoadSetting<VendorSettings>();
            // Send email notification to all users in the email list
            var emailList = voteSet.EmailNotification.Split(',').ToList();
            foreach (var email in emailList)
            {
                var userId = await identityService.GetUserIdAsync(email, vendorId);

                if (userId == null)
                {
                    logger.LogError($"User not found for email: {email}. Skipping notification for vote item upsert");
                    continue;
                }

                var userClaims = await identityService.GetClaimsAsync(userId);

                if (userClaims == null || !userClaims.Any())
                {
                    logger.LogError($"No claims found for user with email: {email}. Skipping notification for vote item upsert");
                    continue;
                }

                var firstName = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                var lastName = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

                // Trigger the mail notification
                await _mediator.Publish(new SendEmailNotification
                {
                    Recipients = new List<EmailAddress>() { new() { Name = $"{firstName} {lastName}", Address = email } },
                    EmailGroupName = "Vote Item Created",
                    TemplateId = vendorSettings.EmailTemplate_VoteItemUploadNotification,
                    EntityId = _user.Id.ToString(),
                    EntityType = "Customer"
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
	}
}