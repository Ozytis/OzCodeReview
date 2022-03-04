global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;

global using System;

global using Task = System.Threading.Tasks.Task;

using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;

using OzCodeReview.ClientApi;

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OzCodeReview
{
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "OzCodeReview", "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionsProvider.GeneralOptions), "OzCodeReview", "GeneralOptions", 0, 0, true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(CodeCommentReviewsWindow.Pane))]
    [Guid(PackageGuids.OzCodeReviewString)]
    public sealed class OzCodeReviewPackage : ToolkitPackage
    {
        public async Task<bool> CheckSettingsAsync()
        {
            var options = await General.GetLiveInstanceAsync();

            if (string.IsNullOrEmpty(options.ServerUrl))
            {
                await VS.MessageBox.ShowWarningAsync("OzCodeReview", "The server url has not been defined in OzCodeReview");
                this.ShowOptionPage(typeof(General));
                return false;
            }

            BaseService.BaseUrl = options.ServerUrl.EndsWith("/") ? options.ServerUrl : options.ServerUrl + "/";

            if (string.IsNullOrEmpty(options.Email) || string.IsNullOrEmpty(options.Password))
            {
                await VS.MessageBox.ShowWarningAsync("OzCodeReview", "The connection email or password have not been defined in OzCodeReview");
                this.ShowOptionPage(typeof(General));
                return false;
            }
            else
            {
                var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
                var usersService = componentModel.GetService<UsersService>();

                var hiddenOptions = await Hidden.GetLiveInstanceAsync();

                if (string.IsNullOrEmpty(hiddenOptions.Token))
                {
                    Assumes.Present(usersService);

                    var loginResult = await usersService.LoginAsync(new ClientApi.Models.LoginModel
                    {
                        Password = options.Password,
                        UserName = options.Email
                    });

                    if (!loginResult.Success)
                    {
                        await VS.MessageBox.ShowErrorAsync(string.Join(", ", loginResult.Errors));
                        return false;
                    }

                    hiddenOptions.Token = loginResult.Data.AccessToken;
                    await hiddenOptions.SaveAsync();
                }

                BaseService.SetBearerToken(hiddenOptions.Token);

                await usersService.LoadUsersAsync();

                return true;
            }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            General.Saved += this.General_Saved;

            BaseService.OnAuthorizeRequired = this.OnAuthorizationRequiredAsync;


            var componentModel = (IComponentModel)await this.GetServiceAsync(typeof(SComponentModel));

            Assumes.Present(componentModel);

            try
            {
                await this.RegisterCommandsAsync();
                this.RegisterToolWindows();
            }
            catch
            {
                // occurs when tools winows have already been initialized
            }

            UsersService usersService = componentModel.GetService<UsersService>();

            if (!await this.CheckSettingsAsync())
            {
                return;
            }

            ReviewsService reviewsService = componentModel.GetService<ReviewsService>();

            string solutionName = (await VS.Solutions.GetCurrentSolutionAsync()).Name;
            await reviewsService.LoadReviewsAsync(solutionName);
        }

        private async Task<bool> OnAuthorizationRequiredAsync()
        {
            BaseService.SetBearerToken(null);

            var hiddenOptions = await Hidden.GetLiveInstanceAsync();
            hiddenOptions.Token = null;
            await hiddenOptions.SaveAsync();

            return await this.CheckSettingsAsync();
        }


#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void General_Saved(General obj)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            if (await this.CheckSettingsAsync())
            {
                var componentModel = (IComponentModel)await this.GetServiceAsync(typeof(SComponentModel));

                Assumes.Present(componentModel);

                ReviewsService reviewsService = componentModel.GetService<ReviewsService>();

                Assumes.Present(reviewsService);

                string solutionName = (await VS.Solutions.GetCurrentSolutionAsync()).Name;
                await reviewsService.LoadReviewsAsync(solutionName);
            }
        }
    }
}