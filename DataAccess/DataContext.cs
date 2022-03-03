using Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole, string,
      IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<ReviewComment> ReviewComments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(options => {
         
            });
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUserRole>().HasKey(p => new { p.UserId, p.RoleId });

            builder.Entity<ApplicationUser>()
                .HasMany(user => user.UserRoles)
                .WithOne().HasForeignKey("UserId");

            builder.Entity<ApplicationUser>()
                .HasMany(a => a.UserRoles)
                .WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId);

            builder.Entity<ApplicationUserRole>()
                .HasOne(a => a.Role)
                .WithMany().HasForeignKey(ur => ur.RoleId);

        }
    }
}