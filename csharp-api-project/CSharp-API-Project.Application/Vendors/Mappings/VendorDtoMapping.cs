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
				.ForMember(dest => dest.FinanceEmail, opt => opt.MapFrom(src => src.FinanceEmail))
				.ForMember(dest => dest.AccountManager, opt => opt.MapFrom(src => src.AccountManager))
                .ForMember(dest => dest.ParticipantPaysHandlingFee, opt => opt.MapFrom(src => src.PariticpantPaysHandlingFee))
                .ForMember(dest => dest.SplitFees, opt => opt.MapFrom(src => src.SplitFees))
                .ForMember(dest => dest.DecimalRounding, opt => opt.MapFrom(src => src.DecimalRounding))
                .ForMember(dest => dest.WebhookUrl, opt => opt.MapFrom(src => src.WebhookUrl))
                .ForMember(dest => dest.WebhookSecret, opt => opt.MapFrom(src => src.WebhookSecret))
                .ForMember(dest => dest.VendorDomainUrl, opt => opt.MapFrom(src => src.DomainUrl))
            ;
				
		}
	}
}
