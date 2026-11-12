namespace Livery.Themes;

using Avalonia.Media;

/// <summary>
/// The derived Fluent system keys: the stock control templates (the Fluent
/// theme) consume a wide family of <c>SystemAccentColor*</c> colors and
/// <c>SystemControl*Brush</c> brushes, and a user JSON palette never
/// mentioned any of them — so those stock paths kept the built-in tint
/// under every theme (T-150's second parallel color system). The module
/// derives the family from the active theme's OWN palette: a decorative
/// accent ramp around <c>Accent</c>, the foreground and surface steps
/// blended from the theme's <c>TextPrimary</c>, <c>Background</c> and
/// <c>Surface</c>, the selection accents and the transparent pair. The
/// derivation is intentionally bounded and formula-pure: the WCAG quality
/// of the shipped palette values is the theme contract's own lane (the
/// consumer's contrast pins iterate the real theme files), not this
/// compiler's.
/// The publish rides ThemeManager's color step, so a theme switch or a
/// hot-reloaded file retints the derived family together with the palette.
/// A palette missing (or mistyping) a foundation key derives NOTHING:
/// a partial publication would leave an incoherent half-family, and the
/// previous theme's values persist — exactly what the palette keys
/// themselves do when a theme file omits a role.
/// </summary>
internal static class FluentSystemKeys
{
    /// <summary>The palette role the accent ramp spins around.</summary>
    public const string AccentPaletteKey = "Accent";

    /// <summary>The palette role the ramps step away from.</summary>
    public const string BackgroundPaletteKey = "Background";

    /// <summary>The palette role the lifted surfaces blend toward.</summary>
    public const string SurfacePaletteKey = "Surface";

    /// <summary>The palette role the foreground ramp fades down from.</summary>
    public const string TextPrimaryPaletteKey = "TextPrimary";

    /// <summary>Pseudo palette key for the lighter ramp target (never a real theme role).</summary>
    public const string WhiteToken = "White";

    /// <summary>Pseudo palette key for the darker ramp target (never a real theme role).</summary>
    public const string BlackToken = "Black";

    /// <summary>Pseudo palette key for the no-tint members (never a real theme role).</summary>
    public const string TransparentToken = "Transparent";

    /// <summary>
    /// The derivation plan: one row per published system key — resource key,
    /// the palette role (or the White/Black token) it blends FROM, the one
    /// it blends TO, and the blend step. The steps mirror Fluent's own step
    /// semantics (a full-strength step for the High members fading toward
    /// the theme's foundations on the way down) tuned to read over the
    /// theme's own Background rather than Fluent's hardcoded grays.
    /// </summary>
    private static readonly (string ResourceKey, string From, string To, double Step)[] Plan =
    [
        // The accent family: the accent itself plus the darker and lighter
        // derivatives the selection glyphs and checked states consume.
        ("SystemAccentColor", "Accent", "Accent", 0.0),
        ("SystemAccentColorDark1", "Accent", "Black", 0.15),
        ("SystemAccentColorDark2", "Accent", "Black", 0.30),
        ("SystemAccentColorDark3", "Accent", "Black", 0.45),
        ("SystemAccentColorLight1", "Accent", "White", 0.15),
        ("SystemAccentColorLight2", "Accent", "White", 0.30),
        ("SystemAccentColorLight3", "Accent", "White", 0.45),

        // The foreground ramp: text-family strokes fading from the theme's
        // own TextPrimary toward its Background (the stock controls' labels,
        // borders and glyph tints).
        ("SystemControlForegroundBaseHighBrush", "TextPrimary", "Background", 0.00),
        ("SystemControlForegroundBaseMediumHighBrush", "TextPrimary", "Background", 0.20),
        ("SystemControlForegroundBaseMediumBrush", "TextPrimary", "Background", 0.45),
        ("SystemControlForegroundBaseMediumLowBrush", "TextPrimary", "Background", 0.65),
        ("SystemControlForegroundBaseLowBrush", "TextPrimary", "Background", 0.82),

        // The disabled family: the disabled read levels the stock templates
        // apply to text and low-contrast shapes.
        ("SystemControlDisabledBaseHighBrush", "TextPrimary", "Background", 0.55),
        ("SystemControlDisabledBaseMediumLowBrush", "TextPrimary", "Background", 0.70),

        // The background ramp: hover/press overlays stepping UP from the
        // theme's Background toward its TextPrimary (subtle planar states
        // the complex templates paint).
        ("SystemControlBackgroundBaseHighBrush", "Background", "TextPrimary", 0.33),
        ("SystemControlBackgroundBaseMediumHighBrush", "Background", "TextPrimary", 0.22),
        ("SystemControlBackgroundBaseMediumBrush", "Background", "TextPrimary", 0.15),
        ("SystemControlBackgroundBaseMediumLowBrush", "Background", "TextPrimary", 0.09),
        ("SystemControlBackgroundBaseLowBrush", "Background", "TextPrimary", 0.05),

        // The lifted surfaces: the overlay hosts (flyouts, popups, complex
        // tool panels) step from the Background up toward the theme's own
        // Surface. AltHigh is the stock flyout root.
        ("SystemControlBackgroundAltHighBrush", "Background", "Surface", 0.35),

        // The chrome family: the scrollbar/tool-window surfaces blend from
        // the Background toward the Surface one step further than the alt
        // ramp (they read as raised tools, not overlays).
        ("SystemControlBackgroundChromeMediumLowBrush", "Background", "Surface", 0.40),
        ("SystemControlBackgroundChromeMediumBrush", "Background", "Surface", 0.55),

        // The selection accents: the checked state and the list selection
        // glyph carry the accent (Alt tints a step brighter for its
        // hover-on-accent occupancy).
        ("SystemControlHighlightAccentBrush", "Accent", "Accent", 0.0),
        ("SystemControlHighlightAltAccentBrush", "Accent", "White", 0.30),

        // The transparent pair: template internals clear their own fills —
        // palette-independent, published with the family for completeness.
        ("SystemControlTransparentBrush", "Transparent", "Transparent", 0.0),
        ("SystemControlBackgroundTransparentBrush", "Transparent", "Transparent", 0.0),
    ];

    /// <summary>
    /// Derive the system-key family from a theme palette. Returns the full
    /// family (never a subset) when every foundation key reads, and null
    /// with a diagnostic when one is missing or unparsable. The White and
    /// Black tokens are ramp targets, never palette roles.
    /// </summary>
    public static IReadOnlyDictionary<string, Color>? TryDerive(
        IReadOnlyDictionary<string, string> palette,
        out string? failure)
    {
        failure = null;

        if (!TryReadFoundation(palette, AccentPaletteKey, out var accent, ref failure) ||
            !TryReadFoundation(palette, BackgroundPaletteKey, out var background, ref failure) ||
            !TryReadFoundation(palette, SurfacePaletteKey, out var surface, ref failure) ||
            !TryReadFoundation(palette, TextPrimaryPaletteKey, out var textPrimary, ref failure))
        {
            return null;
        }

        var foundation = new Dictionary<string, Color>
        {
            [AccentPaletteKey] = accent,
            [BackgroundPaletteKey] = background,
            [SurfacePaletteKey] = surface,
            [TextPrimaryPaletteKey] = textPrimary,
            [WhiteToken] = Colors.White,
            [BlackToken] = Colors.Black,
            [TransparentToken] = Colors.Transparent,
        };

        var derived = new Dictionary<string, Color>(Plan.Length);
        foreach (var (resourceKey, from, to, step) in Plan)
        {
            derived[resourceKey] = Mix(foundation[from], foundation[to], step);
        }

        return derived;
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
            failure = $"the palette carries no '{key}' — the derived system family is all-or-nothing";
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
    /// Linear channel blend, step <paramref name="t"/> toward
    /// <paramref name="to"/>; rounding is deterministic (away from zero) so
    /// the pins can mirror the exact bytes. Shared with the button family's
    /// derivation (<see cref="ButtonKeys"/>) — one blend, one math, so the
    /// two families' pins mirror the same bytes.
    /// </summary>
    internal static Color Mix(Color from, Color to, double t)
    {
        static byte Blend(byte a, byte b, double step) =>
            (byte)Math.Round((a * (1.0 - step)) + (b * step), MidpointRounding.AwayFromZero);

        return Color.FromArgb(
            Blend(from.A, to.A, t),
            Blend(from.R, to.R, t),
            Blend(from.G, to.G, t),
            Blend(from.B, to.B, t));
    }
}
