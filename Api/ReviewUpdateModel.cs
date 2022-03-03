using Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    public class ReviewUpdateModel
    {
        public Guid Id { get; set; }

        public ReviewType ReviewType { get; set; }

        public ReviewStatus ReviewStatus { get; set; }

        public string Comment { get; set; }
    }
}
