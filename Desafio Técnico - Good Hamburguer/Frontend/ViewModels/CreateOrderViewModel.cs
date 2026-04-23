using Desafio_Técnico___Good_Hamburguer.Contracts;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;

public sealed class CreateOrderViewModel
{
    public FeedbackState Feedback { get; } = new();
    public BusyState SubmitState { get; } = new();
    public List<string> SelectedItemCodes { get; } = [];
    public Guid? EditingOrderId { get; set; }

    public bool IsEditing => EditingOrderId is not null;

    public UpsertOrderRequest ToRequest()
        => new() { ItemCodes = SelectedItemCodes.ToList() };

    public void StartEditing(OrderResponse order)
    {
        EditingOrderId = order.Id;
        SelectedItemCodes.Clear();

        foreach (var code in order.Items.Select(x => x.Code))
        {
            SelectedItemCodes.Add(code);
        }
    }

    public void ResetSelection()
    {
        SelectedItemCodes.Clear();
        EditingOrderId = null;
    }
}
