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

            modelBuilder.Entity<StoryUser>(user =>
            {
                user.Property(s => s.FullName)
                    //.IsRequired()
                    .HasMaxLength(MaxLengths.NAME);

                user.Property(s => s.Avatar)
                    .HasMaxLength(MaxLengths.FILE_NAME);

                // =========================================================
                // THÊM CẤU HÌNH MẶC ĐỊNH CHO HỆ THỐNG CẤP ĐỘ VÀ ĐIỂM
                // =========================================================
                user.Property(s => s.Level).HasDefaultValue(0);
                user.Property(s => s.Experience).HasDefaultValue(0);
                user.Property(s => s.Points).HasDefaultValue(0);
            });

            modelBuilder.Entity<StoryRole>(role =>
            {
                role.Property(s => s.Description)
                    //.IsRequired()
                    .HasMaxLength(MaxLengths.DESCRIPTION);
            });
        }
    }
}