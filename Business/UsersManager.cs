using Business.Emails;

using Common;

using DataAccess;

using Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using Ozytis.Common.Core.Emails.Core;
using Ozytis.Common.Core.Managers;
using Ozytis.Common.Core.RazorTemplating;
using Ozytis.Common.Core.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class UsersManager : UserManager<ApplicationUser>
    {
        public UsersManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger, DataContext dataContext, DynamicRazorEngine dynamicRazorEngine,
          IConfiguration configuration)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.DataContext = dataContext;
            this.DynamicRazorEngine = dynamicRazorEngine;
            this.Configuration = configuration;
        }

        public DataContext DataContext { get; private set; }

        public DynamicRazorEngine DynamicRazorEngine { get; }

        public IConfiguration Configuration { get; private set; }

        public IQueryable<ApplicationUser> SelectAllUsers(params Expression<Func<ApplicationUser, object>>[] includes)
        {
            return this.DataContext.Users.LoadIncludes(includes);
        }

        public async Task<string[]> SendPasswordIsLostAsync(string emailAddress, ApplicationUser sender, string linkTemplate = "")
        {
            linkTemplate = string.IsNullOrEmpty(linkTemplate) ?
                $"{this.Configuration["Data:Network:BaseUrl"]}password-reset?token={{0}}&email={{1}}"
                : linkTemplate;


            ApplicationUser user = await this.FindByEmailAsync(emailAddress);

            if (user == null)
            {
                throw new BusinessException("Adresse email inconnue");
            }

            string token = await this.GeneratePasswordResetTokenAsync(user);

            string link = string.Format(
                CultureInfo.InvariantCulture,
                linkTemplate
                    .Replace("%7B", "{", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("%7D", "}", StringComparison.InvariantCultureIgnoreCase),
                WebUtility.UrlEncode(token), WebUtility.UrlEncode(emailAddress));

            PasswordResetModel model = new PasswordResetModel { Link = link, UserFirstName = user.FirstName, UserLastName = user.LastName };

            string html = await this.DynamicRazorEngine.GetHtmlAsync($"Business,Business.Emails.PasswordReset.cshtml", model);
            MimeMessage mail = new MimeMessage();


            mail.From.Add(new MailboxAddress(this.Configuration["Data:Emails:DefaultSenderName"], this.Configuration["Data:Emails:DefaultSenderEmail"]));
            mail.Subject = "Password reset";
            mail.AddBody(null, html);

            mail.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", user.Email));
            await mail.SendWithSmtpAsync();

            return new[] { user.Email };
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user)
        {

            string email = user.Email?.Trim().ToLower();

            if (await this.DataContext.Users.AnyAsync(u => u.Email == email))
            {
                throw new BusinessException("Cet utilisateur existe déjà");
            }

            ApplicationUser newUser = new ApplicationUser
            {
                UserName = email,
                LastName = user.LastName,
                FirstName = user.FirstName,
                EmailConfirmed = true,
                Email = email
            };

            await this.CreateAsync(newUser);
            await this.DataContext.SaveChangesAsync();

            ApplicationUser dbUser = await this.DataContext.Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            string token = await this.GeneratePasswordResetTokenAsync(dbUser);

            Console.WriteLine($"Token {token}");

            string linkTemplate = $"{this.Configuration["Data:Network:BaseUrl"]}activate-account?token={{0}}&email={{1}}";

            string link = string.Format(
                CultureInfo.InvariantCulture,
                linkTemplate
                    .Replace("%7B", "{", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("%7D", "}", StringComparison.InvariantCultureIgnoreCase),
                WebUtility.UrlEncode(token), WebUtility.UrlEncode(email));

            RegistrationModel model = new RegistrationModel { Link = link, UserFirstName = user.FirstName, UserLastName = user.LastName };

            string html = await this.DynamicRazorEngine.GetHtmlAsync($"Business,Business.Emails.Registration.cshtml", model);
            MimeMessage mail = new MimeMessage();


            mail.From.Add(new MailboxAddress(this.Configuration["Data:Emails:DefaultSenderName"], this.Configuration["Data:Emails:DefaultSenderEmail"]));
            mail.Subject = "New OzCode review account";
            mail.AddBody(null, html);

            mail.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", user.Email));
            await mail.SendWithSmtpAsync();

            return dbUser;
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await this.SelectAllUsers().SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new BusinessException("Utilisateur inconnu");
            }


            this.DataContext.Users.Remove(user);

            await this.DataContext.SaveChangesAsync();
        }

        public async Task<ApplicationUser> UpdateUserSelfAccountAsync(ApplicationUser user, string currentUserLogin)
        {
            var dbUser = await this.FindByNameAsync(currentUserLogin);

            if (dbUser == null)
            {
                return null;
            }

            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;

            await this.SaveChangesAsync();

            return dbUser;
        }

        public async Task RequestEmailChangeAsync(ApplicationUser user, string emailAddress)
        {
            if (user == null)
            {
                throw new BusinessException("unknown email");
            }

            string token = await this.GenerateChangeEmailTokenAsync(user, emailAddress);

            Console.WriteLine($"{user.Email} - {emailAddress} {token}");

            string link = $"{this.Configuration["Data:Network:BaseUrl"]}confirm-email-change?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(emailAddress)}";

            var model = new EmailChangeRequestModel
            {
                User = user,
                Link = link
            };

            string html = await this.DynamicRazorEngine.GetHtmlAsync($"Business,Business.Emails.EmailChangeRequest.cshtml", model);

            MimeMessage email = new MimeMessage();
            email.From.Add(new MailboxAddress(this.Configuration["Data:Emails:DefaultSenderName"], this.Configuration["Data:Emails:DefaultSenderEmail"]));


            email.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", emailAddress));

            email.Subject = "OzCode Review - Change email request";
            email.AddBody(null, html);

            await email.SendWithSmtpAsync();
        }

        public async Task SaveChangesAsync()
        {
            await this.DataContext.SaveChangesAsync();
        }
    }
}
