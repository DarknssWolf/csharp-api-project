using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;

public class Region : BaseAuditableEntity
{
	// PK
	public Guid RegionId { get; set; }

	// FK
	public int VendorId { get; set; }

	// Properties
	public string RegionCode { get; set; }
	public string RegionName { get; set; }

	// Navigation
	public Vendor Vendor { get; set; }
}
