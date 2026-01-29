using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WibuHub.ApplicationCore.Interface;

namespace WibuHub.ApplicationCore.Entities
{
    public class Author : ISoftDelete
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Name: nvarchar(150) - Tên tác giả
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Comics: ICollection<Story>
        // Danh sách các truyện của tác giả này
        // (Map ngược lại với khóa ngoại AuthorId trong bảng Story)
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }

  
}