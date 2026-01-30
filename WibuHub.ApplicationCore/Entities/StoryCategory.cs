using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    [Table("StoryCategories")]
    public class StoryCategory
    {
        public Guid StoryId { get; set; }
        public virtual Story Story { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
    }
}