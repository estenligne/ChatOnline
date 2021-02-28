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
            CreateMap<File, FileDTO>();
            CreateMap<UserProfile, UserProfileDTO>().ReverseMap();
            CreateMap<GroupProfile, GroupProfileDTO>().ReverseMap();
            CreateMap<ChatRoom, ChatRoomDTO>();
            CreateMap<UserChatRoom, UserChatRoomDTO>();
            CreateMap<MessageTag, MessageTagDTO>().ReverseMap();
            CreateMap<MessageSent, MessageSentDTO>().ReverseMap();
        }
    }
}
