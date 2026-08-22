using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Vendors.Mappings;
public class VendorStoreDataMapping :Profile
{
    /*
     Adding new mappings?
    Remember to set the default values for vendor 0 in the `GetMyVendorStoreDataQuery`.
     */

    public VendorStoreDataMapping()
    {
        CreateMap<Vendor, VendorStoreDataDto>()
            .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.DecimalRounding, opt => opt.MapFrom(src => src.DecimalRounding))
        ;

        CreateMap<VendorSettings, VendorStoreDataDto>()
            .ForMember(dest => dest.ClaimPeriodLength, opt => opt.MapFrom(src => src.ClaimPeriodDays))
            .ForMember(dest => dest.PointsName, opt => opt.MapFrom(src => src.PointsName))
            .ForMember(dest => dest.PointsAbbreviation, opt => opt.MapFrom(src => src.PointsAbbreviation))
        ;
    }
}
