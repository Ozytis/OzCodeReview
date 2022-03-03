using Api;

using Microsoft.AspNetCore.Components.Authorization;

using Ozytis.Common.Core.ClientApi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using WebClient.Repositories;

namespace WebClient.Security
{
    public class OzytisAuthStateProvider : AuthenticationStateProvider
    {
        public OzytisAuthStateProvider(IServiceProvider services) : base()
        {
            this.Services = services;
        }

        public IServiceProvider Services { get; internal set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            UsersRepository usersRepository = this.Services.GetService<UsersRepository>();

            if (usersRepository.CurrentUser == null && !string.IsNullOrEmpty(BaseService.BearerToken))
            {
                await usersRepository.RefreshCurrentUserAsync(true);
            }

            if (usersRepository.CurrentUser == null)
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.Name, $"{usersRepository.CurrentUser.FirstName} {usersRepository.CurrentUser.LastName}"),
                 new Claim(ClaimTypes.Email, usersRepository.CurrentUser.Email),
            };

            if (!string.IsNullOrEmpty(usersRepository.CurrentUser.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, usersRepository.CurrentUser.Role));
            }

            var identity = new ClaimsIdentity(claims.ToArray(), "Ozytis");

            var user = new ClaimsPrincipal(identity);

            await Task.CompletedTask;

            return new AuthenticationState(user);
        }

        public void NotifyStateChanged(UserModel user)
        {
            this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        }
    }
}
