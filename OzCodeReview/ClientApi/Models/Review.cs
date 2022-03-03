using Microsoft.Build.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi.Models
{
    public class Review
    {
        public string SolutionName { get; set; }

        public string FileName { get; set; }

        public string Branch { get; set; }

        public string Commit { get; set; }

        public string Comment { get; set; }

        public string CommentatorId { get; set; }
        
        public DateTime CreationDate { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public int StartLineNumber { get; set; }

        public int EndLineNumber { get; set; }

        public int StartCharIndex { get; set; }

        public int LastCharIndex { get; set; }       
                
        public ReviewStatus Status { get; set; }

        public ICollection<ReviewComment> Comments { get; set; }

        public ReviewType Type { get; set; }

        public string RecipientId { get;  set; }

        public string ProjectPath { get; set; }

        public Guid Id { get;  set; }
    }
}
