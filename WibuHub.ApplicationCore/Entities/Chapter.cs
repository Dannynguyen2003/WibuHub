using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WibuHub.ApplicationCore.Interface;
using static System.Net.Mime.MediaTypeNames;

namespace WibuHub.ApplicationCore.Entities
{
    [Index(nameof(StoryId), nameof(Number))]
    [Index(nameof(Slug), IsUnique = true)]
    public class Chapter : ISoftDelete
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // ComicId: Guid (Khóa ngoại trỏ về Story)
        [Required]
        public Guid StoryId { get; set; }

        // Navigation property: Liên kết ngược lại với Story
        [ForeignKey("StoryId")]
        public virtual Story? Story { get; set; }

        // Name: nvarchar(150)
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // Number: float (Số thứ tự chap: 1, 1.5, 2...)
        // Dùng double trong C# để map tốt với float/real trong SQL
        public double Number { get; set; }

        // Slug: varchar(150) - URL thân thiện
        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string Slug { get; set; } = string.Empty;

        // ViewCount: int (Lượt xem riêng chap này)
        public int ViewCount { get; set; } = 0;

        // Content: nvarchar(MAX)
        // Lưu ý: Trong EF Core, string mặc định là nvarchar(MAX) nên không cần MaxLength
        public string? Content { get; set; }

        // ServerId: int (Lưu server ảnh: 1=Google, 2=Imgur...)
        public int ServerId { get; set; } = 1;


        // CreateDate: DateTime
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // UnlockPrice: money
        // Dùng decimal cho tiền tệ trong C#, map sang "money" trong SQL
        [Column(TypeName = "money")]
        public decimal Price { get; set; } = 0;
        public decimal Discount { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        //public Guid? ImageId { get; set; }
        //public virtual Image? Image { get; set; }

    }
}
