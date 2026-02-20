namespace WibuHub.MVC.Customer.Models.ShoppingCart
{
    public class Cart
    {
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
