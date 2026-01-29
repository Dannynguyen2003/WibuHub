namespace WibuHub.ApplicationCore.Configuration
{
    public class MomoSettings
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PartnerCode { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = "payWithMethod";
        public string ApiEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
        public string QueryEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/query";
        public string Lang { get; set; } = "vi";
        public bool AutoCapture { get; set; } = true;
    }
}
