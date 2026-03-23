namespace WibuHub.MVC.ViewModels.ShoppingCart
{
    public class CartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Tự động tạo ID cho Item

        public Guid CartId { get; set; }

        // 1. SỬA THÀNH Guid? (Nullable) để có thể lưu Gói VIP (Gói VIP thì StoryId = null)
        public Guid? StoryId { get; set; }

        // 2. Vẫn giữ StoryTitle nhưng sẽ dùng để lưu "Tên Truyện" HOẶC "Tên Gói VIP"
        public string StoryTitle { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;

        // ==========================================
        // 3. THÊM CÁC TRƯỜNG DÀNH RIÊNG CHO VIP
        // ==========================================
        public int VipDays { get; set; } = 0; // Lưu số ngày VIP (Ví dụ: 7, 30, 365)

        // Cờ (Flag) giúp Frontend và Backend check nhanh xem đây là VIP hay Truyện
        public bool IsVip => VipDays > 0;
    }
}