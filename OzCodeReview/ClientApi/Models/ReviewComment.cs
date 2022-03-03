using Microsoft.Build.Framework;

namespace OzCodeReview.ClientApi.Models
{
    public class ReviewComment
    {
        public Guid Id { get; set; }
              
        public Guid ReviewId { get; set; }

        public Review Review { get; set; }

        public string Comment { get; set; }

        public string CommentatorId { get; set; }
        
        public DateTime CreationDate { get; set; }
    }
}