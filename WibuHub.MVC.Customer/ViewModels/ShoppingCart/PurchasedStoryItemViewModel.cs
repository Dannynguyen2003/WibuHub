namespace WibuHub.MVC.Customer.ViewModels.ShoppingCart
{
    public class PurchasedStoryItemViewModel
    {
        public Guid StoryId { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public int TotalPurchasedQuantity { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastPurchasedAt { get; set; }
    }
}
