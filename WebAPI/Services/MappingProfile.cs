﻿using AutoMapper;
using WebAPI.Models;
using Global.Models;

namespace WebAPI.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FileModel, FileDTO>().ReverseMap();
            CreateMap<UserProfile, UserProfileDTO>().ReverseMap();
            CreateMap<GroupProfile, GroupProfileDTO>().ReverseMap();
            CreateMap<ChatRoom, ChatRoomDTO>();
            CreateMap<UserChatRoom, UserChatRoomDTO>();
            CreateMap<MessageTag, MessageTagDTO>().ReverseMap();
            CreateMap<MessageSent, MessageSentDTO>().ReverseMap();
            CreateMap<DeviceUsed, DeviceUsedDTO>();
        }
    }
}
