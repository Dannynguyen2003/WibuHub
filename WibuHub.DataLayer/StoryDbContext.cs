using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Interface;


namespace WibuHub.DataLayer
{
    public class StoryDbContext : DbContext
    {
        public StoryDbContext(DbContextOptions<StoryDbContext> options) : base(options)
        {
        }

        // --- Khai báo các bảng ---
        public DbSet<Story> Stories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<StoryCategory> Genres { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // =============================================================
        // XỬ LÝ SOFT DELETE (GHI ĐÈ SAVESCHANGES)
        // =============================================================
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Lấy tất cả các Entity đang ở trạng thái Deleted và có implement ISoftDelete
            var entries = ChangeTracker.Entries<ISoftDelete>()
                                       .Where(e => e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                // Thay vì xóa thật, ta chuyển sang Modified (Sửa)
                entry.State = EntityState.Modified;

                // Cập nhật cờ xóa
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================================
            // CẤU HÌNH GLOBAL QUERY FILTER (TỰ ĐỘNG LỌC ISDELETED)
            // =============================================================
            // Duyệt qua tất cả các Entity trong Model
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Nếu Entity có implement ISoftDelete
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    // Thêm bộ lọc toàn cục: Chỉ lấy bản ghi chưa bị xóa (!IsDeleted)
                    // Đoạn này dùng Expression Tree để tạo lambda: e => !e.IsDeleted
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(ISoftDelete).GetProperty("IsDeleted").GetMethod;
                    var body = Expression.Not(Expression.Call(parameter, propertyMethodInfo));
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // =============================================================
            // CẤU HÌNH CHI TIẾT TỪNG BẢNG
            // =============================================================

            // 1. Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(1000);
            });

            // 2. Author
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Authors");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(150).IsRequired();
            });

            // 3. Genres
            modelBuilder.Entity<StoryCategory>(entity =>
            {
                entity.ToTable("Genres");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).HasMaxLength(150).IsRequired();
            });

            // 4. Story
            modelBuilder.Entity<Story>(entity =>
            {
                entity.ToTable("Stories");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Title).HasMaxLength(255).IsRequired();
                entity.Property(s => s.Thumbnail).HasColumnType("varchar(500)");
                entity.Property(s => s.Status).HasColumnType("tinyint");

                entity.HasOne(s => s.Category)
                      .WithMany(c => c.Stories)
                      .HasForeignKey(s => s.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Author)
                      .WithMany(a => a.Stories)
                      .HasForeignKey(s => s.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(s => s.Genres)
                      .WithMany(g => g.Stories)
                      .UsingEntity(j => j.ToTable("StoryGenres"));
            });

            // 5. Chapter
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.ToTable("Chapters");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Price).HasColumnType("money").HasPrecision(18, 2);
                entity.Property(c => c.Discount).HasPrecision(5, 2);

                entity.HasIndex(c => new { c.StoryId, c.Number });

                entity.HasOne(c => c.Story)
                      .WithMany(s => s.Chapters)
                      .HasForeignKey(c => c.StoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Comment
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
                      .HasForeignKey(c => c.StoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 7. Follow
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.ToTable("Follows");
                entity.HasKey(f => new { f.UserId, f.StoryId });
                entity.HasOne(f => f.Story)
                      .WithMany()
                      .HasForeignKey(f => f.StoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. Rating
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("Ratings");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Score).HasColumnType("tinyint");
                entity.HasIndex(r => new { r.UserId, r.StoryId }).IsUnique();
            });

            // 9. History
            modelBuilder.Entity<History>(entity =>
            {
                entity.ToTable("Histories");
                entity.HasKey(h => h.Id);
                entity.Property(h => h.DeviceId).HasColumnType("varchar(100)");
                entity.HasIndex(h => new { h.UserId, h.ReadTime });
                entity.HasIndex(h => new { h.DeviceId, h.ReadTime });

                entity.HasOne(h => h.Story)
                      .WithMany()
                      .HasForeignKey(h => h.StoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Chapter)
                      .WithMany()
                      .HasForeignKey(h => h.ChapterId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 10. Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.TargetUrl).HasColumnType("varchar(255)");
                entity.HasIndex(n => new { n.UserId, n.CreateDate });
            });

            // 11. Report
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Reports");
                entity.Property(r => r.Status).HasColumnType("tinyint");
                entity.HasIndex(r => r.Status);
            });
        }
    }
}