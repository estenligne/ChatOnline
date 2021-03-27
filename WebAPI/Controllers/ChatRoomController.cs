using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebAPI.Models;
using Global.Models;
using Global.Enums;
using AutoMapper;

namespace WebAPI.Controllers
{
    public class ChatRoomController : BaseController<ChatRoomController>
    {
        public ChatRoomController(
            ApplicationDbContext context,
            ILogger<ChatRoomController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        [HttpGet]
        [Route(nameof(GetUserChatRoom))]
        public async Task<ActionResult<UserChatRoomDTO>> GetUserChatRoom(long id)
        {
            try
            {
                var userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile.User)
                                            .Include(x => x.ChatRoom.GroupProfile.PhotoFile)
                                            .FirstOrDefaultAsync(x => x.Id == id);

                if (userChatRoom == null)
                    return NotFound("User chat room not found.");

                if (userChatRoom.UserProfile.User.UserName != User.Identity.Name)
                    return Forbid("Not associated to this user chat room!");

                var userChatRoomDto = _mapper.Map<UserChatRoomDTO>(userChatRoom);
                return userChatRoomDto;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ChatRoomInfo>> GetChatRoomInfo(long userChatRoomId)
        {
            try
            {
                var userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile.User)
                                            .Include(x => x.ChatRoom.GroupProfile.PhotoFile)
                                            .FirstOrDefaultAsync(x => x.Id == userChatRoomId);

                if (userChatRoom == null)
                    return NotFound("User chat room not found.");

                if (userChatRoom.UserProfile.User.UserName != User.Identity.Name)
                    return Forbid("Not associated to this user chat room!");

                return await GetChatRoomInfo(userChatRoom);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

                chatRoomInfo.Name = otherUser.Username;
                chatRoomInfo.PhotoFileName = otherUser.PhotoFile?.Name;
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
                chatRoomInfo.LatestMessage.Id = latestMessage.Id;
                chatRoomInfo.LatestMessage.SenderId = latestMessage.SenderId;
                chatRoomInfo.LatestMessage.DateSent = latestMessage.DateSent;

                chatRoomInfo.LatestMessage.ShortBody =
                    (latestMessage.Body != null && latestMessage.Body.Length > 200) ?
                    latestMessage.Body.Substring(0, 200) : latestMessage.Body;

                int totalMembers = dbc.UserChatRooms.Where(x => x.ChatRoomId == userChatRoom.ChatRoomId).Count();
                int receivedCount = dbc.MessagesReceived.Where(x => x.MessageSentId == latestMessage.Id).Count();
                int readCount = dbc.MessagesReceived.Where(x => x.MessageSentId == latestMessage.Id && x.DateRead != null).Count();

                // Note: subtract 1 because the sender does not receive it's own message sent.
                chatRoomInfo.LatestMessage.NotReceivedCount = totalMembers - 1 - receivedCount;
                chatRoomInfo.LatestMessage.NotReadCount = totalMembers - 1 - readCount;
            }
            return chatRoomInfo;
        }

        /// <summary>
        /// Get the ChatRoomInfo of all chat rooms which logged-in user is associated to.
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
                    .Include(x => x.UserProfile.User)
                    .Where(x => x.DateDeleted == null && x.UserProfile.User.UserName == User.Identity.Name
                        && (x.ChatRoom.GroupProfile == null || x.ChatRoom.GroupProfile.DateDeleted == null))
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
        /// Create a private chat room between the logged-in user and the given user.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        [Route(nameof(CreatePrivate))]
        public async Task<ActionResult<UserChatRoomDTO>> CreatePrivate(string emailAddress, string phoneNumber)
        {
            try
            {
                var user1 = await dbc.UserProfiles.FirstOrDefaultAsync(x => x.User.UserName == User.Identity.Name);
                if (user1 == null)
                    return NotFound("User profile not found.");

                UserProfile user2 = await dbc.UserProfiles
                    .Where(x =>
                        (!string.IsNullOrEmpty(emailAddress) && x.User.Email == emailAddress) ||
                        (!string.IsNullOrEmpty(phoneNumber) && x.User.PhoneNumber == phoneNumber))
                    .FirstOrDefaultAsync();

                if (user2 == null)
                    return NotFound("Given user was not found.");

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
                    var dateCreated = DateTime.UtcNow;

                    var chatRoom = new ChatRoom
                    {
                        Type = ChatRoomTypeEnum.Private,
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
                return CreatedAtAction(nameof(GetUserChatRoom), new { id = userChatRoomDto.Id }, userChatRoomDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
