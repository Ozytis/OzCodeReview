using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi.Models
{
    public class ReviewUpdateModel
    {
        public Guid Id { get; set; }      

        public ReviewType ReviewType { get; set; }

        public ReviewStatus ReviewStatus { get; set; }

        public string Comment { get; set; }
    }
}
