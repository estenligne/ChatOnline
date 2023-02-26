﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebAPI.Services;
using WebAPI.Models;
using Global.Models;
using Global.Enums;
using AutoMapper;

namespace WebAPI.Controllers
{
    public class DeviceUsedController : BaseController<DeviceUsedController>
    {
        private readonly PushNotificationService _pushNotificationService;

        public DeviceUsedController(
            PushNotificationService pushNotificationService,
            ApplicationDbContext context,
            ILogger<DeviceUsedController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _pushNotificationService = pushNotificationService;
        }

        [ProducesResponseType(typeof(DeviceUsedDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut]
        public async Task<ActionResult> PutDeviceUsed([FromQuery] UserDeviceDTO dto)
        {
            try
            {
                var userProfile = await dbc.UserProfiles
                                        .Include(x => x.PhotoFile)
                                        .Include(x => x.WallpaperFile)
                                        .FirstOrDefaultAsync(x => x.Id == AccountId());

                if (userProfile == null)
                    return Problem("User profile not found.", statusCode: (int)HttpStatusCode.NotFound);

                var utcNow = DateTimeOffset.UtcNow;
                var userDevice = await dbc.DevicesUsed.FindAsync(dto.Id);

                if (userDevice == null)
                {
                    userDevice = new DeviceUsed
                    {
                        UserProfileId = userProfile.Id,
                        Platform = dto.Platform,
                        DateCreated = utcNow
                    };
                    dbc.DevicesUsed.Add(userDevice);
                }
                else if (userDevice.UserProfileId != userProfile.Id)
                {
                    return Problem("This device does not belong to you!", statusCode: (int)HttpStatusCode.Forbidden);
                }
                else if (userDevice.Platform != dto.Platform && dto.Platform != DevicePlatformEnum.Unknown)
                {
                    return Problem("This device's platform cannot change!", statusCode: (int)HttpStatusCode.Forbidden);
                }
                else if (userDevice.DateDeleted != null)
                {
                    userDevice.DateDeleted = null;
                }

                if (userDevice.PushNotificationToken != dto.PushNotificationToken && dto.PushNotificationToken != null)
                {
                    userDevice.PushNotificationToken = dto.PushNotificationToken;
                    userDevice.DateTokenProvided = utcNow;
                }

                if (dto.Language != null)
                    userDevice.Language = dto.Language;

                if (dto.TimeZone != null)
                    userDevice.Timezone = dto.TimeZone;

                userDevice.DateUpdated = utcNow;
                await dbc.SaveChangesAsync();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        public ActionResult DeleteDeviceUsed(long id)
        {
            try
            {
                string tag = $"User {UserId} {User.Identity.Name} on device {id}";

                DeviceUsed deviceUsed = dbc.DevicesUsed.Find(id);

                if (deviceUsed?.UserProfileId == UserId)
                {
                    if (deviceUsed.DateDeleted == null)
                    {
                        deviceUsed.DateDeleted = DateTimeOffset.UtcNow;
                        dbc.SaveChanges();

                        _logger.LogInformation($"{tag} has signed out.");
                    }
                    else _logger.LogWarning($"{tag} already signed out.");
                }
                else _logger.LogError($"{tag} not found or not valid.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPatch]
        [Route(nameof(RegisterFcmToken))]
        public ActionResult RegisterFcmToken(long deviceUsedId, string fcmToken)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                    return BadRequest("FcmToken not provided.");

                if (fcmToken.Length > 1023)
                    return BadRequest("FcmToken length must be < 1024.");

                var deviceUsed = dbc.DevicesUsed.Find(deviceUsedId);

                if (deviceUsed == null)
                    return NotFound($"DeviceUsed {deviceUsedId} not found.");

                if (deviceUsed.UserProfileId != UserId)
                    return Forbid("User profile does not match!");

                if (deviceUsed.DateDeleted != null)
                    return Conflict($"User already signed out of this device!");

                deviceUsed.PushNotificationToken = fcmToken;
                deviceUsed.DateTokenProvided = DateTimeOffset.UtcNow;

                dbc.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected long AccountId() => UserId;

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpGet(nameof(GetAllData))]
        public async Task<ActionResult> GetAllData(long id)
        {
            try
            {
                var userDevice = await dbc.DevicesUsed
                    .Include(x => x.UserProfile)
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();

                if (userDevice == null)
                    return Problem("User device not found.", statusCode: (int)HttpStatusCode.NotFound);

                if (userDevice.UserProfile.Id != AccountId())
                    return Problem("This device does not belong to you!", statusCode: (int)HttpStatusCode.Forbidden);

                var user = userDevice.UserProfile;

                var userRooms = await dbc.UserChatRooms
                    .Include(x => x.ChatRoom.GroupProfile)
                    .Where(x => x.UserProfileId == user.Id)
                    .ToDictionaryAsync(x => x.ChatRoomId);

                var otherUserRooms = await dbc.UserChatRooms
                    .Where(x => userRooms.Keys.Contains(x.ChatRoomId) && x.UserProfileId != user.Id)
                    .Select(x => new OtherUserRoom()
                    {
                        Id = x.Id,
                        UserProfileId = x.UserProfileId,
                        ChatRoomId = x.ChatRoomId,
                        UserRole = x.UserRole,
                        DateAdded = x.DateAdded,
                    })
                    .ToListAsync();

                var otherUserIds = otherUserRooms.Select(x => x.UserProfileId).ToList();

                var otherUsers = await dbc.UserProfiles
                    .Where(x => otherUserIds.Contains(x.Id))
                    .Select(x => new OtherUser()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DateCreated = x.DateCreated,
                    })
                    .ToListAsync();

                var messagesSent = await dbc.MessagesSent
                    .Where(x => userRooms.Keys.Contains(x.Sender.ChatRoomId))
                    .Select(x => new RoomMessageSent()
                    {
                        Id = x.Id,
                        SenderId = x.Sender.UserProfileId,
                        ChatRoomId = x.Sender.ChatRoomId,
                        Type = x.MessageType,
                        DateDrafted = x.DateSent,
                        DateUserSent = x.DateSent,
                        DateStarred = x.Sender.UserProfileId == user.Id ? x.DateStarred : null,
                        Body = x.Body,
                    })
                    .ToListAsync();

                var myMessagesIds = messagesSent
                    .Where(x => x.SenderId == user.Id)
                    .Select(x => x.Id)
                    .ToList();

                var messagesReceived = await dbc.MessagesReceived
                    .Where(x => myMessagesIds.Contains(x.MessageSentId) || x.Receiver.UserProfileId == user.Id)
                    .Select(x => new RoomMessageReceived()
                    {
                        Id = x.Id,
                        ReceiverId = x.Receiver.UserProfileId,
                        MessageSentId = x.MessageSentId,
                        SenderId = x.MessageSent.Sender.UserProfileId,
                        DateReceived = x.DateReceived,
                        DateRead = x.DateRead,
                        Reaction = x.Reaction,
                        DateStarred = x.Receiver.UserProfileId == user.Id ? x.DateStarred : null,
                    })
                    .ToListAsync();

                string sql = "INSERT OR REPLACE INTO Users (Id, Name, DateCreated) VALUES\n";
                sql += $"\t({user.Id}, {EscapeForSQL(user.Name)}, {user.DateCreated.ToUnixTimeMilliseconds()});\n\n";

                sql += "INSERT OR REPLACE INTO UserDevices (Id, UserId, Platform, DateCreated, DateUpdated) VALUES\n";
                sql += $"\t({userDevice.Id}, {user.Id}, {(int)userDevice.Platform}, {userDevice.DateCreated.ToUnixTimeMilliseconds()}, {userDevice.DateUpdated.ToUnixTimeMilliseconds()});\n\n";

                sql += "INSERT OR REPLACE INTO Users (Id, Name, DateCreated) VALUES\n";

                int remaining = otherUsers.Count;
                foreach (OtherUser x in otherUsers)
                {
                    string comma = GetComma(--remaining);
                    sql += $"\t({x.Id}, {EscapeForSQL(x.Name)}, {x.DateCreated.ToUnixTimeMilliseconds()}){comma}";
                }

                string sql_rooms = "INSERT OR REPLACE INTO Rooms (Id, Type, CreatorId, DateCreated) VALUES\n";
                string sql_groups = "INSERT OR REPLACE INTO Groups (RoomId, Name) VALUES\n";
                string sql_userRooms = "INSERT OR REPLACE INTO UserRooms (Id, UserId, RoomId, Role, DateAdded) VALUES\n";

                remaining = userRooms.Count;
                int remaining_gp = userRooms.Values.Where(ur => ur.ChatRoom.GroupProfile != null).Count();

                foreach (UserChatRoom userRoom in userRooms.Values)
                {
                    ChatRoom room = userRoom.ChatRoom;
                    GroupProfile group = room.GroupProfile;

                    string comma = GetComma(--remaining);

                    sql_rooms += $"\t({room.Id}, {1 + (int)room.Type}, {room.CreatorId}, {room.DateCreated.ToUnixTimeMilliseconds()}){comma}";

                    sql_userRooms += $"\t({userRoom.Id}, {userRoom.UserProfileId}, {room.Id}, {(int)userRoom.UserRole}, {userRoom.DateAdded.ToUnixTimeMilliseconds()}){comma}";

                    if (group != null)
                        sql_groups += $"\t({room.Id}, {EscapeForSQL(group.GroupName)}){GetComma(--remaining_gp)}";
                }

                sql += sql_rooms + sql_groups + sql_userRooms;

                remaining = otherUserRooms.Count;
                sql += "INSERT OR REPLACE INTO UserRooms (Id, UserId, RoomId, Role, DateAdded) VALUES\n";

                foreach (OtherUserRoom userRoom in otherUserRooms)
                {
                    ChatRoom room = userRooms[userRoom.ChatRoomId].ChatRoom;

                    string comma = GetComma(--remaining);
                    sql += $"\t({userRoom.Id}, {userRoom.UserProfileId}, {room.Id}, {(int)userRoom.UserRole}, {userRoom.DateAdded.ToUnixTimeMilliseconds()}){comma}";
                }

                remaining = messagesSent.Count;
                sql += "INSERT OR REPLACE INTO MessagesSent (Id, Type, RoomId, SenderId, DateDrafted, DateUserSent, Status, Body) VALUES\n";

                foreach (RoomMessageSent ms in messagesSent)
                {
                    ChatRoom room = userRooms[ms.ChatRoomId].ChatRoom;
                    string ms_hexId = GetHexId(ms.SenderId, ms.Id);

                    string comma = GetComma(--remaining);
                    sql += $"\t({ms_hexId}, {(int)ms.Type}, {room.Id}, {ms.SenderId}, {ms.DateDrafted.ToUnixTimeMilliseconds()}, {ms.DateUserSent.ToUnixTimeMilliseconds()}, 0, {EscapeForSQL(ms.Body)}){comma}";
                }

                remaining = messagesReceived.Count;
                sql += "INSERT OR REPLACE INTO MessagesReceived (Id, MessageSentId, ReceiverId, DateReceived, Status) VALUES\n";

                foreach (RoomMessageReceived mr in messagesReceived)
                {
                    string ms_hexId = GetHexId(mr.SenderId, mr.MessageSentId);
                    string mr_hexId = GetHexId(mr.ReceiverId, mr.Id);

                    string comma = GetComma(--remaining);
                    sql += $"\t({mr_hexId}, {ms_hexId}, {mr.ReceiverId}, {mr.DateReceived.ToUnixTimeMilliseconds()}, 0){comma}";
                }

                byte[] content = System.Text.Encoding.ASCII.GetBytes(sql);
                return File(content, "application/sql", "alldata.sql");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static string GetHexId(long a, long b)
        {
            return "X'" + a.ToString("X16") + b.ToString("X16") + "'";
        }

        private static string GetComma(int remaining)
        {
            return remaining <= 0 ? ";\n\n" : ",\n";
        }

        private static string EscapeForSQL(string body)
        {
            if (body == null)
                return "null";
            return "'" + body.Replace("'", "''") + "'";
        }

        public class UserDeviceDTO
        {
            public long Id { get; set; }

            public DevicePlatformEnum Platform { get; set; }
            public string Language { get; set; }
            public string TimeZone { get; set; }

            public string LocalIPv4 { get; set; }
            public string RemoteIPv4 { get; set; }

            public string PushNotificationToken { get; set; }
        }

        public class OtherUserRoom
        {
            public long Id { get; set; }
            public long UserProfileId { get; set; }
            public long ChatRoomId { get; set; }
            public UserRoleEnum UserRole { get; set; }
            public DateTimeOffset DateAdded { get; set; }
        }

        public class OtherUser
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public DateTimeOffset DateCreated { get; set; }
        }

        public class RoomMessageSent
        {
            public long Id { get; set; }
            public long SenderId { get; set; }
            public long ChatRoomId { get; set; }
            public MessageTypeEnum Type { get; set; }
            public DateTimeOffset DateDrafted { get; set; }
            public DateTimeOffset DateUserSent { get; set; }

            /// <summary>
            /// private information for sender
            /// </summary>
            public DateTimeOffset? DateStarred { get; set; }

            public string Body { get; set; }
        }

        public class RoomMessageReceived
        {
            public long Id { get; set; }
            public long ReceiverId { get; set; }
            public long MessageSentId { get; set; }
            public long SenderId { get; set; }
            public DateTimeOffset DateReceived { get; set; }
            public DateTimeOffset? DateRead { get; set; }
            public MessageReactionEnum Reaction { get; set; }

            /// <summary>
            /// private information for receiver
            /// </summary>
            public DateTimeOffset? DateStarred { get; set; }
        }
    }
}
