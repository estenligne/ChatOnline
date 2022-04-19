using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Global.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebAPI.Services;

namespace WebAPI.Models
{
    public class ApplicationDbData
    {
        public List<UserProfile> userProfiles;
        public List<ChatRoom> chatRooms;
        public List<UserChatRoom> userChatRooms;
        public List<MessageTag> messageTags;
        public List<MessageSent> messagesSent;
        public List<MessageReceived> messagesReceived;

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

        public static async Task<ApplicationDbData> GetAll(ApplicationDbContext dbc)
        {
            var data = new ApplicationDbData();
            data.userProfiles = await dbc.UserProfiles.ToListAsync();
            data.chatRooms = await dbc.ChatRooms.Include(x => x.GroupProfile).ToListAsync();
            data.userChatRooms = await dbc.UserChatRooms.ToListAsync();
            data.messageTags = await dbc.MessageTags.ToListAsync();
            data.messagesSent = await dbc.MessagesSent.ToListAsync();
            data.messagesReceived = await dbc.MessagesReceived.ToListAsync();
            return data;
        }

        public void ClearAllIds()
        {
            foreach (var chatRoom in chatRooms)
            {
                chatRoom.Id = 0;
                chatRoom.CreatorId = 0;

                var group = chatRoom.GroupProfile;
                if (group != null)
                {
                    group.ChatRoomId = 0;
                    group.PhotoFileId = null;
                    group.WallpaperFileId = null;
                }
            }

            foreach (var userChatRoom in userChatRooms)
            {
                userChatRoom.Id = 0;
                userChatRoom.UserProfileId = 0;
                userChatRoom.ChatRoomId = 0;
                userChatRoom.AdderId = null;
                userChatRoom.BlockerId = null;

                if (userChatRoom.ChatRoom.Creator == null)
                    userChatRoom.ChatRoom.Creator = userChatRoom.UserProfile;
            }

            foreach (var messageTag in messageTags)
            {
                messageTag.Id = 0;
                messageTag.ChatRoomId = 0;
                messageTag.ParentId = null;
                messageTag.CreatorId = null;

                if (messageTag.Name == null)
                    messageTag.Name = "";
            }

            foreach (var messageSent in messagesSent)
            {
                messageSent.Id = 0;
                messageSent.SenderId = 0;
                messageSent.MessageTagId = 0;
                messageSent.LinkedId = null;
                messageSent.AuthorId = null;
                messageSent.FileId = null;
            }

            foreach (var messageReceived in messagesReceived)
            {
                messageReceived.Id = 0;
                messageReceived.ReceiverId = 0;
                messageReceived.MessageSentId = 0;
            }
        }

        /// <summary>
        /// Add the data to the database.
        /// </summary>
        public static async Task AddData(
            ApplicationDbContext dbc,
            ILogger logger,
            IConfiguration configuration,
            ApplicationDbData data)
        {
            using (var httpClient = await HTTPClient.GetAuthenticated(configuration))
            {
                foreach (var userProfile in data.userProfiles)
                {
                    await SetupUser(dbc, logger, httpClient, userProfile);
                }
            }
            data.ClearAllIds();

            dbc.UserChatRooms.AddRange(data.userChatRooms);
            dbc.MessagesSent.AddRange(data.messagesSent);
            dbc.MessagesReceived.AddRange(data.messagesReceived);

            await dbc.SaveChangesAsync();
        }

        private static async Task SetupUser(
            ApplicationDbContext dbc,
            ILogger logger,
            HttpClient httpClient,
            UserProfile userProfile)
        {
            try
            {
                if (userProfile.Id == 0)
                {
                    Trace.Assert(JsonConvert.DeserializeObject<ApplicationUserDTO>(userProfile.Name).Password != null);
                    var stringContent = new StringContent(userProfile.Name, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("/api/Account/Register", stringContent);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var userDto = JsonConvert.DeserializeObject<ApplicationUserDTO>(content);
                        userProfile.Id = userDto.Id;
                        userProfile.Name = userDto.Email ?? userDto.PhoneNumber;
                    }
                    else logger.LogError(content);
                }

                if (userProfile.Id != 0)
                {
                    UserProfile user = dbc.UserProfiles.Find(userProfile.Id);
                    if (user != null)
                    {
                        CopyProfile(userProfile, user);
                        dbc.Entry(user).State = EntityState.Detached;
                        dbc.Entry(userProfile).State = EntityState.Unchanged;
                    }
                    else dbc.UserProfiles.Add(userProfile);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null);
            }
        }

        private static void CopyProfile(UserProfile userProfile, UserProfile user)
        {
            userProfile.Name = user.Name;
            userProfile.About = user.About;
            userProfile.PhotoFileId = user.PhotoFileId;
            userProfile.WallpaperFileId = user.WallpaperFileId;
            userProfile.DateCreated = user.DateCreated;
            userProfile.DateDeleted = user.DateDeleted;
            userProfile.LastConnected = user.LastConnected;
            userProfile.Availability = user.Availability;
        }
    }
}
