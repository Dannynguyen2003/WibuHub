using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    /// <summary>
    /// Many-to-many join table between Story and Category.
    /// This allows a Story to belong to multiple Categories (e.g., Action, Romance, Fantasy).
    /// Note: This is different from StoryCategory (Genres) which represents genre classifications.
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
