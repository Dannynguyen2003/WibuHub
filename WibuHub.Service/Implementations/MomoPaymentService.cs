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
                    Message = "Failed to deserialize MoMo response"
                };
            }
            catch (Exception ex)
            {
                return new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<(bool isSuccess, string message)> HandleMomoCallbackAsync(MomoCallbackRequest callback)
        {
            try
            {
                // Validate signature if needed (recommended for production)
                // For now, we'll check the result code
                
                if (callback.ResultCode == 0)
                {
                    // Payment successful - update order status in database
                    var order = await _context.Orders.FindAsync(Guid.Parse(callback.OrderId.Replace(_momoSettings.PartnerCode, "")));
                    
                    if (order != null)
                    {
                        order.PaymentMethod = "MoMo";
                        order.TransactionId = callback.TransId.ToString();
                        order.PaymentStatus = "Completed";
                        
                        await _context.SaveChangesAsync();
                        
                        return (true, "Payment processed successfully");
                    }
                    
                    return (false, "Order not found");
                }
                else if (callback.ResultCode == 9000)
                {
                    // Payment authorized
                    return (true, "Payment authorized successfully");
                }
                else
                {
                    // Payment failed
                    return (false, $"Payment failed: {callback.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error processing callback: {ex.Message}");
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
                    Message = "Failed to deserialize MoMo response"
                };
            }
            catch (Exception ex)
            {
                return new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = $"Error: {ex.Message}"
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
