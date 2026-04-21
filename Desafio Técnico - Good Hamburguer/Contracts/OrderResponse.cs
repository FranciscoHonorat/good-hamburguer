namespace Desafio_Técnico___Good_Hamburguer.Contracts;

public sealed class OrderResponse
{
    public required Guid Id { get; init; }
    public required IReadOnlyList<MenuItemResponse> Items { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal DiscountPercentage { get; init; }
    public required decimal DiscountAmount { get; init; }
    public required decimal Total { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
