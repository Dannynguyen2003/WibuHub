using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class ChapterService : IChapterService
    {
        private readonly StoryDbContext _context;
        // Cần inject Environment để lấy đường dẫn gốc wwwroot
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public ChapterService(StoryDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<ChapterDto>> GetAllAsync()
        {
            return await _context.Chapters
                .Include(c => c.Story)
                .Select(c => new ChapterDto
                {
                    Id = c.Id,
                    Title = c.Name,
                    ChapterNumber = c.ChapterNumber,
                    StoryId = c.StoryId,
                    StoryTitle = c.Story.Title
                })
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<ChapterDto?> GetByIdAsync(Guid id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Images) // Join bảng ảnh
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null) return null;

            return new ChapterDto
            {
                Id = chapter.Id,
                Title = chapter.Name,
                ChapterNumber = chapter.ChapterNumber,
                StoryId = chapter.StoryId,
                StoryTitle = chapter.Story.Title,
                // Lấy list ảnh và sắp xếp đúng thứ tự trang
                ImageUrls = chapter.Images.OrderBy(i => i.OrderIndex).Select(i => i.ImageUrl).ToList()
            };
        }

        public async Task<List<ChapterDto>> GetByStoryIdAsync(Guid storyId)
        {
            return await _context.Chapters
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.ChapterNumber) // Sắp xếp theo số chương: 1, 2, 3...
                .Select(c => new ChapterDto
                {
                    Id = c.Id,
                    Title = c.Name,
                    ChapterNumber = c.ChapterNumber,
                    StoryId = c.StoryId
                })
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(ChapterDto dto)
        {
            // 1. Tạo Entity Chapter
            var chapter = new Chapter
            {
                Id = Guid.NewGuid(),
                Name = dto.Title,
                ChapterNumber = dto.ChapterNumber,
                StoryId = dto.StoryId,
                CreatedAt = DateTime.Now,
                Images = new List<ChapterImage>()
            };

            // 2. Xử lý Upload Ảnh (Nếu có)
            if (dto.Pages != null && dto.Pages.Count > 0)
            {
                // Tạo thư mục: wwwroot/uploads/stories/{StoryId}/{ChapterId}/
                string folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", dto.StoryId.ToString(), chapter.Id.ToString());
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                int order = 1;
                foreach (var file in dto.Pages)
                {
                    if (file.Length > 0)
                    {
                        // Đặt tên file: 001.jpg, 002.png...
                        string fileName = $"{order:D3}{Path.GetExtension(file.FileName)}";
                        string fullPath = Path.Combine(folderPath, fileName);

                        // Lưu file vật lý
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Thêm vào list ảnh của Entity
                        // Đường dẫn lưu DB: /uploads/stories/...
                        string dbPath = $"/uploads/stories/{dto.StoryId}/{chapter.Id}/{fileName}";

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

            try
            {
                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, ChapterDto dto)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return false;

            chapter.Name = dto.Title;
            chapter.ChapterNumber = dto.ChapterNumber;
            // Lưu ý: Update ảnh thường phức tạp hơn (xóa cũ thêm mới hoặc chèn thêm).
            // Ở đây tạm thời chỉ update thông tin cơ bản. 
            // Nếu muốn update ảnh, bạn cần logic xóa file cũ trong wwwroot.

            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return false;

            // Optional: Xóa file ảnh trong ổ cứng trước khi xóa DB để đỡ rác
            // string folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", chapter.StoryId.ToString(), chapter.Id.ToString());
            // if (Directory.Exists(folderPath)) Directory.Delete(folderPath, true);

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}