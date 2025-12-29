using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ComicId: Guid
        [Required]
        public Guid ComicId { get; set; }

        [ForeignKey("ComicId")]
        public virtual Story Story { get; set; } = null!;

        // ChapterId: Guid? (Có thể null nếu lỗi chung của truyện, không phải lỗi chap cụ thể)
        public Guid? ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        // Content: nvarchar(255)
        [Required]
        [MaxLength(255)]
        public string Content { get; set; } = string.Empty;

        // Status: tinyint (0: Mới, 1: Đã fix, 2: Bỏ qua)
        [Column(TypeName = "tinyint")]
        public int Status { get; set; } = 0;

        // CreateDate: DateTime
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }

    // Enum hỗ trợ code dễ đọc hơn
    public enum ReportStatus
    {
        New = 0,      // Mới
        Fixed = 1,    // Đã sửa
        Ignored = 2   // Bỏ qua (Không phải lỗi)
    }
}