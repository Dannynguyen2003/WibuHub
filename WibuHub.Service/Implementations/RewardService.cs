using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Service.Implementation;

namespace WibuHub.Service.Implementations
{
    public class RewardService : IRewardService
    {
        private readonly UserManager<StoryUser> _userManager;
        private const int ExpPerLevel = 100; // 100 EXP = Lên 1 cấp

        public RewardService(UserManager<StoryUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> AddExpAndPointsAsync(string userId, int expAdded, int pointsAdded)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.Experience += expAdded;
            user.Points += pointsAdded;

            // Kiểm tra và thực hiện Lên Cấp
            while (user.Experience >= ExpPerLevel)
            {
                user.Level += 1;
                user.Experience -= ExpPerLevel; // Trừ EXP đã dùng để lên cấp
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
