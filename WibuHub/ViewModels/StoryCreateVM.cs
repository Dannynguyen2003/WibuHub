using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.MVC.ViewModels
{
    public class StoryCreateVM
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AlternativeName { get; set; }

        public string? Description { get; set; }

        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? Thumbnail { get; set; }

        [Column(TypeName = "tinyint")]
        public int Status { get; set; } = 0;

        public long ViewCount { get; set; } = 0;

        public int FollowCount { get; set; } = 0;

        public double RatingScore { get; set; } = 0;

        public Guid? AuthorId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
