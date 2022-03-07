using Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Emails
{
    public class EmailChangeRequestModel
    {
        public ApplicationUser User { get; set; }

        public string Link { get; set; }
    }
}
