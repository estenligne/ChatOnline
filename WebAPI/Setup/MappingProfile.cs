using AutoMapper;
using WebAPI.Models;
using Global.Models;

namespace WebAPI.Setup
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<File, FileDTO>().ReverseMap();
            CreateMap<User, UserProfileDTO>().ReverseMap();
            CreateMap<Group, GroupProfileDTO>().ReverseMap();
            CreateMap<Room, ChatRoomDTO>();
            CreateMap<UserRoom, UserChatRoomDTO>();
            CreateMap<MessageTag, MessageTagDTO>().ReverseMap();
            CreateMap<MessageSent, MessageSentDTO>().ReverseMap();
            CreateMap<UserDevice, DeviceUsedDTO>();
        }
    }
}
