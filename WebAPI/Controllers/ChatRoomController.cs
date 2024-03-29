﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Global.Enums;
using Global.Helpers;
using Global.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    public class ChatRoomController : BaseController<ChatRoomController>
    {
        private IConfiguration _configuration;

        public ChatRoomController(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<ChatRoomController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _configuration = configuration;
        }

        private async Task<ActionResult> GetUserOrInfo(long charRoomId, long userChatRoomId, bool getInfo)
        {
            try
            {
                UserChatRoom userChatRoom;

                if (userChatRoomId == 0)
                {
                    userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile)
                                            .Include(x => x.ChatRoom.GroupProfile.PhotoFile)
                                            .Where(x => x.UserProfileId == UserId && x.ChatRoomId == charRoomId)
                                            .FirstOrDefaultAsync();
                }
                else
                {
                    userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile)
                                            .Include(x => x.ChatRoom.GroupProfile.PhotoFile)
                                            .FirstOrDefaultAsync(x => x.Id == userChatRoomId);
                }

                if (userChatRoom == null)
                    return NotFound("User chat room not found.");

                if (userChatRoom.UserProfileId != UserId)
                    return Forbid("Not associated to this user chat room!");

                if (getInfo)
                    return Ok(await GetChatRoomInfo(userChatRoom));
                else
                    return Ok(_mapper.Map<UserChatRoomDTO>(userChatRoom));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType(typeof(UserChatRoom), (int)HttpStatusCode.OK)]
        [HttpGet]
        [Route(nameof(GetUser))]
        public Task<ActionResult> GetUser(long id, long userChatRoomId)
        {
            return GetUserOrInfo(id, userChatRoomId, false);
        }

        [ProducesResponseType(typeof(ChatRoomInfo), (int)HttpStatusCode.OK)]
        [HttpGet]
        [Route(nameof(GetInfo))]
        public Task<ActionResult> GetInfo(long id, long userChatRoomId)
        {
            return GetUserOrInfo(id, userChatRoomId, true);
        }

        private async Task<ChatRoomInfo> GetChatRoomInfo(UserChatRoom userChatRoom)
        {
            var chatRoomInfo = new ChatRoomInfo();
            chatRoomInfo.Id = userChatRoom.ChatRoom.Id;
            chatRoomInfo.Type = userChatRoom.ChatRoom.Type;
            chatRoomInfo.UserChatRoomId = userChatRoom.Id;
            chatRoomInfo.UserBlocked = userChatRoom.DateBlocked != null;
            chatRoomInfo.UserExited = userChatRoom.DateExited != null;
            chatRoomInfo.UserMuted = userChatRoom.DateMuted != null;
            chatRoomInfo.UserPinned = userChatRoom.DatePinned != null;

            if (userChatRoom.ChatRoom.GroupProfile == null)
            {
                var otherUser = await dbc.UserChatRooms
                                        .Include(x => x.UserProfile.PhotoFile)
                                        .Where(x => x.ChatRoomId == userChatRoom.ChatRoomId
                                            && x.UserProfileId != userChatRoom.UserProfileId)
                                        .Select(x => x.UserProfile)
                                        .FirstAsync();

                chatRoomInfo.Name = otherUser.Name;
                chatRoomInfo.PhotoFileName = otherUser.PhotoFile?.Name;
                chatRoomInfo.ProfileId = otherUser.Id;
            }
            else
            {
                chatRoomInfo.Name = userChatRoom.ChatRoom.GroupProfile.GroupName;
                chatRoomInfo.PhotoFileName = userChatRoom.ChatRoom.GroupProfile.PhotoFile?.Name;
            }

            var latestMessage = await dbc.MessagesSent
                .Where(m => m.Sender.ChatRoomId == userChatRoom.ChatRoomId)
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            if (latestMessage != null)
            {
                var lm = chatRoomInfo.LatestMessage;

                lm.Id = latestMessage.Id;
                lm.SenderId = latestMessage.SenderId;
                lm.DateSent = latestMessage.DateSent;
                lm.MessageType = latestMessage.MessageType;

                lm.ShortBody = BasicHelpers.GetShortBody(latestMessage.Body, 100);

                int totalMembers = dbc.UserChatRooms.Where(x => x.ChatRoomId == userChatRoom.ChatRoomId).Count();
                int receivedCount = dbc.MessagesReceived.Where(x => x.MessageSentId == latestMessage.Id).Count();
                int readCount = dbc.MessagesReceived.Where(x => x.MessageSentId == latestMessage.Id && x.DateRead != null).Count();

                // Note: subtract 1 because the sender does not receive it's own message sent.
                lm.NotReceivedCount = totalMembers - 1 - receivedCount;
                lm.NotReadCount = totalMembers - 1 - readCount;
            }
            return chatRoomInfo;
        }

        /// <summary>
        /// Get the ChatRoomInfo of all chat rooms which signed-in user is associated to.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<ActionResult<List<ChatRoomInfo>>> GetAll()
        {
            try
            {
                var userChatRooms = await dbc.UserChatRooms
                    .Include(x => x.ChatRoom.GroupProfile.PhotoFile)
                    .Where(x =>
                        x.UserProfileId == UserId &&
                        x.DateDeleted == null &&
                        x.ChatRoom.DateDeleted == null)
                    .ToListAsync();

                var chatRoomsInfos = new List<ChatRoomInfo>();
                foreach (var userChatRoom in userChatRooms)
                {
                    try
                    {
                        chatRoomsInfos.Add(await GetChatRoomInfo(userChatRoom));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, null);
                    }
                }
                return chatRoomsInfos;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a private chat room between the signed-in user and the given user.
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        [Route(nameof(CreatePrivate))]
        public async Task<ActionResult<UserChatRoomDTO>> CreatePrivate(string accountID)
        {
            try
            {
                long userId = 0;
                using (var httpClient = await HTTPClient.GetAuthenticated(_configuration))
                {
                    string userName = HttpUtility.UrlEncode(accountID);
                    string url = "/api/Account/GetUser?userName=" + userName;

                    var response = await httpClient.GetAsync(url);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var userDto = JsonConvert.DeserializeObject<ApplicationUserDTO>(content);
                        userId = userDto.Id;
                    }
                    else return StatusCode((int)response.StatusCode, content);
                }

                UserProfile user1 = dbc.UserProfiles.Find(UserId);

                if (user1 == null)
                    return NotFound("User profile not found.");

                UserProfile user2 = dbc.UserProfiles.Find(userId);

                if (user2 == null)
                    return NotFound("Given user was not found.");

                if (user1.Id == user2.Id)
                    return Forbid("Cannot create a chat room with self!");

                UserChatRoom user1ChatRoom;
                UserChatRoom user2ChatRoom;

                var user2ChatRooms = await dbc.UserChatRooms
                                            .Where(x => x.UserProfileId == user2.Id &&
                                                (x.ChatRoom.Type & ChatRoomTypeEnum.Private) != 0)
                                            .ToListAsync();

                var user2ChatRoomsIds = user2ChatRooms.Select(x => x.ChatRoomId);

                var user1ChatRooms = await dbc.UserChatRooms
                                            .Include(x => x.ChatRoom)
                                            .Where(x => x.UserProfileId == user1.Id &&
                                                user2ChatRoomsIds.Contains(x.ChatRoomId))
                                            .ToListAsync();

                if (user1ChatRooms.Count == 0)
                {
                    var dateCreated = DateTimeOffset.UtcNow;

                    var chatRoom = new ChatRoom
                    {
                        Type = ChatRoomTypeEnum.Private,
                        CreatorId = user1.Id,
                        DateCreated = dateCreated,
                    };

                    user1ChatRoom = new UserChatRoom
                    {
                        UserProfileId = user1.Id,
                        ChatRoom = chatRoom,
                        UserRole = UserRoleEnum.FullNode,
                        DateAdded = dateCreated,
                    };
                    dbc.UserChatRooms.Add(user1ChatRoom);

                    user2ChatRoom = new UserChatRoom
                    {
                        UserProfileId = user2.Id,
                        ChatRoom = chatRoom,
                        UserRole = UserRoleEnum.FullNode,
                        DateAdded = dateCreated,
                    };
                    dbc.UserChatRooms.Add(user2ChatRoom);
                }
                else
                {
                    if (user1ChatRooms.Count > 1)
                        _logger.LogError($"UserProfiles {user1.Id} and {user2.Id} have more than one private chat room.");

                    user1ChatRoom = user1ChatRooms.First();
                    user2ChatRoom = user2ChatRooms.First(x => x.ChatRoomId == user1ChatRoom.ChatRoomId);

                    user1ChatRoom.UserRole |= UserRoleEnum.FullNode;
                    user1ChatRoom.DateBlocked = null;
                    user1ChatRoom.DateDeleted = null;
                    user1ChatRoom.DateExited = null;

                    user2ChatRoom.UserRole |= UserRoleEnum.FullNode;
                    user2ChatRoom.DateDeleted = null; // allow user to unblock if blocked
                    user2ChatRoom.DateExited = null;
                    if (user2ChatRoom.DateBlocked != null)
                        return Forbid("User blocked this chat room.");
                }
                await dbc.SaveChangesAsync();

                var userChatRoomDto = _mapper.Map<UserChatRoomDTO>(user1ChatRoom);
                return CreatedAtAction(nameof(GetUser), new { userChatRoomId = userChatRoomDto.Id }, userChatRoomDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
