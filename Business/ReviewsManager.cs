using DataAccess;

using Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class ReviewsManager : BaseEntityManager<Review>
    {
        public ReviewsManager(DataContext dataContext) : base(dataContext)
        {
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            review.Id = Guid.NewGuid();
            review.CreationDate = DateTime.UtcNow;
            review.Status = ReviewStatus.Pending;
            review.LastUpdateDate = DateTime.UtcNow;

            // TODO : envoyer un email

            review = this.DataContext.Add(review).Entity;

            await this.SaveChangesAsync();

            return review;
        }

        public async Task<Review> UpdateReviewAsync(Guid reviewId, ReviewStatus reviewStatus, ReviewType reviewType, string comment, string updaterId)
        {
            Review review = await this.SelectAsync(reviewId, r => r.Comments);

            if (review == null)
            {
                return review;
            }

            review.LastUpdateDate = DateTime.UtcNow;
            review.Type = reviewType;
            review.Status = reviewStatus;

            if (!string.IsNullOrEmpty(comment))
            {

                review.Comments.Add(new ReviewComment
                {
                    CommentatorId = updaterId,
                    CreationDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    Comment = comment,
                    ReviewId = reviewId
                });
            }

            await this.SaveChangesAsync();

            return review;
        }
    }
}
