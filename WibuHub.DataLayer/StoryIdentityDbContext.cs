using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;

namespace WibuHub.DataLayer
{
    public class StoryIdentityDbContext : IdentityDbContext<StoryUser, StoryRole, string>
    {
        public StoryIdentityDbContext(DbContextOptions<StoryIdentityDbContext> options)
            : base(options)
        {
        }
        public DbSet<StoryUser> StoryUsers { get; set; }
        public DbSet<StoryRole> StoryRoles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StoryUser>(img =>
            {
                img.Property(s => s.FullName)
                    //.IsRequired()
                    .HasMaxLength(MaxLengths.NAME);
                img.Property(s => s.Avatar)
                    .HasMaxLength(MaxLengths.FILE_NAME);

            });
            modelBuilder.Entity<StoryRole>(img =>
            {
                img.Property(s => s.Description)
                    //.IsRequired()
                    .HasMaxLength(MaxLengths.DESCRIPTION);

            });
        }
    }
}
