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
        File.ReadAllText(Path.Combine(RepoRoot, "Themes", "Controls.axaml"));

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
