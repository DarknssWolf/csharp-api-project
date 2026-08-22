using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;

public class Vendor : BaseAuditableEntity
{
	// PK
	public int VendorId { get; set; }
	
	// Properties
	public string Name { get; set; } = "";	
	public string Code { get; set; } = "";	
	public string Email { get; set; } = "";
	
	public string WebhookUrl { get; set; } = "";
	public string WebhookSecret { get; set; } = "";
	
	public string ContactName { get; set; } = "";
	public string ContactEmail { get; set; } = "";
	public string FinanceEmail { get; set; } = "";

    // Navigation
    public ICollection<VendorLanguage> Languages { get; set; } = [];

}


