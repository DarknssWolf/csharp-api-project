using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.VoteSets.Exceptions;

public class VoteSetNotFoundException(int VoteSetId) : BadRequestException($"VoteSet with id {VoteSetId} was not found.");
public class VoteSetWithVendorAndCodeNotFoundException(string VoteSetcode, int VendorId) : BadRequestException($"VoteSet with code {VoteSetcode} and VendorId {VendorId} was not found.");
public class VoteSetWithVendorAndVoteSetIdNotFoundException(int VoteSetId, int VendorId) : BadRequestException($"VoteSet with ID {VoteSetId} and VendorId {VendorId} was not found.");
public class VendorNotFoundException() : BadRequestException("A Vendor with the requested tenant Id was not found.");
public class VoteItemMustHaveADescriptionException (int VoteItemId) : BadRequestException($"VoteItem {VoteItemId} must have a description");
public class BannedWordException (int VoteItemId, string BannedWord) : BadRequestException($"'{BannedWord}' in Vote Item {VoteItemId}, is not allowed in the description");
public class VoteSetDoesNotContainVoteItemException (int VoteSetId, int VoteItemId) : BadRequestException($"Vote Set {VoteSetId} does not contain an item with ID: {VoteItemId}");
public class TextExceedsWordCountException(int WordLimit, string NameOfItem) : BadRequestException($"{NameOfItem} cannot exceed {WordLimit} words");
public class VoteSetExpiredException(int VoteSetId, DateTime EndDate) : BadRequestException($"Vote Set {VoteSetId} has expired on {EndDate.ToShortDateString}");