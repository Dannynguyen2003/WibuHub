using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Interface;
using WibuHub.Common.Contants;

namespace WibuHub.ApplicationCore.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Category : ISoftDelete
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(MaxLengths.NAME)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(MaxLengths.DESCRIPTION)]
        public string? Description { get; set; }

        // Slug: varchar(150) - URL thân thiện
        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string Slug { get; set; } = string.Empty;

        public int Position { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<Story> Stories { get; set; } = new Collection<Story>();
        public virtual ICollection<Comment> Comments { get; set; } = new Collection<Comment>();
        public virtual ICollection<StoryCategory> StoryCategories { get; set; } = new List<StoryCategory>();
    }
}
