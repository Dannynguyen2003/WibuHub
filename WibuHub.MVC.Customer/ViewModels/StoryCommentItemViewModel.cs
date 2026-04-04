namespace WibuHub.MVC.Customer.ViewModels
{
    public class StoryCommentItemViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = "Ngý?i dùng";
        public string? Avatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
    }
}
