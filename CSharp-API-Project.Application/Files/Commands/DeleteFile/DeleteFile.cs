using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.Files.Commands.DeleteFile;

[Authorize(Policy = Policies.CanDelete_File)]
public record DeleteFileCommand : IRequest
{
	public Guid StoredFileId { get; set; }
}

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
	public DeleteFileCommandValidator()
	{
		RuleFor(v => v.StoredFileId).NotEmpty();
	}
}

public class DeleteFileCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver, IUser user) : IRequestHandler<DeleteFileCommand>
{
	public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

		var storedFile = await context.StoredFiles
			.FirstOrDefaultAsync(f => f.StoredFileId == request.StoredFileId && f.VendorId == vendorId, cancellationToken);

		if (storedFile == null)
		{
			throw new StoredFileNotFoundException(request.StoredFileId);
		}

		storedFile.StatusFlag = new EntityStatus(RecordStatus.Deleted, user.Id, DateTimeOffset.UtcNow);
		context.StoredFiles.Update(storedFile);

		await context.SaveChangesAsync(cancellationToken);
	}
}
