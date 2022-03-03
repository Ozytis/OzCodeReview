
//----------------------
// <auto-generated>
//     Generated by Ozytis Model Generator  DO NOT EDIT!
// </auto-generated>
//----------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozytis.Common.Core.Api;
using Ozytis.Common.Core.ClientApi;
using System.Linq;
using Api;


using Entities;

using System.Linq.Expressions;

namespace ClientApi
{
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

		public async Task<Review[]> GetAllCommentsAsync(string solution)
		{
			string url = $"api/reviews/";
			url += "?oz=oz";

			if(solution != default)
			{
				url += $"&solution={Uri.EscapeDataString(solution.ToString())}";
			}
			

			var result = await this.GetAsync<Review[]>(url);
			return result;
		}

	}
}