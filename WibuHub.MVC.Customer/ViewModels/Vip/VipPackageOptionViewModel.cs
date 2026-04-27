namespace WibuHub.MVC.Customer.ViewModels.Vip
{
    public class VipPackageOptionViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public int CoinCost { get; set; }
    }
}
