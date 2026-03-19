using Microsoft.AspNetCore.Identity;
using WibuHub.ApplicationCore.Entities.Identity;

namespace WibuHub.Service.Implementation
{
    public interface IRewardService
    {
        Task<bool> AddExpAndPointsAsync(string userId, int expAdded, int pointsAdded);
    }
}