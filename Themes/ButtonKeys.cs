namespace Livery.Themes;

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

/// <summary>
/// The button vocabulary's derived keys (T-183): the soft-tint hover and
/// press fills and the primary fill's readable foreground, derived from the
/// applied palette — the T-150 publishes-as-brush canon, the T-152
/// scored-foreground shape. The resting button is transparent over its
/// hairline stroke; hover fills with the Accent blended one soft step over
/// the theme's own Surface (12%), pressed a stronger step (22%) — the
/// approved variant A shape: the pointer answers with a tint, never the
/// solid Accent flash. The <c>.primary</c> class fills solid Accent and
/// reads through a scored achromatic foreground (black vs white, the
/// better pair wins — against any single fill at least one candidate
/// clears the WCAG 4.5:1 bar, the same guarantee the chip foreground's
/// scoring rides). The derivation is intentionally bounded and
/// formula-pure: the WCAG quality of the shipped palette values is the
/// theme contract's own lane (the battery scores the derived pairs), not
/// this compiler's.
/// The derivation is all-or-nothing on Accent + Surface: a palette missing
/// (or mistyping) one derives NOTHING and the previous values persist —
/// exactly what the palette keys themselves do when a theme file omits a
/// role.
/// The publish rides ThemeManager's color step, so a theme switch or a
/// hot-reloaded file retints the family together with the palette.
/// </summary>
internal static class ButtonKeys
{
    /// <summary>The resource key of the soft hover fill (Accent at the hover step over Surface).</summary>
    public const string HoverTintKey = "ButtonHoverTint";

    /// <summary>The resource key of the stronger press fill (Accent at the press step over Surface).</summary>
    public const string PressedTintKey = "ButtonPressedTint";

    /// <summary>The resource key of the primary fill's scored readable foreground.</summary>
    public const string PrimaryForegroundKey = "PrimaryForeground";

    /// <summary>The palette role the tints blend toward and the primary scores against.</summary>
    public const string AccentPaletteKey = "Accent";

    /// <summary>The palette role the tints step away from.</summary>
    public const string SurfacePaletteKey = "Surface";

    /// <summary>The soft hover step: Accent blended 12% over the Surface (the approved variant A tint).</summary>
    public const double HoverStep = 0.12;

    /// <summary>The stronger press step: Accent blended 22% over the Surface.</summary>
    public const double PressedStep = 0.22;

    /// <summary>The scored foreground candidates, in argmax tie order (black first keeps a tie deterministic).</summary>
    private static readonly string[] ForegroundCandidates = ["#000000", "#ffffff"];

    /// <summary>
    /// Derive the button family from a theme palette. Returns the full
    /// family (never a subset) when both foundation keys read, and null
    /// with a diagnostic when one is missing or unparsable.
    /// </summary>
    public static IReadOnlyDictionary<string, Color>? TryDerive(
        IReadOnlyDictionary<string, string> palette,
        out string? failure)
    {
        failure = null;

        if (!TryReadFoundation(palette, AccentPaletteKey, out var accent, ref failure) ||
            !TryReadFoundation(palette, SurfacePaletteKey, out var surface, ref failure))
        {
            return null;
        }

        return new Dictionary<string, Color>
        {
            [HoverTintKey] = FluentSystemKeys.Mix(surface, accent, HoverStep),
            [PressedTintKey] = FluentSystemKeys.Mix(surface, accent, PressedStep),
            [PrimaryForegroundKey] = Color.Parse(ChooseForeground(accent)),
        };
    }

    private static bool TryReadFoundation(
        IReadOnlyDictionary<string, string> palette,
        string key,
        out Color color,
        ref string? failure)
    {
        color = default;

        if (!palette.TryGetValue(key, out var raw))
        {
            failure = $"the palette carries no '{key}' — the derived button family is all-or-nothing";
            return false;
        }

        if (!Color.TryParse(raw, out color))
        {
            failure = $"the palette's '{key}' is not a color: '{raw}'";
            return false;
        }

        return true;
    }

    /// <summary>
    /// The candidate scoring over the primary's one fill: both achromatic
    /// candidates against the Accent (the battery's own contrast math),
    /// the better pair wins, and the candidate order keeps a tie
    /// deterministic.
    /// </summary>
    private static string ChooseForeground(Color accent)
    {
        var fill = Format(accent);
        string best = ForegroundCandidates[0];
        double bestRatio = Contrast.Ratio(best, fill);

        foreach (var candidate in ForegroundCandidates.Skip(1))
        {
            var ratio = Contrast.Ratio(candidate, fill);
            if (ratio > bestRatio)
            {
                best = candidate;
                bestRatio = ratio;
            }
        }

        return best;
    }

    private static string Format(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
