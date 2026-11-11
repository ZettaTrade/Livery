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
/// <item><c>Accent</c> — the interactive token: the hover fill of the button
/// and menu hosts (<c>Button:pointerover</c>, <c>MenuItem:pointerover</c>)
/// and the focus stroke of the text hosts (<c>TextBox:focus</c>,
/// <c>ComboBox:focus</c>). It must be visible on its surface pair and its
/// background pair (the WCAG 1.4.11 non-text level, 3.0:1) and its own fill
/// still carries text at 4.5:1 — an accent that fails either leg is a
/// hover/blank state the user never sees (the dark palette shipped at
/// ~1.4:1, the light one ~1.2:1 — the review's finding).</item>
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
/// it broke.
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
    /// The role pair battery: one row per contract duty — the pair's human
    /// name (the gate names it on a break), the foreground role, the
    /// background role it reads against, and the pair's minimum.
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
    /// Walk a theme definition through the contract battery: every role pair
    /// resolves through <see cref="Contrast.Ratio"/> (an unparsable role
    /// value throws there — a corrupt palette must fail loudly, never
    /// silently read as conforming) and the return carries every broken
    /// pair, empty when the palette answers the whole contract.
    /// A missing role throws: the contract cannot read a palette that does
    /// not carry its roles.
    /// </summary>
    public static IReadOnlyList<PaletteContractViolation> Violations(ThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var colors = theme.Colors
            ?? throw new FormatException("the palette carries no colors at all — the role contract cannot read it");

        var violations = new List<PaletteContractViolation>();
        foreach (var (name, foregroundRole, backgroundRole, minimum) in Pairs)
        {
            var foreground = ReadRole(colors, foregroundRole);
            var background = ReadRole(colors, backgroundRole);
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
