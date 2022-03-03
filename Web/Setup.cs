using Business;

using Common;

using Hangfire;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Web
{
    public class Setup
    {
        public Setup(IServiceProvider serviceProvider)
        {

            this.ServiceProvider = serviceProvider;

            this.UsersManager = this.ServiceProvider.GetService<UsersManager>();
            this.RoleManager = this.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        }

        public UsersManager UsersManager { get; set; }

        public RoleManager<IdentityRole> RoleManager { get; set; }



        public IServiceProvider ServiceProvider { get; set; }

        public void Init()
        {


            string job = BackgroundJob.Enqueue(() => this.CheckRoles());
            job = BackgroundJob.ContinueJobWith(job, () => this.CreateAdmin());
        }
        public async Task CheckRoles()
        {
            Console.WriteLine("Checking roles");

            try
            {
                string[] roles = new[] {
                    UserRoleNames.Administrator,
                };

                foreach (var role in roles)
                {
                    if (!await this.RoleManager.RoleExistsAsync(role))
                    {
                        await this.RoleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task CreateAdmin()
        {
            Console.WriteLine("Checking admin");

            try
            {
                var count = this.UsersManager.Users.Count();

                if (count == 0)
                {
                    await this.UsersManager.CreateAsync(new Entities.ApplicationUser
                    {
                        Email = "admin@ozcodereview.fr",
                        EmailConfirmed = true,
                        UserName = "admin@ozcodereview.fr",
                        LastName = "Admin",
                        FirstName = "OzCodeReview"
                    }, "ozCodeReview@*");

                    var user = await this.UsersManager.SelectAllUsers().FirstOrDefaultAsync();

                    await this.UsersManager.AddClaimAsync(user, new System.Security.Claims.Claim(Policy.UserIsAdministrator, "true"));
                    await this.UsersManager.AddToRoleAsync(user, UserRoleNames.Administrator);

                    await this.UsersManager.UpdateAsync(user);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
