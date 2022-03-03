using Microsoft.VisualStudio.Text.Tagging;

using OzCodeReview.ClientApi.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.Reviews
{
    public interface IReviewTag : ITag
    {
        Review Review { get; set; }

        UserModel Commentator { get; set; }

        UserModel Recipient { get; set; }
    }
}
