using Api;

using Microsoft.AspNetCore.Components;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using WebClient.Repositories;

namespace WebClient.Pages.User
{
    public partial class MyPassword : ComponentBase
    {
        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public string[] Errors { get; set; }

        public PasswordUpdateModel Model { get; set; } = new PasswordUpdateModel();

        public bool IsUpdating { get; set; }

        public string NewPasswordConfirmation { get; set; }

        protected async Task SaveAsync()
        {
            if (this.IsUpdating)
            {
                return;
            }

            this.Errors = null;

            if (this.NewPasswordConfirmation != this.Model.NewPassword)
            {
                this.Errors = new[] { "The new password does not match its confirmation." };
            }


            this.IsUpdating = true;

            try
            {
                await this.UsersRepository.UpdatePasswordAsync(this.Model);
                MainLayout.Notify(Color.Success, "Your password has been updated");
            }
            catch (BusinessException ex)
            {
                this.Errors = ex.Messages;
            }

            this.IsUpdating = false;
        }
    }
}
