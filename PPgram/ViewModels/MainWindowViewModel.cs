using PPgram.State;

namespace PPgram.ViewModels;

public partial class MainWindowViewModel(BackgroundState background) : ViewModelBase
{
    private readonly BackgroundState _background = background;

    public string Greeting => "Welcome to Avalonia!";
}
