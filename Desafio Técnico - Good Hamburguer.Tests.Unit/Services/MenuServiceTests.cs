using Desafio_Técnico___Good_Hamburguer.Services;
using Xunit;

namespace Desafio_Técnico___Good_Hamburguer.Tests.Unit.Services;

public sealed class MenuServiceTests
{
    [Fact]
    public void TryGetByCode_ComCodigoValido_DeveRetornarItem()
    {
        var service = new MenuService();

        var found = service.TryGetByCode("XBURGER", out var item);

        Assert.True(found);
        Assert.NotNull(item);
        Assert.Equal("XBURGER", item!.Code);
    }

    [Fact]
    public void TryGetByCode_ComEspacosECaseDiferente_DeveRetornarItem()
    {
        var service = new MenuService();

        var found = service.TryGetByCode("  xburger  ", out var item);

        Assert.True(found);
        Assert.NotNull(item);
        Assert.Equal("XBURGER", item!.Code);
    }

    [Fact]
    public void TryGetByCode_ComCodigoInvalido_DeveRetornarFalse()
    {
        var service = new MenuService();

        var found = service.TryGetByCode("INVALID", out var item);

        Assert.False(found);
        Assert.Null(item);
    }
}
