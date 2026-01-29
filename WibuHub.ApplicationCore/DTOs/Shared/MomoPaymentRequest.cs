using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class MomoPaymentRequest
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string OrderInfo { get; set; } = string.Empty;

        public string? OrderId { get; set; }

        public string? ExtraData { get; set; }
    }
}
