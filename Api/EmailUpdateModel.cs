using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    public class EmailUpdateModel
    {
        [Required(ErrorMessage ="Veuillez renseigner la nouvelle adersse email")]
        public string Email { get; set; }
    }
}
