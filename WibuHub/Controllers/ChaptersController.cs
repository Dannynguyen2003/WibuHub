using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace WibuHub.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChaptersController(StoryDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Chapters
        public async Task<IActionResult> Index()
        {
            var StoryDbContext = _context.Chapters.Include(c => c.Story);
            return View(await StoryDbContext.ToListAsync());
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
        public async Task<IActionResult> Create( ChapterVM chapterVM)
        {
            if (ModelState.IsValid)
            {
                var imageUrls = (chapterVM.ImageUrls ?? new List<string>())
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .ToList();
                var chapterId = Guid.NewGuid();
                var uploadedImageUrls = await SaveUploadedImagesAsync(chapterVM.UploadImages, chapterVM.StoryId, chapterId, imageUrls.Count + 1);
                var orderedImages = imageUrls
                    .Select(url => new { Url = url, StorageType = chapterVM.ServerId })
                    .Concat(uploadedImageUrls.Select(url => new { Url = url, StorageType = 0 }))
                    .ToList();
                var chapter = new Chapter
                {
                    Id = chapterId,
                    StoryId = chapterVM.StoryId,
                    Name = chapterVM.Name.Trim(),
                    ChapterNumber = chapterVM.ChapterNumber,
                    Slug = string.IsNullOrEmpty(chapterVM.Slug)
                           ? GenerateSlug(chapterVM.Name)
                           : chapterVM.Slug.Trim(),
                    ViewCount = 0,
                    ServerId = chapterVM.ServerId,
                    CreatedAt = DateTime.UtcNow,
                    Price = chapterVM.Price,
                    Discount = chapterVM.Discount,
                    Images = orderedImages
                        .Select((image, index) => new ChapterImage
                        {
                            ImageUrl = image.Url,
                            OrderIndex = index + 1,
                            ChapterId = chapterId,
                            StorageType = image.StorageType
                        })
                        .ToList()

                };
                //chapter.Id = Guid.NewGuid();
                //_context.Add(chapter);
                await _context.Chapters.AddAsync(chapter);
                await _context.SaveChangesAsync();
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
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }
            var imageUrls = (chapter.Images ?? new List<ChapterImage>())
                .OrderBy(i => i.OrderIndex)
                .Select(i => i.ImageUrl)
                .ToList();
            var chapterVM = new ChapterVM
            {
                Id = chapter.Id,
                StoryId = chapter.StoryId,
                Name = chapter.Name,
                ChapterNumber = chapter.ChapterNumber,
                Slug = chapter.Slug,
                ViewCount = chapter.ViewCount,
                Content = chapter.Content,
                ImageUrls = imageUrls,
                ServerId = chapter.ServerId,
                CreatedAt = chapter.CreatedAt,
                Price = chapter.Price,
                Discount = chapter.Discount
            };
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapter.StoryId);
            return View(nameof(Create), chapterVM);
        }

        // POST: Chapters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,  ChapterVM chapterVM)
        {
            if (id != chapterVM.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var chapter = await _context.Chapters
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
                    var uploadedImageUrls = await SaveUploadedImagesAsync(chapterVM.UploadImages, chapterVM.StoryId, chapter.Id, imageUrls.Count + 1);
                    var orderedImages = imageUrls
                        .Select(url => new { Url = url, StorageType = chapterVM.ServerId })
                        .Concat(uploadedImageUrls.Select(url => new { Url = url, StorageType = 0 }))
                        .ToList();
                    chapter.StoryId = chapterVM.StoryId;
                    chapter.Name = chapterVM.Name.Trim();
                    chapter.ChapterNumber = chapterVM.ChapterNumber;
                    chapter.Slug = chapterVM.Slug.Trim();
                    chapter.ServerId = chapterVM.ServerId;
                    chapter.Price = chapterVM.Price;
                    chapter.Discount = chapterVM.Discount;
                    _context.ChapterImages.RemoveRange(chapter.Images);
                    chapter.Images = orderedImages
                        .Select((image, index) => new ChapterImage
                        {
                            ImageUrl = image.Url,
                            OrderIndex = index + 1,
                            ChapterId = chapter.Id,
                            StorageType = image.StorageType
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
                    else
                    {
                        throw;
                    }
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

        private async Task<List<string>> SaveUploadedImagesAsync(List<IFormFile>? uploadImages, Guid storyId, Guid chapterId, int startOrder)
        {
            var uploadedImageUrls = new List<string>();
            if (uploadImages == null || uploadImages.Count == 0) return uploadedImageUrls;

            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            const long maxImageSizeBytes = 10 * 1024 * 1024;
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", storyId.ToString(), chapterId.ToString());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var order = startOrder;
            foreach (var file in uploadImages)
            {
                if (file == null || file.Length <= 0) continue;
                if (file.Length > maxImageSizeBytes) continue;
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension)) continue;
                if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) continue;
                await using var uploadStream = file.OpenReadStream();
                if (!await IsAllowedImageSignatureAsync(uploadStream)) continue;
                if (uploadStream.CanSeek) uploadStream.Position = 0;

                var fileName = $"{order:D4}{extension}";
                var fullPath = Path.Combine(folderPath, fileName);
                while (System.IO.File.Exists(fullPath))
                {
                    order++;
                    fileName = $"{order:D4}{extension}";
                    fullPath = Path.Combine(folderPath, fileName);
                }
                await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
                {
                    await uploadStream.CopyToAsync(stream);
                }
                uploadedImageUrls.Add($"/uploads/stories/{storyId}/{chapterId}/{fileName}");
                order++;
            }

            return uploadedImageUrls;
        }

        private static async Task<bool> IsAllowedImageSignatureAsync(Stream stream)
        {
            var header = new byte[12];
            var read = await stream.ReadAsync(header, 0, header.Length);
            if (read < 4) return false;

            // JPEG
            if (header[0] == 0xFF && header[1] == 0xD8) return true;
            // PNG
            if (read >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A) return true;
            // GIF
            if (read >= 6 &&
                header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 &&
                header[3] == 0x38 && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61) return true;
            // WEBP: RIFF....WEBP
            if (read >= 12 &&
                header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;

            return false;
        }
    }
}
