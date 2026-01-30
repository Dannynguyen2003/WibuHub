using System.ComponentModel;

namespace WibuHub.ApplicationCore.Entities
{
    /// <summary>
    /// Transaction history - tracks user payment activities
    /// Links users to their payment methods and external transaction IDs
    /// </summary>
    public class Transaction
    {
        public Transaction()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        [Description("User thực hiện giao dịch")]
        public string UserId { get; set; } = string.Empty;

        [Description("Đơn hàng liên quan (nếu có)")]
        public Guid? OrderId { get; set; }

        [Description("Số tiền giao dịch")]
        public decimal Amount { get; set; }

        [Description("Phương thức thanh toán được sử dụng")]
        public int PaymentMethodId { get; set; }

        [Description("Mã giao dịch từ cổng thanh toán (VD: MoMo TransId)")]
        public string? ExternalTransactionId { get; set; }

        [Description("Trạng thái giao dịch (Success, Failed, Pending, Cancelled)")]
        public string Status { get; set; } = "Pending";

        [Description("Thông tin đơn hàng")]
        public string? OrderInfo { get; set; }

        [Description("Ngày tạo giao dịch")]
        public DateTime CreatedAt { get; set; }

        [Description("Ngày cập nhật trạng thái")]
        public DateTime? UpdatedAt { get; set; }

        [Description("Thông tin bổ sung (JSON)")]
        public string? Metadata { get; set; }

        // Navigation properties
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual Order? Order { get; set; }
    }
}
