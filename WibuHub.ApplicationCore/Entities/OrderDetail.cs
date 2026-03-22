using System;

    namespace WibuHub.ApplicationCore.Entities
    {
        public class OrderDetail
        {
            // Khóa chính độc lập
            public Guid Id { get; set; } = Guid.NewGuid();

            public Guid OrderId { get; set; }

            // Bắt buộc phải có dấu ? để cho phép Null khi mua VIP
            public Guid? StoryId { get; set; }

            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Amount { get; set; }
            public decimal Discount { get; set; }

            public virtual Order Order { get; set; }
            public virtual Story Story { get; set; }
        }
    }
