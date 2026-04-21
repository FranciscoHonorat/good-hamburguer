namespace Desafio_Técnico___Good_Hamburguer.Contracts;

public sealed class UpsertOrderRequest
{
    public List<string> ItemCodes { get; set; } = [];
}
