using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAPIProject.Application.Vendors.Models
{
	public class PriceUpdateLogDto
	{
		public string SupplierCode {get; set;}
		public string PricelistCode {get; set;}
		public string Sku {get; set;}
		public int QtyBreak {get;set;}
		public string Operation {get;set;}
		public DateTime UpdatedAt {get;set;}
		public string Patch {get;set;}
	}
}
