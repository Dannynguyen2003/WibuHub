using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.Common.Contants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Customer = "Customer";
            public const string Admin = "Admin";
            public const string SuperAdmin = "SuperAdmin";
            public const string Uploader = "Uploader";
        }

        public static class ReadingServers
        {
            public const int NormalId = 1;
            public const int VipId = 2;

            public const string NormalKey = "normal";
            public const string VipKey = "vip";
        }

        public const string SuperAdminEmail = "danh49003@gmail.com";
    }
}
