using Messenger.Entities;
using Messenger.Helpers;
using Messenger.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Services
{
    public interface IFileService
    {
        Task<File> InsertFileAsync(FileModel model);
        IEnumerable<File> getFile(int amount, string convId);

        IEnumerable<File> getAllFile(string convId);

        IEnumerable<File> getImage(int amount, string convId);

        IEnumerable<File> getAllImage(string convId);
    }

    public class FileService : IFileService
    {
        private DataContext _context;

        public FileService(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<File> getAllFile(string convId)
        {
            return _context.Files.Where(data => data.type == 5 && data.convId == convId);
        }

        public IEnumerable<File> getAllImage(string convId)
        {
            return _context.Files.Where(data => data.type == 2 && data.convId == convId);
        }

        public IEnumerable<File> getFile(int amount, string convId)
        {
            return _context.Files.Where(data => data.type == 5 && data.convId == convId).Take(amount);
        }

        public IEnumerable<File> getImage(int amount, string convId)
        {
            return _context.Files.Where(data => data.type == 2 && data.convId == convId).Take(amount);
        }

        public async Task<File> InsertFileAsync(FileModel model)
        {
            var file = new File
            {
                Id = new Guid(),
                convId = model.convId,
                content = model.content,
                filePath = model.filePath,
                type = model.type,
                typeofFile = model.typeofFile,

            };

            _context.Files.Add(file);
            await _context.SaveChangesAsync();
            return file;
        }
    }
}
