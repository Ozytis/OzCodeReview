global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;

global using System;

global using Task = System.Threading.Tasks.Task;

using EnvDTE;

using EnvDTE80;

using Microsoft;
using Microsoft.VisualStudio;
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
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "OzCodeReview", "GeneralOptions", 0, 0, true)]
    [ProvideProfile(typeof(OptionsProvider.GeneralOptions), "OzCodeReview", "GeneralOptions", 0, 0, true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(CodeCommentReviewsWindow.Pane))]
    [Guid(PackageGuids.OzCodeReviewString)]
    public sealed class OzCodeReviewPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel));

            Assumes.Present(componentModel);
                     
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();


            UsersService usersService = componentModel.GetService<UsersService>();

            this.AddService(typeof(UsersService), async (container, cancellationToken, type) =>
            {
                await Task.CompletedTask;
                return usersService;
            });

            await this.CheckSettingsAsync();

            ReviewsService reviewsService = componentModel.GetService<ReviewsService>();

            string solutionName = (await VS.Solutions.GetCurrentSolutionAsync()).Name;
            await reviewsService.LoadReviewsAsync(solutionName);

            this.AddService(typeof(ReviewsService), async (container, cancellationToken, type) =>
            {
                await Task.CompletedTask;
                return reviewsService;
            });
        }

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

            var usersService = await this.GetServiceAsync(typeof(UsersService)) as UsersService;

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
}