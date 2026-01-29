using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.Service.Interface;
using static WibuHub.ApplicationCore.DTOs.Shared.Momopayment;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Create a MoMo payment request
        /// POST api/payments/momo
        /// </summary>
        [HttpPost("momo")]
        public async Task<IActionResult> CreateMomoPayment([FromBody] MomoPaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _paymentService.CreateMomoPaymentAsync(request);

                if (response.ErrorCode == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Payment created successfully",
                        data = response
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = response.Message,
                        resultCode = response.ErrorCode
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while creating payment"
                });
            }
        }

        /// <summary>
        /// Handle MoMo payment callback
        /// POST api/payments/momo/callback
        /// </summary>
        [HttpPost("momo/callback")]
        public async Task<IActionResult> MomoCallback([FromBody] MomoCallbackRequest callback)
        {
            try
            {
                _logger.LogInformation("MoMo callback received for OrderId: {OrderId}, ResultCode: {ResultCode}",
                    callback.OrderId, callback.ResultCode);

                var (isSuccess, message) = await _paymentService.HandleMomoCallbackAsync(callback);

                if (!isSuccess)
                {
                    _logger.LogWarning("MoMo callback processing failed: {Message} for OrderId: {OrderId}",
                         message, callback.OrderId);
                }

                // MoMo expects 204 No Content on success and error (to prevent retries)
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo callback for OrderId: {OrderId}", callback.OrderId);
                return NoContent(); // Still return 204 to MoMo even on error
            }
        }

        /// <summary>
        /// Check MoMo transaction status
        /// GET api/payments/momo/status/{orderId}
        /// </summary>
        [HttpGet("momo/status/{orderId}")]
        public async Task<IActionResult> CheckTransactionStatus(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(new { success = false, message = "OrderId is required" });
            }

            try
            {
                var response = await _paymentService.CheckTransactionStatusAsync(orderId);

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transaction status");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while checking status"
                });
            }
        }
    }
}
