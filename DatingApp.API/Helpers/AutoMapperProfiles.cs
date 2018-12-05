using System;
using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Helpers;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.Age, 
                           opt => opt.ResolveUsing(d=> d.DateOfBirth.CalculateAge()))
                .ForMember(dest => dest.PhotoUrl,
                           opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p=> p.IsMain).Url));
            CreateMap<User, UserForDetailedDto>()
                .ForMember(dest => dest.Age, 
                           opt => opt.ResolveUsing(d=> d.DateOfBirth.CalculateAge()))
                .ForMember(dest => dest.PhotoUrl,
                           opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p=> p.IsMain).Url));          
            CreateMap<UserForRegisterDto, User>();
            CreateMap<UserForUpdateDto, User>();

            CreateMap<Photo, PhotoDto>();            
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            
        }
    }
}