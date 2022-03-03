using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi.Models
{
    public class SendPasswordIsLostModel
    {       
        public string Email { get; set; }
    }
}
