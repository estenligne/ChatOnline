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
using WebAPI.Services;

namespace WebAPI.Controllers
{
    public class MessageController : BaseController<MessageController>
    {
        private readonly PushNotificationService _pushNotificationService;

        public MessageController(
            PushNotificationService pushNotificationService,
            ApplicationDbContext context,
            ILogger<MessageController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _pushNotificationService = pushNotificationService;
        }

        private MessageSentDTO GetMessage(MessageSent messageSent, MessageReceived messageReceived)
        {
            var messageSentDto = _mapper.Map<MessageSentDTO>(messageSent);

            messageSentDto.SenderName = messageSent.Sender?.UserProfile?.Name;

            if (messageSentDto.MessageTag == null)
            {
                if (messageSent.Sender != null)
                {
                    messageSentDto.MessageTag = new MessageTagDTO
                    {
                        Id = messageSent.MessageTagId,
                        ChatRoomId = messageSent.Sender.ChatRoomId // needed by app
                    };
                }
                else throw new ArgumentException("MessageTag == null and Sender == null");
            }

            if (messageReceived != null)
            {
                if (messageReceived.MessageSentId != messageSent.Id)
                    _logger.LogError($"In GetMessage(): {messageReceived.MessageSentId} != {messageSent.Id}");

                if (messageReceived.ReceiverId == messageSent.SenderId) // if user received their own message
                    _logger.LogError($"In GetMessage(): SenderId {messageSent.SenderId} received their own message");

                messageSentDto.ReceiverId = messageReceived.ReceiverId;
                messageSentDto.DateReceived = messageReceived.DateReceived;
                messageSentDto.DateRead = messageReceived.DateRead;
                messageSentDto.DateStarred = messageReceived.DateStarred;
                messageSentDto.Reaction = messageReceived.Reaction;

                if (messageReceived.DateDeleted != null)
                    messageSentDto.DateDeleted = messageReceived.DateDeleted;
            }

            if (messageSentDto.DateDeleted != null)
            {
                messageSentDto.Body = null; // then reduce payload size a bit!
                messageSentDto.File = null; // reduce payload size some more!
            }
            return messageSentDto;
        }

        [HttpGet]
        [Route(nameof(GetMany))]
        public async Task<ActionResult<List<MessageSentDTO>>> GetMany(long userChatRoomId)
        {
            try
            {
                var userChatRoom = dbc.UserChatRooms.Find(userChatRoomId);

                if (userChatRoom == null)
                    return NotFound("Sender's user chat room not found.");

                if (userChatRoom.UserProfileId != UserId)
                    return Forbid("Sender's user chat room does not match!");

                if (userChatRoom.DateBlocked != null ||
                    userChatRoom.DateDeleted != null ||
                    userChatRoom.DateExited != null)
                    return Forbid("You are not allowed to view messages in this chat room!");

                var messagesSent = await dbc.MessagesSent
                                            .Include(x => x.Sender.UserProfile)
                                            .Include(x => x.MessageTag)
                                            .Include(x => x.File)
                                            .Where(x => x.Sender.ChatRoomId == userChatRoom.ChatRoomId)
                                            .OrderBy(x => x.Id)
                                            .ToListAsync();

                var messageSentIds = messagesSent.Select(x => x.Id);

                var messagesReceived = await dbc.MessagesReceived
                                                .Where(x => x.ReceiverId == userChatRoomId &&
                                                    messageSentIds.Contains(x.MessageSentId))
                                                .ToDictionaryAsync(x => x.MessageSentId);

                var utcNow = DateTimeOffset.UtcNow;
                var messages = new List<MessageSentDTO>();

                foreach (var messageSent in messagesSent)
                {
                    MessageReceived messageReceived = null;
                    messagesReceived.TryGetValue(messageSent.Id, out messageReceived);

                    if (messageReceived == null && messageSent.SenderId != userChatRoomId)
                    {
                        _logger.LogWarning($"GetMany({userChatRoomId}): user did not receive message {messageSent.Id}.");
                        var messageSentDto = await AddMessageReceived(messageSent, userChatRoom.Id, utcNow, utcNow);
                        messages.Add(messageSentDto);
                    }
                    else messages.Add(GetMessage(messageSent, messageReceived));
                }
                return messages;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static bool LesserTime(DateTimeOffset a, DateTimeOffset b)
        {
            return a < b + TimeSpan.FromMinutes(1);
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        public async Task<ActionResult<MessageSentDTO>> PostMessage(MessageSentDTO messageSentDto)
        {
            try
            {
                var validationResults = ValidateObject(messageSentDto);
                if (validationResults != null)
                    return BadRequest(validationResults);

                if (messageSentDto.Id != 0)
                    return BadRequest("Id must be 0!");

                if (messageSentDto.DateDeleted != null)
                    return Forbid("Cannot delete this message!");

                if (messageSentDto.AuthorId != null && messageSentDto.LinkedId == null)
                    return BadRequest("No linked message provided for the forwarded message.");

                var dateCreated = DateTimeOffset.UtcNow;

                if (!LesserTime(messageSentDto.DateSent, dateCreated))
                    return BadRequest("The date sent cannot be in the future!");

                var userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile)
                                            .Include(x => x.ChatRoom.GroupProfile)
                                            .Where(x => x.Id == messageSentDto.SenderId)
                                            .FirstOrDefaultAsync();

                if (userChatRoom == null)
                    return NotFound("Sender's user chat room not found.");

                if (userChatRoom.UserProfileId != UserId)
                    return Forbid("Sender's user chat room does not match!");

                if (messageSentDto.LinkedId != null)
                {
                    var linked = dbc.MessagesSent.Find(messageSentDto.LinkedId);
                    if (linked == null)
                        return NotFound("Linked message not found.");

                    if (linked.AuthorId == null)
                    {
                        long linkedSenderProfileId = dbc.UserChatRooms
                            .Where(x => x.Id == linked.SenderId)
                            .Select(x => x.UserProfileId)
                            .First();

                        if (messageSentDto.AuthorId != null && messageSentDto.AuthorId != linkedSenderProfileId)
                            return Forbid("The author of this forwarded message is incorrect!");
                    }
                    else
                    {
                        if (messageSentDto.AuthorId != null && messageSentDto.AuthorId != linked.AuthorId)
                            return Forbid("Cannot change the author when forwarding a message!");

                        if (messageSentDto.MessageType != linked.MessageType)
                            return Forbid("Cannot change the message type of a forwarded message!");

                        if (messageSentDto.Body != linked.Body)
                            return Forbid("Cannot change the message body of a forwarded message!");

                        if (messageSentDto.FileId != linked.FileId)
                            return Forbid("Cannot change the file associated to a forwarded message!");
                    }
                }

                var tag = messageSentDto.MessageTag;
                if (tag != null) // will always be true, here just as an excuse to indent!
                {
                    if (tag.Name == null)
                        tag.Name = "";

                    if (tag.ChatRoomId != userChatRoom.ChatRoomId)
                        return BadRequest($"tag.ChatRoomId {tag.ChatRoomId} != ChatRoomId {userChatRoom.ChatRoomId}");

                    var messageTag = await dbc.MessageTags
                                            .Where(x => x.Name == tag.Name && x.ChatRoomId == userChatRoom.ChatRoomId)
                                            .FirstOrDefaultAsync();

                    if (messageTag == null)
                    {
                        if (tag.Id != 0)
                            return BadRequest("The MessageTag.Id must be 0!");

                        messageTag = new MessageTag()
                        {
                            Name = tag.Name,
                            ChatRoomId = userChatRoom.ChatRoomId,
                            CreatorId = userChatRoom.Id,
                            DateCreated = dateCreated,
                        };

                        dbc.MessageTags.Add(messageTag);
                        await dbc.SaveChangesAsync(); // get messageTag.Id
                    }
                    else if (tag.Id != 0 && tag.Id != messageTag.Id)
                        return Forbid("The MessageTag.Id is not valid!");

                    tag.Id = messageTag.Id;
                }
                messageSentDto.MessageTagId = tag.Id; // prepare for mapper

                var messageSent = _mapper.Map<MessageSent>(messageSentDto);
                messageSent.DateCreated = dateCreated;

                messageSent.MessageTag = null; // do not update database data
                messageSent.File = null; // do not update file database data

                dbc.MessagesSent.Add(messageSent);
                await dbc.SaveChangesAsync();

                messageSentDto.Id = messageSent.Id;
                messageSentDto.SenderName = userChatRoom.UserProfile.Name;

                string title = userChatRoom.UserProfile.Name;
                string group = userChatRoom.ChatRoom.GroupProfile?.GroupName;
                if (group != null)
                    title += " - " + group;

                // Send the MessageSent push notification
                var pushNotificationDto = new PushNotificationDTO
                {
                    Topic = PushNotificationTopic.MessageSent,
                    DateCreated = dateCreated,
                    MessageSent = messageSentDto,

                    Id = dateCreated.Ticks,
                    Title = title,
                    Body = messageSentDto.Body,

                    //Priority = userChatRoom.ChatRoom.Type == ChatRoomTypeEnum.Private,
                };

                await SendPushNotificationToEveryone(userChatRoom, pushNotificationDto);

                return CreatedAtAction(nameof(GetMany), null, messageSentDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        [Route(nameof(Received))]
        public async Task<ActionResult<MessageSentDTO>> Received(long messageSentId, DateTimeOffset dateReceived)
        {
            try
            {
                var messageSent = await dbc.MessagesSent
                                            .Include(x => x.Sender.UserProfile)
                                            .Where(x => x.Id == messageSentId)
                                            .FirstOrDefaultAsync();

                if (messageSent == null)
                    return NotFound($"messageSentId {messageSentId} not found.");

                if (messageSent.Sender == null)
                    return BadRequest($"messageSentId {messageSentId} has no sender.");

                if (!LesserTime(messageSent.DateCreated, dateReceived))
                    return BadRequest("Date user received cannot be before date server received!");

                var dateCreated = DateTimeOffset.UtcNow;
                if (!LesserTime(dateReceived, dateCreated))
                    return BadRequest("Date received cannot be in the future!");

                var userChatRoom = await dbc.UserChatRooms
                                            .Where(x => x.UserProfileId == UserId &&
                                                x.ChatRoomId == messageSent.Sender.ChatRoomId)
                                            .FirstOrDefaultAsync();

                if (userChatRoom == null)
                    return NotFound($"User chat room not found.");

                if (messageSent.SenderId == userChatRoom.Id)
                    return BadRequest("Cannot receive your own message sent!");

                var messageReceived = await dbc.MessagesReceived
                                            .Where(x => x.ReceiverId == userChatRoom.Id && x.MessageSentId == messageSentId)
                                            .FirstOrDefaultAsync();

                if (messageReceived != null)
                    return Conflict("Message already received.");

                var messageSentDto = await AddMessageReceived(messageSent, userChatRoom.Id, dateCreated, dateReceived);

                return CreatedAtAction(nameof(GetMany), null, messageSentDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private async Task<MessageSentDTO> AddMessageReceived(MessageSent messageSent, long userChatRoomId, DateTimeOffset dateCreated, DateTimeOffset dateReceived)
        {
            var messageReceived = new MessageReceived()
            {
                ReceiverId = userChatRoomId,
                MessageSentId = messageSent.Id,
                DateCreated = dateCreated,
                DateReceived = dateReceived,
            };

            dbc.MessagesReceived.Add(messageReceived);
            await dbc.SaveChangesAsync();

            var messageSentDto = GetMessage(messageSent, messageReceived);

            if (messageSent.SenderId.HasValue)
            {
                // Send the MessageReceived push notification
                var pushNotificationDto = new PushNotificationDTO
                {
                    Topic = PushNotificationTopic.MessageReceived,
                    DateCreated = dateCreated,
                    MessageSent = messageSentDto,
                };

                var userProfileIds = new List<long> { messageSent.Sender.UserProfileId };
                await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);
            }

            return messageSentDto;
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPatch]
        [Route(nameof(Read))]
        public async Task<ActionResult<MessageSentDTO>> Read(long messageSentId, DateTimeOffset dateRead)
        {
            try
            {
                var messageReceived = await dbc.MessagesReceived
                                        .Include(x => x.MessageSent.Sender)
                                        .Where(x => x.MessageSentId == messageSentId &&
                                            x.Receiver.UserProfileId == UserId)
                                        .FirstOrDefaultAsync();

                if (messageReceived == null)
                    return NotFound($"messageReceived.MessageSentId {messageSentId} not found.");

                if (!LesserTime(messageReceived.DateReceived, dateRead))
                    return BadRequest("Date read cannot be before date received!");

                var utcNow = DateTimeOffset.UtcNow;

                if (!LesserTime(dateRead, utcNow))
                    return BadRequest("Date read cannot be in the future!");

                if (messageReceived.DateRead != null)
                    return Conflict("Message already read.");

                messageReceived.DateRead = dateRead;
                await dbc.SaveChangesAsync();

                MessageSent messageSent = messageReceived.MessageSent;

                // Send the MessageRead push notification
                var pushNotificationDto = new PushNotificationDTO
                {
                    Topic = PushNotificationTopic.MessageRead,
                    DateCreated = utcNow,
                    MessageSent = GetMessage(messageSent, messageReceived)
                };

                var userProfileIds = new List<long> { messageSent.Sender.UserProfileId };
                var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPatch]
        [Route(nameof(Starred))]
        public async Task<IActionResult> Starred(long messageSentId, DateTimeOffset dateStarred)
        {
            MessageSent message = await dbc.MessagesSent
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x => x.Id == messageSentId);

            var utcNow = DateTimeOffset.UtcNow;

            if (message.Sender?.UserProfileId != UserId)
                return Forbid("This message doesn't belong to you!");

            if (!LesserTime(dateStarred, utcNow))
                return BadRequest("Date starred cannot be in the future!");

            if (!LesserTime(message.DateSent, dateStarred))
                return BadRequest("Date starred cannot be before date sent!");

            message.DateStarred = dateStarred;
            await dbc.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long messageSentId, DateTimeOffset dateDeleted)
        {
            MessageSent message = await dbc.MessagesSent
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x => x.Id == messageSentId);

            var utcNow = DateTimeOffset.UtcNow;

            if (message.Sender?.UserProfileId != UserId)
                return Forbid("This message doesn't belong to you!");

            if (!LesserTime(dateDeleted, utcNow))
                return BadRequest("Date deleted cannot be in the future!");

            if (!LesserTime(message.DateSent, dateDeleted))
                return BadRequest("Date deleted cannot be before date sent!");

            if (utcNow - message.DateSent >= TimeSpan.FromMinutes(30))
                return Forbid("Can't delete the message at this time!");

            message.DateDeleted = dateDeleted;
            await dbc.SaveChangesAsync();

            var pushNotificationDto = new PushNotificationDTO()
            {
                Topic = PushNotificationTopic.MessageDeleted,
                DateCreated = utcNow,
                MessageSent = GetMessage(message, null),
            };

            await SendPushNotificationToEveryone(message.Sender, pushNotificationDto);

            return NoContent();
        }

        private async Task SendPushNotificationToEveryone(UserChatRoom userChatRoom, PushNotificationDTO pushNotificationDto)
        {
            var userProfileIds = await dbc.UserChatRooms
                                          .Where(x => x.Id != userChatRoom.Id &&
                                              x.ChatRoomId == userChatRoom.ChatRoomId &&
                                              x.DateBlocked == null &&
                                              x.DateDeleted == null &&
                                              x.DateExited == null)
                                          .Select(x => x.UserProfileId)
                                          .ToListAsync();

            var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);
        }
    }
}
