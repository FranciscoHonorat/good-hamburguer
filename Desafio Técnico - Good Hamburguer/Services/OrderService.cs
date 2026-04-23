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
        ValidatePagination(page, pageSize);

        var query = CreateOrdersQuery();

        var totalItems = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return CreatePaginatedResponse(entities, page, pageSize, totalItems);
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
        var existingOrder = await GetTrackedOrderOrThrowAsync(id, cancellationToken);

        var items = ValidateAndMapItems(request.ItemCodes);
        var now = DateTimeOffset.UtcNow;
        var rebuilt = BuildOrderEntity(id, items, existingOrder.CreatedAt, now);

        await ReplaceOrderAsync(existingOrder, rebuilt, cancellationToken);

        return MapToOrder(rebuilt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetTrackedOrderOrThrowAsync(id, cancellationToken);

        dbContext.Orders.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ReplaceOrderAsync(OrderEntity existingOrder, OrderEntity rebuilt, CancellationToken cancellationToken)
    {
        dbContext.Orders.Remove(existingOrder);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Orders.Add(rebuilt);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page <= 0)
        {
            throw new ValidationException("O parâmetro 'page' deve ser maior que zero.");
        }

        if (pageSize <= 0 || pageSize > MaxPageSize)
        {
            throw new ValidationException($"O parâmetro 'pageSize' deve estar entre 1 e {MaxPageSize}.");
        }
    }

    private IQueryable<OrderEntity> CreateOrdersQuery()
        => dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderBy(x => x.CreatedAt);

    private static PaginatedResponse<Order> CreatePaginatedResponse(
        IReadOnlyCollection<OrderEntity> entities,
        int page,
        int pageSize,
        int totalItems)
        => new()
        {
            Items = entities.Select(MapToOrder).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };

    private async Task<OrderEntity> GetTrackedOrderOrThrowAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
           ?? throw new NotFoundException("Pedido não encontrado.");

    private List<MenuItem> ValidateAndMapItems(IEnumerable<string>? codes)
    {
        EnsureCodesWereProvided(codes);

        var normalizedCodes = NormalizeCodes(codes!);
        EnsureAtLeastOneCode(normalizedCodes);
        EnsureNoDuplicateCodes(normalizedCodes);

        var mappedItems = MapCodesToItems(normalizedCodes);
        EnsureSingleItemPerCategory(mappedItems);

        return mappedItems;
    }

    private static void EnsureCodesWereProvided(IEnumerable<string>? codes)
    {
        if (codes is null)
        {
            throw new ValidationException("Pedido inválido. Informe os itens do pedido.");
        }
    }

    private static List<string> NormalizeCodes(IEnumerable<string> codes)
        => codes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();

    private static void EnsureAtLeastOneCode(List<string> normalizedCodes)
    {
        if (normalizedCodes.Count == 0)
        {
            throw new ValidationException("Pedido inválido. Informe ao menos um item.");
        }
    }

    private static void EnsureNoDuplicateCodes(List<string> normalizedCodes)
    {
        var duplicatesByCode = normalizedCodes
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicatesByCode.Count > 0)
        {
            throw new ValidationException($"Itens duplicados não são permitidos: {string.Join(", ", duplicatesByCode)}.");
        }
    }

    private List<MenuItem> MapCodesToItems(List<string> normalizedCodes)
    {
        return normalizedCodes
            .Select(MapCodeToItem)
            .ToList();
    }

    private MenuItem MapCodeToItem(string code)
    {
        if (!menuService.TryGetByCode(code, out var item) || item is null)
        {
            throw new ValidationException($"Item inválido no pedido: '{code}'.");
        }

        return item;
    }

    private static void EnsureSingleItemPerCategory(List<MenuItem> mappedItems)
    {
        var duplicateCategories = mappedItems
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateCategories.Count > 0)
        {
            throw new ValidationException("Cada pedido pode conter apenas um sanduíche, uma batata e um refrigerante.");
        }
    }

    private static OrderEntity BuildOrderEntity(Guid id, List<MenuItem> items, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        var (subtotal, discountPercentage, discountAmount, total) = CalculateTotals(items);

        return new OrderEntity
        {
            Id = id,
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            Total = total,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Items = CreateOrderItems(id, items)
        };
    }

    private static (decimal subtotal, decimal discountPercentage, decimal discountAmount, decimal total) CalculateTotals(List<MenuItem> items)
    {
        var subtotal = items.Sum(x => x.Price);
        var discountPercentage = ResolveDiscountPercentage(items);
        var discountAmount = RoundMoney(subtotal * discountPercentage);
        var total = RoundMoney(subtotal - discountAmount);

        return (subtotal, discountPercentage, discountAmount, total);
    }

    private static decimal RoundMoney(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal ResolveDiscountPercentage(List<MenuItem> items)
    {
        var hasSandwich = HasCategory(items, MenuCategory.Sandwich);
        var hasFries = HasCategory(items, MenuCategory.Fries);
        var hasSoda = HasCategory(items, MenuCategory.Soda);

        return hasSandwich switch
        {
            true when hasFries && hasSoda => 0.20m,
            true when hasSoda => 0.15m,
            true when hasFries => 0.10m,
            _ => 0m
        };
    }

    private static bool HasCategory(List<MenuItem> items, string category)
        => items.Any(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));

    private static List<OrderItemEntity> CreateOrderItems(Guid orderId, List<MenuItem> items)
        => items.Select(x => new OrderItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Code = x.Code,
            Name = x.Name,
            Category = x.Category,
            Price = x.Price
        }).ToList();

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
