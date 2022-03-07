using Api;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Ozytis.Common.Core.Web.Razor.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebClient.Pages.User;

using WebClient.Repositories;

namespace WebClient.Shared
{
    public partial class UserBox : ComponentBase
    {
        [Inject]
        public UsersRepository UsersRepository { get; set; }

        [Inject]
        public NavigationWithHistoryManager NavigationManager { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AuthenticationState state = await this.AuthenticationStateTask;
            System.Security.Claims.ClaimsPrincipal user = state.User;

            this.Email = this.UsersRepository.CurrentUser?.Email ?? user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            this.User = this.UsersRepository.CurrentUser;
            //this.IsConnectedAs = await this.UsersRepository.IsUserConnectedAsAsync();

            //this.UsersRepository.OnCurrentUserUpdated += this.UsersRepository_OnCurrentUserUpdated;            


            await base.OnInitializedAsync();
        }

        private void UsersRepository_OnCurrentUserUpdated(object sender, System.EventArgs e)
        {
            this.User = this.UsersRepository.CurrentUser;
            
            this.StateHasChanged();
        }

        public string Email { get; set; }

        public UserModel User { get; set; }

        //public bool IsConnectedAs { get; set; }

        public async Task LogOutAsync()
        {
            await this.UsersRepository.LogoutAsync();
            this.User = null;
            this.StateHasChanged();
            this.NavigationManager.NavigateTo(LoginPage.Url);
        }


    }
}
