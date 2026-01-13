using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.DTOs.Admin
{
    public class Admin
    {
        public record CreateStoryDto(string Title, string Description, string CoverImageUrl, List<int> CategoryIds);

        // DTO sửa truyện
        public record UpdateStoryDto(string Title, string Description, string CoverImageUrl, bool IsCompleted);

        // DTO up chương (Giả sử nội dung là text hoặc link ảnh đã upload sẵn)
        public record UploadChapterDto(string Title, float ChapterNumber, string ContentUrl, decimal Price = 0);

        // DTO chỉnh giá
        public record SetPriceDto(decimal Price);

        // DTO rút tiền
        public record WithdrawDto(decimal Amount, string BankInfo);
    }
}
