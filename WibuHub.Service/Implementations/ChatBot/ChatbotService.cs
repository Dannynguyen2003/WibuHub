using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations.ChatBot
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        // Khai báo DbContext của bạn ở đây
        private readonly StoryDbContext _db;

        public ChatbotService(HttpClient httpClient, IConfiguration configuration, StoryDbContext db)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _db = db;
        }

        public async Task<string> GetStoryRecommendationAsync(string userMessage)
        {
            try
            {
                // 1. LẤY DỮ LIỆU TỪ DATABASE
                var rawStories = await _db.Stories
                    .OrderByDescending(t => t.ViewCount)
                    .Take(10)
                    .Select(t => new
                    {
                        StoryName = t.StoryName,
                        Description = t.Description,
                        Categories = t.StoryCategories.Select(sc => sc.Category.Name).ToList()
                    })
                    .ToListAsync();

                var stories = rawStories.Select(t =>
                    $"- Tên: {t.StoryName}, Thể loại: {string.Join(", ", t.Categories)}, Tóm tắt: {t.Description}"
                ).ToList();

                string dbContextInfo = string.Join("\n", stories);

                // 2. GỌI GOOGLE GEMINI
                string systemPrompt = $@"Bạn là nhân viên tư vấn truyện của web WibuHub. 
Khách hàng hỏi: '{userMessage}'.
Hãy dựa vào danh sách các truyện đang có sẵn sau đây để gợi ý cho khách: 
{dbContextInfo}
Yêu cầu: Trả lời ngắn gọn, thân thiện, chỉ gợi ý truyện có trong danh sách trên.";

                var apiKey = _configuration["Gemini:ApiKey"];
                var baseUrl = _configuration["Gemini:Url"];

                var requestBody = new { contents = new[] { new { parts = new[] { new { text = systemPrompt } } } } };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{baseUrl}{apiKey}", jsonContent);

                // Đọc lỗi từ Google nếu có
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Google từ chối: Mã {response.StatusCode} - Chi tiết: {errorContent}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseString);
                return doc.RootElement.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text").GetString();
            }
            catch (Exception ex)
            {
                // IN THẲNG LỖI RA KHUNG CHAT ĐỂ BẮT BỆNH
                return $"[Bắt được lỗi]: {ex.Message}";
            }
        }
    }
}
