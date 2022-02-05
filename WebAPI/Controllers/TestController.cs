using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class TestController : BaseController<TestController>
    {
        private IConfiguration _configuration;

        public TestController(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<TestController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
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
    }
}
