using Api;

using Business;

using Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Ozytis.Common.Core.Web.WebApi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Authorize]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        public ReviewsController(ReviewsManager reviewsManager, UsersManager usersManager)
        {
            this.ReviewsManager = reviewsManager;
            this.UsersManager = usersManager;
        }

        public ReviewsManager ReviewsManager { get; }
        public UsersManager UsersManager { get; }

        [HttpPost]
        [ValidateModel, HandleBusinessException]
        public async Task<Review> CreateReview([FromBody] ReviewCreationModel creationModel)
        {
            var creator = await this.UsersManager.FindByEmailAsync(this.User.Identity.Name);

            return await this.ReviewsManager.CreateReviewAsync(new Review
            {
                Branch = creationModel.Branch,
                Comment = creationModel.Comment,
                CommentatorId = creator.Id,
                Commit = creationModel.Commit,
                EndLineNumber = creationModel.EndLineNumber,
                FileName = creationModel.FileName,
                LastCharIndex = creationModel.LastCharIndex,
                RecipientId = creationModel.RecipientId,
                SolutionName = creationModel.SolutionName,
                StartCharIndex = creationModel.StartCharIndex,
                StartLineNumber = creationModel.StartLineNumber,
                Type = creationModel.Type,
                ProjectPath = creationModel.ProjectPath,
            });
        }

        [HttpPut]
        [ValidateModel, HandleBusinessException]
        public async Task<Review> UpdateReviewAsync([FromBody]ReviewUpdateModel model)
        {
            var updater = await this.UsersManager.FindByEmailAsync(this.User.Identity.Name);

            return await this.ReviewsManager.UpdateReviewAsync(model.Id, model.ReviewStatus, model.ReviewType, model.Comment, updater.Id);
        }

        [HttpGet]
        public async Task<Review[]> GetAllComments(string solution)
        {
            var requester = await this.UsersManager.FindByEmailAsync(this.User.Identity.Name);

            return await this.ReviewsManager
                .SelectAll().Include(r => r.Comments)
                .Where(r => r.SolutionName == solution && (r.CommentatorId == requester.Id || r.RecipientId == requester.Id))
                .ToArrayAsync();
        }
    }
}
