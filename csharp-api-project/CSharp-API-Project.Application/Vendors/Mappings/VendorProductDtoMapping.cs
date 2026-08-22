using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSharpAPIProject.Application.Vendors.Mappings
{
	public class VendorProductDtoMapping: Profile
	{
		public VendorProductDtoMapping()
		{
			CreateMap<Product, VendorProductDto>()
				.ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier.Code))
				.ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku))
				.ForMember(dest => dest.CategoryCode, opt => opt.MapFrom(src => src.CategoryCode))
				.ForMember(dest => dest.CategoryPath, opt => opt.MapFrom(src => src.Category.Path))
				.ForMember(dest => dest.ProductModel, opt => opt.MapFrom(src =>  JsonSerializer.Serialize( src.Model,new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, 
						WriteIndented = true })));
		}
	}
}
