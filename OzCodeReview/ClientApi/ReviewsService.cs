using OzCodeReview.ClientApi.Models;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi
{
    [Export]
    public class ReviewsService : BaseService
	{
		public async Task<OperationResult<Review>> CreateReviewAsync(ReviewCreationModel creationModel, Expression<Action> onConnectionRetrievedCallBack = null)
		{
			string url = $"api/reviews/";
			var result = await this.PostAsync<Review>(url, creationModel, onConnectionRetrievedCallBack: onConnectionRetrievedCallBack);
			return result;
		}

        public async Task<OperationResult<Review>> UpdateReviewAsync(ReviewUpdateModel model, Expression<Action> onConnectionRetrievedCallBack = null)
        {
            string url = $"api/reviews/";
            var result = await this.PutAsync<Review>(url, model, onConnectionRetrievedCallBack: onConnectionRetrievedCallBack);
            return result;
        }

        public async Task<Review[]> GetAllReviewsAsync(string solution)
        {
            string url = $"api/reviews/";
            url += "?oz=oz";

            if (solution != default)
            {
                url += $"&solution={Uri.EscapeDataString(solution.ToString())}";
            }

            var result = await this.GetAsync<Review[]>(url);
            return result;
        }

        public event EventHandler<IEnumerable<Review>> OnReviewChanged;
       
        public List<Review> Reviews { get; private set; }

        internal async Task LoadReviewsAsync(string solutionName)
        {
            var reviews = await this.GetAllReviewsAsync(solutionName);
            this.Reviews = reviews?.ToList();

            try
            {
                this.OnReviewChanged?.Invoke(this, this.Reviews);
            }
            catch
            {

            }
        }
    }
}
