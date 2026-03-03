using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string UploadInvalidMessage = "Ảnh tải lên không hợp lệ hoặc vượt giới hạn.";
        private const string UploadCountLimitMessage = "Chỉ được tải tối đa 40 ảnh mỗi lần.";
        private const int MaxUploadImageCount = 40;

        public ChaptersController(StoryDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Chapters
        public async Task<IActionResult> Index()
        {
            var storyDbContext = _context.Chapters.Include(c => c.Story);
            return View(await storyDbContext.ToListAsync());
        }

        // GET: Chapters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // GET: Chapters/Create
        public async Task<IActionResult> Create()
        {
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName");
            return View();
        }

        // POST: Chapters/Create
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000, ValueCountLimit = 4096)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterVM chapterVM)
        {
            if (ModelState.IsValid)
            {
                if ((chapterVM.UploadImages?.Count ?? 0) > MaxUploadImageCount)
                {
                    ModelState.AddModelError(nameof(chapterVM.UploadImages), UploadCountLimitMessage);
                    ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
                    return View(chapterVM);
                }
                var imageUrls = (chapterVM.ImageUrls ?? new List<string>())
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .ToList();
                if (imageUrls.Count == 0 && !string.IsNullOrWhiteSpace(chapterVM.Content))
                {
                    imageUrls = chapterVM.Content
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(url => url.Trim())
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .ToList();
                }
                var chapterId = Guid.NewGuid();
                var uploadResult = await SaveUploadedImagesAsync(chapterVM.UploadImages, chapterVM.StoryId, chapterId, imageUrls.Count + 1);
                imageUrls.AddRange(uploadResult.UploadedImageUrls);
                if (IsAllUploadsRejected(chapterVM.UploadImages, uploadResult))
                {
                    ModelState.AddModelError(nameof(chapterVM.UploadImages), UploadInvalidMessage);
                    ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
                    return View(chapterVM);
                }

                var chapter = new Chapter
                {
                    Id = chapterId,
                    StoryId = chapterVM.StoryId,
                    Name = chapterVM.Name.Trim(),
                    ChapterNumber = chapterVM.ChapterNumber,
                    Slug = string.IsNullOrWhiteSpace(chapterVM.Slug)
                           ? GenerateSlug(chapterVM.Name)
                           : chapterVM.Slug.Trim(),
                    ViewCount = 0,
                    Content = string.Join(Environment.NewLine, imageUrls),
                    ServerId = chapterVM.ServerId,
                    CreatedAt = DateTime.UtcNow,
                    Price = chapterVM.Price,
                    Discount = chapterVM.Discount,
                    Images = imageUrls
                        .Select((url, index) => new ChapterImage
                        {
                            ImageUrl = url,
                            OrderIndex = index + 1,
                            ChapterId = chapterId
                        })
                        .ToList()
                };
                try
                {
                    await _context.Chapters.AddAsync(chapter);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Create));
                }
                catch (DbUpdateException)
                {
                    var folderPath = GetChapterUploadPath(chapterVM.StoryId, chapterId);
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                    throw;
                }
            }

            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
            return View(chapterVM);
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

        // GET: Chapters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }
            var chapterVM = new ChapterVM
            {
                Id = chapter.Id,
                StoryId = chapter.StoryId,
                Name = chapter.Name,
                ChapterNumber = chapter.ChapterNumber,
                Slug = chapter.Slug,
                ViewCount = chapter.ViewCount,
                Content = chapter.Content,
                ImageUrls = chapter.Images.OrderBy(i => i.OrderIndex).Select(i => i.ImageUrl).ToList(),
                ServerId = chapter.ServerId,
                CreatedAt = chapter.CreatedAt,
                Price = chapter.Price,
                Discount = chapter.Discount
            };
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapter.StoryId);
            return View(nameof(Create), chapterVM);
        }

        // POST: Chapters/Edit/5
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000, ValueCountLimit = 4096)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ChapterVM chapterVM)
        {
            if (id != chapterVM.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if ((chapterVM.UploadImages?.Count ?? 0) > MaxUploadImageCount)
                    {
                        ModelState.AddModelError(nameof(chapterVM.UploadImages), UploadCountLimitMessage);
                        ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
                        return View(nameof(Create), chapterVM);
                    }
                    var chapter = await _context.Chapters
                        .Include(c => c.Story)
                        .Include(c => c.Images)
                        .FirstOrDefaultAsync(c => c.Id == id);
                    if (chapter == null)
                    {
                        return BadRequest();
                    }
                    var imageUrls = (chapterVM.ImageUrls ?? new List<string>())
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Select(url => url.Trim())
                        .ToList();
                    var uploadResult = await SaveUploadedImagesAsync(chapterVM.UploadImages, chapterVM.StoryId, chapter.Id, imageUrls.Count + 1);
                    imageUrls.AddRange(uploadResult.UploadedImageUrls);
                    if (IsAllUploadsRejected(chapterVM.UploadImages, uploadResult))
                    {
                        ModelState.AddModelError(nameof(chapterVM.UploadImages), UploadInvalidMessage);
                        ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
                        return View(nameof(Create), chapterVM);
                    }
                    chapter.StoryId = chapterVM.StoryId;
                    chapter.Name = chapterVM.Name.Trim();
                    chapter.ChapterNumber = chapterVM.ChapterNumber;
                    chapter.Slug = chapterVM.Slug.Trim();
                    chapter.Content = string.Join(Environment.NewLine, imageUrls);
                    chapter.ServerId = chapterVM.ServerId;
                    chapter.Price = chapterVM.Price;
                    chapter.Discount = chapterVM.Discount;
                    _context.ChapterImages.RemoveRange(chapter.Images);
                    chapter.Images = imageUrls
                        .Select((url, index) => new ChapterImage
                        {
                            ImageUrl = url,
                            OrderIndex = index + 1,
                            ChapterId = chapter.Id
                        })
                        .ToList();
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Create));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapterVM.Id))
                    {
                        return BadRequest();
                    }

                    ModelState.AddModelError(string.Empty, "Chapter was modified or deleted by another user. Please reload and try again.");
                    ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
                    return View(nameof(Create), chapterVM);
                }
            }
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
            return View(nameof(Create), chapterVM);
        }

        // GET: Chapters/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Chapters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return Json(new { isOK = false });
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
            return Json(new { isOK = true });
        }

        // GET: Chapters/Reload
        public async Task<IActionResult> Reload()
        {
            return ViewComponent("ChapterList");
        }

        private bool ChapterExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id);
        }

        private async Task<(List<string> UploadedImageUrls, int RejectedCount)> SaveUploadedImagesAsync(List<IFormFile>? uploadImages, Guid storyId, Guid chapterId, int startOrder)
        {
            var uploadedImageUrls = new List<string>();
            if (uploadImages == null || uploadImages.Count == 0) return (uploadedImageUrls, 0);

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".jfif", ".bmp", ".heic", ".heif", ".avif" };
            const long maxImageSizeBytes = 10 * 1024 * 1024;
            var folderPath = GetChapterUploadPath(storyId, chapterId);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var order = startOrder;
            var rejectedCount = 0;
            foreach (var file in uploadImages)
            {
                if (file == null || file.Length <= 0)
                {
                    rejectedCount++;
                    continue;
                }
                if (file.Length > maxImageSizeBytes)
                {
                    rejectedCount++;
                    continue;
                }
                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    rejectedCount++;
                    continue;
                }

                var fileName = $"{order:D4}{extension.ToLowerInvariant()}";
                var fullPath = Path.Combine(folderPath, fileName);
                while (System.IO.File.Exists(fullPath))
                {
                    order++;
                    fileName = $"{order:D4}{extension.ToLowerInvariant()}";
                    fullPath = Path.Combine(folderPath, fileName);
                }
                while (true)
                {
                    try
                    {
                        await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
                        {
                            await file.CopyToAsync(stream);
                        }
                        break;
                    }
                    catch (IOException)
                    {
                        order++;
                        fileName = $"{order:D4}{extension.ToLowerInvariant()}";
                        fullPath = Path.Combine(folderPath, fileName);
                    }
                }

                uploadedImageUrls.Add($"/uploads/stories/{storyId}/{chapterId}/{fileName}");
                order++;
            }

            return (uploadedImageUrls, rejectedCount);
        }

        private string GetChapterUploadPath(Guid storyId, Guid chapterId)
        {
            return Path.Combine(_env.WebRootPath, "uploads", "stories", storyId.ToString(), chapterId.ToString());
        }

        private static bool IsAllUploadsRejected(List<IFormFile>? files, (List<string> UploadedImageUrls, int RejectedCount) uploadResult)
        {
            return (files?.Count ?? 0) > 0 && uploadResult.UploadedImageUrls.Count == 0 && uploadResult.RejectedCount > 0;
        }
    }
}
