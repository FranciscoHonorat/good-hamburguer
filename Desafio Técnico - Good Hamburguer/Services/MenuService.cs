using Desafio_Técnico___Good_Hamburguer.Models;

namespace Desafio_Técnico___Good_Hamburguer.Services;

public sealed class MenuService
{
    private static readonly IReadOnlyList<MenuItem> Items =
    [
        new("XBURGER", "X Burger", MenuCategory.Sandwich, 5.00m),
        new("XEGG", "X Egg", MenuCategory.Sandwich, 4.50m),
        new("XBACON", "X Bacon", MenuCategory.Sandwich, 7.00m),
        new("FRIES", "Batata frita", MenuCategory.Fries, 2.00m),
        new("SODA", "Refrigerante", MenuCategory.Soda, 2.50m)
    ];

    private static readonly IReadOnlyDictionary<string, MenuItem> ItemsByCode = Items
        .ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<MenuItem> GetAll() => Items;

    public bool Exists(string code)
        => TryGetByCode(code, out _);

    public bool TryGetByCode(string code, out MenuItem? item)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            item = null;
            return false;
        }

        var found = ItemsByCode.TryGetValue(NormalizeCode(code), out var menuItem);
        item = menuItem;
        return found;
    }

    private static string NormalizeCode(string code) => code.Trim();
}
