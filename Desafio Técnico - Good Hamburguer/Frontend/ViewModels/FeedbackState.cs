namespace Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;

public sealed class FeedbackState
{
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public void Clear()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void SetError(string message)
    {
        ErrorMessage = message;
        SuccessMessage = null;
    }

    public void SetSuccess(string message)
    {
        SuccessMessage = message;
        ErrorMessage = null;
    }
}
