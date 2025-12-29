using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Class.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; } 
        public virtual ICollection<Story> Stories { get; set; } = new Collection<Story>();

    }
}
