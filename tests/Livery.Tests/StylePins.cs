namespace Livery.Tests;

using System.IO;
using FluentAssertions;
using Xunit;

/// <summary>
/// The style pins that used to live in the ZettaTrade monorepo's App.Tests
/// (the T-066 chrome facts and the T-110 accessibility facts) moved home
/// with the split (T-130): the owning repo asserts its own Controls.axaml
/// source, the consumer matrix asserts the compiled package surface. The
/// view pins have no headless render harness yet, so they ride the
/// source-of-truth file exactly like the consumer pins they mirror.
/// </summary>
public sealed class StylePins
{
    private static readonly string RepoRoot = FindRepositoryRoot();

    private static string ControlsThemes =>
        File.ReadAllText(Path.Combine(RepoRoot, "Themes", "Controls.axaml"))
            .Replace("\r\n", "\n");

    [Fact]
    public void TheChromeStyles_DeclareBothVariantsWithTheAvalonia12Mechanism()
    {
        // The chrome pair: extended client area + BorderOnly decorations (the
        // Avalonia 12 way: resize borders/AeroSnap survive, the system caption
        // does not) and both styles keep the same mechanism so the compact
        // variant stays consistent with the full one.
        var source = ControlsThemes;

        source.Should().Contain("<Style Selector=\"Window.chrome\">")
            .And.Contain("<Style Selector=\"Window.chrome-compact\">");

        foreach (var setter in new[]
                 {
                     "<Setter Property=\"ExtendClientAreaToDecorationsHint\" Value=\"True\" />",
                     "<Setter Property=\"WindowDecorations\" Value=\"BorderOnly\" />",
                 })
        {
            System.Text.RegularExpressions.Regex
                .Matches(source, System.Text.RegularExpressions.Regex.Escape(setter))
                .Should().HaveCount(2, "both chrome styles must carry the full mechanism");
        }
    }

    [Fact]
    public void EveryChromeTemplate_PresentsTheWindowContent()
    {
        // Avalonia 12 presents ContentControl.Content only through the
        // explicit Content template bindings in the template itself: a bare
        // PART_ContentPresenter never receives the window's content and the
        // window renders as the bare background (the black-window
        // regression). Both chrome variants must carry the native set.
        var source = ControlsThemes;

        foreach (var binding in new[]
                 {
                     "Content=\"{TemplateBinding Content}\"",
                     "ContentTemplate=\"{TemplateBinding ContentTemplate}\"",
                 })
        {
            System.Text.RegularExpressions.Regex
                .Matches(source, System.Text.RegularExpressions.Regex.Escape(binding))
                .Should().HaveCount(2, "both chrome templates must present the window content");
        }
    }

    /// <summary>
    /// The optional logo slot (the WindowChrome.Logo contract): both chrome
    /// templates wire the host window's attached property into the strip's
    /// Logo, and the slot presents it hit-invisible — the strip keeps its
    /// drag surface with the mark on it (the T-066 contract).
    /// </summary>
    [Fact]
    public void TheLogoSlot_IsHitInvisible_AndRidesTheWindowAttachedProperty()
    {
        var source = ControlsThemes;

        System.Text.RegularExpressions.Regex
            .Matches(
                source,
                @"Logo=""\{Binding Path=\(livery:WindowChrome\.Logo\), RelativeSource=\{RelativeSource TemplatedParent\}\}""")
            .Should().HaveCount(
                2,
                "both chrome variants wire the optional logo through the same attached property");

        var slotPattern = @"<ContentPresenter Name=""PART_Logo""[\s\S]*?IsHitTestVisible=""False""[\s\S]*?/>";
        System.Text.RegularExpressions.Regex
            .IsMatch(source, slotPattern)
            .Should().BeTrue(
                "the logo slot never takes presses — the strip's drag zones survive the mark");
    }

    [Fact]
    public void EveryTitleBarStripButton_CarriesItsAutomationName()
    {
        var source = ControlsThemes;

        source.Should().Contain("AutomationProperties.Name=\"{loc:Localized Chrome.Minimize}\"",
            "the minimize button is icon-only; assistive tech needs the name");
        source.Should().Contain("AutomationProperties.Name=\"{loc:Localized Chrome.Maximize}\"",
            "the maximize button is icon-only");
        source.Should().Contain("AutomationProperties.Name=\"{loc:Localized Chrome.Restore}\"",
            "the restore button is icon-only");
        source.Should().Contain("AutomationProperties.Name=\"{loc:Localized Chrome.Close}\"",
            "the close button is icon-only — its name must never rely on the tooltip");
    }

    /// <summary>
    /// The strip's grid is Auto,*,Auto,Auto,Auto: the logo, the title, the
    /// minimize button, the maximize/restore state pair and the close
    /// button. The maximize/restore pair shares one state slot BY DESIGN —
    /// one of them hides with the other showing. The close button rides its
    /// OWN column: the logo-slot addition (Livery 0.1.8) shifted the other
    /// buttons one column right but left the close button's attribute — it
    /// overlapped the maximize slot ever since (the user-reported T-167
    /// "the close and maximize buttons pile onto each other").
    /// </summary>
    [Fact]
    public void EveryTitleBarStripButton_OccupiesItsOwnColumnOrTheStatePair()
    {
        var source = ControlsThemes;

        StripButtonColumn(source, "PART_MinimizeButton").Should().Be(2);
        StripButtonColumn(source, "PART_MaximizeButton").Should().Be(3);
        StripButtonColumn(source, "PART_RestoreButton").Should().Be(3,
            "the maximize/restore pair shares one state slot by design — one of them hides with the other showing");
        StripButtonColumn(source, "PART_CloseButton").Should().Be(4,
            "the close button rides its own column; sharing the maximize slot piles the two buttons together (T-167)");
    }

    /// <summary>The grid column a named strip button block declares.</summary>
    private static int StripButtonColumn(string source, string partName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            source,
            $@"<Button Name=""{partName}""[\s\S]*?Grid\.Column=""(\d+)""");
        match.Success.Should().BeTrue($"the {partName} block must declare its grid column");
        return int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    [Fact]
    public void TheSharedThemes_CarryTheKeyboardFocusIndicators()
    {
        var source = ControlsThemes;

        foreach (var selector in new[]
                 {
                     "<Style Selector=\"Button:focus\">",
                     "<Style Selector=\"ToggleButton:focus\">",
                     "<Style Selector=\"TabItem:focus\">",
                     "<Style Selector=\"MenuItem:focus\">",
                     "<Style Selector=\"livery|TitleBar Button.strip-icon:focus\">",
                 })
        {
            source.Should().Contain(
                selector,
                "keyboard focus needs a visible marker on every focusable family");
        }

        source.Should().Contain(
            "Value=\"{DynamicResource Highlight}\"",
            "the focus markers borrow the palette's on-state accent");
    }

    // ============================================================
    // The shared chrome kit (T-154): the vocabulary the views once
    // re-typed by hand lives here exactly once — these pins hold the
    // class shapes so the consumers can assert their copies are gone.
    // ============================================================

    [Fact]
    public void TheChromeKit_CarriesTheChipVocabulary()
    {
        var source = ControlsThemes;

        foreach (var selector in new[]
                 {
                     "<Style Selector=\"Border.chip\">",
                     "<Style Selector=\"Border.chip > TextBlock\">",
                     "<Style Selector=\"Border.chip.paper\">",
                     "<Style Selector=\"Border.chip.testnet\">",
                     "<Style Selector=\"Border.chip.live\">",
                 })
        {
            source.Should().Contain(
                selector,
                "the chip class owns the badge shape and the three state fills");
        }

        // The readable foreground (T-152): the chip caption rides the
        // host's derived brush through the descendant style, never the
        // ambient theme foreground.
        source.Should().Contain(
            "Foreground\" Value=\"{DynamicResource ChipForeground}\"",
            "the chip caption treatment carries the derived readable foreground");

        // The state fills keep the palette keys the dots already speak.
        foreach (var (selector, fill) in new[]
                 {
                     ("Border.chip.paper", "CandleUp"),
                     ("Border.chip.testnet", "Highlight"),
                     ("Border.chip.live", "Error"),
                 })
        {
            source.Should().Contain(
                $"<Style Selector=\"{selector}\">\n        <Setter Property=\"Background\" Value=\"{{DynamicResource {fill}}}\" />",
                "the chip's {0} state keeps its palette fill key", selector);
        }
    }

    [Fact]
    public void TheChromeKit_CarriesTheStateDotStates()
    {
        var source = ControlsThemes;

        foreach (var (selector, fill) in new[]
                 {
                     ("Ellipse.dot.connected", "CandleUp"),
                     ("Ellipse.dot.reconnecting", "Highlight"),
                     ("Ellipse.dot.down", "Error"),
                     ("Ellipse.dot.unsupervised", "TextSecondary"),
                 })
        {
            source.Should().Contain(
                $"<Style Selector=\"{selector}\">\n        <Setter Property=\"Fill\" Value=\"{{DynamicResource {fill}}}\" />",
                "the dot's {0} state keeps its palette fill key", selector);
        }
    }

    [Fact]
    public void TheChromeKit_CarriesTheToggleOnClass()
    {
        var source = ControlsThemes;

        source.Should().Contain(
            "<Style Selector=\"Button.on, ToggleButton.on\">",
            "the armed-toggle vocabulary covers both toggle families");
        source.Should().Contain(
            "BorderBrush\" Value=\"{DynamicResource Highlight}\"",
            "the armed toggle borrows the on-state stroke");
    }

    [Fact]
    public void TheChromeKit_CarriesTheIconButtonClass()
    {
        var source = ControlsThemes;

        foreach (var selector in new[]
                 {
                     "<Style Selector=\"Button.icon\">",
                     "<Style Selector=\"Button.icon /template/ ContentPresenter#PART_ContentPresenter\">",
                     "<Style Selector=\"Button.icon:pointerover\">",
                     "<Style Selector=\"Button.icon:pressed\">",
                     "<Style Selector=\"Button.icon:focus\">",
                     "<Style Selector=\"Button.icon.framed\">",
                     "<Style Selector=\"Button.icon.framed /template/ ContentPresenter#PART_ContentPresenter\">",
                 })
        {
            source.Should().Contain(
                selector,
                "the icon-button class carries the strip-action shape, its state feedback and the framed composition");
        }
    }

    [Fact]
    public void TheChromeKit_TheIconButtonAnswersThePointerWithTints()
    {
        var source = ControlsThemes;

        // T-232: the icon button's pointer answer is the button family's
        // tint language painted on the template presenter — the old opacity
        // race (0.85 -> 1.0) never read as feedback — and the armed contrast
        // stands on the kept rest dimming.
        source.Should().Contain(
            "<Style Selector=\"Button.icon:pointerover /template/ ContentPresenter#PART_ContentPresenter\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource ButtonHoverTint}\" />",
            "the icon hover answers with the family's soft tint on the presenter");
        source.Should().Contain(
            "<Style Selector=\"Button.icon:pressed /template/ ContentPresenter#PART_ContentPresenter\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource ButtonPressedTint}\" />",
            "the icon press answers with the stronger tint step on the presenter");
        source.Should().Contain(
            "<Style Selector=\"Button.icon:focus /template/ ContentPresenter#PART_ContentPresenter\">\n        <Setter Property=\"Background\" Value=\"Transparent\" />\n        <Setter Property=\"BorderBrush\" Value=\"{DynamicResource Highlight}\" />\n        <Setter Property=\"BorderThickness\" Value=\"1\" />",
            "keyboard focus lands the Highlight ring on the presenter (a 0-thickness button border paints nothing)");
    }

    [Fact]
    public void TheChromeKit_TheFramedChipReadsAsAButtonAtRest()
    {
        var source = ControlsThemes;

        // The T-232 review round: the transfer arrows' framed composition
        // fills the Surface under the family's hairline — the bare
        // outline read as a text field, not a button — and the chip's
        // state steps ride after the generic .icon blocks, so the fill
        // survives every state (the generic hover tint and the focus
        // clear would thin it out).
        source.Should().Contain(
            "<Style Selector=\"Button.icon.framed\">\n        <Setter Property=\"BorderThickness\" Value=\"1\" />\n        <Setter Property=\"Opacity\" Value=\"1\" />",
            "the chip reads full-bright at rest (the glyph dimming is for bare marks)");
        source.Should().Contain(
            "<Style Selector=\"Button.icon.framed /template/ ContentPresenter#PART_ContentPresenter\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Surface}\" />\n        <Setter Property=\"BorderBrush\" Value=\"{DynamicResource ControlHairline}\" />",
            "the chip fills the Surface under the family hairline at rest");
        source.Should().Contain(
            "<Style Selector=\"Button.icon.framed:focus /template/ ContentPresenter#PART_ContentPresenter\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Surface}\" />",
            "keyboard focus keeps the fill the generic focus rule clears (the ring answers through the Highlight border)");
        source.Should().Contain(
            "<Style Selector=\"Button.icon.framed:pointerover /template/ ContentPresenter#PART_ContentPresenter\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Surface}\" />\n        <Setter Property=\"BorderBrush\" Value=\"{DynamicResource Highlight}\" />",
            "the hover answers with the Highlight stroke and keeps the fill");
        source.Should().Contain(
            "<Style Selector=\"Button.icon.framed:pressed\">\n        <Setter Property=\"Opacity\" Value=\"0.8\" />",
            "the press dims the whole chip like the row grammar's press");
    }

    [Fact]
    public void TheChromeKit_TheInfoIconIsFocusableAndNamed()
    {
        var source = ControlsThemes;

        source.Should().Contain(
            "<Setter Property=\"Focusable\" Value=\"True\" />",
            "the info icon is the a11y step up: the pointer-only cue becomes keyboard-reachable");
        source.Should().Contain(
            "Name=\"PART_Ring\"",
            "the template ring is the focus-marker target");
        source.Should().Contain(
            "<Style Selector=\"livery|InfoIcon:focus /template/ Border#PART_Ring\">",
            "keyboard focus lands a visible marker on the ring");
    }

    [Fact]
    public void TheChromeKit_CarriesThePanelHeaderScaffold()
    {
        var source = ControlsThemes;

        // The scaffold: the ONE padding grammar riding the scale's uniform
        // inset, and the title treatment published once for the heads with
        // actions (the DockPanel's own TextBlock children) and for the bare
        // captions and tab headers (the .panel-title class).
        source.Should().Contain(
            "<Style Selector=\"DockPanel.panel-header\">",
            "the head scaffold lives in the kit, never re-typed per panel");
        source.Should().Contain(
            "<Style Selector=\"DockPanel.panel-header > TextBlock, TextBlock.panel-title\">",
            "the title treatment covers both shapes — scaffold children and bare captions");
        source.Should().Contain(
            "Margin\" Value=\"{DynamicResource InsetS}\"",
            "the scaffold's padding grammar rides the spacing scale, not a hand literal");
        source.Should().Contain(
            "FontSize\" Value=\"{DynamicResource SubtitleStrongFontSize}\"",
            "the title rides the ramp's SubtitleStrong step, not a hand literal");
        source.Should().Contain(
            "FontWeight\" Value=\"Bold\"",
            "the head titles read emphasized");
    }

    [Fact]
    public void TheChromeKit_TheArmedIconInksItsGlyph()
    {
        var source = ControlsThemes;

        // The armed icon (.on composed over .icon): full-bright with the
        // glyph stroking in the armed ink — the Path content the Foreground
        // setter cannot reach. Both rules ride AFTER the .icon block, so
        // the last-matching-wins race settles in the armed state's favor.
        source.Should().Contain(
            "<Style Selector=\"Button.icon.on\">",
            "the armed icon button's full-bright state lives in the kit");
        source.Should().Contain(
            "<Style Selector=\"Button.icon.on Path\">",
            "the armed glyph ink lives in the kit — the consumer never re-types it");
        source.Should().Contain(
            "Stroke\" Value=\"{DynamicResource Highlight}\"",
            "the armed ink borrows the palette's on-state accent");

        var iconIndex = source.IndexOf("<Style Selector=\"Button.icon\">", StringComparison.Ordinal);
        var armedIndex = source.IndexOf("<Style Selector=\"Button.icon.on\">", StringComparison.Ordinal);
        armedIndex.Should().BeGreaterThan(
            iconIndex,
            "the armed rules must ride after the .icon block — the last matching style wins the opacity race");
    }

    // ============================================================
    // The button kit (T-183): the quiet family, the derived soft-tint
    // states, the primary action and the quiet carrier — the source
    // pins hold the vocabulary the consumer windows adopt.
    // ============================================================

    [Fact]
    public void TheButtonFamily_RidesTheDerivedSoftTintStates()
    {
        var source = ControlsThemes;

        // The resting button: transparent over the derived hairline stroke
        // — the quiet default the kit's vocabulary assumes; the stroke
        // rides its own key (the plain Border role vanishes against the
        // cards the button sits on). The rounding rides the corner token.
        source.Should().Contain(
            "<Style Selector=\"Button\">\n        <Setter Property=\"Background\" Value=\"Transparent\" />",
            "the resting button is quiet chrome, never a Surface slab");
        source.Should().Contain(
            "<Setter Property=\"Background\" Value=\"Transparent\" />\n        <Setter Property=\"BorderBrush\" Value=\"{DynamicResource ControlHairline}\" />",
            "the resting stroke rides the derived hairline step");
        System.Text.RegularExpressions.Regex
            .Matches(
                source,
                System.Text.RegularExpressions.Regex.Escape(
                    "<Setter Property=\"CornerRadius\" Value=\"{DynamicResource CornerM}\" />"))
            .Should().HaveCount(
                3,
                "the button/text/combo family all ride the corner token");

        // The states answer with the derived tints — never the solid
        // Accent flash (the offender the task retires).
        source.Should().Contain(
            "<Style Selector=\"Button:pointerover\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource ButtonHoverTint}\" />",
            "the hover fills with the derived soft tint");
        source.Should().Contain(
            "<Style Selector=\"Button:pressed\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource ButtonPressedTint}\" />",
            "the press answers with the stronger tint step");
        source.Should().NotContain(
            "<Style Selector=\"Button:pointerover\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Accent}\" />",
            "no button flashes a solid Accent fill on hover");
    }

    [Fact]
    public void ThePrimaryClass_IsTheSolidAccentFillWithTheScoredForeground()
    {
        var source = ControlsThemes;

        source.Should().Contain(
            "<Style Selector=\"Button.primary\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Accent}\" />",
            "the primary fills solid Accent");
        source.Should().Contain(
            "<Setter Property=\"Foreground\" Value=\"{DynamicResource PrimaryForeground}\" />",
            "the primary's ink is the derivation's scored readable foreground");
        source.Should().Contain(
            "<Style Selector=\"Button.primary:pointerover\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Accent}\" />",
            "the hover holds the fill — a scored pair stays the rendered pair");
        source.Should().Contain(
            "<Style Selector=\"Button.primary:pressed\">\n        <Setter Property=\"Background\" Value=\"{DynamicResource Accent}\" />",
            "the press holds the fill too");
        source.Should().Contain(
            "<Setter Property=\"Opacity\" Value=\"0.8\" />",
            "the press rides the strip's dim step");

        // The emphasis ring answers the hover (the row grammar's own
        // hover language), never a recolor.
        var hoverBlock = source[source.IndexOf(
            "<Style Selector=\"Button.primary:pointerover\">", StringComparison.Ordinal)..];
        hoverBlock.Should().Contain(
            "<Setter Property=\"BorderBrush\" Value=\"{DynamicResource Highlight}\" />",
            "the primary's hover rides the emphasis ring");
    }

    [Fact]
    public void TheQuietClass_IsChromeFreeAtEveryState()
    {
        var source = ControlsThemes;

        foreach (var selector in new[]
                 {
                     "<Style Selector=\"Button.quiet\">",
                     "<Style Selector=\"Button.quiet /template/ ContentPresenter#PART_ContentPresenter\">",
                     "<Style Selector=\"Button.quiet:pointerover\">",
                     "<Style Selector=\"Button.quiet:pointerover /template/ ContentPresenter#PART_ContentPresenter\">",
                     "<Style Selector=\"Button.quiet:pressed\">",
                     "<Style Selector=\"Button.quiet:pressed /template/ ContentPresenter#PART_ContentPresenter\">",
                     "<Style Selector=\"Button.quiet:focus /template/ ContentPresenter#PART_ContentPresenter\">",
                 })
        {
            source.Should().Contain(
                selector,
                "the quiet carrier suppresses the button chrome at every state — the child card owns it");
        }

        var quiet = source[source.IndexOf("<Style Selector=\"Button.quiet\">", StringComparison.Ordinal)..];
        quiet.Should().Contain(
            "<Setter Property=\"BorderThickness\" Value=\"0\" />",
            "the quiet button carries no stroke of its own");
    }

    [Fact]
    public void TheTextHosts_RideTheCornerToken()
    {
        var source = ControlsThemes;

        foreach (var selector in new[] { "TextBox", "ComboBox" })
        {
            source.Should().Contain(
                $"<Style Selector=\"{selector}\">",
                "the {0} family carries its restyling block", selector);
            source.Should().Contain(
                $"<Style Selector=\"{selector}\">\n        <Setter Property=\"Background\" Value=\"{{DynamicResource SurfaceHigh}}\" />",
                "the {0} host fills the derived raised well — a second flat Surface melts into the card plane (T-223)", selector);
            source.Should().Contain(
                $"<Setter Property=\"Background\" Value=\"{{DynamicResource SurfaceHigh}}\" />\n        <Setter Property=\"BorderBrush\" Value=\"{{DynamicResource ControlHairline}}\" />",
                "the {0} host's stroke rides the derived hairline step (T-223)", selector);
        }

        System.Text.RegularExpressions.Regex
            .Matches(
                source,
                System.Text.RegularExpressions.Regex.Escape(
                    "<Setter Property=\"CornerRadius\" Value=\"{DynamicResource CornerM}\" />"))
            .Should().HaveCount(
                3,
                "the button/text/combo rounding rides the corner token — one consistent shape, never a hand literal");
    }

    /// <summary>
    /// Walk up from the test's base directory to the folder carrying the
    /// Livery.csproj marker: tests and CI runs all execute inside the
    /// repository, so this resolves identically for every consumer.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Livery.csproj")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the test must run inside a Livery repository checkout");
        return directory!.FullName;
    }
}
