using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<T> : ControllerBase
    {
        /// <summary>
        /// Application Database Context
        /// </summary>
        protected readonly ApplicationDbContext dbc;
        protected readonly ILogger<T> _logger;
        protected readonly IMapper _mapper;

        public BaseController(
            ApplicationDbContext context,
            ILogger<T> logger,
            IMapper mapper)
        {
            dbc = context;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Log the exception and return an InternalServerError status code
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected ActionResult InternalServerError(Exception ex, string message = null)
        {
            _logger.LogError(ex, message);
            if (message == null)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                message = ex.Message;
            }
            return StatusCode((int)HttpStatusCode.InternalServerError, message);
        }
    }
}
