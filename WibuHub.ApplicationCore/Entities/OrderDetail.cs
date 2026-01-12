using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.Entities
{
    public class OrderDetail
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; }

        public Guid ChapterId { get; set; }
        public virtual Chapter Chapter { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        [Description("Tỷ lệ chiết khấu 10% = 0.1")]
        public double Discount { get; set; }

        public decimal Amount { get; set; }
    }
}
