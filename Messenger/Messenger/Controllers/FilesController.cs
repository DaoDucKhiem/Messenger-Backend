using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messenger.Entities;
using Messenger.Helpers;
using Messenger.Models.Files;
using Messenger.Services;

namespace Messenger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFileService _fileService;

        public FilesController(DataContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/Files
        [HttpGet("getAllFile")]
        public ActionResult<IEnumerable<FileModel>> GetFiles([FromQuery] string convId)
        {
            return Ok(_fileService.getAllFile(convId));
        }

        // GET: api/Files/5
        [HttpGet("getFiles")]
        public ActionResult<FileModel> GetFile([FromQuery] string convId)
        {
            var amount = 3;
            return Ok(_fileService.getFile(amount, convId));
        }


        // GET: api/Files
        [HttpGet("getAllImage")]
        public ActionResult<IEnumerable<ImageModel>> GetImages([FromQuery] string convId)
        {
            return Ok(_fileService.getAllImage(convId));
        }

        // GET: api/Files/5
        [HttpGet("getImages")]
        public ActionResult<ImageModel> GetImage([FromQuery] string convId)
        {
            var amount = 6;
            return Ok(_fileService.getImage(amount, convId));
        }




        // PUT: api/Files/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFile(Guid id, File file)
        {
            if (id != file.Id)
            {
                return BadRequest();
            }

            _context.Entry(file).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Files
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public ActionResult<File> PostFile(FileModel file)
        {
            var _file = _fileService.InsertFileAsync(file);

            return Ok(_file);
        }

        // DELETE: api/Files/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<File>> DeleteFile(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            return file;
        }

        private bool FileExists(Guid id)
        {
            return _context.Files.Any(e => e.Id == id);
        }
    }
}
