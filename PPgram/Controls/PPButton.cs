using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PPgram.Controls;

public enum PPButtonTone
{
    Neutral,
    Accent,
    Danger,
}

public class PPButton : Button
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<PPButton, object?>(nameof(Icon));

    public static readonly StyledProperty<PPButtonTone> ToneProperty =
        AvaloniaProperty.Register<PPButton, PPButtonTone>(nameof(Tone), PPButtonTone.Neutral);

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<PPButton, IBrush?>(nameof(OutlineBrush));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PPButtonTone Tone
    {
        get => GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ToneProperty || change.Property == OutlineBrushProperty)
        {
            UpdateTonePseudoClasses();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateTonePseudoClasses();
    }

    private void UpdateTonePseudoClasses()
    {
        PseudoClasses.Set(":neutral", Tone == PPButtonTone.Neutral);
        PseudoClasses.Set(":accept", Tone == PPButtonTone.Accent);
        PseudoClasses.Set(":danger", Tone == PPButtonTone.Danger);
    }
}
