using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    public class PasswordUpdateModel
    {
        [Required(ErrorMessage ="Please fill your current pawword")]        
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage ="Please fill your new password")]
        public string NewPassword { get; set; }
    }
}
