using AutoMapper;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Dto.PollResultDtos;
using PollSystemApp.Application.Common.Dto.UserDtos;
using PollSystemApp.Application.Common.Dto.VoteDtos;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Domain.Users;
using System.Linq;

namespace PollSystemApp.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<OptionForCreationDto, Option>();
            CreateMap<OptionForUpdateDto, Option>();
            CreateMap<Option, OptionDto>();
           

            CreateMap<PollForCreationDto, Poll>()
                .ForMember(dest => dest.Tags, opt => opt.Ignore());
            CreateMap<PollForUpdateDto, Poll>();
            
            CreateMap<Poll, PollDto>()
                .ForMember(dest => dest.Options, opt => opt.Ignore()) 
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null ? src.Tags.Select(t => t.Name).ToList() : new List<string>()));


            CreateMap<Vote, VoteDto>();

            
            CreateMap<UserForRegistrationDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));


            CreateMap<User, UserDto>();              
                                                                                             

            CreateMap<OptionVoteSummary, OptionVoteSummaryDto>()
                .ForMember(dest => dest.OptionText, opt => opt.Ignore()); 

            CreateMap<PollResult, PollResultDto>()
                .ForMember(dest => dest.PollTitle, opt => opt.Ignore()) 
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        }
    }
}