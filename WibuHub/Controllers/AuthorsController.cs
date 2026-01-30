using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.Service.Interface;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(IAuthorService authorService, ILogger<AuthorsController> logger)
        {
            _authorService = authorService;
            _logger = logger;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            try
            {
                var authors = await _authorService.GetAllAsync();
                return View(authors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authors list");
                return View("Error");
            }
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var author = await _authorService.GetByIdAsync(id.Value);
                if (author == null)
                {
                    return NotFound();
                }

                return View(author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving author details for ID: {Id}", id);
                return View("Error");
            }
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorDto authorDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isSuccess = await _authorService.CreateAsync(authorDto);
                    if (isSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể tạo tác giả.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating author");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo tác giả.");
                }
            }
            return View(authorDto);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var authorDto = await _authorService.GetByIdAsDtoAsync(id.Value);
                if (authorDto == null)
                {
                    return NotFound();
                }
                return View(authorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading author for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AuthorDto authorDto)
        {
            if (id != authorDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var isSuccess = await _authorService.UpdateAsync(authorDto);
                    if (isSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể cập nhật tác giả.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating author: {Id}", id);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật tác giả.");
                }
            }
            return View(authorDto);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var author = await _authorService.GetByIdAsync(id.Value);
                if (author == null)
                {
                    return NotFound();
                }

                return View(author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading author for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var isSuccess = await _authorService.DeleteAsync(id);
                if (!isSuccess)
                {
                    _logger.LogWarning("Failed to delete author: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author: {Id}", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
