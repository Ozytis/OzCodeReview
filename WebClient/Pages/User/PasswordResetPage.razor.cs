using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using System.Threading.Tasks;
using WebClient.Repositories;
using WebClient.Shared;

namespace WebClient.Pages.User
{
    [Route(PasswordResetPage.Url)]
    [AllowAnonymous]
    public partial class PasswordResetPage : ComponentBase
    {
        public const string Url = "/password-reset";

        public PasswordResetPage()
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
        public string Email { get; set; } = "";

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
                AnonymousLayout.Notify(Color.Success, "Votre mot de passe a été modifié");
                await Task.Delay(2000);
                this.NavigationManager.NavigateTo(LoginPage.Url);
            }
            catch (BusinessException ex)
            {
                this.Errors = ex.Messages;
            }

            this.IsProcessing = false;
        }
    }
}
