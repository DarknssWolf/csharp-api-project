using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.VoteSubmissions.Exceptions;
public class VoteItemNotFoundException(int VoteItemId) : BadRequestException($"Vote Item with ID {VoteItemId} was not found.");
public class VoteSetNotFoundException(int VoteSetId) : BadRequestException($"Vote Set with ID {VoteSetId} was not found.");
public class VoteItemNotApprovedException(int VoteItemId) : BadRequestException($"Vote Item with ID {VoteItemId} is not approved.");
public class VendorNotFoundException() : BadRequestException("A Vendor with the requested tenant Id was not found.");
public class VoteSetExpiredException(int VoteSetId, DateTime EndOn) : BadRequestException($"Vote Set with ID {VoteSetId} has expired on {EndOn}.");
public class VoteSetNotStartedException(int VoteSetId, DateTime StartOn) : BadRequestException($"Vote Set with ID {VoteSetId} will open on {StartOn}");