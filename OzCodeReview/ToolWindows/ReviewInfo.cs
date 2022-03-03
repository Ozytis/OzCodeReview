using OzCodeReview.ClientApi.Models;
using OzCodeReview.Reviews;

using System.Collections.Generic;

namespace OzCodeReview
{
    public class ReviewInfo
    {
        public string SolutionName { get; set; }

        public string FileName { get; set; }

        public string Branch { get; set; }

        public string Commit { get; set; }

        public string Comment { get; set; }

        public string CommentatorId { get; set; }

        public string CreationDate { get; set; }

        public string LastUpdateDate { get; set; }

        public int StartLineNumber { get; set; }

        public int EndLineNumber { get; set; }

        public int StartCharIndex { get; set; }

        public int LastCharIndex { get; set; }

        public string Status { get; set; }

        public ICollection<ReviewComment> Comments { get; set; }

        public string Type { get; set; }

        public string RecipientId { get; set; }

        public string ProjectPath { get; set; }

        public Guid Id { get; set; }

        public string RecipientName { get; set; }

        public string CommentatorName { get; set; }
    }
}