using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Entities.Identity; // Namespace chứa StoryUser
using WibuHub.ApplicationCore.Interface;

namespace WibuHub.DataLayer
{
    // 1. SỬA KẾ THỪA: Dùng IdentityDbContext để hỗ trợ bảng User/Role
    public class StoryDbContext : IdentityDbContext<StoryUser>
    {
        public StoryDbContext(DbContextOptions<StoryDbContext> options) : base(options)
        {
        }

        // --- Khai báo các bảng ---
        public DbSet<Story> Stories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }

        // 2. CẬP NHẬT: Thêm bảng ảnh và bảng trung gian
        public DbSet<ChapterImage> ChapterImages { get; set; }
        public DbSet<StoryCategory> StoryCategories { get; set; } // Bảng nối Story - Category

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // =============================================================
        // XỬ LÝ SOFT DELETE (GHI ĐÈ SAVESCHANGES)
        // =============================================================
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<ISoftDelete>()
                                       .Where(e => e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // QUAN TRỌNG: Phải gọi base để Identity cấu hình các bảng User, Role...
            base.OnModelCreating(modelBuilder);

            // =============================================================
            // CẤU HÌNH GLOBAL QUERY FILTER (TỰ ĐỘNG LỌC ISDELETED)
            // =============================================================
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
                    var body = Expression.Equal(isDeletedProperty, Expression.Constant(false));
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

            // 3. StoryCategory (MANY-TO-MANY Configuration) - UPDATE QUAN TRỌNG
            modelBuilder.Entity<StoryCategory>(entity =>
            {
                entity.ToTable("StoryCategories");
                // Khóa chính phức hợp (Composite Key)
                entity.HasKey(sc => new { sc.StoryId, sc.CategoryId });

                entity.HasOne(sc => sc.Story)
                      .WithMany(s => s.StoryCategories)
                      .HasForeignKey(sc => sc.StoryId);

                entity.HasOne(sc => sc.Category)
                      .WithMany(c => c.StoryCategories)
                      .HasForeignKey(sc => sc.CategoryId);
            });

            // 4. Story
            modelBuilder.Entity<Story>(entity =>
            {
                entity.ToTable("Stories");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Title).HasMaxLength(255).IsRequired();
                entity.Property(s => s.Status).HasColumnType("tinyint");

                // Author relationship
                entity.HasOne(s => s.Author)
                      .WithMany(a => a.Stories)
                      .HasForeignKey(s => s.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull);

                // (Optional) Quan hệ 1-N cũ với Category nếu bạn vẫn muốn giữ để làm "Category Chính"
                entity.HasOne(s => s.Category)
                      .WithMany(c => c.Stories)
                      .HasForeignKey(s => s.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. Chapter & ChapterImage - UPDATE QUAN TRỌNG
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.ToTable("Chapters");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Price).HasColumnType("money").HasPrecision(18, 2);
                entity.Property(c => c.Discount).HasPrecision(5, 2);
                entity.HasIndex(c => new { c.StoryId, c.ChapterNumber });

                entity.HasOne(c => c.Story)
                      .WithMany(s => s.Chapters)
                      .HasForeignKey(c => c.StoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng ChapterImages
            modelBuilder.Entity<ChapterImage>(entity =>
            {
                entity.ToTable("ChapterImages");
                entity.HasKey(ci => ci.Id);

                // Khi xóa Chapter -> Xóa luôn ảnh
                entity.HasOne(ci => ci.Chapter)
                      .WithMany(c => c.Images)
                      .HasForeignKey(ci => ci.ChapterId)
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

            // 12. Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Amount).HasColumnType("money").HasPrecision(18, 2);
                entity.Property(o => o.Tax).HasColumnType("money").HasPrecision(18, 2);
                entity.Property(o => o.TotalAmount).HasColumnType("money").HasPrecision(18, 2);
                entity.Property(o => o.Phone).HasMaxLength(20).IsRequired();
                entity.Property(o => o.Email).HasMaxLength(100);
                entity.Property(o => o.PaymentMethod).HasMaxLength(50);
                entity.Property(o => o.TransactionId).HasMaxLength(100);
                entity.Property(o => o.PaymentStatus).HasMaxLength(50);
            });

            // 13. OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasKey(od => new { od.OrderId, od.ChapterId });
                entity.Property(od => od.UnitPrice).HasColumnType("money").HasPrecision(18, 2);
                entity.Property(od => od.Amount).HasColumnType("money").HasPrecision(18, 2);

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Chapter)
                      .WithMany()
                      .HasForeignKey(od => od.ChapterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}