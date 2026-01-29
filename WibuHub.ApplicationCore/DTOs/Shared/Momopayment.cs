using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class Momopayment
    {
        // MomoPaymentRequest.cs
        public class MomoPaymentRequest
        {
            public string OrderId { get; set; }
            public string FullName { get; set; }
            public string OrderInfo { get; set; }
            public long Amount { get; set; }
        }

        // MomoPaymentResponse.cs
        public class MomoPaymentResponse
        {
            public string RequestId { get; set; }
            public int ErrorCode { get; set; }
            public string OrderId { get; set; }
            public string Message { get; set; }
            public string LocalMessage { get; set; }
            public string RequestType { get; set; }
            public string PayUrl { get; set; } // URL để redirect user sang MoMo quét mã
            public string Signature { get; set; }
            public string QrCodeUrl { get; set; }
            public string Deeplink { get; set; }
            public string DeeplinkWebInApp { get; set; }
        }

        // MomoCallbackRequest.cs (Dữ liệu MoMo bắn về)
        public class MomoCallbackRequest
        {
            public string PartnerCode { get; set; }
            public string OrderId { get; set; }
            public string RequestId { get; set; }
            public long Amount { get; set; }
            public string OrderInfo { get; set; }
            public string OrderType { get; set; }
            public long TransId { get; set; }
            public int ResultCode { get; set; }
            public string Message { get; set; }
            public string PayType { get; set; }
            public long ResponseTime { get; set; }
            public string ExtraData { get; set; }
            public string Signature { get; set; }
        }
    }
}
