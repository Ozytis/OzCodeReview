using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi.Models
{
    public class LoginResult
    {
        public string AccessToken { get; set; }

        public DateTime AccessTokenValidUntil { get; set; }

        public UserModel User { get; set; }
    }
}
