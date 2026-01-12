using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.Entities
{
    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid();
            OrderDetails = new Collection<OrderDetail>();
        }

        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string? Note { get; set; }

        [Description("Tổng giá trị của đơn hàng CHƯA bao gồm thuế")]
        public decimal Amount { get; set; }
        [Description("Tiền thuế")]
        public decimal Tax { get; set; }
        [Description("Tổng giá trị của đơn hàng ĐÃ bao gồm thuế")]
        public decimal TotalAmount { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
