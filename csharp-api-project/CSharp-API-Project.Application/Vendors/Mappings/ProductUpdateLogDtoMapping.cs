using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Vendors.Mappings
{
	public class ProductUpdateLogDtoMapping: Profile
	{
		public ProductUpdateLogDtoMapping()
		{
			CreateMap<ProductChangeLog, ProductUpdateLogDto>()
				.ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
				.ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku))
				.ForMember(dest => dest.Operation, opt => opt.MapFrom(src => src.Operation.ToString()))
				.ForMember(dest => dest.Patch, opt => opt.MapFrom(src => src.Patch))
				.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
				;
		}
	}
}
