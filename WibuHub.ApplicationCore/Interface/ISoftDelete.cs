namespace WibuHub.ApplicationCore.Interface
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; } // Thêm cái này để biết xóa lúc nào (tùy chọn)
    }
}