using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.VoteItems.Exceptions;

public class VoteSetNotFoundException(int VoteSetId) : BadRequestException($"VoteSet with id {VoteSetId} was not found.");
public class VoteItemNotFoundException(int VoteItemId) : BadRequestException($"VoteItem with id {VoteItemId} was not found.");
public class VoteItemWithVendorAndCodeNotFoundException(string VoteItemcode, int VendorId) : BadRequestException($"VoteItem with code {VoteItemcode} and VendorId {VendorId} was not found.");
public class VoteItemWithVendorAndVoteItemIdNotFoundException(int VoteItemId, int VendorId) : BadRequestException($"VoteItem with ID {VoteItemId} and VendorId {VendorId} was not found.");
public class VendorNotFoundException() : BadRequestException("A Vendor with the requested tenant Id was not found.");
public class VoteItemMustHaveADescriptionException (int VoteItemId) : BadRequestException($"VoteItem {VoteItemId} must have a description");
public class BannedWordException (int VoteItemId, string BannedWord) : BadRequestException($"'{BannedWord}' in Vote Item {VoteItemId}, is not allowed in the description");
public class VoteItemDoesNotContainVoteItemException (int VoteSetId, int VoteItemId) : BadRequestException($"Vote Set {VoteSetId} does not contain an item with ID: {VoteItemId}");
public class CustomerNotAllowedToCreateItemForVoteSetException(int VoteSetId) : BadRequestException($"Vote Set {VoteSetId} does not allow customers to create items");
public class CustomerNotFoundException(): BadRequestException("Customer not found");
public class TextExceedsWordCountException(int WordLimit, string NameOfItem) : BadRequestException($"{NameOfItem} cannot exceed {WordLimit} words");
public class VoteSetExpiredException(int VoteSetId, DateTime EndOn) : BadRequestException($"Vote Set {VoteSetId} expired on {EndOn}");
public class VoteSetNotStartedException(int VoteSetId, DateTime StartDate) : BadRequestException($"Vote Set {VoteSetId} opens on {StartDate}");
public class VoteSetDoesNotAllowMultipleException(int voteSetId) : BadRequestException($"Vote Set {voteSetId}: User has already uploaded a moment");