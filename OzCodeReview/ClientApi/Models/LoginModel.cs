using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }       
    }
}
