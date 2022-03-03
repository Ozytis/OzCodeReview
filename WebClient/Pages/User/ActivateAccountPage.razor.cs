using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using WebClient.Repositories;

namespace WebClient.Pages.User
{
    [Route(ActivateAccountPage.Url)]
    [AllowAnonymous]
    public partial class ActivateAccountPage : ComponentBase
    {
        public const string Url = "/activate-account";

        public ActivateAccountPage()
        {

        }

        protected override void OnInitialized()
        {
            var uri = this.NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var queryParameters = QueryHelpers.ParseQuery(uri.Query);

            if (queryParameters.TryGetValue("email", out var email))
            {
                this.Email = email;
            }

            if (queryParameters.TryGetValue("token", out var token))
            {
                this.Token = token;
            }

            base.OnInitialized();
        }

        [Parameter]
        public string Token { get; set; }

        [Parameter]
        public string Email { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public string[] Errors { get; set; }

        public bool IsProcessing { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirmation { get; set; }

        public async Task ProcessAsync()
        {
            if (this.IsProcessing)
            {
                return;
            }

            this.IsProcessing = true;
            this.Errors = null;

            if (this.NewPassword != this.NewPasswordConfirmation)
            {
                this.IsProcessing = false;
                this.Errors = new[] { "Le mot de passe et sa confirmation ne correspondent pas" };
                return;
            }

            try
            {
                await this.UsersRepository.ResetPasswordAsync(this.Email, this.Token, this.NewPassword);
                AnonymousLayout.Notify(Color.Success, "Votre mot de passe a été enregistré. Vous pouvez dès à présent vous connecter");
                await Task.Delay(2000);
                this.NavigationManager.NavigateTo(WebClient.Pages.User.LoginPage.Url);
            }
            catch (BusinessException ex)
            {
                this.Errors = ex.Messages?.Select(m => m.Replace(".,", ".")).ToArray();
            }

            this.IsProcessing = false;
        }
    }
}
