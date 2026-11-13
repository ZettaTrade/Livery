namespace Livery.Themes;

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

/// <summary>
/// The control vocabulary's derived keys (T-183, the surface-family
/// extension T-223): the soft-tint hover and press fills and the primary
/// fill's readable foreground, derived from the applied palette — the T-150
/// publishes-as-brush canon, the T-152 scored-foreground shape. The resting
/// button is transparent over its hairline stroke; hover fills with the
/// Accent blended one soft step over the theme's own Surface (12%), pressed
/// a stronger step (22%) — the approved variant A shape: the pointer
/// answers with a tint, never the solid Accent flash. The <c>.primary</c>
/// class fills solid Accent and reads through a scored achromatic
/// foreground (black vs white, the better pair wins — against any single
/// fill at least one candidate clears the WCAG 4.5:1 bar, the same
/// guarantee the chip foreground's scoring rides). The derivation is
/// intentionally bounded and formula-pure: the WCAG quality of the shipped
/// palette values is the theme contract's own lane (the battery scores the
/// derived pairs), not this compiler's — with the surface family's one
/// contractual exception: the hairline and the raised well derive from the
/// Surface alone (a value the theme author tunes freely), so their steps
/// GUARANTEE the contract's soft-tint floor
/// (<see cref="PaletteContract.SubtleTintStepMinimum"/>): a ground that
/// leaves the requested step short raises it by the floor grain until the
/// step reads (the oled palette's #0a0a0a ground left the 3% well one grain
/// short). The hover/press tints keep the request-pure shape: they derive
/// from the Accent, the theme's own interactive color, so a no-op blend
/// fails the battery by name — the palette must ship a usable Accent.
/// The surface-family keys (T-223) blend from the Surface alone: the
/// resting control's boundary stroke (<see cref="HairlineKey"/> — the
/// plain <c>Border</c> role vanishes against the very cards and strips a
/// button sits on, so the stroke derives its own visible step) and the
/// input wells' raised fill (<see cref="RaisedKey"/> — the combo/textbox
/// hosts ride the approved variant A's raised-well look). Both lift toward
/// the surface's contrast pole, chosen by the surface's own luminance: a
/// dark theme's stroke brightens, a light theme's darkens — the step must
/// read AGAINST the plane it sits on, whatever the variant, and the floor
/// walk keeps that promise on the extreme grounds.
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

    /// <summary>The resource key of the resting control's boundary stroke (the Surface lifted one hairline step).</summary>
    public const string HairlineKey = "ControlHairline";

    /// <summary>The resource key of the input hosts' raised well fill (the Surface lifted one raised step).</summary>
    public const string RaisedKey = "SurfaceHigh";

    /// <summary>The palette role the tints blend toward and the primary scores against.</summary>
    public const string AccentPaletteKey = "Accent";

    /// <summary>The palette role the tints step away from and the surface-family keys derive from.</summary>
    public const string SurfacePaletteKey = "Surface";

    /// <summary>The soft hover step: Accent blended 12% over the Surface (the approved variant A tint).</summary>
    public const double HoverStep = 0.12;

    /// <summary>The stronger press step: Accent blended 22% over the Surface.</summary>
    public const double PressedStep = 0.22;

    /// <summary>
    /// The hairline's requested step: the Surface lifted 7% toward its
    /// contrast pole — the floor walk raises it only when the ground leaves
    /// the pair short of the contract floor.
    /// </summary>
    public const double HairlineStep = 0.07;

    /// <summary>
    /// The raised well's requested step: the Surface lifted 3% toward its
    /// contrast pole — the floor walk raises it only when the ground leaves
    /// the pair short of the contract floor.
    /// </summary>
    public const double RaisedStep = 0.03;

    /// <summary>
    /// The floor walk's grain: the surface-family steps raise by 0.5% per
    /// pass until the blended pair clears the contract floor — the smallest
    /// lift the guarantee ever ships (one grain moved the oled well from
    /// 0.03 to 0.035).
    /// </summary>
    internal const double FloorGrain = 0.005;

    /// <summary>The scored foreground candidates, in argmax tie order (black first keeps a tie deterministic).</summary>
    private static readonly string[] ForegroundCandidates = ["#000000", "#ffffff"];

    /// <summary>The contrast poles the surface-family steps blend toward (the sRGB extremes).</summary>
    private static readonly Color PoleLight = Color.Parse("#ffffff");
    private static readonly Color PoleDark = Color.Parse("#000000");

    /// <summary>
    /// Derive the control family from a theme palette. Returns the full
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

        var pole = SurfaceIsDark(surface) ? PoleLight : PoleDark;
        return new Dictionary<string, Color>
        {
            [HoverTintKey] = FluentSystemKeys.Mix(surface, accent, HoverStep),
            [PressedTintKey] = FluentSystemKeys.Mix(surface, accent, PressedStep),
            [PrimaryForegroundKey] = Color.Parse(ChooseForeground(accent)),
            [HairlineKey] = BlendWithFloor(surface, pole, HairlineStep, PaletteContract.SubtleTintStepMinimum),
            [RaisedKey] = BlendWithFloor(surface, pole, RaisedStep, PaletteContract.SubtleTintStepMinimum),
        };
    }

    /// <summary>
    /// Blend <paramref name="from"/> → <paramref name="to"/> at the requested
    /// step, then raise the step by <see cref="FloorGrain"/> while the blended
    /// color's ratio over its ground sits below the floor: the request stays
    /// the answer whenever it already clears the floor (the pinned bytes
    /// hold), and the guarantee costs the minimum lift otherwise — the walk
    /// stops at the first clearing step, deterministically. The contrast pole
    /// clears the floor over any ground, so the walk terminates.
    /// </summary>
    private static Color BlendWithFloor(Color from, Color to, double step, double floor)
    {
        var blended = FluentSystemKeys.Mix(from, to, step);
        var ground = Format(from);

        while (step < 1.0 && Contrast.Ratio(Format(blended), ground) < floor)
        {
            step += FloorGrain;
            blended = FluentSystemKeys.Mix(from, to, step);
        }

        return blended;
    }

    /// <summary>
    /// The surface's own darkness, by the sRGB-byte luminance weights: the
    /// surface-family steps lift a dark surface toward white and sink a
    /// light one toward black. The byte-weight approximation serves the
    /// pole branch only — the scored pairs' WCAG quality stays the
    /// contract battery's own lane.
    /// </summary>
    private static bool SurfaceIsDark(Color surface) =>
        ((0.2126 * surface.R) + (0.7152 * surface.G) + (0.0722 * surface.B)) / 255.0 < 0.5;

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
