using System;
using System.Collections.Generic;

namespace WibuHub.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalStories { get; set; }
        public int TotalChapters { get; set; }
        public int TotalUsers { get; set; }
        public long TotalViews { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public IReadOnlyList<string> ViewLabels { get; set; } = new List<string>();
        public IReadOnlyList<int> ViewCounts { get; set; } = new List<int>();
        public IReadOnlyList<DashboardActivityItem> RecentActivities { get; set; } = new List<DashboardActivityItem>();
        public IReadOnlyList<DashboardStoryItem> RecentStories { get; set; } = new List<DashboardStoryItem>();
    }

    public class DashboardActivityItem
    {
        public string IconClass { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }

    public class DashboardStoryItem
    {
        public Guid Id { get; set; }
        public string StoryName { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public int ChapterCount { get; set; }
        public long ViewCount { get; set; }
        public int Status { get; set; }
    }
}
