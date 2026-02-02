using FileTypeChecker;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult Create()
        {
            ViewData["StoryId"] = new SelectList(_context.Chapters, "Id", "Title");
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
                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(), // Generate ID first because ProcessImageUploadsAsync uses it for file paths
                    StoryId = chapterVM.StoryId,
                    Name = chapterVM.Name.Trim(),
                    ChapterNumber = chapterVM.ChapterNumber,
                    Slug = string.IsNullOrEmpty(chapterVM.Slug)
                           ? GenerateSlug(chapterVM.Name)
                           : chapterVM.Slug.Trim(),
                    ViewCount = 0,
                    Content = chapterVM.Content?.Trim(),
                    ServerId = chapterVM.ServerId,
                    CreatedAt = DateTime.UtcNow,
                    Price = chapterVM.Price,
                    Discount = chapterVM.Discount,

                };

                // Xử lý upload ảnh nếu có
                if (chapterVM.UploadImages != null && chapterVM.UploadImages.Count > 0)
                {
                    await ProcessImageUploadsAsync(chapter, chapterVM.UploadImages, chapterVM.StoryId, 1);
                }

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
            var chapterVM = new ChapterVM
            {
                Id = chapter.Id,
                StoryId = chapter.StoryId,
                Name = chapter.Name,
                ChapterNumber = chapter.ChapterNumber,
                Slug = chapter.Slug,
                ViewCount = chapter.ViewCount,
                Content = chapter.Content,
                ServerId = chapter.ServerId,
                CreatedAt = chapter.CreatedAt,
                Price = chapter.Price,
                Discount = chapter.Discount
            };
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapter.StoryId);
            ViewData["ExistingImages"] = chapter.Images.OrderBy(i => i.OrderIndex).ToList();
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
                    chapter.StoryId = chapterVM.StoryId;
                    chapter.Name = chapterVM.Name.Trim();
                    chapter.ChapterNumber = chapterVM.ChapterNumber;
                    chapter.Slug = chapterVM.Slug.Trim();
                    chapter.Content = chapterVM.Content?.Trim();
                    chapter.ServerId = chapterVM.ServerId;
                    chapter.Price = chapterVM.Price;
                    chapter.Discount = chapterVM.Discount;

                    // Xử lý upload ảnh mới nếu có (thêm vào danh sách hiện tại)
                    if (chapterVM.UploadImages != null && chapterVM.UploadImages.Count > 0)
                    {
                        // Lấy OrderIndex lớn nhất hiện tại
                        int startOrder = chapter.Images.Any() ? chapter.Images.Max(i => i.OrderIndex) + 1 : 1;
                        await ProcessImageUploadsAsync(chapter, chapterVM.UploadImages, chapterVM.StoryId, startOrder);
                    }

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

        // Helper method để xử lý upload ảnh với validation
        private async Task ProcessImageUploadsAsync(Chapter chapter, List<IFormFile> files, Guid storyId, int startOrder)
        {
            // Danh sách extension được phép
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            const long maxFileSize = 10 * 1024 * 1024; // 10MB

            // Tạo thư mục: wwwroot/uploads/stories/{StoryId}/{ChapterId}/
            string folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", storyId.ToString(), chapter.Id.ToString());
            
            try
            {
                if (!Directory.Exists(folderPath)) 
                    Directory.CreateDirectory(folderPath);

                int order = startOrder;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Validation: Kiểm tra kích thước file
                        if (file.Length > maxFileSize)
                        {
                            throw new InvalidOperationException($"File {file.FileName} exceeds maximum allowed size (10MB)");
                        }

                        // Validation: Kiểm tra extension
                        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                        {
                            throw new InvalidOperationException($"File {file.FileName} has unsupported format. Only allowed: {string.Join(", ", allowedExtensions)}");
                        }

                        // Đặt tên file: 001.jpg, 002.png...
                        string fileName = $"{order:D3}{extension}";
                        string fullPath = Path.Combine(folderPath, fileName);

                        // Lưu file vật lý
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Đường dẫn lưu DB: /uploads/stories/...
                        string dbPath = $"/uploads/stories/{storyId}/{chapter.Id}/{fileName}";

                        chapter.Images.Add(new ChapterImage
                        {
                            Id = Guid.NewGuid(),
                            ImageUrl = dbPath,
                            OrderIndex = order,
                            ChapterId = chapter.Id
                        });

                        order++;
                    }
                }
            }
            catch (IOException ex)
            {
                // Wrap IO exception with meaningful error message
                throw new InvalidOperationException($"Error saving file: {ex.Message}", ex);
            }
        }
    }
}
