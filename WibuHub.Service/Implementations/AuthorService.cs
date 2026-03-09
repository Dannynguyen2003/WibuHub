using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;
namespace WibuHub.Service.Implementations
{
    public class AuthorService : IAuthorService
    {
        private readonly StoryDbContext _context;
        public AuthorService(StoryDbContext context)
        {
            _context = context;
        }
        public async Task<List<Author>> GetAllAsync()
        {
            return await _context.Authors.ToListAsync();
        }
        public async Task<Author?> GetByIdAsync(Guid id)
        {
            return await _context.Authors.FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<AuthorDto?> GetByIdAsDtoAsync(Guid id)
        {
            return await _context.Authors
                .Where(a => a.Id == id)
                .Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Slug = a.Slug
                })
                .SingleOrDefaultAsync();
        }
        public async Task<bool> CreateAsync(AuthorDto authorDto)
        {
            try
            {
                var author = new Author
                {
                    Id = Guid.NewGuid(),
                    Name = authorDto.Name.Trim(),
                    Slug = GenerateSlug(authorDto.Name)
                };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UpdateAsync(AuthorDto authorDto)
        {
            try
            {
                var author = await _context.Authors.FindAsync(authorDto.Id);
                if (author == null) return false;
                author.Name = authorDto.Name.Trim();
                author.Slug = GenerateSlug(authorDto.Name);
                _context.Update(author);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var author = await _context.Authors.FindAsync(id);
                if (author == null) return false;
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static string GenerateSlug(string phrase)
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
    }
}