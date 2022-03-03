using Api;

using ClientApi;
using Entities;

using Ozytis.Common.Core.ClientApi;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using WebClient.Security;

namespace WebClient.Repositories
{
    public class UsersRepository
    {
        private const string TokenKey = "token";

        private readonly AuthService accountsService = new();

        public UsersRepository(OzytisAuthStateProvider ozytisAuthStateProvider,LocalStorageService localStorageService)
        {
            this.StateProvider = ozytisAuthStateProvider;
            this.LocalStorageService = localStorageService;
        }

        public event EventHandler OnCurrentUserUpdated;

        public UserModel CurrentUser { get; set; }

        public OzytisAuthStateProvider StateProvider { get; }
        
        public LocalStorageService LocalStorageService { get; }


        public async Task LoginAsync(string login, string password)
        {
            this.CurrentUser = null;

            OperationResult<LoginResult> result = await this.accountsService.LoginAsync(new LoginModel
            {
                Password = password,             
                UserName = login
            }, true);

            if (!result.Success)
            {
                throw new BusinessException(result.Errors);
            }

            this.CurrentUser = result.Data.User;
            await this.LocalStorageService.SetItemAsync(TokenKey, result.Data.AccessToken);
            BaseService.SetBearerToken(result.Data.AccessToken);

            this.StateProvider.NotifyStateChanged(this.CurrentUser);
        }

        internal async Task RemoveUserAsync(string userId)
        {
            await this.accountsService.RemoveUserAsync(userId);
        }

        public async Task SendPasswordRecoveryAsync(string email)
        {
            OperationResult<SendPasswordIsLostResult> result = await this.accountsService.SendPasswordIsLostAsync(new SendPasswordIsLostModel { Email = email }, true);

            if (!result.Success)
            {
                throw new BusinessException(result.Errors);
            }
        }

        public async Task RefreshCurrentUserAsync(bool emitEvent = false)
        {
            this.CurrentUser = await this.accountsService.GetCurrentUserAsync();

            if (emitEvent)
            {
                this.OnCurrentUserUpdated?.Invoke(this, new EventArgs());
            }
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(Expression<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>> query)
        {
            try
            {
                Console.WriteLine("getting all users");
                var result = await this.accountsService.GetAllUsersAsync(query);           

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            await this.LocalStorageService.RemoveItemAsync(TokenKey);
            BaseService.SetBearerToken(null);
            this.CurrentUser = null;
        }

        public async Task<UserModel> CreateUserAsync(UserCreationModel model)
        {
            var result = await this.accountsService.RegisterAsync(model);

            if (!result.Success)
            {
                throw new BusinessException(result.Errors);
            }

            return result.Data;
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            OperationResult<object> result = await this.accountsService.PasswordResetAsync(new PasswordResetModel
            {
                Email = email,
                Password = newPassword,
                Token = token
            }, true);

            if (!result.Success)
            {
                throw new BusinessException(result.Errors);
            }
        }
    }
}
