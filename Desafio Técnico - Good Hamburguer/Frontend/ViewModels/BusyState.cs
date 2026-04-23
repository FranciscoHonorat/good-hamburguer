namespace Desafio_Técnico___Good_Hamburguer.Frontend.ViewModels;

public sealed class BusyState
{
    public bool IsBusy { get; private set; }

    public bool IsIdle => !IsBusy;

    public void Start() => IsBusy = true;

    public void Stop() => IsBusy = false;

    public async Task ExecuteAsync(Func<Task> action)
    {
        Start();

        try
        {
            await action();
        }
        finally
        {
            Stop();
        }
    }
}
