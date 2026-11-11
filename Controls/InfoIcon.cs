namespace Livery.Controls;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;

/// <summary>
/// The info affordance (T-154): the circled "i" cue — 16×16, transparent
/// ring, the glyph at the center. The settings surface built this shape
/// out of Border+TextBlock copies before the kit named it; the control is
/// the vocabulary and the accessibility step up: the copies were
/// pointer-only, the control is focusable (the focus ring borrows the
/// palette's on-state accent) and automation-named — the consumer carries
/// its <c>AutomationProperties.Name</c> and its tooltip.
/// </summary>
public sealed class InfoIcon : TemplatedControl
{
}
