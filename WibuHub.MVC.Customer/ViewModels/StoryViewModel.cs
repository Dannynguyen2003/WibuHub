namespace WibuHub.MVC.Customer.ViewModels
{
    public class StoryViewModel
    {
        public Guid Id { get; set; } // Bạn có thể đổi thành kiểu Guid hoặc string tùy theo DB của bạn
        public string StoryName { get; set; } = string.Empty;
        public string CoverImage { get; set; }
        public string? LatestChapter { get; set; }
        public long ViewCount { get; set; }
        public int FollowCount { get; set; }

        public string TimeAgo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
