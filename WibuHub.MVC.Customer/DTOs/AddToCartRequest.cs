using System.ComponentModel.DataAnnotations;

namespace WibuHub.MVC.Customer.DTOs
{
    public class AddToCartRequest
    {
        [Range(1, int.MaxValue)]
        public int ChapterId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
