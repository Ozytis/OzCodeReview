using Business.Emails;

using DataAccess;

using Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using MimeKit;

using Ozytis.Common.Core.Emails.Core;
using Ozytis.Common.Core.RazorTemplating;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class ReviewsManager : BaseEntityManager<Review>
    {
        public ReviewsManager(DataContext dataContext, DynamicRazorEngine dynamicRazorEngine, IConfiguration configuration) : base(dataContext)
        {
            this.DynamicRazorEngine = dynamicRazorEngine;
            this.Configuration = configuration;
        }

        public DynamicRazorEngine DynamicRazorEngine { get; }
        public IConfiguration Configuration { get; }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            review.Id = Guid.NewGuid();
            review.CreationDate = DateTime.UtcNow;
            review.Status = ReviewStatus.Pending;
            review.LastUpdateDate = DateTime.UtcNow;

            review = this.DataContext.Add(review).Entity;

            await this.SaveChangesAsync();

            review = await this.SelectAll()
                .Include(r => r.Commentator)
                .Include(r => r.Recipient)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            var model = new NewReviewModel
            {
                Review = review
            };

            string html = await this.DynamicRazorEngine.GetHtmlAsync($"Business,Business.Emails.NewReview.cshtml", model);
            MimeMessage mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(this.Configuration["Data:Emails:DefaultSenderName"], this.Configuration["Data:Emails:DefaultSenderEmail"]));
            mail.Subject = "New code review";
            mail.AddBody(null, html);

            mail.To.Add(new MailboxAddress($"{review.Recipient.FirstName} {review.Recipient.LastName}", review.Recipient.Email));
            await mail.SendWithSmtpAsync();

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

            review = await this.SelectAll()
               .Include(r => r.Commentator)
               .Include(r => r.Recipient)
               .FirstOrDefaultAsync(r => r.Id == review.Id);

            var model = new ReviewUpdatedModel
            {
                Review = review,
                Commentator = updaterId == review.CommentatorId ? review.Commentator : review.Recipient,
                Recipient = updaterId == review.RecipientId ? review.Commentator : review.Recipient,
            };

            string html = await this.DynamicRazorEngine.GetHtmlAsync($"Business,Business.Emails.ReviewUpdated.cshtml", model);
            MimeMessage mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(this.Configuration["Data:Emails:DefaultSenderName"], this.Configuration["Data:Emails:DefaultSenderEmail"]));
            mail.Subject = "Code review updated";
            mail.AddBody(null, html);

            mail.To.Add(new MailboxAddress($"{model.Recipient.FirstName} {model.Recipient.LastName}", model.Recipient.Email));
            await mail.SendWithSmtpAsync();

            return review;
        }
    }
}
