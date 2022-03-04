using Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Emails
{
    public class ReviewUpdatedModel
    {
        public Review Review { get; set; }

        public ApplicationUser Commentator { get; set; }

        public ApplicationUser Recipient { get; set; }
    }
}
