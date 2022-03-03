using Microsoft.AspNetCore.Authorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using WebClient.Repositories;

namespace WebClient.Security
{
    public class OzytisAuthService : IAuthorizationService
    {
        public IServiceProvider Services { get; internal set; }

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            await Task.CompletedTask;
            return this.Services.GetService<UsersRepository>().CurrentUser != null ? AuthorizationResult.Success() : AuthorizationResult.Failed();
        }

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
        {
            await Task.CompletedTask;
            return this.Services.GetService<UsersRepository>().CurrentUser != null ? AuthorizationResult.Success() : AuthorizationResult.Failed();
        }
    }
}
