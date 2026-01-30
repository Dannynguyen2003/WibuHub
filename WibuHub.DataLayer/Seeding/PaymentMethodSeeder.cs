using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.DataLayer.Seeding
{
    public static class PaymentMethodSeeder
    {
        public static void SeedPaymentMethods(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod
                {
                    Id = 1,
                    Name = "Ví MoMo",
                    Code = "MOMO",
                    IsActive = true,
                    LogoUrl = "https://developers.momo.vn/v3/img/logo.svg",
                    DisplayOrder = 1,
                    Description = "Thanh toán qua ví điện tử MoMo - Nhanh chóng và an toàn"
                },
                new PaymentMethod
                {
                    Id = 2,
                    Name = "VNPay",
                    Code = "VNPAY",
                    IsActive = false,
                    LogoUrl = "https://vnpay.vn/assets/images/logo-primary.svg",
                    DisplayOrder = 2,
                    Description = "Thanh toán qua cổng VNPay"
                },
                new PaymentMethod
                {
                    Id = 3,
                    Name = "Chuyển khoản ngân hàng",
                    Code = "BANK",
                    IsActive = true,
                    LogoUrl = null,
                    DisplayOrder = 3,
                    Description = "Chuyển khoản trực tiếp qua ngân hàng"
                },
                new PaymentMethod
                {
                    Id = 4,
                    Name = "ZaloPay",
                    Code = "ZALOPAY",
                    IsActive = false,
                    LogoUrl = null,
                    DisplayOrder = 4,
                    Description = "Thanh toán qua ví ZaloPay"
                },
                new PaymentMethod
                {
                    Id = 5,
                    Name = "Thẻ Visa/MasterCard",
                    Code = "VISA",
                    IsActive = false,
                    LogoUrl = null,
                    DisplayOrder = 5,
                    Description = "Thanh toán bằng thẻ tín dụng quốc tế"
                }
            );
        }
    }
}
