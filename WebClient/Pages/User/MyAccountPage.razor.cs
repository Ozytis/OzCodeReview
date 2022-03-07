using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebClient.Repositories;

namespace WebClient.Pages.User
{
    [Route(MyAccountPage.Url)]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public partial class MyAccountPage : ComponentBase
    {
        public const string Url = "/mon-compte";

        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public string Email { get; set; }

        protected override void OnInitialized()
        {
            this.Email = this.UsersRepository.CurrentUser.Email;
            base.OnInitialized();
        }

        public bool ShowEmailEditionModal { get; set; }

        public async Task CloseEmailEditionAsync()
        {
            await this.UsersRepository.RefreshCurrentUserAsync();
            this.Email = this.UsersRepository.CurrentUser.Email;
            this.ShowEmailEditionModal = false;
            this.StateHasChanged();
        }       
    }
}
