using WibuHub.ApplicationCore.Entities;

namespace WibuHub.MVC.Customer.ViewModels
{
    public class ChapterReadViewModel
    {
        public Chapter Chapter { get; set; } = null!;
        public IReadOnlyList<Chapter> Chapters { get; set; } = Array.Empty<Chapter>();
        public Guid? PreviousChapterId { get; set; }
        public Guid? NextChapterId { get; set; }
        public bool IsFollowed { get; set; }
    }
}
