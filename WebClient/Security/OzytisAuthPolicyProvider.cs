using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClient.Security
{
    public class OzytisAuthPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public OzytisAuthPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public new async Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            await Task.CompletedTask;

            return new AuthorizationPolicy(new[] {
               new Microsoft.AspNetCore.Authorization.Infrastructure.DenyAnonymousAuthorizationRequirement()
            }, new string[0]);
        }

        public new async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            switch (policyName)
            {
                default:
                    return await base.GetPolicyAsync(policyName);
            }
        }
    }
}
