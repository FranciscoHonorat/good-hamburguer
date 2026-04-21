using Desafio_Técnico___Good_Hamburguer.Data;
using Desafio_Técnico___Good_Hamburguer.Data.Entities;
using Desafio_Técnico___Good_Hamburguer.Contracts;
using Desafio_Técnico___Good_Hamburguer.Exceptions;
using Desafio_Técnico___Good_Hamburguer.Models;
using Microsoft.EntityFrameworkCore;

namespace Desafio_Técnico___Good_Hamburguer.Services;

public sealed class OrderService(MenuService menuService, AppDbContext dbContext)
{
    private const int MaxPageSize = 100;

    public async Task<PaginatedResponse<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            throw new ValidationException("O parâmetro 'page' deve ser maior que zero.");
        }

        if (pageSize <= 0 || pageSize > MaxPageSize)
        {
            throw new ValidationException($"O parâmetro 'pageSize' deve estar entre 1 e {MaxPageSize}.");
        }

        var query = dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderBy(x => x.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Order>
        {
            Items = entities.Select(MapToOrder).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => (await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? MapToOrder(entity)
            : null;

    public async Task<Order> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default) =>
        await GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Pedido não encontrado.");

    public async Task<Order> CreateAsync(UpsertOrderRequest request, CancellationToken cancellationToken = default)
    {
        var items = ValidateAndMapItems(request.ItemCodes);

        var now = DateTimeOffset.UtcNow;
        var entity = BuildOrderEntity(Guid.NewGuid(), items, now, now);
        dbContext.Orders.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToOrder(entity);
    }

    public async Task<Order> UpdateAsync(Guid id, UpsertOrderRequest request, CancellationToken cancellationToken = default)
    {
        var existingOrder = await dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existingOrder is null)
        {
            throw new NotFoundException("Pedido não encontrado.");
        }

        var items = ValidateAndMapItems(request.ItemCodes);
        var now = DateTimeOffset.UtcNow;
        var rebuilt = BuildOrderEntity(id, items, existingOrder.CreatedAt, now);
        dbContext.Orders.Remove(existingOrder);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Orders.Add(rebuilt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToOrder(rebuilt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException("Pedido não encontrado.");
        }

        dbContext.Orders.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private List<MenuItem> ValidateAndMapItems(IEnumerable<string>? codes)
    {
        if (codes is null)
        {
            throw new ValidationException("Pedido inválido. Informe os itens do pedido.");
        }

        var normalizedCodes = codes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();

        if (normalizedCodes.Count == 0)
        {
            throw new ValidationException("Pedido inválido. Informe ao menos um item.");
        }

        var duplicatesByCode = normalizedCodes
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicatesByCode.Count > 0)
        {
            throw new ValidationException($"Itens duplicados não são permitidos: {string.Join(", ", duplicatesByCode)}.");
        }

        var mappedItems = new List<MenuItem>(normalizedCodes.Count);

        foreach (var code in normalizedCodes)
        {
            if (!menuService.TryGetByCode(code, out var item) || item is null)
            {
                throw new ValidationException($"Item inválido no pedido: '{code}'.");
            }

            mappedItems.Add(item);
        }

        var duplicateCategories = mappedItems
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateCategories.Count > 0)
        {
            throw new ValidationException("Cada pedido pode conter apenas um sanduíche, uma batata e um refrigerante.");
        }

        return mappedItems;
    }

    private static OrderEntity BuildOrderEntity(Guid id, List<MenuItem> items, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        var subtotal = items.Sum(x => x.Price);

        var hasSandwich = items.Any(x => string.Equals(x.Category, MenuCategory.Sandwich, StringComparison.OrdinalIgnoreCase));
        var hasFries = items.Any(x => string.Equals(x.Category, MenuCategory.Fries, StringComparison.OrdinalIgnoreCase));
        var hasSoda = items.Any(x => string.Equals(x.Category, MenuCategory.Soda, StringComparison.OrdinalIgnoreCase));

        var discountPercentage = hasSandwich switch
        {
            true when hasFries && hasSoda => 0.20m,
            true when hasSoda => 0.15m,
            true when hasFries => 0.10m,
            _ => 0m
        };

        var discountAmount = Math.Round(subtotal * discountPercentage, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(subtotal - discountAmount, 2, MidpointRounding.AwayFromZero);

        return new OrderEntity
        {
            Id = id,
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            Total = total,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Items = items.Select(x => new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = id,
                Code = x.Code,
                Name = x.Name,
                Category = x.Category,
                Price = x.Price
            }).ToList()
        };
    }

    private static Order MapToOrder(OrderEntity entity)
        => new()
        {
            Id = entity.Id,
            Items = entity.Items.Select(x => new MenuItem(x.Code, x.Name, x.Category, x.Price)).ToList(),
            Subtotal = entity.Subtotal,
            DiscountPercentage = entity.DiscountPercentage,
            DiscountAmount = entity.DiscountAmount,
            Total = entity.Total,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
}
