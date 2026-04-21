namespace Desafio_Técnico___Good_Hamburguer.Data.Entities;

public sealed class OrderItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public OrderEntity? Order { get; set; }
}
