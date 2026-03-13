namespace WibuHub.MVC.ViewModels.ShoppingCart
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public Guid CartId { get; set; }
        public Guid StoryId { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }      // Giá ti?n
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;
    }
}
