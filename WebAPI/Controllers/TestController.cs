using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Global.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    // [Authorize(Roles = Role.SystemAdmin)]
    [AllowAnonymous]
    public class TestController : BaseController<TestController>
    {
        private readonly PushNotificationService _pushNotificationService;
        private IConfiguration _configuration;

        public TestController(
            PushNotificationService pushNotificationService,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<TestController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _pushNotificationService = pushNotificationService;
            _configuration = configuration;
        }

        [HttpGet]
        [Route(nameof(DatabaseData))]
        public async Task<ActionResult> DatabaseData()
        {
            var data = await ApplicationDbData.GetAll(dbc);
            string content = data.ToJSON();
            return CompressedFile(content, "application/json", "database_data.json");
        }

        [HttpPost]
        [Route(nameof(DatabaseData))]
        public async Task<ActionResult> DatabaseData(IFormFile file)
        {
            if (file == null)
                return BadRequest("File not provided");

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string json = reader.ReadToEnd();
                var data = ApplicationDbData.FromJSON(json);
                await ApplicationDbData.AddData(dbc, _logger, _configuration, data);
            }
            return NoContent();
        }

        [HttpPost]
        [Route(nameof(SendPushNotification))]
        public async Task<ActionResult<List<PushNotificationOutcome>>> SendPushNotification(
            [FromQuery] List<long> userProfileIds,
            [FromBody] PushNotificationDTO pushNotificationDto)
        {
            var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);
            return Ok(outcomes);
        }

        public class MessageLinked
        {
            public string LinkedMessageBody { get; set; } // a class property
            public string LinkerMessageBody; // a class field
        }

        [Route("ImportDiscussion")]
        [HttpPost]
        public async Task<ActionResult> CreateUserProfile(IFormFile file)
        {
            try
            {
                string pathToFile = @"C:\Users\ndili\Downloads\WhatsApp Chat with EstEnLigne.txt";
                var createdAccounts = new List<UserProfileDTO>();
                var listOfMessages = ExtractMessagesFromWhatsAppExportedDiscussion(pathToFile, null);

                ChatRoom chatRoom = null;
                MessageTag messageTag = null;

                using (var httpClient = await HTTPClient.GetAuthenticated(_configuration))
                {
                    foreach (var message in listOfMessages)
                    {
                        var dateUserAdded = DateTimeOffset.UtcNow;
                        string phoneNumber = message.SenderPhoneNumber;

                        var userProfile = dbc.UserProfiles.FirstOrDefault(u => u.Name == phoneNumber);
                        if (userProfile == null)
                        {
                            userProfile = new UserProfile();
                            var userDto = new ApplicationUserDTO { PhoneNumber = phoneNumber, Password = phoneNumber };
                            userProfile.Name = JsonConvert.SerializeObject(userDto);
                            userProfile.DateCreated = dateUserAdded;
                            await ApplicationDbData.SetupUser(dbc, _logger, httpClient, userProfile);
                        }

                        string groupName = "EstEnLigne";

                        if (chatRoom == null)
                            chatRoom = dbc.ChatRooms.FirstOrDefault(u => u.GroupProfile.GroupName == groupName);

                        if (chatRoom == null)
                        {
                            chatRoom = new ChatRoom()
                            {
                                CreatorId = userProfile.Id,
                                DateCreated = dateUserAdded,

                                GroupProfile = new GroupProfile()
                                {
                                    GroupName = groupName,
                                    JoinToken = groupName,
                                }
                            };
                            dbc.ChatRooms.Add(chatRoom);
                            dbc.SaveChanges(); // get the id of chatRoom
                        }

                        if (messageTag == null)
                        {
                            messageTag = await dbc.MessageTags
                                .Where(x => x.Name == "" && x.ChatRoomId == chatRoom.Id)
                                .FirstOrDefaultAsync();
                        }

                        if (messageTag == null)
                        {
                            messageTag = new MessageTag()
                            {
                                Name = "",
                                ChatRoom = chatRoom,
                                DateCreated = DateTimeOffset.UtcNow,
                            };
                            dbc.MessageTags.Add(messageTag);
                            dbc.SaveChanges(); // get the id of messageTag
                        }

                        var userChatRoom = dbc.UserChatRooms
                            .Where(x => x.UserProfileId == userProfile.Id && x.ChatRoomId == chatRoom.Id)
                            .FirstOrDefault();

                        if (userChatRoom == null)
                        {
                            userChatRoom = new UserChatRoom()
                            {
                                UserProfileId = userProfile.Id,
                                ChatRoomId = chatRoom.Id,
                                DateAdded = dateUserAdded,
                            };
                            dbc.UserChatRooms.Add(userChatRoom);
                            dbc.SaveChanges();
                        }

                        var messageSent = new MessageSent
                        {
                            Sender = userChatRoom,
                            MessageTag = messageTag,
                            DateSent = message.DateSent,
                            DateCreated = message.DateSent,
                            Body = message.MessageBody,
                        };
                        dbc.MessagesSent.Add(messageSent);
                    }
                    await dbc.SaveChangesAsync();
                }
                return Ok("Created All available accounts");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public static List<ExtractedMessage> ExtractMessagesFromWhatsAppExportedDiscussion(string exportedDiscussion, List<MyContact>? userContacts)
        {
            List<ExtractedMessage> extractedMessages = new List<ExtractedMessage>();
            string[] messages = System.IO.File.ReadAllLines(exportedDiscussion);
            DateTime dateSent = DateTime.Parse("2/2/22 12:22", CultureInfo.InvariantCulture);
            string testMultiline = @"^([a-zA-Z])?\s?";
            string timeReg = @"([0-1]?[0-9]|2[0-3]):[0-5][0-9]";
            string dateReg = @"^([0-9]*)/([0-9]*)/([0-9]*)";
            string senderReg = @"\+[0-9]+\s?[0-9]+\s?[0-9]+\s?[0-9]+\s?[0-9]+\s?[0-9]+\s?[0-9]+\s?[0-9]+\s?";
            string senderReg2 = @"\-\s[a-zA-Z]+\s?\s?[a-zA-Z]+\s?";

            var regexTime = new Regex(timeReg);
            var regexDate = new Regex(dateReg);
            var regexSender = new Regex(senderReg);
            var regexSender2 = new Regex(senderReg2);
            var mutlineRegex = new Regex(testMultiline);

            string messageBody = "";
            for (int i = 0; i < messages.Length; i++)
            {

                var time = regexTime.Match(messages[i].Trim());
                var date = regexDate.Match(messages[i].Trim());
                var sender = regexSender.Match(messages[i].Trim());
                var sender2 = regexSender2.Match(messages[i].Trim());
                var multilineMsg = mutlineRegex.Match(messages[i].Trim());
                string[] body = messages[i].Trim().Split(new string[] { ": " }, StringSplitOptions.None);

                if (i == messages.Length - 1)
                {
                    // since i checks +1 ahead, it might not have checked the last message item.
                    int ln = extractedMessages.Count;

                    if (multilineMsg.Success || !date.Success)
                    {
                        try
                        {
                            // Console.WriteLine("Before..... {0}", extractedMessages[ln - 1].MessageBody);
                            extractedMessages[ln - 1].MessageBody += "\n" + messages[i];

                        }
                        catch
                        {
                            // Handle IndexOutOfBounds exception here.
                        }


                    }
                }
                if (i < messages.Length - 1)
                {
                    int ln = extractedMessages.Count;
                    // check if the next message contains an > messages[i+1]
                    multilineMsg = mutlineRegex.Match(messages[i + 1].Trim());

                    date = regexDate.Match(messages[i + 1].Trim());
                    if (multilineMsg.Success || !date.Success)
                    {
                        try
                        {
                            // Console.WriteLine("Before..... {0}", extractedMessages[ln - 1].MessageBody);
                            extractedMessages[ln - 1].MessageBody += "\n" + messages[i + 1];
                            // Console.WriteLine("After..... {0}", extractedMessages[ln - 1].MessageBody);

                        }
                        catch
                        {
                            // Handle IndexOutOfBounds exception here.
                        }

                    }
                    // match the current message for the date.
                    date = regexDate.Match(messages[i].Trim());
                    if (date.Success && body != null && messages[i].Length > 0)
                    {
                        try
                        {
                            messageBody = body[1];
                        }
                        catch
                        {
                            //Console.WriteLine(" See it failed here {0} becuae", messages[i].Length);
                            messageBody = messages[i].Trim().Split(new string[] { "- " }, StringSplitOptions.None)[1];
                        }
                        var dateString = $"{date} {time}";
                        // Console.WriteLine(dateString);
                        dateSent = DateTime.Parse(dateString, CultureInfo.InvariantCulture);

                        extractedMessages.Add(new ExtractedMessage(dateSent, sender.Success ? sender.ToString().Length > 5 || sender.ToString().Trim() == "-" ? sender.ToString().Trim() : "GROUP" : sender2.ToString().Trim(), messageBody));
                    }
                }

                if (!date.Success && extractedMessages.Count > 0 && body == null)
                {
                    int ln = extractedMessages.Count;
                    messageBody = messageBody + "\n" + messages[i].Trim();
                    extractedMessages[ln].MessageBody += "\n" + messages[i];

                }

            }

            return extractedMessages;
        }


    }

    public class ExtractedMessage
    {
        public DateTime DateSent;

        public string SenderPhoneNumber;

        public string MessageBody;

        public ExtractedMessage(DateTime dateSent, string senderPhoneNumber, string messageBody)
        {
            DateSent = dateSent;
            SenderPhoneNumber = senderPhoneNumber;
            MessageBody = messageBody;
        }
    }
    public class MyContact
    {
        public string FullName;

        public string PhoneNumber;
    }
}
