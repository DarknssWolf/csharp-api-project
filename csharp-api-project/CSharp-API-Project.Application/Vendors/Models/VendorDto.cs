using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.Vendors.Models;

public class VendorDto
{
    public int VendorId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string WebhookUrl { get; set; }
    public string WebhookSecret { get; set; }
    public string Code { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
    public string FinanceEmail { get; set; }
    public Guid AccountManager { get; set; }
    public bool ParticipantPaysHandlingFee { get; set; }
    public bool SplitFees { get; set; }
    public int DecimalRounding { get; set; }
    public string VendorDomainUrl { get; set; }
}
