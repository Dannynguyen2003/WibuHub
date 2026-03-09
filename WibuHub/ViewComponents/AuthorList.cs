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
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Authors
                .Where(a => !a.IsDeleted);

            var totalCount = await query.LongCountAsync();

            var authors = await query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuthorVM
                {
                    Id = a.Id,
                    Name = a.Name,
                    Slug = a.Slug
                })
                .ToListAsync();

            var result = new PagedResult<AuthorVM>(authors, page, pageSize, totalCount);
            return View(result);
        }
    }
}