using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Global.Enums;
using Global.Models;
using WebAPI.Models;
using Newtonsoft.Json;

namespace WebAPI.Services
{
    public class PushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;
        private readonly HttpClient _httpClient;
        private const string fcmUrl = "https://fcm.googleapis.com/fcm/send";

        public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();

            string authorization = "key=" + configuration.GetValue<string>("FcmServerKey");
            if (!_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorization))
                _logger.LogError("Failed to add the FcmServerKey to the Authorization header.");
        }

        public async Task<List<PushNotificationOutcome>> SendAsync(ApplicationDbContext dbc, List<long> userProfileIds, PushNotificationDTO pushNotificationDto)
        {
            var outcomes = new List<PushNotificationOutcome>();
            bool failure = false;

            var devices = await dbc.DevicesUsed
                                    .Where(x => userProfileIds.Contains(x.UserProfileId) && x.DateDeleted == null)
                                    .ToListAsync();

            foreach (var device in devices)
            {
                var outcome = new PushNotificationOutcome();
                outcome.deviceId = device.Id;
                outcome.userProfileId = device.UserProfileId;

                if (string.IsNullOrEmpty(device.PushNotificationToken))
                {
                    outcome.errorMessage = $"device.PushNotificationToken was not provided.";
                    failure = true;
                    continue;
                }

                var data = new Dictionary<string, string>();
                data[nameof(PushNotificationDTO)] = JsonConvert.SerializeObject(pushNotificationDto);

                var payload = new
                {
                    to = device.PushNotificationToken,
                    data
                };

                string content = JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(fcmUrl, httpContent);
                content = await response.Content.ReadAsStringAsync();
                outcome.statusCode = response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    outcome.fcmResponse = JsonConvert.DeserializeObject<PushNotificationOutcome.FcmResponse>(content);
                    if (outcome.fcmResponse.failure != 0)
                        failure = true;
                }
                else
                {
                    outcome.errorMessage = content;
                    failure = true;
                }
                outcomes.Add(outcome);
            }

            if (failure)
            {
                var model = new { userProfileIds, pushNotificationDto, outcomes };
                _logger.LogError(JsonConvert.SerializeObject(model));
            }
            return outcomes;
        }
    }

    public class PushNotificationOutcome
    {
        public long deviceId { get; set; }
        public long userProfileId { get; set; }
        public HttpStatusCode statusCode { get; set; }
        public string errorMessage { get; set; }
        public FcmResponse fcmResponse { get; set; }

        public class FcmResponse
        {
            public long multicast_id { get; set; }
            public long success { get; set; }
            public long failure { get; set; }
            public long canonical_ids { get; set; }
            public List<FcmResult> results { get; set; }
        }

        public class FcmResult
        {
            public string error { get; set; }
        }
    }
}
