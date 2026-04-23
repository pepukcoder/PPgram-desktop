using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace PPgram.Controls;

public class PPInputField : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PPInputField, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<PPInputField, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<PPInputField, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> SecretProperty =
        AvaloniaProperty.Register<PPInputField, bool>(nameof(Secret), false);

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<PPInputField, bool>(nameof(HasError), false);

    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<PPInputField, char>(nameof(PasswordChar), default(char));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool Secret
    {
        get => GetValue(SecretProperty);
        set => SetValue(SecretProperty, value);
    }

    public bool HasError
    {
        get => GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        private set => SetValue(PasswordCharProperty, value);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty ||
            change.Property == PlaceholderProperty ||
            change.Property == SecretProperty ||
            change.Property == HasErrorProperty)
        {
            UpdateState();
        }
    }

    private void UpdateState()
    {
        PseudoClasses.Set(":has-icon", Icon is not null);
        PseudoClasses.Set(":secret", Secret);
        PseudoClasses.Set(":error", HasError);
        PasswordChar = Secret ? '*' : default(char);
    }
}
