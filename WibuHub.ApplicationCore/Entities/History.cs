using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class History
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // UserId: Guid? (Nullable - dành cho user đã login)
        public Guid? UserId { get; set; }

        // DeviceId: varchar(100) (Fingerprint - dành cho khách vãng lai)
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? DeviceId { get; set; }

        // ComicId: Guid (Truyện nào)
        [Required]
        public Guid StoryId { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; } = null!;

        // ChapterId: Guid (Chap gần nhất đã đọc)
        [Required]
        public Guid ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; } = null!;

        // ReadTime: DateTime
        public DateTime ReadTime { get; set; } = DateTime.UtcNow;
    }
}
