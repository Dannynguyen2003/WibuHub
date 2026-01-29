using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class MomoTransactionStatusRequest
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
    }
}
