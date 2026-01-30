using System.ComponentModel;

namespace WibuHub.ApplicationCore.Entities
{
    /// <summary>
    /// Master data for payment methods (Admin manages)
    /// Used to display available payment options and control which gateways are active
    /// </summary>
    public class PaymentMethod
    {
        public int Id { get; set; }

        [Description("Tên phương thức thanh toán (VD: Ví MoMo, VNPay, Chuyển khoản ngân hàng)")]
        public string Name { get; set; } = string.Empty;

        [Description("Mã phương thức (VD: MOMO, VNPAY, BANK)")]
        public string Code { get; set; } = string.Empty;

        [Description("Trạng thái kích hoạt - Admin có thể bật/tắt")]
        public bool IsActive { get; set; } = true;

        [Description("URL logo phương thức thanh toán")]
        public string? LogoUrl { get; set; }

        [Description("Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        [Description("Mô tả phương thức thanh toán")]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
