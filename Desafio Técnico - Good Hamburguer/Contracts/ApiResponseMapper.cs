using Desafio_Técnico___Good_Hamburguer.Models;

namespace Desafio_Técnico___Good_Hamburguer.Contracts;

public static class ApiResponseMapper
{
    public static MenuItemResponse ToResponse(this MenuItem item)
        => new(item.Code, item.Name, item.Category, item.Price);

    public static OrderResponse ToResponse(this Order order)
        => new()
        {
            Id = order.Id,
            Items = order.Items.Select(ToResponse).ToList(),
            Subtotal = order.Subtotal,
            DiscountPercentage = order.DiscountPercentage,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };

    public static PaginatedResponse<OrderResponse> ToResponse(this PaginatedResponse<Order> paginated)
        => new()
        {
            Items = paginated.Items.Select(ToResponse).ToList(),
            Page = paginated.Page,
            PageSize = paginated.PageSize,
            TotalItems = paginated.TotalItems,
            TotalPages = paginated.TotalPages
        };
}
