using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.VoteItems.Exceptions;
using CSharpAPIProject.Application.VoteItems.Helpers;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

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
	IApplicationDbContext _context,
	ITenantResolver _tenantResolver,
	IUser _user,
    IAuthHelper _authHelper
) : IRequestHandler<UpsertVoteItemCommand>
{
	public async Task Handle(UpsertVoteItemCommand request, CancellationToken cancellationToken)
	{
		// use tenant resolver
		var vendorId = _tenantResolver.ResolveTenantId();
		var vendor = await _context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);

		if (vendor == null)
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
        var isAdministrator = await _authHelper.LoggedIn_IsPlatformAdmin();
        var isClientAdmin = await _authHelper.LoggedIn_IsVendorAdmin();

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
				VoteItemData = request.VoteItemData
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
			{
				voteItem.VoteItemData = request.VoteItemData;
			}

		}

        if (voteSet.EmailNotification != null && voteSet.EmailNotification.Length > 0)
        {
            // send emails here
        }

        await _context.SaveChangesAsync(cancellationToken);
	}
}
