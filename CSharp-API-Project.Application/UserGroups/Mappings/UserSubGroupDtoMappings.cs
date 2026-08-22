using CSharpAPIProject.Application.UserGroups.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.UserGroups.Mappings;

public class UserSubGroupDtoMappings : Profile
{
	public UserSubGroupDtoMappings()
	{
        CreateMap<UserSubGroup, UserSubGroupDto>()
            .ForMember(dest => dest.SubGroupId, opt => opt.MapFrom(src => src.SubGroupId))
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber));

	}
}
