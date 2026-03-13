using WibuHub.MVC.ViewModels.ShoppingCart;

namespace WibuHub.MVC.Customer.ViewModels.ShoppingCart
{
    public class ShoppingCartViewModel
    {
        public Cart Cart { get; set; } = new();
        public List<StoryCatalogItem> Stories { get; set; } = new();
        public List<VipPackageItem> VipPackages { get; set; } = new();
    }

    public class StoryCatalogItem
    {
        public Guid Id { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class VipPackageItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
    }
}
