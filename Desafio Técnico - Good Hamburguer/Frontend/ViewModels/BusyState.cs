namespace Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;

public sealed class BusyState
{
    public bool IsBusy { get; private set; }

    public void Start() => IsBusy = true;

    public void Stop() => IsBusy = false;
}
