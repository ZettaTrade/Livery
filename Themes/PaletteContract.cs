namespace Livery.Themes;

/// <summary>
/// The palette role contract (T-151): one documented shape for what every
/// theme palette role is FOR, and the WCAG conformance battery any theme
/// file must answer before it ships. The roles walk the styles the library
/// restyles (the <c>Controls.axaml</c> selectors) and the chips the views
/// paint:
/// <list type="bullet">
/// <item><c>Background</c>/<c>Surface</c>/<c>Border</c> — the planar roles:
/// window fills, control surfaces and their separators/strokes. No
/// contrast duty of their own; every interactive role must read over them.</item>
/// <item><c>Accent</c> — the interactive token: the menu host's hover fill
/// (<c>MenuItem:pointerover</c>), the primary button's solid fill and the
/// focus stroke of the text hosts (<c>TextBox:focus</c>,
/// <c>ComboBox:focus</c>) — the generic button hover left the solid fill
/// for the DERIVED soft tint in T-183 (the derived rows below). It must be
/// visible on its surface pair and its background pair (the WCAG 1.4.11
/// non-text level, 3.0:1) and its own fill still carries text at 4.5:1 —
/// an accent that fails either leg is a hover/blank state the user never
/// sees (the dark palette shipped at ~1.4:1, the light one ~1.2:1 — the
/// review's finding).</item>
/// <item>the derived control keys (<see cref="ButtonKeys"/>) — the generic
/// button family's hover/press fills blended from the palette's own Accent
/// over its Surface, plus the surface-family keys (the resting stroke's
/// hairline and the input wells' raised fill, blended off the Surface
/// toward its luminance pole). The soft hover and the wells are
/// deliberately below the WCAG non-text bar (the approved variant A look —
/// the visibility duty rides the persistent hairline stroke, the tints are
/// the state's color hints), so their battery legs are state-existence
/// floors (a blend measurably off its ground, never a no-op) plus the text
/// legs over the fills and the primary fill's scored foreground.</item>
/// <item><c>Highlight</c> — the on-state token: the checked decorations and
/// the keyboard-focus tints (<c>Button:focus</c>'s border, the menu/tab
/// focus text). Its text-bearing duty is the text-contrast battery's.</item>
/// <item><c>TextPrimary</c> — the reading token: the body text, the text ON
/// interactive fills included.</item>
/// <item><c>Buy</c>/<c>Sell</c> — the status tokens: the chip fills the views
/// paint the trading state with; both must be visible against the surface
/// they sit on (3.0:1, the WCAG 1.4.11 level for non-text UI parts).</item>
/// </list>
/// This shape pins the ROLE pairs, not the arbitrary color pairs — a value
/// that retunes a role answers the battery, and a palette that ships an
/// unusable hover, focus or chip state fails the gate naming the exact pair
/// it broke. The derived rows join the walk when the palette serves the
/// derivation's foundations: their roles resolve through the same merged
/// role table, so a derived pair is named exactly like a raw one.
/// </summary>
public static class PaletteContract
{
    /// <summary>
    /// The WCAG AA minimum for the text-bearing pair: text over an
    /// interactive fill must stay readable at 4.5:1.
    /// </summary>
    public const double TextPairMinimum = 4.5;

    /// <summary>
    /// The WCAG 1.4.11 minimum for the non-text UI pairs: a fill or a stroke
    /// the user must be able to SEE must read at 3.0:1 against its ground.
    /// </summary>
    public const double VisualPairMinimum = 3.0;

    /// <summary>
    /// The soft-tint state floor (T-183): the derived hover tint must read
    /// measurably off its surface ground — a no-op blend (an Accent equal
    /// to the Surface) fails by name. Deliberately below the WCAG non-text
    /// bar: the soft hover is a color hint over the persistent hairline
    /// stroke, not the boundary duty the 3.0 leg guards.
    /// </summary>
    public const double SubtleTintStepMinimum = 1.05;

    /// <summary>
    /// The role pair battery: one row per contract duty — the pair's human
    /// name (the gate names it on a break), the foreground role, the
    /// background role it reads against, and the pair's minimum. The raw
    /// rows read the theme file's own roles; the derived rows (appended in
    /// <see cref="Violations"/>) read the derivation's keys.
    /// </summary>
    private static readonly (string Name, string Foreground, string Background, double Minimum)[] Pairs =
    [
        ("interactive hover fill over surface", "Accent", "Surface", VisualPairMinimum),
        ("text over the interactive hover fill", "TextPrimary", "Accent", TextPairMinimum),
        ("focus stroke over background", "Accent", "Background", VisualPairMinimum),
        ("buy chip fill over surface", "Buy", "Surface", VisualPairMinimum),
        ("sell chip fill over surface", "Sell", "Surface", VisualPairMinimum),
    ];

    /// <summary>
    /// The derived control rows (T-183, the surface-family extension
    /// T-223): the pairs the control family's derived states create,
    /// resolved through the merged role table so the walk stays uniform.
    /// The text legs over both tints and the raised well, the primary
    /// fill's scored foreground, and the state-existence floors (the hover
    /// tint, the raised well and the hairline stroke must each sit
    /// measurably off their surface ground — a no-op blend fails by name;
    /// the hairline stroke carries the resting boundary duty, the tints
    /// are the states' color hints).
    /// The two surface-family floors (the well's and the hairline's) are
    /// the derivation's own guarantee, not the palette's duty: those keys
    /// derive from the Surface alone (a value the theme author tunes
    /// freely), so the derivation's floor walk raises the step until the
    /// pair clears (<see cref="ButtonKeys"/>) — these rows serve as the
    /// regression net over the shipped themes. The tint's floor stays the
    /// palette's duty: the Accent is the theme's own interactive color, so
    /// a no-op hover blend fails by name.
    /// </summary>
    private static readonly (string Name, string Foreground, string Background, double Minimum)[] DerivedPairs =
    [
        ("text over the button hover tint", "TextPrimary", ButtonKeys.HoverTintKey, TextPairMinimum),
        ("text over the button pressed tint", "TextPrimary", ButtonKeys.PressedTintKey, TextPairMinimum),
        ("the primary fill's foreground", ButtonKeys.PrimaryForegroundKey, "Accent", TextPairMinimum),
        ("the button hover tint is a visible step over its surface", ButtonKeys.HoverTintKey, "Surface", SubtleTintStepMinimum),
        ("text over the raised well fill", "TextPrimary", ButtonKeys.RaisedKey, TextPairMinimum),
        ("the raised well is a visible step over its surface", ButtonKeys.RaisedKey, "Surface", SubtleTintStepMinimum),
        ("the control hairline is a visible step over its surface", ButtonKeys.HairlineKey, "Surface", SubtleTintStepMinimum),
    ];

    /// <summary>
    /// Walk a theme definition through the contract battery: every role pair
    /// resolves through <see cref="Contrast.Ratio"/> (an unparsable role
    /// value throws there — a corrupt palette must fail loudly, never
    /// silently read as conforming) and the return carries every broken
    /// pair, empty when the palette answers the whole contract.
    /// A missing role throws: the contract cannot read a palette that does
    /// not carry its roles. The button derivation joins when its
    /// foundations (Accent, Surface) read — their raw rows already throw
    /// for a missing or unparsable value, so a palette that reaches the
    /// derived rows always derives.
    /// </summary>
    public static IReadOnlyList<PaletteContractViolation> Violations(ThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var colors = theme.Colors
            ?? throw new FormatException("the palette carries no colors at all — the role contract cannot read it");

        var roles = new Dictionary<string, string>(colors, StringComparer.Ordinal);
        var pairs = Pairs;
        if (ButtonKeys.TryDerive(colors, out var failure) is { } derived)
        {
            foreach (var (key, color) in derived)
            {
                roles[key] = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }

            pairs = [.. Pairs, .. DerivedPairs];
        }

        var violations = new List<PaletteContractViolation>();
        foreach (var (name, foregroundRole, backgroundRole, minimum) in pairs)
        {
            var foreground = ReadRole(roles, foregroundRole);
            var background = ReadRole(roles, backgroundRole);
            var ratio = Contrast.Ratio(foreground, background);
            if (ratio < minimum)
            {
                violations.Add(new PaletteContractViolation(
                    name, foreground, background, ratio, minimum));
            }
        }

        return violations;
    }

    private static string ReadRole(IReadOnlyDictionary<string, string> colors, string role)
    {
        if (!colors.TryGetValue(role, out var value))
        {
            throw new FormatException(
                $"the palette carries no '{role}' — the role contract cannot read it.");
        }

        return value;
    }
}

/// <summary>One broken role pair: what the palette failed, the literal colors
/// it shipped, and the ratio it reached against its minimum.</summary>
public sealed record PaletteContractViolation(
    string Pair,
    string Foreground,
    string Background,
    double Ratio,
    double Minimum);
