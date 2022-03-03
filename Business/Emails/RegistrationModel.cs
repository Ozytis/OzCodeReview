using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Emails
{
    public class RegistrationModel
    {
        public string Link { get; set; }

        public string UserTitle { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }
    }
}
