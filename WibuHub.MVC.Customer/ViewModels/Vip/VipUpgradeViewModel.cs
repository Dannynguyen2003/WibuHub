namespace WibuHub.MVC.Customer.ViewModels.Vip
{
    public class VipUpgradeViewModel
    {
        public int CurrentCoins { get; set; }
        public int DailyCheckInRewardCoins { get; set; }
        public bool HasCheckedInToday { get; set; }
        public IReadOnlyList<VipPackageOptionViewModel> Packages { get; set; } = Array.Empty<VipPackageOptionViewModel>();
    }
}
