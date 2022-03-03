using BlazorPro.BlazorSize;

using Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

using Ozytis.Common.Core.ClientApi;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Utilities;

using Syncfusion.Blazor;

using WebClient;
using WebClient.Repositories;
using WebClient.Security;

namespace WebClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(SyncfusionKey.Key);

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            BaseService.BaseUrl = builder.HostEnvironment.BaseAddress;
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddSingleton<AuthenticationStateProvider, WebClient.Security.OzytisAuthStateProvider>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, WebClient.Security.OzytisAuthPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationService>(sp => new WebClient.Security.OzytisAuthService()
            {
                Services = sp
            });

            builder.Services.AddSingleton<OzytisAuthStateProvider>(sp => sp.GetService<AuthenticationStateProvider>() as OzytisAuthStateProvider);

            builder.Services.AddSingleton<LocalStorageService>();

            builder.Services.AddSingleton<NavigationWithHistoryManager>();
            builder.Services.AddSingleton<AdditionalAssemblyProvider>();

            builder.Services.AddSingleton<ComponentMapping>(new ComponentMapping() {
                { Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI.ComponentMappingPart.Menu, typeof(WebClient.Shared.MainMenu) },
                { Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI.ComponentMappingPart.UserBox, typeof(WebClient.Shared.UserBox) },
            });

            builder.Services.AddAuthorizationCore(options =>
                            options.AddPolicy(Policy.UserIsAdministrator, policy => policy.RequireRole(UserRoleNames.Administrator)));

            foreach (Type manager in typeof(UsersRepository).Assembly.GetTypes().Where(t => t.Name.EndsWith("Repository") && !t.IsAbstract))
            {
                builder.Services.AddSingleton(manager);
            }

            builder.Services.AddSingleton<ResizeListener>();
            builder.Services.AddSingleton<IMediaQueryService, MediaQueryService>();

            builder.Services.AddSyncfusionBlazor(options =>
            {

            });

            WebAssemblyHost host = builder.Build();

            await CommonScriptLoader.EnsureOzytisCommonIsPresentAsync(host.Services.GetService<IJSRuntime>());

            string token = await host.Services.GetService<LocalStorageService>().GetItemAsync<string>("token");
            BaseService.SetBearerToken(token);


            await host.RunAsync();
        }
    }
}
