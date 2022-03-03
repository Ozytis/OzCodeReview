using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Review
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string SolutionName { get; set; }

        [Required, MaxLength(2000)]
        public string FileName { get; set; }

        [Required, MaxLength(2000)]
        public string Branch { get; set; }
        
        [Required, MaxLength(2000)]
        public string Commit { get; set; }

        public string Comment { get; set; }

        [ForeignKey(nameof(Commentator))]
        [Required, MaxLength(ApplicationUser.IdMaxLength)]
        public string CommentatorId { get; set; }

        public ApplicationUser Commentator { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public int StartLineNumber { get; set; }

        public int EndLineNumber { get; set; }

        public int StartCharIndex { get; set; }

        public int LastCharIndex { get; set; }

        [ForeignKey(nameof(Recipient))]
        [MaxLength(ApplicationUser.IdMaxLength)]
        public string RecipientId { get; set; }

        public ApplicationUser Recipient { get; set; }

        public ReviewStatus Status { get; set; }

        public ICollection<ReviewComment> Comments { get; set; }

        public ReviewType Type { get; set; }

        [Required, MaxLength(2000)]
        public string ProjectPath { get; set; }

    }
}
