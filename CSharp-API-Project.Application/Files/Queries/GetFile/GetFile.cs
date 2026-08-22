using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Exceptions;
using CSharpAPIProject.Application.Files.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Files.Queries.GetFile;

[Authorize(Policy = Policies.CanView_File)]
public record GetFileQuery(Guid StoredFileId) : IRequest<StoredFileDto>;

public class GetFileQueryValidator : AbstractValidator<GetFileQuery>
{
	public GetFileQueryValidator()
	{
		RuleFor(v => v.StoredFileId).NotEmpty();
	}
}

public class GetFileQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver) : IRequestHandler<GetFileQuery, StoredFileDto>
{
	public async Task<StoredFileDto> Handle(GetFileQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

		var storedFile = await context.StoredFiles
			.Where(f => f.StoredFileId == request.StoredFileId)
			.Where(f => f.VendorId == vendorId)
			.Where(f => f.StatusFlag.Status == RecordStatus.Active)
			.FirstOrDefaultAsync(cancellationToken);

		if (storedFile == null)
		{
			throw new StoredFileNotFoundException(request.StoredFileId);
		}

		return new StoredFileDto
		{
			StoredFileId = storedFile.StoredFileId,
			FileName = storedFile.FileName,
			ContentType = storedFile.ContentType,
			Content = storedFile.Content
		};
	}
}
