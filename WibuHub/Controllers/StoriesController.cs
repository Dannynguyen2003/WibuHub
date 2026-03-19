using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using WibuHub.ApplicationCore.Entities;
using WibuHub.Common.Contants;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.Areas.Admin.Controllers
{
    //[Area("Admin")]
    [Authorize(Roles = AppConstants.Roles.Uploader + "," + AppConstants.Roles.SuperAdmin)]
    public class StoriesController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jfif", ".png", ".webp", ".gif", ".bmp"
        };
        private const long MaxImageSizeBytes = 10 * 1024 * 1024;

        public StoriesController(StoryDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.StoryCategories)
                .ThenInclude(sc => sc.Category)
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (story == null) return NotFound();
            return View(story);
        }

        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name");
            ViewData["CategoryId"] = new MultiSelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryVM storyVM)
        {
            var selectedCategoryIds = storyVM.CategoryIds?.Distinct().ToList() ?? new List<Guid>();
            if (!selectedCategoryIds.Any())
            {
                ModelState.AddModelError(nameof(storyVM.CategoryIds), "Vui lòng chọn ít nhất một danh mục.");
            }

            var storyId = storyVM.Id == Guid.Empty ? Guid.NewGuid() : storyVM.Id;
            string? coverImagePath = null;

            if (storyVM.CoverImageFile != null && storyVM.CoverImageFile.Length > 0)
            {
                coverImagePath = await SaveCoverImageAsync(storyVM.CoverImageFile, storyId);
                if (string.IsNullOrWhiteSpace(coverImagePath))
                {
                    ModelState.AddModelError(nameof(storyVM.CoverImageFile), "Ảnh bìa không hợp lệ hoặc vượt quá dung lượng cho phép.");
                }
            }

            if (ModelState.IsValid)
            {
                var story = new Story
                {
                    Id = storyId,
                    StoryName = storyVM.Title.Trim(),
                    AlternativeName = storyVM.AlternativeName?.Trim(),
                    Description = storyVM.Description?.Trim(),
                    Slug = string.IsNullOrEmpty(storyVM.Slug) ? GenerateSlug(storyVM.Title) : storyVM.Slug.Trim(),
                    Price = storyVM.Price,
                    Discount = storyVM.Discount,
                    Status = storyVM.Status,
                    ViewCount = 0,
                    FollowCount = 0,
                    RatingScore = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    AuthorId = storyVM.AuthorId,
                    CoverImage = coverImagePath
                };

                story.StoryCategories = selectedCategoryIds
                    .Select(categoryId => new StoryCategory { StoryId = story.Id, CategoryId = categoryId })
                    .ToList();

                _context.Add(story);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new MultiSelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryIds);
            return View(storyVM);
        }

        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) return "";
            string str = phrase.ToLower().Trim();
            string[] vietnameseSigns = {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ", "ÍÌỊỈĨ",
            "đ", "Đ",
            "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-").Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"-+", "-");
            return str;
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var story = await _context.Stories
                .Include(s => s.StoryCategories)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null || story.IsDeleted) return NotFound();

            var categoryIds = story.StoryCategories.Select(sc => sc.CategoryId).ToList();

            var storyVM = new StoryVM
            {
                Id = story.Id,
                Title = story.StoryName,
                AlternativeName = story.AlternativeName,
                Description = story.Description,
                Slug = story.Slug,
                Price = story.Price,
                Discount = story.Discount,
                Status = story.Status,
                ViewCount = story.ViewCount,
                FollowCount = story.FollowCount,
                RatingScore = story.RatingScore,
                AuthorId = story.AuthorId,
                CategoryIds = categoryIds,
                CoverImage = story.CoverImage
            };

            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", story.AuthorId);
            ViewData["CategoryId"] = new MultiSelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryIds);

            return View(nameof(Create), storyVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, StoryVM storyVM)
        {
            if (id != storyVM.Id) return BadRequest();

            var selectedCategoryIds = storyVM.CategoryIds?.Distinct().ToList() ?? new List<Guid>();
            if (!selectedCategoryIds.Any())
            {
                ModelState.AddModelError(nameof(storyVM.CategoryIds), "Vui lòng chọn ít nhất một danh mục.");
            }

            string? coverImagePath = null;
            if (storyVM.CoverImageFile != null && storyVM.CoverImageFile.Length > 0)
            {
                coverImagePath = await SaveCoverImageAsync(storyVM.CoverImageFile, storyVM.Id);
                if (string.IsNullOrWhiteSpace(coverImagePath))
                {
                    ModelState.AddModelError(nameof(storyVM.CoverImageFile), "Ảnh bìa không hợp lệ hoặc vượt quá dung lượng cho phép.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var story = await _context.Stories
                        .Include(s => s.StoryCategories)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (story == null || story.IsDeleted) return NotFound();

                    story.StoryName = storyVM.Title.Trim();
                    story.AlternativeName = storyVM.AlternativeName?.Trim();
                    story.Description = storyVM.Description?.Trim();
                    story.Slug = storyVM.Slug.Trim();
                    story.Price = storyVM.Price;
                    story.Discount = storyVM.Discount;
                    story.Status = storyVM.Status;
                    story.AuthorId = storyVM.AuthorId;
                    story.UpdateDate = DateTime.UtcNow;

                    if (!string.IsNullOrWhiteSpace(coverImagePath))
                    {
                        story.CoverImage = coverImagePath;
                    }

                    _context.StoryCategories.RemoveRange(story.StoryCategories);
                    story.StoryCategories = selectedCategoryIds
                        .Select(categoryId => new StoryCategory { StoryId = story.Id, CategoryId = categoryId })
                        .ToList();

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoryExists(storyVM.Id)) return BadRequest();
                    else throw;
                }
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new MultiSelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryIds);

            return View(nameof(Create), storyVM);
        }

        private async Task<string?> SaveCoverImageAsync(IFormFile file, Guid storyId)
        {
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
            {
                return null;
            }
            if (file.Length == 0 || file.Length > MaxImageSizeBytes)
            {
                return null;
            }
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", storyId.ToString());
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var fileName = $"cover{extension}";
            var fullPath = Path.Combine(folderPath, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/stories/{storyId}/{fileName}";
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var story = await _context.Stories
                .Include(s => s.Author)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (story == null) return NotFound();
            return View(story);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null) return Json(new { isOK = false });

            story.IsDeleted = true;
            story.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { isOK = true });
        }

        public async Task<IActionResult> Reload(int page = 1, int pageSize = 10)
        {
            return ViewComponent("StoryList", new { page, pageSize });
        }

        private bool StoryExists(Guid id)
        {
            return _context.Stories.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}