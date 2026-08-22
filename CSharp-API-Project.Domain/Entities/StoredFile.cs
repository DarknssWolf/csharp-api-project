using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;

public class StoredFile : BaseAuditableEntity
{
	// PK
	public Guid StoredFileId { get; set; }
	public int VendorId { get; set; }

	// Properties
	public string FileName { get; set; }
	public string ContentType { get; set; }
	public long Length { get; set; }
	public byte[] Content { get; set; }

	// Navigation Properties
	public Vendor Vendor { get; set; }
}
