using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    /// <summary>
    /// Represents an image belonging to a chapter.
    /// Used for storing individual pages of manga/comic chapters.
    /// </summary>
    public class ChapterImage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        /// <summary>
        /// URL of the image (stored on external server or CDN)
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Order of the image in the chapter (1-based index)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
