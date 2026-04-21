using Desafio_Técnico___Good_Hamburguer.Contracts;
using Desafio_Técnico___Good_Hamburguer.Data;
using Desafio_Técnico___Good_Hamburguer.Exceptions;
using Desafio_Técnico___Good_Hamburguer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Desafio_Técnico___Good_Hamburguer.Tests.Unit.Services;

public sealed class OrderServiceTests
{
    private static OrderService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        return new OrderService(new MenuService(), dbContext);
    }

    [Fact]
    public async Task Create_WhenOrderHasSandwichFriesAndSoda_ShouldApplyTwentyPercentDiscount()
    {
        var orderService = CreateService();
        var request = new UpsertOrderRequest
        {
            ItemCodes = ["XBURGER", "FRIES", "SODA"]
        };

        var order = await orderService.CreateAsync(request);

        Assert.Equal(9.50m, order.Subtotal);
        Assert.Equal(0.20m, order.DiscountPercentage);
        Assert.Equal(1.90m, order.DiscountAmount);
        Assert.Equal(7.60m, order.Total);
    }

    [Fact]
    public async Task Create_WhenOrderHasDuplicateItemCode_ShouldThrowValidationException()
    {
        var orderService = CreateService();
        var request = new UpsertOrderRequest
        {
            ItemCodes = ["SODA", "SODA"]
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => orderService.CreateAsync(request));

        Assert.Contains("duplicados", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Update_WhenOrderDoesNotExist_ShouldThrowNotFoundException()
    {
        var orderService = CreateService();
        var request = new UpsertOrderRequest
        {
            ItemCodes = ["XBURGER"]
        };

        await Assert.ThrowsAsync<NotFoundException>(() => orderService.UpdateAsync(Guid.NewGuid(), request));
    }
}
