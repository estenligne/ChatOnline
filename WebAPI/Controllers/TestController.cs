using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        /*
         * <summary>
         * type==1 is for linked messages, 0 for extractedMessages from whatsapp
         * </summary>
         */
        public static List<string> ReadAsList(IFormFile file, int type)
        {

            List<string> result = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                  
                        result.Add(reader.ReadLine());
                }
            }
            return result;
        }

        [Route("ImportDiscussion")]
        [HttpPost]
        public async Task<ActionResult> CreateUserProfile(IFormFile file)
        {
            try
            {
                // string pathToFile = @"C:\Users\ndili\Downloads\WhatsApp Chat with EstEnLigne.txt";
                var createdAccounts = new List<UserProfileDTO>();
                var listOfMessages = ExtractMessagesFromWhatsAppExportedDiscussion(file, null);

                ChatRoom chatRoom = null;
                MessageTag messageTag = null;

                using (var httpClient = await HTTPClient.GetAuthenticated(_configuration))
                {
                    foreach (var message in listOfMessages)
                    {
                        var dateUserAdded = DateTimeOffset.UtcNow;
                        string phoneNumber = message.SenderPhoneNumber;

                        var userProfile = dbc.UserProfiles.FirstOrDefault(u => u.Name.Replace(" ","") == phoneNumber);
                        if (userProfile == null)
                        {
                            userProfile = new UserProfile();
                            var userDto = new ApplicationUserDTO { PhoneNumber = phoneNumber.Replace(" ", ""), Password = "Aa1_" + phoneNumber };
                           

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

        [Route("LinkMessages")]
        [HttpPost]
        public async Task<ActionResult> LinkedMessages(IFormFile file)
        {
            try
            {
                var listOfMessages = ExtractLinkedMessages(file);
                if (dbc.MessagesSent.Count() > 1)
                {
                    int insertCount = 0;
                    foreach (var message in listOfMessages)
                    {
                        try {
                            if (message.LinkerMessageBody == "" || message.LinkerMessageBody == "")
                            {
                                continue;
                            }
                            //14380
                            var linked = dbc.MessagesSent.First(m => m.Body.Contains(message.LinkedMessageBody));
                            var linker = dbc.MessagesSent.First(m => m.Body.Contains(message.LinkerMessageBody));
                            if ((linked != null && linker != null) && (linked != linker))
                            {
                                if (linked.LinkedId != null)
                                {
                                    linked.LinkedId = linker.Id;
                                    await dbc.SaveChangesAsync();
                                }
                                insertCount++;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (InvalidOperationException ex) when (ex.Message == "Sequence contains no elements")
                        {
                            // No parent for the current message.
                            continue;
                        }
                       
                    }

                }
                return Ok("Linked all available messages");
            }
            catch (InvalidOperationException ex) when (ex.Message == "Sequence contains no elements")
            {
                return NotFound(ex.Message + ": Some messages were not linked because their parent were not found. \nMake sure every message has an already existing parent.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        public static List<ExtractedMessage> ExtractMessagesFromWhatsAppExportedDiscussion(IFormFile exportedDiscussion, List<MyContact>? userContacts)
        {
            List<ExtractedMessage> extractedMessages = new List<ExtractedMessage>();
            var messages = ReadAsList(exportedDiscussion, 0);
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
            for (int i = 0; i < messages.Count; i++)
            {

                var time = regexTime.Match(messages[i].Trim());
                var date = regexDate.Match(messages[i].Trim());
                var sender = regexSender.Match(messages[i].Trim());
                var sender2 = regexSender2.Match(messages[i].Trim());
              
                if (sender2.Success)
                {
                        continue; 
                }
               
                var multilineMsg = mutlineRegex.Match(messages[i].Trim());
                string[] body = messages[i].Trim().Split(new string[] { ": " }, StringSplitOptions.None);

                if (i == messages.Count - 1)
                {
                    var a = messages;
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
                            continue;
                        }


                    }
                }
                if (i < messages.Count - 1)
                {
                    int ln = extractedMessages.Count;
                    // check if the next message is a continuation of the previous
                    multilineMsg = mutlineRegex.Match(messages[i + 1].Trim());

                    date = regexDate.Match(messages[i + 1].Trim());
                    if (multilineMsg.Success && !date.Success)
                    {
                        if (ln > 0)
                        {
                            try
                            {
                                // Console.WriteLine("Before..... {0}", extractedMessages[ln - 1].MessageBody);
                                extractedMessages[ln - 1].MessageBody += "\n" + messages[i + 1];
                                // Console.WriteLine("After..... {0}", extractedMessages[ln - 1].MessageBody);

                            }
                            catch (IndexOutOfRangeException)
                            {
                                continue;
                            }
                        }

                    }
                    // match the current message for the date.
                    date = regexDate.Match(messages[i].Trim());
                    if (date.Success && body != null && messages[i].Length > 0)
                    {
                        {
                            try
                            {
                                messageBody = body[1];

                            }
                            catch (IndexOutOfRangeException)
                            {
                                continue;
                            }
                        }


                        var dateString = $"{date} {time}";
                        dateSent = DateTime.Parse(dateString, CultureInfo.InvariantCulture);

                        extractedMessages.Add(new ExtractedMessage(dateSent, sender.ToString().Trim(), messageBody));
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

        public static List<MessageLinked> ExtractLinkedMessages(IFormFile linkedMessages)
        {
            List<MessageLinked> extractedMessages = new List<MessageLinked>();
            var messages = ReadAsList(linkedMessages, 1);
            foreach (string message in messages)
            {
                string[] body = message.Split(new string[] { ", "}, StringSplitOptions.None);
                
                    if (body.Length > 0)
                    {
                    try { 
                        extractedMessages.Add(new MessageLinked { LinkedMessageBody = body[0], LinkerMessageBody = body[1] });
                    }
                    catch(IndexOutOfRangeException) { continue; }
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
