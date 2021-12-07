using System;
using System.Net;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        protected long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        protected ActionResult Forbid(string message)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, message);
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

        /// <summary>
        /// Apply the Data Annotations Attributes set on the properties/fields of the given model.
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="obj"></param>
        /// <returns>The list of validation results, or null if there is no validation error.</returns>
        protected List<ValidationResult> ValidateObject<TObj>(TObj obj)
        {
            var results = new List<ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(obj, serviceProvider: null, items: null);
            var isValid = Validator.TryValidateObject(obj, context, results);
            if (isValid)
                return null;
            else return results;
        }
    }
}
