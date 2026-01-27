using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using WibuHub.ApplicationCore.Interface;
using WibuHub.Common.Contants;

namespace WibuHub.ApplicationCore.Entities
{
    public class Category : ISoftDelete
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(MaxLengths.NAME)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(MaxLengths.DESCRIPTION)]
        public string? Description { get; set; }

        public int Position { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<Story> Stories { get; set; } = new Collection<Story>();
        public virtual ICollection<Comment> Comments { get; set; } = new Collection<Comment>();
    }
}
