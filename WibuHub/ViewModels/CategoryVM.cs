using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WibuHub.Common.Contants;

namespace WibuHub.MVC.ViewModels
{
    [Bind("Id,Name,Description,Slug,Position")]
    [Index(nameof(Name), IsUnique = true)]
    public class CategoryVM
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(MaxLengths.TITLE)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(MaxLengths.DESCRIPTION)]
        public string? Description { get; set; }

        // Slug: varchar(150) - URL thân thiện
        [Required]
        [MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        public int Position { get; set; }
    }
}
