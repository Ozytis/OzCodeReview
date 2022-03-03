using Api;

using Common;

using Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;
using Ozytis.Common.Core.Web.Razor.Utilities;

using System.Threading.Tasks;

using WebClient.Repositories;

namespace WebClient.Pages.Users
{
    [Route(UsersPage.Url)]
    [Authorize(Policy = Policy.UserIsAdministrator)]
    public partial class UsersPage : ComponentBase
    {
        public const string Url = "/utilisateurs";

        [Inject]
        public UsersRepository UsersRepository { get; set; }

        [Inject]
        public NavigationWithHistoryManager NavigationManager { get; set; }

        public bool ShowUserCreation { get; set; }

        public UsersPage()
        {

        }

        public OzSimpleRemoteTable<ApplicationUser, UserModel> Table { get; set; }


        public UserModel[] Users { get; set; }

        public async Task DeleteAsync(UserModel user)
        {
            if (!await Confirm.AskAsync($"Are you sure you want to remove {user.FirstName} {user.LastName} from users ?"))
            {
                return;
            }

            try
            {
                await this.UsersRepository.RemoveUserAsync(user.Id);
            }
            catch (BusinessException ex)
            {
                MainLayout.Notify(Color.Danger, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            this.Table.RemoveEntity(user);
            this.StateHasChanged();
        }

        //public async Task ConnectAs(UserModel user)
        //{
        //    if (!await Confirm.AskAsync($"Etes vous sûr de vouloir vous connecter avec le compte de {user.FirstName} {user.LastName} ?"))
        //    {
        //        return;
        //    }

        //    await this.UsersRepository.ConnectAs(user.Id);
        //    this.NavigationManager.NavigateTo(IndexPage.Url, true);
        //}
    }
}
