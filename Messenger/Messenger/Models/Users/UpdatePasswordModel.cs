using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Models.Users
{
    public class UpdatePasswordModel
    {
        [Required]
        public Guid Id { get; set; }
        public string OldPass { get; set; }
        public string NewPass{ get; set; }
    }
}
