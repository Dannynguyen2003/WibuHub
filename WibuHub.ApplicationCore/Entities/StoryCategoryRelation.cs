using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    /// <summary>
    /// Many-to-many join table between Story and Category
    /// </summary>
    public class StoryCategoryRelation
    {
        [Required]
        public Guid StoryId { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story? Story { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}
