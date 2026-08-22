namespace CSharpAPIProject.Application.Vendors.Models
{
	public class ProductUpdateLogDto
	{
		public string SupplierCode {get; set;}
		public string Sku {get; set;}
		public string Operation {get;set;}
		public DateTime UpdatedAt {get;set;}
		public string Patch {get;set;}
	}
}
