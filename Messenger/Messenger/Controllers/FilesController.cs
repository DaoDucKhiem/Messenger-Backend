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
using Microsoft.AspNetCore.Authorization;

namespace Messenger.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
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

        /// <summary>
        /// api lấy tất cả các file của cuộc trò chuyện
        /// </summary>
        /// <param name="convId">id của cuộc trò chuyện</param>
        /// <returns>danh sách tất cả các file trong cuộc trò chuyện đó</returns>
        [HttpGet("getAllFile")]
        public ActionResult<IEnumerable<FileModel>> GetFiles([FromQuery] string convId)
        {
            return Ok(_fileService.getAllFile(convId));
        }

        /// <summary>
        /// api để lấy một số file của cuộc trò chuyện
        /// </summary>
        /// <param name="convId">id của cuộc trò chuyện</param>
        /// <returns>danh sách một số file định dạng model theo định dạng của tin nhắn</returns>
        /// create by Đào Đức Khiêm
        [HttpGet("getFiles")]
        public ActionResult<FileModel> GetFile([FromQuery] string convId)
        {
            var amount = 2; //số lượng file lấy
            return Ok(_fileService.getFile(amount, convId));
        }


        /// <summary>
        /// api để lấy tất cả các ảnh của cuộc trò chuyện
        /// </summary>
        /// <param name="convId">id của cuộc trò chuyện</param>
        /// <returns>danh sách các file dưới dạng tin nhắn</returns>
        /// create by Đào Đức Khiêm
        [HttpGet("getAllImage")]
        public ActionResult<IEnumerable<ImageModel>> GetImages([FromQuery] string convId)
        {
            return Ok(_fileService.getAllImage(convId));
        }

        /// <summary>
        /// api lấy một số ảnh của cuộc trò chuyện
        /// </summary>
        /// <param name="convId">id của cuộc trò chuyện</param>
        /// <returns>danh sách các ảnh được định dạng theo tin nhắn</returns>
        /// create by Đào Đức Khiêm
        [HttpGet("getImages")]
        public ActionResult<ImageModel> GetImage([FromQuery] string convId)
        {
            var amount = 3; //số lượng ảnh cần lấy
            return Ok(_fileService.getImage(amount, convId));
        }



        //api này không dùng đến
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

        /// <summary>
        /// Thực hiện insert file hoặc ảnh vào database
        /// </summary>
        /// <param name="file">file model gửi từ client lên</param>
        /// <returns>file</returns>
        /// create by Đào Đức Khiêm
        [HttpPost]
        public ActionResult<File> PostFile(FileModel file)
        {
            var _file = _fileService.InsertFileAsync(file);

            return Ok(_file);
        }

        //api này chưa dùng đến
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
