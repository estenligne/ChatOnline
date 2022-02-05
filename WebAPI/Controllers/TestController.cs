using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Global.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
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
            if (UserId != 1)
                return Unauthorized();

            var data = await ApplicationDbData.GetAll(dbc);
            string content = data.ToJSON();

            return CompressedFile(content, "application/json", "database_data.json");
        }

        [HttpPost]
        [Route(nameof(DatabaseData))]
        public async Task<ActionResult> DatabaseData(IFormFile file)
        {
            if (UserId != 1)
                return Unauthorized();

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
            if (UserId != 1)
                return Unauthorized();
            var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);
            return Ok(outcomes);
        }
    }
}
