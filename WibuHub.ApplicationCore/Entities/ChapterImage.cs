using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    [Table("ChapterImages")]
    public class ChapterImage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        // ImageUrl: Đường dẫn ảnh (VD: /uploads/story1/chap1/01.jpg hoặc link CDN)
        [Required]
        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string ImageUrl { get; set; } = string.Empty;

        // OrderIndex: Quan trọng để sắp xếp thứ tự trang (Trang 1, Trang 2...)
        public int OrderIndex { get; set; }

        // StorageType: (Optional) 0: Local, 1: GoogleDrive, 2: AWS S3...
        public int StorageType { get; set; } = 0;
    }
}