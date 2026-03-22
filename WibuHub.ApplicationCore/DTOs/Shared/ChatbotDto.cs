using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class ChatbotRequest
    {
        public string Message { get; set; }
    }

    public class ChatbotResponse
    {
        public string Reply { get; set; }
    }
}
