using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class ComicCategory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Name: nvarchar(150)
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // Navigation: many-to-many with Story
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }
}