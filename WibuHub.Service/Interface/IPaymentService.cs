using WibuHub.ApplicationCore.DTOs.Shared;
using static WibuHub.ApplicationCore.DTOs.Shared.Momopayment;
namespace WibuHub.Service.Interface
{
    public interface IPaymentService
    {
        /// <summary>
        /// Create a MoMo payment request
        /// </summary>
        Task<MomoPaymentResponse> CreateMomoPaymentAsync(MomoPaymentRequest request);
        /// <summary>
        /// Handle MoMo callback after payment
        /// </summary>
        Task<(bool isSuccess, string message)> HandleMomoCallbackAsync(MomoCallbackRequest callback);
        /// <summary>
        /// Check transaction status with MoMo
        /// </summary>
        Task<MomoPaymentResponse> CheckTransactionStatusAsync(string orderId);
    }
}