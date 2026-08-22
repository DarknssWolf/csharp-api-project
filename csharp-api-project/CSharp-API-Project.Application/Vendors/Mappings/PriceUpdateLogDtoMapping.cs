using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Vendors.Mappings
{
	public class PriceUpdateLogDtoMapping: Profile
	{
		public PriceUpdateLogDtoMapping()
		{
			CreateMap<PriceChangeLog, PriceUpdateLogDto>()
				.ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
				.ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku))
				.ForMember(dest => dest.PricelistCode, opt => opt.MapFrom(src => src.PricelistCode))
				.ForMember(dest => dest.QtyBreak, opt => opt.MapFrom(src => src.QtyBreak))
				.ForMember(dest => dest.Operation, opt => opt.MapFrom(src => src.Operation.ToString()))
				.ForMember(dest => dest.Patch, opt => opt.MapFrom(src => src.Patch))
				.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
				;
		}
	}
}
