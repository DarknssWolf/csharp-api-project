using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.VoteSets.Mappings;

public class UserGroupDtoMappings : Profile
{
	public UserGroupDtoMappings()
	{
		CreateMap<VoteSet, VoteSetDto>()
			.ForMember(dest => dest.VoteSetId, opt => opt.MapFrom(src => src.VoteSetId))
			.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
			.ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
			.ForMember(dest => dest.StartOn, opt => opt.MapFrom(src => src.StartOn))
			.ForMember(dest => dest.EndOn, opt => opt.MapFrom(src => src.EndOn))
			.ForMember(dest => dest.CanEarnPoints, opt => opt.MapFrom(src => src.CanEarnPoints))
			.ForMember(dest => dest.AllowMultipleTries, opt => opt.MapFrom(src => src.AllowMultipleTries))
			.ForMember(dest => dest.AllowMultipleItems, opt => opt.MapFrom(src => src.AllowMultipleItems))
			.ForMember(dest => dest.EmailNotification, opt => opt.MapFrom(src => src.EmailNotification))
			.ForMember(dest => dest.VoteSetData, opt => opt.MapFrom(src => src.VoteSetData))
			.ForMember(dest => dest.WhoCanCreateVoteItemFlag, opt => opt.MapFrom(src => src.WhoCanCreateVoteItemFlag))
			.ForMember(dest => dest.VoteItems, opt => opt.MapFrom(src => src.VoteItems ?? new List<VoteItem>()))
			.ForMember(dest => dest.VisibilityConfigurationId, opt => opt.MapFrom(src => src.VisibilityConfigurationId))
		;

		// Mapping for the SurveyQuestions property
		CreateMap<VoteItem, VoteSetItemDto>();

		CreateMap<VoteSetDto, VoteSet>()
			.ForMember(dest => dest.VoteSetId, opt => opt.MapFrom(src => src.VoteSetId))
			.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
			.ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
			.ForMember(dest => dest.StartOn, opt => opt.MapFrom(src => src.StartOn))
			.ForMember(dest => dest.EndOn, opt => opt.MapFrom(src => src.EndOn))
			.ForMember(dest => dest.CanEarnPoints, opt => opt.MapFrom(src => src.CanEarnPoints))
			.ForMember(dest => dest.AllowMultipleTries, opt => opt.MapFrom(src => src.AllowMultipleTries))
			.ForMember(dest => dest.AllowMultipleItems, opt => opt.MapFrom(src => src.AllowMultipleItems))
			.ForMember(dest => dest.EmailNotification, opt => opt.MapFrom(src => src.EmailNotification))
			.ForMember(dest => dest.VoteSetData, opt => opt.MapFrom(src => src.VoteSetData))
			.ForMember(dest => dest.WhoCanCreateVoteItemFlag, opt => opt.MapFrom(src => src.WhoCanCreateVoteItemFlag))
			.ForMember(dest => dest.VoteItems, opt => opt.MapFrom(src => src.VoteItems))
			.ForMember(dest => dest.VisibilityConfigurationId, opt => opt.MapFrom(src => src.VisibilityConfigurationId))
		;

		// Mapping for the SurveyQuestions property
		CreateMap<VoteSetItemDto, VoteItem>();
	}
}
