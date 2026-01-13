using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class Author
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Name: nvarchar(150) - Tên tác giả
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // Comics: ICollection<Story>
        // Danh sách các truyện của tác giả này
        // (Map ngược lại với khóa ngoại AuthorId trong bảng Story)
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }

    public enum AuthorStatus
    {
        Ongoing = 0,    // Đang tiến hành
        Completed = 1,  // Hoàn thành
        Paused = 2      // Tạm ngưng
    }
}