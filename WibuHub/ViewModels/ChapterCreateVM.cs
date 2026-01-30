using System.ComponentModel.DataAnnotations;

namespace WibuHub.MVC.ViewModels
{
    public class ChapterCreateVM
    {
        [Required]
        public Guid StoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public double Number { get; set; }

        [Required]
        [MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        public string? Content { get; set; }

        public int ServerId { get; set; } = 1;

        public decimal Price { get; set; } = 0;

        public decimal Discount { get; set; } = 0;
    }
}
