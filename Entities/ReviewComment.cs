using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ReviewComment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Review))]
        public Guid ReviewId { get; set; }

        public Review Review { get; set; }

        public string Comment { get; set; }

        [ForeignKey(nameof(Commentator))]
        [Required, MaxLength(ApplicationUser.IdMaxLength)]
        public string CommentatorId { get; set; }

        public ApplicationUser Commentator { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
