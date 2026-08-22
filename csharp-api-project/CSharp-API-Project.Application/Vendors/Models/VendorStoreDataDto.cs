using CSharpAPIProject.Application.Models.Settings;

namespace CSharpAPIProject.Application.Vendors.Models;
public class VendorStoreDataDto
{
    public int VendorId { get; set; }
    public string Name { get; set; }
    public decimal DecimalRounding { get; set; }
    public int ClaimPeriodLength { get; set; }
    public string PointsName { get; set; }
    public string PointsAbbreviation { get; set; }
}
