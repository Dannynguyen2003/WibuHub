namespace WibuHub.MVC.Customer.ViewModels
{
    public class FollowItemVM
    {
        public Guid StoryId { get; set; } // Sửa thành int nếu StoryId của bạn là int
        public string StoryTitle { get; set; }
        public string CoverImage { get; set; }
        public int TotalChapters { get; set; }
        // Match Story.ViewCount which is a long (bigint) in the data model
        public long ViewCount { get; set; }
    }
}
