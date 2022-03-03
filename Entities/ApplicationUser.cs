using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class ApplicationUser : IdentityUser
    {
        public const int IdMaxLength = 450;

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}