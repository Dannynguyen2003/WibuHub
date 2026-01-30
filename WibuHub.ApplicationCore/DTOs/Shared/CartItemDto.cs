using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class CartItemDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid CartId { get; set; }
        
        public Guid ChapterId { get; set; }

        [Required]
        [Display(Name = "Tên chương")]
        public string ChapterName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tên truyện")]
        public string StoryTitle { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Giảm giá")]
        public decimal Discount { get; set; }
    }
}
