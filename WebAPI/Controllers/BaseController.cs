﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using AutoMapper;
using Global.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using WebAPI.Models;
using WebAPI.Services;

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

        protected long UserId => CustomAuthenticationHandler.GetUserId(User);

        protected ActionResult Forbid(string message)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, message);
        }

        protected ActionResult Failure<TObj>(Return<TObj> returned)
        {
            if (returned.exception != null)
                return InternalServerError(returned.exception);
            else
                return StatusCode((int)returned.code, returned.message);
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

        protected FileResult FileCompressed(byte[] content, string contentType, string fileDownloadName)
        {
            string acceptEncoding = Request.Headers["Accept-Encoding"];
            if (acceptEncoding.Contains("gzip"))
            {
                using (var compressed = new MemoryStream())
                {
                    using (var stream = new GZipStream(compressed, CompressionMode.Compress))
                    {
                        stream.Write(content, 0, content.Length);
                        stream.Close();
                        Response.Headers.Add("Content-Encoding", "gzip");
                        content = compressed.ToArray();
                    }
                }
            }
            return File(content, contentType, fileDownloadName);
        }
    }
}
