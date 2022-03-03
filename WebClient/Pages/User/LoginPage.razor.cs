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
using Microsoft.AspNetCore.Authorization;

namespace WebClient.Pages.User
{
    [AllowAnonymous]
    [Route(LoginPage.Url)]
    public partial class LoginPage : ComponentBase
    {
        public const string Url = "/login";

        [Inject]
        public NavigationWithHistoryManager NavigationManager { get; set; }

        [Inject]
        public UsersRepository UsersRepository { get; set; }



        public void NavigateTo(string to)
        {
            this.NavigationManager.NavigateTo(to);
        }

        public string Email { get; set; }

        public string Password { get; set; }

        public string[] Errors { get; set; }

        public bool IsSubmitting { get; set; }

        public async Task SubmitLoginAsync()
        {
            if (this.IsSubmitting)
            {
                return;
            }

            this.Errors = null;
            this.IsSubmitting = true;

            this.StateHasChanged();

            try
            {
                await this.UsersRepository.LoginAsync(this.Email, this.Password);

                this.NavigateTo(IndexPage.Url);
            }
            catch (Ozytis.Common.Core.Utilities.BusinessException ex)
            {
                this.Errors = ex.Messages ?? new[] { ex.Message };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            this.IsSubmitting = false;

            this.StateHasChanged();
        }
    }
}