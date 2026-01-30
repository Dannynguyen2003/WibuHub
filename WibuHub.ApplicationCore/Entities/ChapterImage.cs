using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class ChapterImage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        // ImageUrl: varchar(500) - URL of the image
        [Required]
        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string ImageUrl { get; set; } = string.Empty;

        // PageNumber: int - Order of the image in the chapter
        public int PageNumber { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
