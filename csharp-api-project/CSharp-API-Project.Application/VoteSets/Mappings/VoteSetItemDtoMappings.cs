using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.VoteSets.Mappings;

public class VoteItemDtoMappings : Profile
    {
	public VoteItemDtoMappings()
	{
		CreateMap<VoteItem, VoteSetItemDto>()
			.ForMember(dest => dest.VoteItemId, opt => opt.MapFrom(src => src.VoteItemId))
			.ForMember(dest => dest.VoteSetId, opt => opt.MapFrom(src => src.VoteSetId))
			.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
			.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
			.ForMember(dest => dest.VoteItemStatusReason, opt => opt.MapFrom(src => src.VoteItemStatusReason))
			.ForMember(dest => dest.VoteItemData, opt => opt.MapFrom(src => src.VoteItemData))
			.ForMember(dest => dest.VisibilityConfigurationId, opt => opt.MapFrom(src => src.VisibilityConfigurationId))
		;

		// Add the reverse mapping
		CreateMap<VoteSetItemDto, VoteItem>()
		.ForMember(dest => dest.VoteItemId, opt => opt.MapFrom(src => src.VoteItemId))
			.ForMember(dest => dest.VoteSetId, opt => opt.MapFrom(src => src.VoteSetId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
			.ForMember(dest => dest.VoteItemStatusReason, opt => opt.MapFrom(src => src.VoteItemStatusReason))
			.ForMember(dest => dest.VoteItemData, opt => opt.MapFrom(src => src.VoteItemData))
			.ForMember(dest => dest.VisibilityConfigurationId, opt => opt.MapFrom(src => src.VisibilityConfigurationId))
		;
	}
}
