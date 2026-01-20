using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WibuHub.MVC.ViewModels;
using WibuHub.Service.Interface;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL sẽ là: api/categories
    //[Authorize] // Bảo vệ API nếu cần
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories); // Trả về HTTP 200 và JSON data
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục" }); // HTTP 404
            }
            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // HTTP 400
            }

            var result = await _categoryService.CreateAsync(categoryVM);

            if (result)
            {
                // Trong API chuẩn REST, thường sẽ trả về CreatedAtAction (HTTP 201)
                // Nhưng vì Service hiện tại chỉ trả về bool, ta trả về OK kèm thông báo
                return Ok(new { success = true, message = "Tạo mới thành công" });
            }
            else
            {
                return BadRequest(new { success = false, message = "Tên danh mục đã tồn tại" });
            }
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryVM categoryVM)
        {
            if (id != categoryVM.Id)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _categoryService.UpdateAsync(categoryVM);

            if (result)
            {
                return Ok(new { success = true, message = "Cập nhật thành công" });
            }
            else
            {
                return NotFound(new { success = false, message = "Không tìm thấy danh mục hoặc lỗi cập nhật" });
            }
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (result.isSuccess)
            {
                return Ok(new { success = true, message = result.message });
            }
            else
            {
                // Tùy vào lỗi mà trả về 404 hoặc 400
                return BadRequest(new { success = false, message = result.message });
            }
        }
    }
}