using Api;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using WebClient.Repositories;

namespace WebClient.Pages.User
{
    public partial class MyAccountInfo : ComponentBase
    {
        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public string[] Errors { get; set; }

        public bool IsEditing { get; set; }

        public bool IsUpdating { get; set; }

        public UserModel UserModel { get; set; }

        public UserUpdateModel UpdateModel { get; set; }

        protected void ChangeEditingMode()
        {
            this.Errors = null;
            this.ReinitUpdateModel();
            this.IsEditing = !this.IsEditing;
        }

        protected override async Task OnInitializedAsync()
        {
            this.ReinitUpdateModel();
            await base.OnInitializedAsync();
        }

        protected void ReinitUpdateModel()
        {
            this.UserModel = UsersRepository.CurrentUser;

            this.UpdateModel = new UserUpdateModel
            {
                FirstName = this.UserModel.FirstName,
                LastName = this.UserModel.LastName
            };
        }

        protected async Task SaveAsync()
        {
            if (this.IsUpdating)
            {
                return;
            }

            this.IsUpdating = true;
            this.Errors = null;

            try
            {
                await this.UsersRepository.UpdateCurrentUserAsync(this.UpdateModel);
                MainLayout.Notify(Color.Success, "Vos informations ont bien été mises à jour");
                this.IsEditing = false;
                this.ReinitUpdateModel();
            }
            catch (BusinessException ex)
            {
                this.Errors = ex.Messages;
            }

            this.IsUpdating = false;
        }
    }
}
