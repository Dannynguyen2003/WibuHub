namespace WibuHub.MVC.ViewModels.ShoppingCart
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public Guid CartId { get; set; }
        public Guid ChapterId { get; set; }
        public string ChapterName { get; set; } // VD: "Chapter 100"
        public string StoryTitle { get; set; }  // VD: "Đảo Hải Tặc"
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }      // Giá tiền
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;
    }
}
