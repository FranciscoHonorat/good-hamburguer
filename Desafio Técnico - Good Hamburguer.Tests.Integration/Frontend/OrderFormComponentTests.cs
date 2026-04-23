using Bunit;
using Desafio_Técnico___Good_Hamburguer.Components.Orders;
using Desafio_Técnico___Good_Hamburguer.Contracts;
using Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Desafio_Técnico___Good_Hamburguer.Tests.Integration.Frontend;

public sealed class OrderFormComponentTests : TestContext
{
    [Fact]
    public void Render_ComNenhumItemSelecionado_DeveDesabilitarBotaoPrincipal()
    {
        var viewModel = new CreateOrderViewModel();

        var component = RenderComponent<OrderForm>(parameters => parameters
            .Add(p => p.ViewModel, viewModel)
            .Add(p => p.MenuItems, BuildMenu())
            .Add(p => p.IsBusy, false)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { })));

        var submitButton = component.Find("button.btn-primary");

        Assert.True(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public void ToggleItem_DeveHabilitarBotaoPrincipal()
    {
        var viewModel = new CreateOrderViewModel();

        var component = RenderComponent<OrderForm>(parameters => parameters
            .Add(p => p.ViewModel, viewModel)
            .Add(p => p.MenuItems, BuildMenu())
            .Add(p => p.IsBusy, false)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { })));

        component.Find("#item-XBURGER").Change(true);

        var submitButton = component.Find("button.btn-primary");

        Assert.False(submitButton.HasAttribute("disabled"));
        Assert.Contains("XBURGER", viewModel.SelectedItemCodes);
    }

    [Fact]
    public void Limpar_DeveResetarEstadoDeEdicao()
    {
        var viewModel = new CreateOrderViewModel();
        viewModel.StartEditing(new OrderResponse
        {
            Id = Guid.NewGuid(),
            Items = [new MenuItemResponse("XBURGER", "X Burger", "Sanduíche", 5m)],
            Subtotal = 5m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            Total = 5m,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        var component = RenderComponent<OrderForm>(parameters => parameters
            .Add(p => p.ViewModel, viewModel)
            .Add(p => p.MenuItems, BuildMenu())
            .Add(p => p.IsBusy, false)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { })));

        component.Find("button.btn-outline-secondary").Click();

        Assert.False(viewModel.IsEditing);
        Assert.Empty(viewModel.SelectedItemCodes);
    }

    private static IReadOnlyList<MenuItemResponse> BuildMenu()
        =>
        [
            new("XBURGER", "X Burger", "Sanduíche", 5m),
            new("SODA", "Refrigerante", "Refrigerante", 2.5m)
        ];
}
