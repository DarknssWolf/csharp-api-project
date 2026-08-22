using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.ValueObjects;

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
	public Currency BaseCurrency { get; set; }

	// Navigation
	public Vendor Vendor { get; set; }
}
