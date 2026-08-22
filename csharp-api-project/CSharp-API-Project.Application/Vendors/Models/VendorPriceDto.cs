namespace CSharpAPIProject.Application.Vendors.Models
{
	public class VendorPriceDto
	{
		public string SupplierCode {get;set;}
		public string Sku {get;set;}
		public string? PricelistCode {get;set;}
		public int QtyBreak {get;set;}
		public decimal Amount {get;set; }
	}
}
