using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Global.Helpers;
using Global.Models;
using Microsoft.AspNetCore.Authorization;
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
        private AccountDbContext _accountDbc;

        public TestController(
            PushNotificationService pushNotificationService,
            AccountDbContext accountDbc,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<TestController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _pushNotificationService = pushNotificationService;
            _configuration = configuration;
            _accountDbc = accountDbc;
        }

        protected FileResult CompressedFile(string content, string contentType, string fileDownloadName)
        {
            using (var compressed = new MemoryStream())
            {
                using (var stream = new GZipStream(compressed, CompressionMode.Compress))
                {
                    byte[] array = Encoding.UTF8.GetBytes(content);
                    stream.Write(array, 0, array.Length);
                    stream.Close();
                    Response.Headers.Add("Content-Encoding", "gzip");
                    return File(compressed.ToArray(), contentType, fileDownloadName);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(DatabaseData))]
        public async Task<ActionResult> DatabaseData()
        {
            var data = await ApplicationDbData.GetAll(dbc, _accountDbc, _mapper, _logger);
            string content = data.ToJSON();
            return CompressedFile(content, "application/json", "database_data.json");
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
    }
}
