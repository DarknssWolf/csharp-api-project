using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.ValueObjects;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities
{
	public class Customer : BaseAuditableEntity
	{
		// PK
		public int CustomerId { get; set; }
		public int VendorId { get; set; }
		public Guid VendorRegionId { get; set; }

		// Properties
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string IDNumber { get; set; } = "";

		// Contact Details
		public string Email { get; set; } = "";
		public string Phone { get; set; } = "";
		public Address Address { get; set; }

		// Customer Data Properties
		public string CustomerData { get; set; } = ""; // This could be used for any additional data that is not covered by the above properties.
        public RecordStatus RecordStatus { get; set; }
        public DateTimeOffset? RecordDeletedDate { get; set; }

        // Navigation
		public Vendor Vendor { get; set; }
		public Region VendorRegion { get; set; }
    }
}
