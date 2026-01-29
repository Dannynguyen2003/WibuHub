using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class MomoPaymentService : IPaymentService
    {
        private readonly MomoSettings _momoSettings;
        private readonly StoryDbContext _context;
        private readonly HttpClient _httpClient;

        public MomoPaymentService(IOptions<MomoSettings> momoSettings, StoryDbContext context, HttpClient httpClient)
        {
            _momoSettings = momoSettings.Value;
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<MomoPaymentResponse> CreateMomoPaymentAsync(MomoPaymentRequest request)
        {
            // Generate orderId if not provided
            var orderId = request.OrderId ?? $"{_momoSettings.PartnerCode}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var requestId = orderId;
            var amount = ((long)request.Amount).ToString();
            var extraData = request.ExtraData ?? string.Empty;

            // Create raw signature string
            var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                             $"&amount={amount}" +
                             $"&extraData={extraData}" +
                             $"&ipnUrl={_momoSettings.IpnUrl}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo={request.OrderInfo}" +
                             $"&partnerCode={_momoSettings.PartnerCode}" +
                             $"&redirectUrl={_momoSettings.RedirectUrl}" +
                             $"&requestId={requestId}" +
                             $"&requestType={_momoSettings.RequestType}";

            // Generate signature using HMAC SHA256
            var signature = ComputeHmacSha256(rawSignature, _momoSettings.SecretKey);

            // Create request body
            var requestBody = new
            {
                partnerCode = _momoSettings.PartnerCode,
                partnerName = "WibuHub",
                storeId = "WibuHubStore",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = request.OrderInfo,
                redirectUrl = _momoSettings.RedirectUrl,
                ipnUrl = _momoSettings.IpnUrl,
                lang = _momoSettings.Lang,
                requestType = _momoSettings.RequestType,
                autoCapture = _momoSettings.AutoCapture,
                extraData = extraData,
                orderGroupId = string.Empty,
                signature = signature
            };

            // Send request to MoMo
            try
            {
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_momoSettings.ApiEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return momoResponse ?? new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = "Failed to process payment request"
                };
            }
            catch
            {
                // Log the error internally, return generic message to client
                return new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = "Unable to connect to payment service"
                };
            }
        }

        public async Task<(bool isSuccess, string message)> HandleMomoCallbackAsync(MomoCallbackRequest callback)
        {
            try
            {
                // Validate signature for security
                var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                                 $"&amount={callback.Amount}" +
                                 $"&extraData={callback.ExtraData}" +
                                 $"&message={callback.Message}" +
                                 $"&orderId={callback.OrderId}" +
                                 $"&orderInfo={callback.OrderInfo}" +
                                 $"&orderType={callback.OrderType}" +
                                 $"&partnerCode={callback.PartnerCode}" +
                                 $"&payType={callback.PayType}" +
                                 $"&requestId={callback.RequestId}" +
                                 $"&responseTime={callback.ResponseTime}" +
                                 $"&resultCode={callback.ResultCode}" +
                                 $"&transId={callback.TransId}";

                var expectedSignature = ComputeHmacSha256(rawSignature, _momoSettings.SecretKey);

                if (callback.Signature != expectedSignature)
                {
                    return (false, "Invalid signature");
                }

                if (callback.ResultCode == 0)
                {
                    // Payment successful - update order status in database
                    // Extract the actual order ID (remove partner code prefix if present)
                    var orderIdStr = callback.OrderId;
                    if (orderIdStr.StartsWith(_momoSettings.PartnerCode))
                    {
                        orderIdStr = orderIdStr.Substring(_momoSettings.PartnerCode.Length);
                    }

                    // Try to parse as GUID, if it fails, just log the transaction
                    if (Guid.TryParse(orderIdStr, out var orderId))
                    {
                        var order = await _context.Orders.FindAsync(orderId);
                        
                        if (order != null)
                        {
                            // Check if payment is already completed to prevent duplicate processing
                            if (order.PaymentStatus == "Completed")
                            {
                                return (true, "Payment already processed");
                            }

                            order.PaymentMethod = "MoMo";
                            order.TransactionId = callback.TransId.ToString();
                            order.PaymentStatus = "Completed";
                            
                            await _context.SaveChangesAsync();
                            
                            return (true, "Payment processed successfully");
                        }
                    }
                    
                    // Order not found, but payment was successful
                    return (true, "Payment successful but order not found in system");
                }
                else if (callback.ResultCode == 9000)
                {
                    // Payment authorized
                    return (true, "Payment authorized successfully");
                }
                else
                {
                    // Payment failed
                    return (false, "Payment failed");
                }
            }
            catch
            {
                // Log the error internally but return generic message
                return (false, "Error processing callback");
            }
        }

        public async Task<MomoPaymentResponse> CheckTransactionStatusAsync(string orderId)
        {
            var requestId = orderId;
            
            // Create raw signature for status check
            var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                             $"&orderId={orderId}" +
                             $"&partnerCode={_momoSettings.PartnerCode}" +
                             $"&requestId={requestId}";

            var signature = ComputeHmacSha256(rawSignature, _momoSettings.SecretKey);

            // Create request body
            var requestBody = new
            {
                partnerCode = _momoSettings.PartnerCode,
                requestId = requestId,
                orderId = orderId,
                signature = signature,
                lang = _momoSettings.Lang
            };

            try
            {
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_momoSettings.QueryEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return momoResponse ?? new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = "Failed to process status request"
                };
            }
            catch
            {
                // Log the error internally, return generic message to client
                return new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = "Unable to check transaction status"
                };
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
