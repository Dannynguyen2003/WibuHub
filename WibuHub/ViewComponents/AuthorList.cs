using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var authors = await _context.Authors
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .Select(a => new AuthorVM
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();
            return View(authors);
        }
    }
}