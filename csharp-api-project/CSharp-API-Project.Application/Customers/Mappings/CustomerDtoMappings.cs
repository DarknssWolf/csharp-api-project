using CSharpAPIProject.Application.Customers.Models;

namespace CSharpAPIProject.Application.Customers.Mappings
{
	public class CustomerDtoMappings : Profile
	{
		public CustomerDtoMappings()
		{
			CreateMap<Domain.Entities.Customer, CustomerDto>()
				.ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
				.ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
				.ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
				.ForMember(dest => dest.IDNumber, opt => opt.MapFrom(src => src.IDNumber))
				.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
				.ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
				.ForMember(dest => dest.CustomerData, opt => opt.MapFrom(src => src.CustomerData))
                .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.VendorRegionId))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.PointsLedger.OrderByDescending(x => x.TransactionDate).FirstOrDefault().Balance))
                ;
        }
    }

}