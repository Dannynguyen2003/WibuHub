using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using WibuHub.ApplicationCore.Entities;
using WibuHub.Common.Contants;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.Controllers
{
    //[Area("Admin")]
    [Authorize(Roles = AppConstants.Roles.Uploader + "," + AppConstants.Roles.SuperAdmin)]
    public class ChaptersController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jfif", ".png", ".webp", ".gif", ".bmp"
        };
        const long maxImageSizeBytes = 10 * 1024 * 1024;
        public ChaptersController(StoryDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Chapters
        public IActionResult Index()
        {
            return View();
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterVM chapterVM)
        {
            var uploadFiles = chapterVM.UploadImages?.Where(file => file != null && file.Length > 0).ToList()
                ?? new List<IFormFile>();
            foreach (var file in uploadFiles)
            {
                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension) || file.Length > maxImageSizeBytes)
                {
                    ModelState.AddModelError(nameof(chapterVM.UploadImages), "Ảnh upload không hợp lệ hoặc vượt quá dung lượng cho phép.");
                    break;
                }
            }
            if (ModelState.IsValid)
            {
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
                var chapterId = chapterVM.Id != Guid.Empty ? chapterVM.Id : Guid.NewGuid();
                if (uploadFiles.Count > 0)
                {
                    var uploadedUrls = await SaveUploadImagesAsync(uploadFiles, chapterVM.StoryId, chapterId, imageUrls.Count + 1);
                    imageUrls.AddRange(uploadedUrls);
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
                    ImageUrls = imageUrls,
                    Images = imageUrls
                        .Select((url, index) => new ChapterImage
                        {
                            ImageUrl = url,
                            OrderIndex = index + 1
                        })
                        .ToList()

                };
                //chapter.Id = Guid.NewGuid();
                //_context.Add(chapter);
                await _context.Chapters.AddAsync(chapter);

                var followerIds = await _context.Follows
                    .Where(f => f.StoryId == chapter.StoryId)
                    .Select(f => f.UserId)
                    .ToListAsync();

                var notifications = followerIds
                    .Select(userId => new Notification
                    {
                        UserId = userId,
                        Title = "Chapter mới ra lò!",
                        Message = $"{chapter.Name} vừa được cập nhật.",
                        TargetUrl = $"/Chapters/Read/{chapter.Id}",
                        CreateDate = DateTime.UtcNow
                    })
                    .ToList();

                if (notifications.Any())
                {
                    _context.Notifications.AddRange(notifications);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo chapter thành công!";
                return RedirectToAction(nameof(Create));
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
                CreatedAt = chapter.CreatedAt
            };
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapter.StoryId);
            return View(nameof(Create), chapterVM);
        }

        // POST: Chapters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ChapterVM chapterVM)
        {
            if (id != chapterVM.Id)
            {
                return BadRequest();
            }
            var uploadFiles = chapterVM.UploadImages?.Where(file => file != null && file.Length > 0).ToList()
                ?? new List<IFormFile>();
            foreach (var file in uploadFiles)
            {
                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension) || file.Length > maxImageSizeBytes)
                {
                    ModelState.AddModelError(nameof(chapterVM.UploadImages), "Ảnh upload không hợp lệ hoặc vượt quá dung lượng cho phép.");
                    break;
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var chapter = await _context.Chapters
                       .Include(c => c.Story)
                       .Include(c => c.Images)
                       .FirstOrDefaultAsync(c => c.Id == id);
                    if (chapter == null)
                    {
                        return BadRequest();
                    }
                    var imageUrls = chapterVM.ImageUrls
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Select(url => url.Trim())
                        .ToList();
                    if (uploadFiles.Count > 0)
                    {
                        var uploadedUrls = await SaveUploadImagesAsync(uploadFiles, chapter.StoryId, chapter.Id, imageUrls.Count + 1);
                        imageUrls.AddRange(uploadedUrls);
                    }
                    chapter.StoryId = chapterVM.StoryId;
                    chapter.Name = chapterVM.Name.Trim();
                    chapter.ChapterNumber = chapterVM.ChapterNumber;
                    chapter.Slug = chapterVM.Slug.Trim();
                    chapter.Content = string.Join(Environment.NewLine, imageUrls);
                    chapter.ServerId = chapterVM.ServerId;
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
                }
            }
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
            return View(nameof(Create), chapterVM);
        }

        private async Task<List<string>> SaveUploadImagesAsync(IReadOnlyList<IFormFile> files, Guid storyId, Guid chapterId, int startIndex)
        {
            var results = new List<string>();
            if (files.Count == 0)
            {
                return results;
            }
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", storyId.ToString(), chapterId.ToString());
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var orderIndex = startIndex;
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{orderIndex:D3}{extension}";
                var fullPath = Path.Combine(folderPath, fileName);
                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                results.Add($"/uploads/stories/{storyId}/{chapterId}/{fileName}");
                orderIndex++;
            }
            return results;
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
        public async Task<IActionResult> Reload(Guid? storyId, int page = 1, int pageSize = 10)
        {
            return ViewComponent("ChapterList", new { storyId, page, pageSize });
        }

        private bool ChapterExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id);
        }
    }
}
