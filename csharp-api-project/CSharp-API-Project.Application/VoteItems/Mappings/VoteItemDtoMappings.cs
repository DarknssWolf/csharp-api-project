using CSharpAPIProject.Application.VoteItems.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.VoteItems.Mappings;

public class VoteItemDtoMappings : Profile
{
	public VoteItemDtoMappings()
	{
		CreateMap<VoteItem, VoteItemDto>()
			.ForMember(dest => dest.VoteItemId, opt => opt.MapFrom(src => src.VoteItemId))
			.ForMember(dest => dest.VoteSetId, opt => opt.MapFrom(src => src.VoteSetId))
            .ForMember(dest => dest.VoteSetTitle, opt => opt.MapFrom(src => src.VoteSet.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
			.ForMember(dest => dest.VoteItemStatusReason, opt => opt.MapFrom(src => src.VoteItemStatusReason))
			.ForMember(dest => dest.VoteItemData, opt => opt.MapFrom(src => src.VoteItemData))
		;

		CreateMap<VoteItemDto, VoteItem>()
			.ForMember(dest => dest.VoteItemId, opt => opt.MapFrom(src => src.VoteItemId))
			.ForMember(dest => dest.VoteSetId, opt => opt.MapFrom(src => src.VoteSetId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.VoteSetTitle))
			.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
			.ForMember(dest => dest.VoteItemStatusReason, opt => opt.MapFrom(src => src.VoteItemStatusReason))
			.ForMember(dest => dest.VoteItemData, opt => opt.MapFrom(src => src.VoteItemData))
		;
	}
}
