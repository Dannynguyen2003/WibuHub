using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.Service.Interface;

namespace WibuHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskBot([FromBody] ChatbotRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { Message = "Bạn chưa nhập tin nhắn." });

            var answer = await _chatbotService.GetStoryRecommendationAsync(request.Message);
            return Ok(new { Reply = answer });
        }
    }
}
