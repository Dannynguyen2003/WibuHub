using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Class.Entities
{
    public class Story
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;
        [Description("Ngày tạo")]
        public DateTime DateCreated { get; set; }
        public string Status { get; set; } = string.Empty;
        public long ViewCount { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
