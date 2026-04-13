using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations.ChatBot
{
    public class ChatbotService : IChatbotService
    {
        private sealed class StorySuggestionSource
        {
            public string StoryName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<string> Categories { get; set; } = new();
        }

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
                    .Select(t => new StorySuggestionSource
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
                string systemPrompt = $@"Bạn là 'Wibu-chan', nữ trợ lý ảo siêu dễ thương của web đọc truyện WibuHub. 
Khách hàng vừa nhắn: '{userMessage}'.

Dưới đây là danh sách truyện đang có sẵn trên hệ thống: 
{dbContextInfo}

Luật trả lời của bạn:
1. Luôn xưng hô là 'mình' và gọi người dùng là 'bạn' hoặc 'đạo hữu'. Thêm emoji dễ thương vào câu trả lời (như uwu, (≧◡≦)).
2. CHỈ ĐƯỢC gợi ý truyện CÓ TRONG DANH SÁCH trên. Tuyệt đối không được bịa ra truyện bên ngoài.
3. Nếu khách hỏi truyện không có trong danh sách, hãy nói xin lỗi dễ thương và gợi ý một truyện ngẫu nhiên trong danh sách.
4. Trả lời ngắn gọn, có gạch đầu dòng rõ ràng để dễ đọc.";

                var apiKey = _configuration["Gemini:ApiKey"];
                var baseUrl = _configuration["Gemini:Url"];

                var requestBody = new { contents = new[] { new { parts = new[] { new { text = systemPrompt } } } } };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{baseUrl}{apiKey}", jsonContent);

                // Đọc lỗi từ Google nếu có
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    if ((int)response.StatusCode == 429)
                    {
                        return BuildFallbackReply(rawStories, userMessage,
                            "⚠️ AI tạm bận (quota), hệ thống đang dùng gợi ý nội bộ:");
                    }

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
                var fallbackStories = await _db.Stories
                    .OrderByDescending(t => t.ViewCount)
                    .Take(10)
                    .Select(t => new StorySuggestionSource
                    {
                        StoryName = t.StoryName,
                        Description = t.Description,
                        Categories = t.StoryCategories.Select(sc => sc.Category.Name).ToList()
                    })
                    .ToListAsync();

                return BuildFallbackReply(fallbackStories, userMessage,
                    "⚠️ AI tạm bận, hệ thống đang dùng gợi ý nội bộ:");
            }
        }

        private static string BuildFallbackReply(List<StorySuggestionSource> stories, string userMessage, string prefix)
        {
            if (stories.Count == 0)
            {
                return "Hiện chưa có dữ liệu truyện để gợi ý.";
            }

            var normalizedUserMessage = NormalizeText(userMessage ?? string.Empty);

            var isGenericSuggestionRequest =
                string.IsNullOrWhiteSpace(normalizedUserMessage)
                || normalizedUserMessage.Contains("goi y", StringComparison.Ordinal)
                || normalizedUserMessage.Contains("de xuat", StringComparison.Ordinal)
                || normalizedUserMessage.Contains("tu van", StringComparison.Ordinal)
                || normalizedUserMessage.Contains("recommend", StringComparison.Ordinal)
                || normalizedUserMessage.Contains("suggest", StringComparison.Ordinal);

            if (isGenericSuggestionRequest)
            {
                var topStories = stories.Take(3).ToList();
                var topLines = topStories.Select(s =>
                    $"- {s.StoryName} ({(s.Categories.Count > 0 ? string.Join(", ", s.Categories) : "Chưa phân loại")})");

                return "Mình gợi ý top truyện có lượt xem cao hiện tại:\n" + string.Join("\n", topLines);
            }

            var keywords = normalizedUserMessage
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(k => k.Length >= 2)
                .Distinct()
                .ToList();

            var matchedStories = stories
                .Select(story =>
                {
                    var normalizedName = NormalizeText(story.StoryName);
                    var normalizedDescription = NormalizeText(story.Description ?? string.Empty);
                    var normalizedCategories = story.Categories.Select(NormalizeText).ToList();

                    var score = 0;
                    foreach (var keyword in keywords)
                    {
                        if (normalizedName.Contains(keyword, StringComparison.Ordinal))
                        {
                            score += 4;
                        }

                        if (normalizedCategories.Any(c => c.Contains(keyword, StringComparison.Ordinal)))
                        {
                            score += 3;
                        }

                        if (normalizedDescription.Contains(keyword, StringComparison.Ordinal))
                        {
                            score += 1;
                        }
                    }

                    return new { Story = story, Score = score };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Story.StoryName)
                .Take(3)
                .Select(x => x.Story)
                .ToList();

            if (matchedStories.Count == 0)
            {
                return "Mình chưa tìm thấy truyện khớp yêu cầu. Bạn thử mô tả rõ hơn (thể loại, mood, độ dài, tình trạng full/chưa full) nhé.";
            }

            var lines = matchedStories.Select(s =>
                $"- {s.StoryName} ({(s.Categories.Count > 0 ? string.Join(", ", s.Categories) : "Chưa phân loại")})");

            return $"{prefix}\n" + string.Join("\n", lines);
        }

        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var normalized = input.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            var noDiacritics = builder.ToString().Normalize(NormalizationForm.FormC);
            return Regex.Replace(noDiacritics.ToLowerInvariant(), @"\s+", " ").Trim();
        }
    }
}
