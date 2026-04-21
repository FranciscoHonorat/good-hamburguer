namespace Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;

public sealed class CreateOrderViewModel
{
    public FeedbackState Feedback { get; } = new();
    public BusyState SubmitState { get; } = new();
    public List<string> SelectedItemCodes { get; } = [];
    public Guid? EditingOrderId { get; set; }
}
