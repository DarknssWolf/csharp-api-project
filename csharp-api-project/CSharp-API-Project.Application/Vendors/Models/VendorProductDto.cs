using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAPIProject.Application.Vendors.Models
{
	public class VendorProductDto
	{
		public string SupplierCode {get;set;}
		public int? CategoryCode {get;set;}
		public string CategoryPath {get;set;}
		public string Sku {get;set;}
		public string ProductModel {get;set;}
	}
}
