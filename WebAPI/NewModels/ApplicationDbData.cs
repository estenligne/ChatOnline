using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Global.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebAPI.Models
{
    public class ApplicationDbData
    {
        public List<NewModels.UserProfile> userProfiles = new();
        public List<NewModels.ChatRoom> chatRooms = new();
        public List<NewModels.UserChatRoom> userChatRooms = new();
        public List<NewModels.MessageTag> messageTags = new();
        public List<NewModels.MessageSent> messagesSent = new();
        public List<NewModels.MessageReceived> messagesReceived = new();

        private static readonly JsonSerializerSettings settings = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public string ToJSON()
        {
            settings.Formatting = Formatting.Indented;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            return JsonConvert.SerializeObject(this, settings);
        }

        public static ApplicationDbData FromJSON(string json)
        {
            return JsonConvert.DeserializeObject<ApplicationDbData>(json, settings);
        }

        public static async Task<ApplicationDbData> GetAll(ApplicationDbContext dbc, AccountDbContext accountDbc, IMapper mapper, ILogger logger)
        {
            var old_userProfiles = await dbc.UserProfiles.ToListAsync();
            var old_chatRooms = await dbc.ChatRooms.Include(x => x.GroupProfile).ToListAsync();
            var old_userChatRooms = await dbc.UserChatRooms.ToListAsync();
            var old_messageTags = await dbc.MessageTags.ToListAsync();
            var old_messagesSent = await dbc.MessagesSent.ToListAsync();
            var old_messagesReceived = await dbc.MessagesReceived.ToListAsync();

            var data = new ApplicationDbData();

            foreach (UserProfile old in old_userProfiles)
            {
                var new_userProfile = new NewModels.UserProfile
                {
                    OriginalId = old.Id,
                    Name = old.Username,
                    About = old.About,
                    DateCreated = old.DateCreated
                };
                data.userProfiles.Add(new_userProfile);

                ApplicationUser user = accountDbc.Users
                    .Where(u => u.UserName == old.Identity)
                    .FirstOrDefault();

                if (user == null)
                {
                    logger.LogError($"How comes user {old.Id} {old.Identity} is not found?!");
                }
                else new_userProfile.Id = user.Id;
            }

            foreach (ChatRoom old in old_chatRooms)
            {
                var new_chatRoom = new NewModels.ChatRoom
                {
                    Id = old.Id,
                    Type = old.Type,
                    DateCreated = old.DateCreated,
                };

                if (new_chatRoom.Type == ChatRoomTypeEnum.Group)
                {
                    new_chatRoom.Creator = data.userProfiles.First(u => u.OriginalId == old.GroupProfile.CreatorId);

                    new_chatRoom.GroupProfile = new NewModels.GroupProfile
                    {
                        GroupName = old.GroupProfile.GroupName,
                        JoinToken = old.GroupProfile.JoinToken,
                        About = old.GroupProfile.About,
                    };
                }
                data.chatRooms.Add(new_chatRoom);
            }

            foreach (UserChatRoom old in old_userChatRooms)
            {
                var new_userchatRoom = new NewModels.UserChatRoom();
                new_userchatRoom.Id = old.Id;

                new_userchatRoom.UserProfile = data.userProfiles.First(u => u.OriginalId == old.UserProfileId);

                new_userchatRoom.ChatRoom = data.chatRooms.First(x => x.Id == old.ChatRoomId);

                new_userchatRoom.UserRole = old.UserRole;

                if (old.AdderId != null)
                    new_userchatRoom.Adder = data.userProfiles.First(x => x.OriginalId == old.AdderId);

                if (old.BlockerId != null)
                    new_userchatRoom.Blocker = data.userProfiles.First(x => x.OriginalId == old.BlockerId);

                new_userchatRoom.DateAdded = old.DateAdded;
                new_userchatRoom.DateBlocked = old.DateBlocked;

                new_userchatRoom.DateDeleted = old.DateDeleted;
                new_userchatRoom.DateExited = old.DateExited;
                new_userchatRoom.DateMuted = old.DateMuted;
                new_userchatRoom.DatePinned = old.DatePinned;

                data.userChatRooms.Add(new_userchatRoom);
            }

            foreach (MessageTag old in old_messageTags)
            {
                var new_messageTag = new NewModels.MessageTag
                {
                    Id = old.Id,
                    Name = old.Name,

                    ChatRoom = data.chatRooms.First(x => x.Id == old.ChatRoomId),

                    //Creator = data.userChatRooms.First(u => u.Id == old.CreatorId),

                    DateCreated = old.DateCreated,
                };
                data.messageTags.Add(new_messageTag);
            }

            foreach (MessageSent old in old_messagesSent)
            {
                var new_messageSent = new NewModels.MessageSent
                {
                    Id = old.Id,
                    Body = old.Body,
                    MessageType = old.MessageType,

                    Sender = data.userChatRooms.First(x => x.Id == old.SenderId),
                    MessageTag = data.messageTags.First(x => x.Id == old.MessageTagId),

                    Linked = data.messagesSent.FirstOrDefault(x => x.Id == old.LinkedId),

                    DateSent = old.DateSent,
                    DateDeleted = old.DateDeleted,
                    DateCreated = old.DateCreated,
                    DateStarred = old.DateStarred,
                };
                data.messagesSent.Add(new_messageSent);

                if (new_messageSent.Linked == null && old.LinkedId != null)
                {
                    logger.LogError($"LinkedId == null for message {old.Id}");
                }
            }


            foreach (MessageReceived old in old_messagesReceived)
            {
                var new_messageReceived = new NewModels.MessageReceived
                {
                    Id = old.Id,
                    Receiver = data.userChatRooms.First(x => x.Id == old.ReceiverId),
                    MessageSent = data.messagesSent.First(x => x.Id == old.MessageSentId),

                    DateCreated = old.DateCreated,
                    DateReceived = old.DateReceived,
                    DateRead = old.DateRead,
                    DateDeleted = old.DateDeleted,
                    DateStarred = old.DateStarred,
                    Reaction = old.Reaction,
                };
                data.messagesReceived.Add(new_messageReceived);
            }

            return data;
        }
    }
}
