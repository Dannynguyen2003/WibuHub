using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WibuHub.Common.Contants;

namespace WibuHub.MVC.ViewModels
{
    [Bind("Id,Name,Description,Position")]
    public class CategoryVM
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(MaxLengths.TITLE)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(MaxLengths.DESCRIPTION)]
        public string? Description { get; set; }
        public int Position { get; set; }
    }
}
