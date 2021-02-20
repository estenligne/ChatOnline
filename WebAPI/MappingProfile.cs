using AutoMapper;
using WebAPI.Models;
using Global.Models;

namespace WebAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserDTO>().ReverseMap();
            CreateMap<File, FileDTO>().ReverseMap();
            CreateMap<UserProfile, UserProfileDTO>().ReverseMap();
            CreateMap<GroupProfile, GroupProfileDTO>().ReverseMap();
        }
    }
}
