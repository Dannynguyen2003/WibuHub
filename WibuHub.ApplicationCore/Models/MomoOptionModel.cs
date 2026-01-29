using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.Models
{
    public class MomoOptionModel
    {
        public string MomoApiUrl { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty; // Callback/IPN
        public string ReturnUrl { get; set; } = string.Empty; // Redirect sau khi thanh toán
    }
}
