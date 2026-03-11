using System;
using System.Collections.Generic;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.MVC.Customer.ViewModels
{
    public class CategoryStoriesViewModel
    {
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public IReadOnlyList<Story> Stories { get; set; } = Array.Empty<Story>();
    }
}
