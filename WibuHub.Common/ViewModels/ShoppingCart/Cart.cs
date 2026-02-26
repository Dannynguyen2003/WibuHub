namespace WibuHub.MVC.ViewModels.ShoppingCart
{
    public class Cart
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Ngày t?o gi? hàng
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Danh sách s?n ph?m trong gi?
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
