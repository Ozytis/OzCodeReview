
using Api;

using Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using WebClient.Repositories;

namespace WebClient.Pages.Users
{
    [Authorize(Policy = Policy.UserIsAdministrator)]
    public partial class AddUserForm : ComponentBase
    {
        [Parameter]
        public EventCallback OnCancelled { get; set; }

        [Parameter]
        public EventCallback<UserModel> OnSuccess { get; set; }

        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public UserCreationModel Model { get; set; } = new();

        public bool IsProcessing { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public async Task ProcessAsync()
        {
            if (this.IsProcessing)
            {
                return;
            }

            this.IsProcessing = true;
            this.StateHasChanged();

            var user = await this.UsersRepository.CreateUserAsync(this.Model);
       
            MainLayout.Notify(Color.Success, "L'utilisateur a été créé, il va recevoir un mail d'invitation.");
         
            await this.OnSuccess.InvokeAsync(user);
        }


    }
}
