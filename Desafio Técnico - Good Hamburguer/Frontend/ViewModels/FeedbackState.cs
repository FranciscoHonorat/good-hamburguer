namespace Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;

public sealed class FeedbackState
{
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

    public void Clear()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void SetError(string message)
    {
        ErrorMessage = NormalizeMessage(message);
        SuccessMessage = null;
    }

    public void SetSuccess(string message)
    {
        SuccessMessage = NormalizeMessage(message);
        ErrorMessage = null;
    }

    private static string NormalizeMessage(string message)
        => string.IsNullOrWhiteSpace(message) ? "Não foi possível concluir a operação." : message.Trim();
}
