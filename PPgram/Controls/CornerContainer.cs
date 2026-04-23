using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PPgram.Controls;

public class CornerContainer : ContentControl
{
    public static readonly StyledProperty<double> CornerWidthProperty =
        AvaloniaProperty.Register<CornerContainer, double>(nameof(CornerWidth), 24d);

    public static readonly StyledProperty<double> CornerHeightProperty =
        AvaloniaProperty.Register<CornerContainer, double>(nameof(CornerHeight), 24d);

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<CornerContainer, IBrush?>(nameof(OutlineBrush));
    public static readonly StyledProperty<IBrush?> CornerColorProperty =
        AvaloniaProperty.Register<CornerContainer, IBrush?>(nameof(CornerColor), Brushes.White);

    public double CornerWidth
    {
        get => GetValue(CornerWidthProperty);
        set => SetValue(CornerWidthProperty, value);
    }

    public double CornerHeight
    {
        get => GetValue(CornerHeightProperty);
        set => SetValue(CornerHeightProperty, value);
    }

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }
    public IBrush? CornerColor
    {
        get => GetValue(CornerColorProperty);
        set => SetValue(CornerColorProperty, value);
    }
}
