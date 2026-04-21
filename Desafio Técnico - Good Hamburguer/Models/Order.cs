namespace Desafio_Técnico___Good_Hamburguer.Models;

public sealed class Order
{
    public Guid Id { get; init; }
    public List<MenuItem> Items { get; init; } = [];
    public decimal Subtotal { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
