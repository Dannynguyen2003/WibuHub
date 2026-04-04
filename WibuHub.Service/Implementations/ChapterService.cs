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
                    Name = c.Name,
                    ChapterNumber = c.ChapterNumber,
                    StoryId = c.StoryId,
                    StoryName= c.Story.StoryName
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
                Name = chapter.Name,
                ChapterNumber = chapter.ChapterNumber,
                StoryId = chapter.StoryId,
                StoryName = chapter.Story.StoryName,
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
                    Name = c.Name,
                    ChapterNumber = c.ChapterNumber,
                    StoryId = c.StoryId
                })
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(ChapterDto dto)
        {
            // Bắt đầu một Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo Entity Chapter
                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    ChapterNumber = dto.ChapterNumber,
                    StoryId = dto.StoryId,
                    CreatedAt = DateTime.Now,
                    Images = new List<ChapterImage>()
                };

                // 2. Xử lý danh sách URL ảnh (nếu có) theo đúng thứ tự client gửi lên
                var imageOrderIndex = 1;
                if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
                {
                    foreach (var imageUrl in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                    {
                        chapter.Images.Add(new ChapterImage
                        {
                            Id = Guid.NewGuid(),
                            ImageUrl = imageUrl.Trim(),
                            OrderIndex = imageOrderIndex++,
                            ChapterId = chapter.Id
                        });
                    }
                }

                // 3. Xử lý Upload Ảnh vật lý (Nếu có)
                if (dto.UploadImages != null && dto.UploadImages.Count > 0)
                {
                    // Tạo thư mục: wwwroot/uploads/stories/{StoryId}/{ChapterId}/
                    string folderPath = Path.Combine(_env.WebRootPath, "uploads", "stories", dto.StoryId.ToString(), chapter.Id.ToString());
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    foreach (var file in dto.UploadImages)
                    {
                        if (file.Length > 0)
                        {
                            // Đặt tên file: 001.jpg, 002.png...
                            string fileName = $"{imageOrderIndex:D3}{Path.GetExtension(file.FileName)}";
                            string fullPath = Path.Combine(folderPath, fileName);

                            // Lưu file vật lý
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Thêm vào list ảnh của Entity
                            string dbPath = $"/uploads/stories/{dto.StoryId}/{chapter.Id}/{fileName}";

                            chapter.Images.Add(new ChapterImage
                            {
                                Id = Guid.NewGuid(),
                                ImageUrl = dbPath,
                                OrderIndex = imageOrderIndex,
                                ChapterId = chapter.Id
                            });

                            imageOrderIndex++;
                        }
                    }
                }

                // 4. Lưu Chapter vào DB
                _context.Chapters.Add(chapter);

                // --- BẮT ĐẦU LOGIC TẠO THÔNG BÁO ---

                // 5. Lấy danh sách UserId của những người đang Follow truyện này
                var followerIds = await _context.Follows
                                                .Where(f => f.StoryId == dto.StoryId)
                                                .Select(f => f.UserId)
                                                .ToListAsync();

                // 6. Tạo list thông báo cho các followers
                var notifications = new List<Notification>();

                foreach (var userId in followerIds)
                {
                    notifications.Add(new Notification
                    {
                        UserId = userId,
                        Title = "Chapter mới ra lò!",
                        // Sử dụng chapter.ChapterNumber để lấy đúng thông tin vừa tạo
                        Message = $"Truyện bạn theo dõi vừa cập nhật Chapter {chapter.ChapterNumber}.",
                        TargetUrl = $"/Chapters/Read/{chapter.Id}",
                        CreateDate = DateTime.UtcNow
                    });
                }

                // 7. Lưu loạt thông báo vào DB
                if (notifications.Any())
                {
                    _context.Notifications.AddRange(notifications);
                }

                // --- KẾT THÚC LOGIC TẠO THÔNG BÁO ---

                // 8. Commit tất cả thay đổi (Chapter + Images + Notifications)
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (ví dụ lỗi lưu DB hoặc lưu file), rollback lại toàn bộ
                await transaction.RollbackAsync();

                // Bạn có thể log lỗi ex.Message ở đây để dễ debug
                // _logger.LogError(ex, "Lỗi khi tạo chapter mới");

                return false;
            }
        }
        public async Task<bool> UpdateAsync(Guid id, ChapterDto dto)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (chapter == null) return false;

            chapter.Name = dto.Name;
            chapter.ChapterNumber = dto.ChapterNumber;
            chapter.StoryId = dto.StoryId;
            if (dto.ImageUrls != null)
            {
                _context.ChapterImages.RemoveRange(chapter.Images);
                var imageOrderIndex = 1;
                foreach (var imageUrl in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    chapter.Images.Add(new ChapterImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = imageUrl.Trim(),
                        OrderIndex = imageOrderIndex++,
                        ChapterId = chapter.Id
                    });
                }
            }

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