namespace WibuHub.MVC.Customer.ViewModels
{
    public class HistoryItemVM
    {
        public Guid StoryId { get; set; } // Sửa thành int nếu StoryId của bạn là int
        public string StoryTitle { get; set; }
        public string CoverImage { get; set; }
        // Match Chapter.ChapterNumber which is a double in the data model
        public double ChapterNumber { get; set; }
        public DateTime ReadTime { get; set; }
    }
}
