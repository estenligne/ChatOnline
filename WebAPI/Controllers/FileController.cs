using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Net;
using Global.Models;
using WebAPI.Models;
using File = WebAPI.Models.File;

namespace WebAPI.Controllers
{
    public class FileController : BaseController<FileController>
    {
        private readonly IConfiguration _configuration;

        public FileController(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<FileController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<FileDTO> GetFile(long id)
        {
            try
            {
                File file = dbc.Files.Find(id);

                if (file == null)
                    return NotFound($"File {id} not found");

                if (file.UploaderId != UserId)
                    return Forbid("This file doesn't belong to you!");

                var fileDTO = _mapper.Map<FileDTO>(file);
                return Ok(fileDTO);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType(typeof(FileDTO), (int)HttpStatusCode.Created)]
        [HttpPost]
        public async Task<ActionResult<FileDTO>> PostFile(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("File not provided");

                string filePath = _configuration.GetValue<string>("PathToFiles");
                string dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss");
                string fileName = $"File {dateTime} {file.FileName}";
                long fileSize = file.Length;

                using (FileStream fileStream = new FileStream(Path.Combine(filePath, fileName), FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }

                File _file = new File();

                _file.Name = fileName;
                _file.Size = fileSize;

                _file.UploaderId = UserId;
                _file.DateUploaded = DateTimeOffset.UtcNow;

                dbc.Files.Add(_file);
                dbc.SaveChanges();

                var fileDTO = _mapper.Map<FileDTO>(_file);
                return CreatedAtAction(nameof(GetFile), null, fileDTO);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut]
        public async Task<ActionResult> PutFileAsync(long id, IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("File not provided");

                File _file = dbc.Files.Find(id);
                if (_file == null)
                    return NotFound($"File {id} not found");

                if (_file.UploaderId != UserId)
                    return Forbid("This file doesn't belong to you!");

                string folder = _configuration.GetValue<string>("PathToFiles");
                string dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss");
                string fileName = $"File {dateTime} {file.FileName}";
                long fileSize = file.Length;

                using (FileStream fileStream = new FileStream(Path.Combine(folder, fileName), FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }

                _file.Name = fileName;
                _file.Size = fileSize;
                _file.DateUploaded = DateTimeOffset.UtcNow;

                dbc.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            try
            {
                File file = await dbc.Files.FindAsync(id);

                if (file.UploaderId != UserId)
                    return Forbid("This file doesn't belong to you!");

                file.DateDeleted = DateTimeOffset.UtcNow;
                await dbc.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ResponseCache(NoStore = false, Duration = 365 * 24 * 60 * 60)]
        [HttpGet]
        [AllowAnonymous]
        [Route(nameof(Download))]
        public ActionResult Download(string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    string filePathName = _configuration.GetValue<string>("PathToFiles") + fileName;

                    if (System.IO.File.Exists(filePathName))
                    {
                        var modificationDate = System.IO.File.GetLastWriteTimeUtc(filePathName);
                        FileStream fileStream = System.IO.File.OpenRead(filePathName);
                        string contentType = MimeTypes.MimeTypeMap.GetMimeType(fileName);

                        var contentDisposition = new System.Net.Mime.ContentDisposition()
                        {
                            FileName = fileName,
                            Inline = true,
                            Size = fileStream.Length,
                            ModificationDate = modificationDate
                        };
                        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                        return File(fileStream, contentType);
                    }
                    else return NotFound($"File '{fileName}' not found");
                }
                else return BadRequest("File name not provided");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
