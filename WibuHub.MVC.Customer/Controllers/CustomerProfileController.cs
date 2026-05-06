using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels;

namespace WibuHub.MVC.Customer.Controllers
{
    [Authorize]
    public class CustomerProfileController : Controller
    {
        private readonly UserManager<StoryUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly SignInManager<StoryUser> _signInManager;
        private readonly StoryDbContext _context;

        public CustomerProfileController(
            UserManager<StoryUser> userManager,
            IWebHostEnvironment environment,
            SignInManager<StoryUser> signInManager,
            StoryDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _signInManager = signInManager;
            _context = context;
        }

        // ==========================================
        // 1. CHỨC NĂNG QUẢN LÝ THÔNG TIN (INDEX)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            int expPerLevel = 100;
            // FIX LỖI ÉP KIỂU AN TOÀN TẠI ĐÂY:
            int percentage = Convert.ToInt32(((double)user.Experience / expPerLevel) * 100);
            if (percentage > 100) percentage = 100;
            if (percentage < 0) percentage = 0;

            var model = new CustomerProfileVM
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Email = user.Email,
                UserName = user.UserName,
                Level = user.Level,
                Points = user.Points,
                ExpPercentage = percentage,
                CurrentExperience = user.Experience,
                ExpPerLevel = expPerLevel
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CustomerProfileVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            model.Email = user.Email;
            model.UserName = user.UserName;
            model.Level = user.Level;
            model.Points = user.Points;
            int expPerLevel = 100;
            model.CurrentExperience = user.Experience;
            model.ExpPerLevel = expPerLevel;
            // FIX LỖI ÉP KIỂU AN TOÀN TẠI ĐÂY:
            double rawPercentage = ((double)user.Experience / expPerLevel) * 100;
            int percentage = (int)Math.Round(rawPercentage);
            if (percentage > 100) percentage = 100;
            if (percentage < 0) percentage = 0;
            model.ExpPercentage = percentage;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var extension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
                var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.AvatarFile), "Invalid image format.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.Avatar = $"/uploads/avatars/{fileName}";
                model.Avatar = user.Avatar;
            }
            else
            {
                user.Avatar = model.Avatar;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            ViewData["StatusMessage"] = "Cập nhật hồ sơ thành công.";
            return View(model);
        }

        // ==========================================
        // 2. CHỨC NĂNG ĐỔI MẬT KHẨU
        // ==========================================
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Đổi mật khẩu thành công!";
            ModelState.Clear();
            return View(new ChangePasswordVM());
        }

        // ==========================================
        // 3. TRANG DANH SÁCH THEO DÕI (CẬP NHẬT LẤY CHAPTER MỚI NHẤT)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Followed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            Guid userIdGuid = Guid.Parse(user.Id);

            var follows = await _context.Follows
                .Include(f => f.Story)
                .Where(f => f.UserId == userIdGuid)
                .Select(f => new FollowItemVM
                {
                    StoryId = f.StoryId,
                    StoryTitle = f.Story.StoryName,
                    CoverImage = f.Story.CoverImage,

                    // LOGIC MỚI: Quét trong bảng Chapter, lấy ra số Chapter lớn nhất (mới nhất)
                    TotalChapters = _context.Chapters
                                        .Where(c => c.StoryId == f.StoryId)
                                        .Max(c => (int?)c.ChapterNumber) ?? 0,

                    ViewCount = f.Story.ViewCount
                }).ToListAsync();

            return View(follows);
        }

        // ==========================================
        // HÀM BỎ THEO DÕI (DÙNG CHUNG)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Unfollow(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            Guid userIdGuid = Guid.Parse(user.Id);

            var follow = await _context.Follows.FirstOrDefaultAsync(f => f.UserId == userIdGuid && f.StoryId == storyId);
            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Followed");
        }

        // ==========================================
        // 4. TRANG LỊCH SỬ ĐỌC TRUYỆN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            Guid userIdGuid = Guid.Parse(user.Id);

            // Lấy lịch sử đọc
            var rawHistories = await _context.Histories
                .Include(h => h.Story).Include(h => h.Chapter)
                .Where(h => h.UserId == userIdGuid)
                .OrderByDescending(h => h.ReadTime)
                .ToListAsync();

            // LOGIC FIX: Gộp nhóm theo truyện (GroupBy StoryId) để một truyện chỉ xuất hiện 1 lần duy nhất
            var histories = rawHistories
                .GroupBy(h => h.StoryId)
                .Select(g => g.First()) // Lấy dòng mới đọc nhất
                .Take(20)
                .Select(h => new HistoryItemVM
                {
                    StoryId = h.StoryId,
                    StoryTitle = h.Story.StoryName,
                    CoverImage = h.Story.CoverImage,
                    ChapterNumber = h.Chapter.ChapterNumber,
                    ReadTime = h.ReadTime
                }).ToList();

            return View(histories);
        }

        // ==========================================
        // HÀM XÓA LỊCH SỬ ĐỌC (MỚI THÊM)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> RemoveHistory(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            Guid userIdGuid = Guid.Parse(user.Id);

            var history = await _context.Histories.FirstOrDefaultAsync(h => h.UserId == userIdGuid && h.StoryId == storyId);
            if (history != null)
            {
                _context.Histories.Remove(history);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("History");
        }
    }
}
