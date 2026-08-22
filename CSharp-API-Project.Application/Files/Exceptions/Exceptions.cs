using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.Files.Exceptions;

public class StoredFileNotFoundException(Guid StoredFileId) : BadRequestException($"File with id {StoredFileId} was not found.");
public class VendorNotFoundException() : BadRequestException("A Vendor with the requested tenant Id was not found.");
