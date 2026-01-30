using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.Service.Interface
{
    public interface IAuthorService
    {
        Task<List<Author>> GetAllAsync();
        Task<Author?> GetByIdAsync(Guid id);
        Task<AuthorDto?> GetByIdAsDtoAsync(Guid id);
        Task<bool> CreateAsync(AuthorDto authorDto);
        Task<bool> UpdateAsync(AuthorDto authorDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
