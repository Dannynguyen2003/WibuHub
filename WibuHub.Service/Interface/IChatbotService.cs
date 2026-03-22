using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.Service.Interface
{
    public interface IChatbotService
    {
        Task<string> GetStoryRecommendationAsync(string userMessage);
    }
}
