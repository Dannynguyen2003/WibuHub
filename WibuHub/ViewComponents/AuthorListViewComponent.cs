using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.ViewComponents
{
    public class AuthorList : ViewComponent
    {
        private readonly StoryDbContext _context;

        public AuthorList(StoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IViewComponentResult> InvokeAsync(int page = 1, int pageSize = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Authors
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name);

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paged data
            var authors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuthorVM
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();

            var pagedResult = new PagedResult<AuthorVM>(authors, page, pageSize, totalCount);

            return View(pagedResult);
        }
    }
}
