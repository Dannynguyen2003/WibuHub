using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.Service.Implementation;
using WibuHub.Service.Interface;
using static WibuHub.ApplicationCore.DTOs.Shared.Momopayment;

namespace WibuHub.Service.Implementations.PayMent
{
    public class MomoPaymentService : IPaymentService
    {
        private readonly MomoSettings _momoSettings;
        private readonly StoryDbContext _context;
        private readonly StoryIdentityDbContext _identityContext;
        private readonly IRewardService _rewardService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MomoPaymentService> _logger;

        public MomoPaymentService(IOptions<MomoSettings> momoSettings, StoryDbContext context, StoryIdentityDbContext identityContext, IRewardService rewardService, HttpClient httpClient, ILogger<MomoPaymentService> logger)
        {
            _momoSettings = momoSettings.Value;
            _context = context;
            _identityContext = identityContext;
            _rewardService = rewardService;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MomoPaymentResponse> CreateMomoPaymentAsync(MomoPaymentRequest request)
        {
            // Generate orderId if not provided
            var orderId = request.OrderId ?? $"{_momoSettings.PartnerCode}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var requestId = orderId;
            var amount = ((long)request.Amount).ToString();
            var extraData = string.Empty;

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
                    ErrorCode = -1,
                    Message = "Failed to process payment request"
                };
            }
            catch
            {
                // Log the error internally, return generic message to client
                return new MomoPaymentResponse
                {
                    ErrorCode = -1,
                    Message = "Unable to connect to payment service"
                };
            }
        }

        public async Task<(bool isSuccess, string message)> HandleMomoCallbackAsync(MomoCallbackRequest callback)
        {
            Console.WriteLine("Received MoMo callback:", callback);
            try
            {


                string rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                                      $"&amount={callback.Amount}" +
                                      $"&extraData={callback.ExtraData ?? ""}" + // Handle null ExtraData
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


                string expectedSignature = ComputeHmacSha256(rawSignature, _momoSettings.SecretKey);


                if (callback.Signature != expectedSignature)
                {

                    return (false, "Invalid signature");
                }


                if (callback.ResultCode == 0)
                {
                    var orderIdStr = callback.OrderId;


                    if (orderIdStr.StartsWith(_momoSettings.PartnerCode))
                    {
                        orderIdStr = orderIdStr.Substring(_momoSettings.PartnerCode.Length);
                    }

                    if (Guid.TryParse(orderIdStr, out var orderId))
                    {
                        var order = await _context.Orders
                            .Include(o => o.OrderDetails)
                            .FirstOrDefaultAsync(o => o.Id == orderId);

                        if (order != null)
                        {
                            StoryUser? user = null;
                            if (!string.IsNullOrWhiteSpace(order.UserId))
                            {
                                user = await FindUserAsync(order.UserId);
                                if (user == null)
                                {
                                    return (false, $"User not found for order {order.Id}");
                                }

                                _logger.LogInformation(
                                    "MoMo callback user resolved for OrderId: {OrderId}. OrderUserId: {OrderUserId}, ResolvedUserId: {ResolvedUserId}, UserName: {UserName}",
                                    order.Id,
                                    order.UserId,
                                    user.Id,
                                    user.UserName);
                            }

                            // Avoid processing the same paid order multiple times.
                            if (order.PaymentStatus == "Completed")
                            {
                                return (true, "Payment already processed");
                            }

                            if (user != null)
                            {
                                var totalVipDaysBought = CalculateVipDaysFromOrderDetails(order.OrderDetails);
                                if (totalVipDaysBought > 0)
                                {
                                    var currentVipEnd = user.VipExpireDate.HasValue && user.VipExpireDate.Value > DateTime.UtcNow
                                        ? user.VipExpireDate.Value
                                        : DateTime.UtcNow;

                                    user.VipExpireDate = currentVipEnd.AddDays(totalVipDaysBought);
                                }

                                _logger.LogInformation(
                                    "MoMo callback benefit calculation for OrderId: {OrderId}. VipDays: {VipDays}, TotalAmount: {TotalAmount}",
                                    order.Id,
                                    totalVipDaysBought,
                                    order.TotalAmount);

                                var rewardGranted = await GrantOrderRewardsAsync(user, order.TotalAmount);
                                if (!rewardGranted)
                                {
                                    return (false, $"Failed to grant rewards for order {order.Id}");
                                }
                            }

                            // Mark payment completed only after account benefits are handled.
                            order.PaymentMethod = "MoMo";
                            order.TransactionId = callback.TransId.ToString();
                            order.PaymentStatus = "Completed";

                            await _identityContext.SaveChangesAsync();
                            await _context.SaveChangesAsync();

                            return (true, "Payment processed successfully");
                        }
                    }
                    return (false, "Order not found");
                }
                else
                {
                    return (false, $"Payment failed with ResultCode: {callback.ResultCode}");
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
                    ErrorCode = -1,
                    Message = "Failed to process status request"
                };
            }
            catch
            {
                // Log the error internally, return generic message to client
                return new MomoPaymentResponse
                {
                    ErrorCode = -1,
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

        private static int CalculateVipDaysFromOrderDetails(IEnumerable<OrderDetail> orderDetails)
        {
            var totalVipDaysBought = 0;

            foreach (var detail in orderDetails)
            {
                if (detail.StoryId != null && detail.StoryId != Guid.Empty)
                {
                    continue;
                }

                var package = VipPackages.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(detail.ItemName)
                    && string.Equals(p.Name, detail.ItemName, StringComparison.OrdinalIgnoreCase));

                if (package == null)
                {
                    package = VipPackages.FirstOrDefault(p => p.Price == detail.UnitPrice);
                }

                if (package != null)
                {
                    totalVipDaysBought += package.DurationDays * Math.Max(1, detail.Quantity);
                }
            }

            return totalVipDaysBought;
        }

        private async Task<StoryUser?> FindUserAsync(string orderUserId)
        {
            if (string.IsNullOrWhiteSpace(orderUserId))
            {
                return null;
            }

            orderUserId = orderUserId.Trim();
            var normalized = orderUserId.ToUpperInvariant();

            if (Guid.TryParse(orderUserId, out _))
            {
                var byId = await _identityContext.Users.FirstOrDefaultAsync(u => u.Id == orderUserId);
                if (byId != null)
                {
                    return byId;
                }
            }

            return await _identityContext.Users.FirstOrDefaultAsync(u =>
                u.UserName == orderUserId
                || u.NormalizedUserName == normalized
                || u.Email == orderUserId
                || u.NormalizedEmail == normalized);
        }

        private sealed record VipPackageDefinition(string Name, decimal Price, int DurationDays);

        private static readonly IReadOnlyList<VipPackageDefinition> VipPackages = new List<VipPackageDefinition>
        {
            new("1 Month VIP", 99000m, 30),
            new("3 Months VIP", 249000m, 90),
            new("12 Months VIP", 799000m, 365),
            new("VIP 1 Month", 99000m, 30),
            new("VIP 3 Months", 249000m, 90),
            new("VIP 12 Months", 799000m, 365)
        };

        private async Task<bool> GrantOrderRewardsAsync(StoryUser user, decimal totalAmount)
        {
            if (totalAmount <= 0)
            {
                return true;
            }

            var expAdded = Math.Max(1, (int)Math.Floor(totalAmount / 10000m));
            var pointsAdded = Math.Max(3, (int)Math.Floor(totalAmount / 20000m));

            return await _rewardService.AddExpAndPointsAsync(user.Id, expAdded, pointsAdded);
        }
    }
}






