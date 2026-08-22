using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.Vendors.Models;

public class VendorDto
{
    public int VendorId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
}
