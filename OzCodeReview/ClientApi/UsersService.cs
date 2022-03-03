using OzCodeReview.ClientApi.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi
{
    [Export]
    public class UsersService : BaseService
    {
        public async Task<OperationResult<LoginResult>> LoginAsync(LoginModel model, bool anonymous = false, Expression<Action> onConnectionRetrievedCallBack = null)
        {
            string url = $"api/auth/login";
            var result = await this.PostAsync<LoginResult>(url, model, anonymous: anonymous, onConnectionRetrievedCallBack: onConnectionRetrievedCallBack);
            return result;
        }

        public async Task<OperationResult<SendPasswordIsLostResult>> SendPasswordIsLostAsync(SendPasswordIsLostModel model, bool anonymous = false, Expression<Action> onConnectionRetrievedCallBack = null)
        {
            string url = $"api/auth/sendPasswordIsLost";
            var result = await this.PostAsync<SendPasswordIsLostResult>(url, model, anonymous: anonymous, onConnectionRetrievedCallBack: onConnectionRetrievedCallBack);
            return result;
        }

        public async Task<UserModel> GetCurrentUserAsync()
        {
            string url = $"api/auth/current";
            var result = await this.GetAsync<UserModel>(url);
            return result;
        }

        public async Task<UserModel[]> GetAllUsersAsync()
        {
            string url = $"api/auth/";
            var result = await this.GetAsync<UserModel[]>(url);
            return result;
        }

        public EventHandler<IEnumerable<UserModel>> OnUsersChanged { get; set; }

        public List<UserModel> Users { get; private set; }

        public async Task LoadUsersAsync()
        {
            this.Users = (await this.GetAllUsersAsync()).ToList();
            this.OnUsersChanged?.Invoke(this, this.Users);
        }
    }
}
