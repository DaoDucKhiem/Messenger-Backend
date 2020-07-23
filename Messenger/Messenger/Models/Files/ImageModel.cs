using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Models.Files
{
    public class ImageModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string convId { get; set; }

        [Required]
        public string filePath { get; set; }

        [Required]
        public int type { get; set; }
    }
}
