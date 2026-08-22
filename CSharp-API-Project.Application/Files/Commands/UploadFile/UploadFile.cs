using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CSharpAPIProject.Application.Files.Commands.UploadFile;

[Authorize(Policy = Policies.CanCreate_File)]
public record UploadFileCommand(
	IFormFile File
) : IRequest<Guid>;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
	public UploadFileCommandValidator(IConfiguration configuration)
	{
		var maxUploadBytes = configuration.GetValue<long?>("FileStorage:MaxUploadBytes") ?? 5242880;
		var validFileTypes = configuration.GetSection("FileStorage:ValidFileTypes").Get<string[]>() ?? [];

		RuleFor(x => x.File).NotNull();
		RuleFor(x => x.File.Length)
			.GreaterThan(0)
			.LessThanOrEqualTo(maxUploadBytes)
			.WithMessage($"File cannot be larger than {maxUploadBytes} bytes.");
		RuleFor(x => x.File.ContentType)
			.Must(validFileTypes.Contains)
			.WithMessage($"File type must be one of: {string.Join(", ", validFileTypes)}.");
	}
}

public class UploadFileCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver) : IRequestHandler<UploadFileCommand, Guid>
{
	public async Task<Guid> Handle(UploadFileCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var vendor = await context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
		if (vendor == null)
			throw new VendorNotFoundException();

		using var stream = new MemoryStream();
		await request.File.CopyToAsync(stream, cancellationToken);

		var storedFile = new StoredFile
		{
			StoredFileId = Guid.NewGuid(),
			VendorId = vendorId,
			FileName = request.File.FileName,
			ContentType = request.File.ContentType,
			Length = request.File.Length,
			Content = stream.ToArray()
		};

		context.StoredFiles.Add(storedFile);
		await context.SaveChangesAsync(cancellationToken);

		return storedFile.StoredFileId;
	}
}
