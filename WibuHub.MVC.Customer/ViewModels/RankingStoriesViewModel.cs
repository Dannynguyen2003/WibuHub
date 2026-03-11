using System;
using System.Collections.Generic;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.MVC.Customer.ViewModels
{
    public class RankingStoriesViewModel
    {
        public string Title { get; set; } = string.Empty;
        public IReadOnlyList<Story> Stories { get; set; } = Array.Empty<Story>();
    }
}
