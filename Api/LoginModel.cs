using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Adresse email requise")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mot de passe requis")]
        public string Password { get; set; }       
    }
}
