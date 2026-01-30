using Microsoft.AspNetCore.Mvc;
using WibuHub.Service.Interface;

namespace WibuHub.MVC.Customer.Controllers
{
    public class StoriesController : Controller
    {
        private readonly IStoryService _storyService;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(IStoryService storyService, ILogger<StoriesController> logger)
        {
            _storyService = storyService;
            _logger = logger;
        }

        // GET: Stories
        public async Task<IActionResult> Index()
        {
            try
            {
                var stories = await _storyService.GetAllAsync();
                return View(stories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stories");
                return View("Error");
            }
        }

        // GET: Stories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var story = await _storyService.GetByIdAsync(id.Value);
                if (story == null)
                {
                    return NotFound();
                }

                return View(story);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving story details for ID: {Id}", id);
                return View("Error");
            }
        }
    }
}
