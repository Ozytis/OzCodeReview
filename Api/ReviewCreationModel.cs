using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace Api
{
    public class ReviewCreationModel
    {
        [Required, MaxLength(200)]
        public string SolutionName { get; set; }

        [Required, MaxLength(2000)]
        public string FileName { get; set; }

        [Required, MaxLength(2000)]
        public string Branch { get; set; }

        [Required, MaxLength(2000)]
        public string Commit { get; set; }

        public string Comment { get; set; }

        [Required, MaxLength(ApplicationUser.IdMaxLength)]
        public string CommentatorId { get; set; }

        public int StartLineNumber { get; set; }

        public int EndLineNumber { get; set; }

        public int StartCharIndex { get; set; }

        public int LastCharIndex { get; set; }

        [MaxLength(ApplicationUser.IdMaxLength)]
        public string RecipientId { get; set; }

        public ReviewType Type { get; set; }

        [Required, MaxLength(2000)]
        public string ProjectPath { get; set; }
    }
}