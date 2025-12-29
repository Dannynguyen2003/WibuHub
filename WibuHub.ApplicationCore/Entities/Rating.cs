using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class Rating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // UserId: Guid
        [Required]
        public Guid UserId { get; set; }
        // public virtual AppUser User { get; set; } // Uncomment nếu đã có class User

        // ComicId: Guid (Đánh giá truyện nào)
        [Required]
        public Guid ComicId { get; set; }

        [ForeignKey("ComicId")]
        public virtual Story Story { get; set; } = null!;

        // Score: tinyint (1 đến 5 sao)
        [Required]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        [Column(TypeName = "tinyint")]
        public int Score { get; set; }

        // Note: nvarchar(500) (Nhận xét ngắn)
        [MaxLength(500)]
        public string? Note { get; set; }

        // CreateDate: DateTime
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}