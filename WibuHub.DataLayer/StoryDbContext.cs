using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.DataLayer
{
    public class StoryDbContext : DbContext
    {
        public StoryDbContext(DbContextOptions<StoryDbContext> options) : base(options)
        {
        }

        // --- Khai báo toàn bộ các bảng ---
        public DbSet<Story> Stories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }

        public DbSet<ComicCategory> Genres { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(1000);

                // Đã bỏ cấu hình Slug
            });

            // 2. Cấu hình Author
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Authors");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(150).IsRequired();

                // Đã bỏ cấu hình Slug
            });

            
            modelBuilder.Entity<ComicCategory>(entity =>
            {
                entity.ToTable("Genres");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).HasMaxLength(150).IsRequired();

                // Đã bỏ cấu hình Slug
            });

            // 4. Cấu hình Story
            modelBuilder.Entity<Story>(entity =>
            {
                entity.ToTable("Stories");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Title).HasMaxLength(255).IsRequired();

                // Đã bỏ cấu hình Slug

                entity.Property(s => s.Thumbnail).HasColumnType("varchar(500)");
                entity.Property(s => s.Status).HasColumnType("tinyint");

                // Quan hệ: 1 Story thuộc 1 Category
                entity.HasOne(s => s.Category)
                      .WithMany(c => c.Stories)
                      .HasForeignKey(s => s.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ: 1 Story thuộc 1 Author
                entity.HasOne(s => s.Author)
                      .WithMany(a => a.Stories)
                      .HasForeignKey(s => s.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ: Many-to-Many với Genre
                entity.HasMany(s => s.Genres)
                      .WithMany(g => g.Stories)
                      .UsingEntity(j => j.ToTable("StoryGenres"));
            });

            // 5. Cấu hình Chapter
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.ToTable("Chapters");
                entity.HasKey(c => c.Id);

                // Đã bỏ cấu hình Slug

                entity.Property(c => c.UnlockPrice).HasColumnType("money");

                // Index kép
                entity.HasIndex(c => new { c.ComicId, c.Number });

                // Xóa Story -> Xóa hết Chapter
                entity.HasOne(c => c.Story)
                      .WithMany(s => s.Chapters)
                      .HasForeignKey(c => c.ComicId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Cấu hình Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).HasMaxLength(1000).IsRequired();

                entity.HasOne(c => c.Parent)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Story)
                      .WithMany(s => s.Comments)
                      .HasForeignKey(c => c.ComicId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 7. Cấu hình Follow
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.ToTable("Follows");
                entity.HasKey(f => new { f.UserId, f.ComicId });

                entity.HasOne(f => f.Story)
                      .WithMany()
                      .HasForeignKey(f => f.ComicId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. Cấu hình Rating
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("Ratings");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Score).HasColumnType("tinyint");
                entity.HasIndex(r => new { r.UserId, r.ComicId }).IsUnique();
            });

            // 9. Cấu hình History
            modelBuilder.Entity<History>(entity =>
            {
                entity.ToTable("Histories");
                entity.HasKey(h => h.Id);
                entity.Property(h => h.DeviceId).HasColumnType("varchar(100)");
                entity.HasIndex(h => new { h.UserId, h.ReadTime });
                entity.HasIndex(h => new { h.DeviceId, h.ReadTime });
            });

            // 10. Cấu hình Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.TargetUrl).HasColumnType("varchar(255)");
                entity.HasIndex(n => new { n.UserId, n.CreateDate });
            });

            // 11. Cấu hình Report
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Reports");
                entity.Property(r => r.Status).HasColumnType("tinyint");
                entity.HasIndex(r => r.Status);
            });
        }
    }
}