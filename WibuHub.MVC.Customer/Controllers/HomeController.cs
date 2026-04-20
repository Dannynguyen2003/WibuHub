using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels; // Ensure this namespace matches your project

namespace WibuHub.MVC.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoryDbContext _context;

        // Fixed: Must inject StoryDbContext into the constructor
        public HomeController(ILogger<HomeController> logger, StoryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get the story with the highest view count
            var featuredStory = await _context.Stories
                .OrderByDescending(s => s.ViewCount)
                .FirstOrDefaultAsync();

            // If you want to display the New Stories Slider on the Index view, 
            // you can pass it via ViewBag or use a comprehensive HomeViewModel.
            // Example using ViewBag:
            ViewBag.NewStories = await GetNewlyUploadedStoriesAsync();

            return View(featuredStory);
        }

        public async Task<List<StoryViewModel>> GetNewlyUploadedStoriesAsync()
        {
            // Step 1: Fetch data from the Database (Only use operations supported by EF Core)
            var rawStories = await _context.Stories
                // FIX 1: Replace "Published" with the integer representing that status in the DB. 
                // Example: 1 is Published, 0 is Draft. Replace 1 with your actual status code!
                .Where(s => s.Status == 1)
                .OrderByDescending(s => s.CreatedAt)
                .Take(12)
                .Select(s => new
                {
                    Id = s.Id,
                    StoryName = s.StoryName,
                    CoverImage = s.CoverImage,
                    LatestChapter = s.Chapters.OrderByDescending(c => c.CreatedAt).FirstOrDefault().Name,
                    CreatedAt = s.CreatedAt // Fetch the creation date into memory
                })
                .ToListAsync();

            // Step 2: Map data to ViewModel and process CalculateTimeAgo in memory (C#)
            var viewModels = rawStories.Select(s => new StoryViewModel
            {
                Id = s.Id,
                StoryName = s.StoryName,
                CoverImage = s.CoverImage,
                LatestChapter = s.LatestChapter,
                // FIX 2: Calling the C# function here prevents EF Core translation errors
                TimeAgo = CalculateTimeAgo(s.CreatedAt)
            }).ToList();

            return viewModels;
        }

        // Added: Function to calculate time elapsed (e.g., "4 hours ago")
        private static string CalculateTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";

            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} days ago";

            // If more than 30 days, display the exact date
            return createdAt.ToString("dd/MM/yyyy");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}