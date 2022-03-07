using Api;

using Business;

using Common;

using Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Ozytis.Common.Core.Utilities;
using Ozytis.Common.Core.Web.WebApi;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        public AuthController(IConfiguration configuration, UsersManager usersManager, SignInManager<ApplicationUser> signInManager)
        {
            this.Configuration = configuration;
            this.UsersManager = usersManager;
            this.SignInManager = signInManager;
        }

        public IConfiguration Configuration { get; }

        public UsersManager UsersManager { get; }

        public SignInManager<ApplicationUser> SignInManager { get; }

        [HttpPost("login"), ValidateModel, HandleBusinessException, AllowAnonymous]
        public async Task<LoginResult> Login([FromBody] LoginModel model)
        {
            if (model == null)
            {
                throw new BusinessException("Invalid data");
            }

            Microsoft.AspNetCore.Identity.SignInResult result = null;

            try
            {
                result = await this.SignInManager.PasswordSignInAsync(model.UserName, model.Password, true, false);
            }
            catch (Exception)
            {
                throw new BusinessException("Database error");
            }

            if (!result.Succeeded)
            {
                throw new BusinessException("Identifiants invalides");
            }


            var user = await this.UsersManager.SelectAllUsers().Select(u => new
            {
                User = u,
                Roles = u.UserRoles.Select(ur => ur.Role),
            }).FirstOrDefaultAsync(u => u.User.UserName == model.UserName);


            List<Claim> claims = (await this.UsersManager.GetClaimsAsync(user.User)).ToList();
            claims = AuthController.GenerateClaims(user.User, claims).ToList();

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }


            byte[] key = Encoding.ASCII.GetBytes(this.Configuration["Authentication:AuthenticationTokenSecretSigningKey"]);
            var tokenSigningKey = new SymmetricSecurityKey(key);
            DateTime expirationDate = DateTime.UtcNow.Add(new TimeSpan(365, 0, 0, 0));

            JwtSecurityToken jwt = new JwtSecurityToken
            (
                 claims: claims,
                 expires: DateTime.UtcNow.Add(TimeSpan.FromDays(365)),
                 signingCredentials: new SigningCredentials(tokenSigningKey, SecurityAlgorithms.HmacSha256)
             );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string wroteToken = tokenHandler.WriteToken(jwt);

            LoginResult response = new LoginResult
            {
                AccessToken = wroteToken,
                AccessTokenValidUntil = expirationDate,
                User = new UserModel
                {
                    Id = user.User.Id,
                    UserName = user.User.UserName,
                    Email = user.User.Email,
                    Role = user.Roles.Select(r => r.Name).FirstOrDefault(),
                    FirstName = user.User.FirstName,
                    LastName = user.User.LastName,
                }
            };

            return response;
        }

        [HttpPost("sendPasswordIsLost"), ValidateModel, AllowAnonymous]
        public async Task<SendPasswordIsLostResult> SendPasswordIsLost([FromBody] SendPasswordIsLostModel model)
        {
            if (model == null)
            {
                throw new BusinessException("Données invalides");
            }

            string[] emails = await this.UsersManager.SendPasswordIsLostAsync(model.Email, null);

            return new SendPasswordIsLostResult
            {
                Emails = emails
            };
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<UserModel> GetCurrentUserAsync()
        {
            var user = await this.UsersManager.SelectAllUsers().Select(u => new
            {
                User = u,
                Roles = u.UserRoles.Select(ur => ur.Role)
            }).FirstOrDefaultAsync(u => u.User.UserName == this.User.Identity.Name);

            return new UserModel
            {
                Id = user.User.Id,
                UserName = user.User.UserName,
                Email = user.User.Email,
                Role = user.Roles.Select(r => r.Name).FirstOrDefault(),
                FirstName = user.User.FirstName,
                LastName = user.User.LastName,
            };
        }

        [HttpGet]
        [WebApiQueryable2(nameof(ApplicationUser))]
        [Authorize]
        public async Task<UserModel[]> GetAllUsersAsync()
        {
            var users = await this.UsersManager
                .SelectAllUsers()
                .Where(u => !u.UserRoles.Any(ur => ur.Role.Name == UserRoleNames.Administrator))
                .ApplyQueryV2Options(this.Request)
                .Select(u => new
                {
                    User = u,
                    Roles = u.UserRoles.Select(ur => ur.Role)
                }).ToArrayAsync();

            return users.Select(user => new UserModel
            {
                Id = user.User.Id,
                UserName = user.User.UserName,
                Email = user.User.Email,
                Role = user.Roles.Select(r => r.Name).FirstOrDefault(),
                FirstName = user.User.FirstName,
                LastName = user.User.LastName,
            }).ToArray();
        }

        [AllowAnonymous, HttpPut("resetpassword"), ValidateModel, HandleBusinessException]
        public async Task PasswordResetAsync([FromBody] PasswordResetModel model)
        {
            if (model == null)
            {
                throw new BusinessException("Données invalides");
            }

            ApplicationUser user = await this.UsersManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                throw new BusinessException("Données invalides");
            }

            Console.WriteLine($"{model.Token}");

            IdentityResult result = await this.UsersManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (!result.Succeeded)
            {
                throw new BusinessException(result.Errors.Select(e => e.Description).ToArray());
            }
        }

        [HttpPost]
        [ValidateModel, HandleBusinessException]
        [Authorize(Policy = Policy.UserIsAdministrator)]
        public async Task<UserModel> RegisterAsync([FromBody] UserCreationModel model)
        {
            bool currentUserIsAdmin = this.User.Identity.IsAuthenticated && await this.UsersManager
                .SelectAllUsers().Select(u => new
                {
                    User = u,
                    Roles = u.UserRoles.Select(ur => ur.Role)
                }).AnyAsync(u => u.User.UserName == this.User.Identity.Name && u.Roles.Select(ur => ur.Name).Contains(UserRoleNames.Administrator));

            ApplicationUser user;

            user = await this.UsersManager.CreateUserAsync(new ApplicationUser
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            });


            var result = await this.UsersManager.SelectAllUsers().Select(u => new
            {
                User = u,
                Roles = u.UserRoles.Select(ur => ur.Role)
            }).FirstOrDefaultAsync(user => user.User.Email == model.Email);

            return new UserModel
            {
                Id = result.User.Id,
                UserName = result.User.UserName,
                Email = result.User.Email,
                Role = result.Roles.Select(r => r.Name).FirstOrDefault(),
                FirstName = result.User.FirstName,
                LastName = result.User.LastName,
            };
        }

        [HttpDelete("{userId}")]
        [ValidateModel, HandleBusinessException]
        [Authorize(Policy = Policy.UserIsAdministrator)]
        public async Task RemoveUserAsync(string userId)
        {
            await this.UsersManager.DeleteUserAsync(userId);
        }

        [HttpPut("myaccount")]
        [ValidateModel, HandleBusinessException]
        [Authorize]
        public async Task<UserModel> UpdateMyAccountAsync([FromBody] UserUpdateModel model)
        {
            ApplicationUser user = new ApplicationUser
            {
                LastName = model.LastName,
                FirstName = model.FirstName,                
            };

            await this.UsersManager.UpdateUserSelfAccountAsync(user, this.User.Identity.Name);
            
            return await this.GetCurrentUserAsync();
        }

        [HttpPut("email")]
        [ValidateModel, HandleBusinessException]
        [Authorize]
        public async Task RequestEmailChangeAsync([FromBody] EmailUpdateModel model)
        {
            var user = await this.UsersManager.FindByNameAsync(this.User.Identity.Name);

            if (user == null)
            {
                return;
            }

            await this.UsersManager.RequestEmailChangeAsync(user, model.Email);
        }

        [HttpPut("mypassword")]
        [ValidateModel, HandleBusinessException]
        [Authorize]
        public async Task UpdateMyPasswordAsync([FromBody] PasswordUpdateModel model)
        {
            var user = await this.UsersManager.FindByNameAsync(this.User.Identity.Name);

            if (user == null)
            {
                return;
            }

            var result = await this.UsersManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                throw new BusinessException(result.Errors.Select(e => e.Description).ToArray());
            }
        }

        private static IEnumerable<Claim> GenerateClaims(ApplicationUser user, List<Claim> claims)
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToUnixStamp().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64));

            return claims;
        }
    }
}
