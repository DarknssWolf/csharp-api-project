using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;
public class VendorLanguage : BaseAuditableEntity
{
    // PK
    public Guid VendorLanguageId { get; set; }

    // FK
    public int VendorId { get; set; }

    // Properties
    public string Name { get; set; }
    public string ISOCode { get; set; }
    public bool IsDefault { get; set; }

    // Navigation Properties
    public Vendor Vendor { get; set; }
}
