using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using WebClient;
using WebClient.Repositories;
using static Ozytis.Common.Core.Web.Razor.Utilities.BlazorHelper;
using BlazorPro.BlazorSize;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;
using Ozytis.Common.Core.Web.Razor.Layout.Bootstrap4;
using Ozytis.Common.Core.Web.Razor.Utilities;
using Ozytis.Common.Core.Api;
using FontAwesomeIcons = Ozytis.Common.Core.Web.Razor.FontAwesomeIcons;
using Entities;
using Api;
using Common;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using OzRazor = Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.ClientApi;
using System.Reflection;
using WebClient.Pages.User;

namespace WebClient
{
    public partial class App : ComponentBase
    {
        public App() : base()
        {
            BaseService.OnAuthorizeRequired += (sender, args) =>
            {
                this.NavigationManager.NavigateTo(LoginPage.Url);
            };
        }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationWithHistoryManager NavigationManager { get; set; }

        [Inject]
        public AdditionalAssemblyProvider AdditionalAssemblyProvider { get; set; }

        [Inject]
        public LocalStorageService LocalStorageService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Ozytis.Common.Core.Web.Razor.CommonScriptLoader.EnsureOzytisCommonIsPresentAsync(this.JSRuntime);        
            this.AdditionalAssemblies = this.AdditionalAssemblyProvider.GetAssemblies();

            Console.WriteLine("Additional assemblies loaded : " + string.Join(", ", this.AdditionalAssemblies.Select(a => a.FullName)));
            BlazorHelper.CurrentTheme.EnableClosedSidebar = true;
            //BlazorHelper.CurrentTheme.HeaderMobileOpen = true;
            BlazorHelper.CurrentTheme.Update();

            await base.OnInitializedAsync();
        }

        public Assembly[] AdditionalAssemblies { get; set; }
    }
}