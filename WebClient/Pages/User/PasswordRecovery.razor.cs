using Microsoft.AspNetCore.Components;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using System.Threading.Tasks;

using WebClient.Repositories;

namespace WebClient.Pages.User
{
    public partial class PasswordRecovery : ComponentBase
    {
        public string Email { get; set; }

        public string[] Errors { get; set; }

        public bool IsProcessing { get; set; }

        public bool IsSuccess { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public void NavigateTo(string to)
        {
            this.NavigationManager.NavigateTo(to);
        }

        public async Task SendPasswordRecoveryAsync()
        {
            if (this.IsProcessing)
            {
                return;
            }

            this.Errors = null;
            this.IsProcessing = true;

            try
            {
                await this.UsersRepository.SendPasswordRecoveryAsync(this.Email);
                this.IsSuccess = true;
                AnonymousLayout.Notify(Color.Success, "Un email contenant les instructions pour réinitialiser votre mot de passe vous a été envoyé");
                this.StateHasChanged();
                await Task.Delay(4000);

                this.NavigateTo(LoginPage.Url);
            }
            catch (BusinessException ex)
            {
                AnonymousLayout.Notify(Color.Danger, ex.Message);
            }

            this.IsProcessing = false;
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }
    }
}