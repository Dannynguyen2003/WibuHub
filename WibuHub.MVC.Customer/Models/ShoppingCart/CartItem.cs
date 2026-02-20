namespace WibuHub.MVC.Customer.Models.ShoppingCart
{
    public class CartItem
    {
        public Guid StoryId { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }
}
