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

        public ChaptersController(StoryDbContext context)
        {
            _context = context;
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
                    StoryId = chapterVM.StoryId,
                    Name = chapterVM.Name,
                    ChapterNumber = chapterVM.ChapterNumber,
                    Slug = chapterVM.Slug,
                    ViewCount = chapterVM.ViewCount,
                    Content = chapterVM.Content,
                    ServerId = chapterVM.ServerId,
                    CreatedAt = chapterVM.CreatedAt,
                    Price = chapterVM.Price,
                    Discount = chapterVM.Discount,

                };
                //chapter.Id = Guid.NewGuid();
                //_context.Add(chapter);
                await _context.Chapters.AddAsync(chapter);

                //=== Xử lý file ===//
                //var file = chapterVM.Image;
                //if (file == null || file.Length == 0)
                //{
                //    ViewBag.Message = "Error: No file selected.";
                //    return View(chapterVM); // Return to the upload page with an error
                //}

                //// 1. Get the path to the "uploads" folder in wwwroot
                ////    _environment.WebRootPath gives you the path to the wwwroot folder
                //string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                //// 2. Create the folder if it doesn't exist
                //if (!Directory.Exists(uploadsFolder))
                //{
                //    Directory.CreateDirectory(uploadsFolder);
                //}

                //// 3. Create a unique file name to prevent overwriting
                ////    (SECURITY: Never use the user's file name directly)
                //string extension = Path.GetExtension(file.FileName);

                ////string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                //string uniqueFileName = Guid.NewGuid().ToString() + extension;
                //string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //await using var stream = file.OpenReadStream();
                //if (!FileTypeValidator.IsImage(stream))
                //{
                //    ModelState.AddModelError("File", "The uploaded file is not a valid image.");
                //    return BadRequest(ModelState);
                //}
                ////if (!(FileTypeValidator.IsTypePng(stream) || FileTypeValidator.IsTypeJpg(stream)))
                ////{
                ////     }

                //// 4. Save the file to the server
                //try
                //{
                //    using (var fileStream = new FileStream(filePath, FileMode.Create))
                //    {
                //        await file.CopyToAsync(fileStream);
                //    }

                //    ViewBag.Message = "File uploaded successfully!";
                //    // You can store 'uniqueFileName' or '/uploads/' + uniqueFileName in your database

                //    var newImage = new Image
                //    {
                //        Name = file.Name,
                //        FileName = uniqueFileName,
                //        Extension = extension
                //    };
                //    await _context.Images.AddAsync(newImage);
                //    chapter.ImageId = newImage.Id;
                //}
                //catch (Exception ex)
                //{
                //    ViewBag.Message = "Error: " + ex.Message;
                //}

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StoryId"] = new SelectList(_context.Chapters, "Id", "Title", chapterVM.StoryId);
            return View(chapterVM);
        }

        // GET: Chapters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }
            ViewData["StoryId"] = new SelectList(_context.Chapters, "Id", "Title", chapter.StoryId);
            return View(chapter);
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
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chapterVM);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapterVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StoryId"] = new SelectList(_context.Chapters, "Id", "Title", chapterVM.StoryId);
            return View(chapterVM);
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
            if (chapter != null)
            {
                _context.Chapters.Remove(chapter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChapterExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id);
        }
    }
}
