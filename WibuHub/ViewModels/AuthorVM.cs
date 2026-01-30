using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WibuHub.Common.Contants;

namespace WibuHub.MVC.ViewModels
{
    [Bind("Id,Name,Slug")]
    public class AuthorVM
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(MaxLengths.NAME)]
        public string Name { get; set; } = string.Empty;

        // Slug: varchar(150) - URL thân thiện
        [Required]
        [MaxLength(150)]
        public string Slug { get; set; } = string.Empty;
    }
}
