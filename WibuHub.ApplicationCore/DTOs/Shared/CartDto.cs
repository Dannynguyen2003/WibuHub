namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class CartDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
