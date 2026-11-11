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
                 })
        {
            source.Should().Contain(
                selector,
                "the icon-button class carries the strip-action shape and its state feedback");
        }
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
