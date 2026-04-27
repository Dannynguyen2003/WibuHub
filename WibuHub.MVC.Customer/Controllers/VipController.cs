using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.MVC.Customer.ViewModels.Vip;

namespace WibuHub.MVC.Customer.Controllers
{
    [Authorize]
    public class VipController : Controller
    {
        private readonly UserManager<StoryUser> _userManager;
        private const string CheckInTokenProvider = "WibuHub";
        private const string CheckInTokenName = "DailyCheckInDate";
        private const int DailyCheckInRewardCoins = 3;

        private static readonly IReadOnlyList<VipPackageOptionViewModel> VipPackages = new List<VipPackageOptionViewModel>
        {
            new() { Code = "VIP_MONTH", Name = "VIP 1 Month", DurationDays = 30, Price = 99000, CoinCost = 5 },
            new() { Code = "VIP_QUARTER", Name = "VIP 3 Months", DurationDays = 90, Price = 249000, CoinCost = 12 },
            new() { Code = "VIP_YEAR", Name = "VIP 12 Months", DurationDays = 365, Price = 799000, CoinCost = 35 }
        };

        public VipController(UserManager<StoryUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Route("vip/upgrade")]
        public async Task<IActionResult> Upgrade()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var model = new VipUpgradeViewModel
            {
                CurrentCoins = user.Points,
                DailyCheckInRewardCoins = DailyCheckInRewardCoins,
                HasCheckedInToday = await HasCheckedInTodayAsync(user),
                Packages = VipPackages
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("vip/checkin")]
        public async Task<IActionResult> CheckInDaily()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (await HasCheckedInTodayAsync(user))
            {
                TempData["VipError"] = "Bạn đã điểm danh hôm nay rồi.";
                return RedirectToAction(nameof(Upgrade));
            }

            user.Points += DailyCheckInRewardCoins;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["VipError"] = "Điểm danh thất bại. Vui lòng thử lại.";
                return RedirectToAction(nameof(Upgrade));
            }

            var tokenResult = await _userManager.SetAuthenticationTokenAsync(
                user,
                CheckInTokenProvider,
                CheckInTokenName,
                DateTime.UtcNow.ToString("yyyyMMdd"));

            if (!tokenResult.Succeeded)
            {
                TempData["VipError"] = "Điểm danh thất bại. Vui lòng thử lại.";
                return RedirectToAction(nameof(Upgrade));
            }

            TempData["VipSuccess"] = $"Điểm danh thành công! +{DailyCheckInRewardCoins} coins.";
            return RedirectToAction(nameof(Upgrade));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("vip/redeem")]
        public async Task<IActionResult> RedeemByCoins(string code)
        {
            var selectedPackage = VipPackages.FirstOrDefault(p => p.Code == code);
            if (selectedPackage == null)
            {
                TempData["VipError"] = "Gói VIP không hợp lệ.";
                return RedirectToAction(nameof(Upgrade));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (user.Points < selectedPackage.CoinCost)
            {
                TempData["VipError"] = $"Bạn cần {selectedPackage.CoinCost} coins để đổi gói {selectedPackage.Name}.";
                return RedirectToAction(nameof(Upgrade));
            }

            user.Points -= selectedPackage.CoinCost;

            var currentVipEnd = user.VipExpireDate.HasValue && user.VipExpireDate.Value > DateTime.UtcNow
                ? user.VipExpireDate.Value
                : DateTime.UtcNow;

            user.VipExpireDate = currentVipEnd.AddDays(selectedPackage.DurationDays);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["VipError"] = "Không thể đổi VIP bằng coin. Vui lòng thử lại.";
                return RedirectToAction(nameof(Upgrade));
            }

            TempData["VipSuccess"] = $"Đổi thành công {selectedPackage.Name} bằng {selectedPackage.CoinCost} coins.";
            return RedirectToAction(nameof(Upgrade));
        }

        private async Task<bool> HasCheckedInTodayAsync(StoryUser user)
        {
            var todayKey = DateTime.UtcNow.ToString("yyyyMMdd");
            var checkInDate = await _userManager.GetAuthenticationTokenAsync(user, CheckInTokenProvider, CheckInTokenName);
            return string.Equals(checkInDate, todayKey, StringComparison.Ordinal);
        }
    }
}