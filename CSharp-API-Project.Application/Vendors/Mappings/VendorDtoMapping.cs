using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Vendors.Mappings
{
	public class VendorDtoMapping: Profile
	{
		public VendorDtoMapping()
		{
			CreateMap<Vendor, VendorDto>()
                .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.VendorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
				.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
				.ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
				.ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.ContactName))
				.ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
            ;
				
		}
	}
}
