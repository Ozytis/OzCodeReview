using Api;
using Microsoft.AspNetCore.Components;
using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.Razor;
using Ozytis.Common.Core.Web.Razor.Layout;
using Ozytis.Common.Core.Web.Razor.Layout.ArchitectUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebClient.Repositories;

namespace WebClient.Pages.User
{
    public partial class MyEmailEdition : ComponentBase
    {
        [Parameter]
        public Func<Task> OnCloseAsync { get; set; }

        [Inject]
        public UsersRepository UsersRepository { get; set; }

        public EmailUpdateModel Model { get; set; }

        public string[] Errors { get; set; }

        public bool IsProcessing { get; set; }

        public bool IsSuccess { get; set; }

        protected async Task ProcessAsync()
        {
            if (this.IsProcessing)
            {
                return;
            }

            this.IsProcessing = true;
            this.IsSuccess = false;
            this.Errors = null;

            try
            {
                await this.UsersRepository.RequestEmailChangeAsync(this.Model);
                
                MainLayout.Notify(Color.Success, $"Votre demande a bien été prise en compte. Un email contenant les instructions vous permettant de valider votre demande vous a été envoyé à l'adresse {this.Model.Email}");

                await this.OnCloseAsync();
                this.IsSuccess = true;               
            }
            catch (BusinessException ex)
            {
                this.Errors = ex.Messages;
            }

            this.IsProcessing = false;
        }

        protected override void OnInitialized()
        {
            this.Model = new EmailUpdateModel
            {
                Email = this.UsersRepository.CurrentUser.Email
            };

            base.OnInitialized();
        }
    }
}
