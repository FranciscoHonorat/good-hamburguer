using Desafio_Técnico___Good_Hamburguer.Contracts;
using Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;
using Xunit;

namespace Desafio_Técnico___Good_Hamburguer.Tests.Unit.Frontend;

public sealed class CreateOrderViewModelTests
{
    [Fact]
    public void ToRequest_DeveRetornarCodigosSelecionados()
    {
        var viewModel = new CreateOrderViewModel();
        viewModel.SelectedItemCodes.AddRange(["XBURGER", "SODA"]);

        var request = viewModel.ToRequest();

        Assert.Equal(["XBURGER", "SODA"], request.ItemCodes);
    }

    [Fact]
    public void StartEditing_DeveCarregarIdECodigos()
    {
        var viewModel = new CreateOrderViewModel();
        var order = new OrderResponse
        {
            Id = Guid.NewGuid(),
            Items =
            [
                new MenuItemResponse("XBURGER", "X Burger", "Sanduíche", 5m),
                new MenuItemResponse("SODA", "Refrigerante", "Refrigerante", 2.5m)
            ],
            Subtotal = 7.5m,
            DiscountPercentage = 0.15m,
            DiscountAmount = 1.13m,
            Total = 6.37m,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        viewModel.StartEditing(order);

        Assert.True(viewModel.IsEditing);
        Assert.Equal(order.Id, viewModel.EditingOrderId);
        Assert.Equal(["XBURGER", "SODA"], viewModel.SelectedItemCodes);
    }

    [Fact]
    public void ResetSelection_DeveLimparEdicaoESelecao()
    {
        var viewModel = new CreateOrderViewModel();
        viewModel.SelectedItemCodes.Add("XBURGER");
        viewModel.EditingOrderId = Guid.NewGuid();

        viewModel.ResetSelection();

        Assert.False(viewModel.IsEditing);
        Assert.Empty(viewModel.SelectedItemCodes);
        Assert.Null(viewModel.EditingOrderId);
    }
}
