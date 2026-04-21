namespace Desafio_Técnico___Good_Hamburguer.Data.Entities;

public sealed class OrderEntity
{
    public Guid Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<OrderItemEntity> Items { get; set; } = [];
}
