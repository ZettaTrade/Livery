namespace Livery.Controls;

using Avalonia;
using Avalonia.Controls;

/// <summary>
/// The window-level chrome knobs the kit's templates read: attached
/// properties a host window sets and the chrome template's parts bind from
/// their templated parent. Brand-neutral by contract: the kit renders
/// whatever the app passes and never ships a mark of its own.
/// </summary>
public static class WindowChrome
{
    /// <summary>Identifies the <see cref="Logo"/> attached property.</summary>
    public static readonly AttachedProperty<object?> LogoProperty =
        AvaloniaProperty.RegisterAttached<Window, object?>("Logo", typeof(WindowChrome));

    /// <summary>
    /// Gets the logomark the chrome strip shows at its left edge. Null (the
    /// default) hides the slot — the strip stays exactly the drag surface it
    /// always was.
    /// </summary>
    /// <param name="window">The host window the mark rides.</param>
    public static object? GetLogo(Window window) => window.GetValue(LogoProperty);

    /// <summary>
    /// Sets the strip's logomark on the host window: any control the app
    /// passes rides the slot (the template presents it hit-invisible at the
    /// strip's left edge, before the title).
    /// </summary>
    /// <param name="window">The host window the mark rides.</param>
    /// <param name="value">The mark (any control), or null to hide the slot.</param>
    public static void SetLogo(Window window, object? value) => window.SetValue(LogoProperty, value);
}
