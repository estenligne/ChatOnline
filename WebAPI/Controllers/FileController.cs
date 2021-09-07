using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using File = WebAPI.Models.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Net;

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
        public IActionResult GetFile(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            File file = dbc.Files.Find(id);

            if (file == null)
            {
                return NotFound();
            }

            return Ok(file);
        }

        [HttpPost]
        public async Task<IActionResult> PostFile(IFormFile file)
        {

            if (file != null)
            {
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
                _file.UploaderId = 1;
                _file.DateUploaded = DateTimeOffset.UtcNow;

                dbc.Files.Add(_file);
                dbc.SaveChanges();

                return Ok(file);
            }
            else
            {
                return BadRequest("file not provided");
            }
        }


        [HttpPut]
        public async Task<IActionResult> PutFileAsync(long id, IFormFile file)
        {
            if (file == null)
                return BadRequest("File model not provided");

            File file2 = dbc.Files.Find(id);

            if (file2 == null)
            {
                return NotFound($"File with id {id} not found");

            }

            if (file != null)
            {
                string folder = _configuration.GetValue<string>("PathToFiles");
                string dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss");
                string fileName = $"File {dateTime} {file.FileName}";
                long fileSize = file.Length;

                using (FileStream fileStream = new FileStream(Path.Combine(folder, fileName), FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }
                file2.Name = fileName;
                file2.Size = fileSize;
                file2.DateUploaded = DateTimeOffset.UtcNow;
            }

            dbc.SaveChanges();

            return Ok(file2);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {

            File file = await dbc.Files.FindAsync(id);

            var query = dbc.UserProfiles.Where(u => u.Identity == UserIdentity);

            var user = query.First();

            if (file.UploaderId != user.Id)
            {
                return Forbid("This file doesn't belong to you");
            }

            //_context.Files.Remove(file);
            file.DateDeleted = DateTimeOffset.UtcNow;
            await dbc.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}

//File 2021-09-06 23-59-47 tenue-femme-stylée-avec-pantalon-fluide-taille-haute-et-crop-top-noir-manches-longues-vetement-en-wax-pantalon-blanc-et-orange-boucle-d-oreilles-rouges.jpg
