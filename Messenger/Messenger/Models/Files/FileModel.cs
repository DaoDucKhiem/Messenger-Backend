using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Models.Files
{
    public class FileModel
    {
        [Required]
        public string convId { get; set; }

        public string content { get; set; }

        [Required]
        public string filePath { get; set; }

        [Required]
        public int type { get; set; }

        public int typeofFile { get; set; }
    }
}
