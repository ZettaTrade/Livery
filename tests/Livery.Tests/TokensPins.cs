namespace Livery.Tests;

using System.IO;
using FluentAssertions;
using Xunit;

/// <summary>
/// The type tokens (T-155) hold their documented shapes: the ramp values,
/// the spacing scale in both shapes, the numeric stack membership and the
/// packaged resource inclusion. The source-of-truth file is asserted
/// directly, exactly like the chrome-kit pins they sit beside.
/// </summary>
public sealed class TokensPins
{
    private static readonly string RepoRoot = FindRepositoryRoot();

    private static string TokensThemes =>
        File.ReadAllText(Path.Combine(RepoRoot, "Themes", "Tokens.axaml"))
            .Replace("\r\n", "\n");

    [Fact]
    public void TheRamp_CarriesTheFourDocumentedSteps()
    {
        var source = TokensThemes;

        foreach (var (key, value) in new[]
                 {
                     ("CaptionFontSize", "11"),
                     ("BodyFontSize", "13"),
                     ("SubtitleStrongFontSize", "14"),
                     ("TitleFontSize", "16"),
                 })
        {
            source.Should().Contain(
                $"<x:Double x:Key=\"{key}\">{value}</x:Double>",
                "the ramp's {0} step is a documented constant, not an ad-hoc value", key);
        }

        // The platform default (12) stays implicit: the dictionary carries
        // no 12 ramp step, so an unset FontSize IS the default — hosts
        // carry no 12 literal at all. (The spacing scale's own 12 step is
        // a different token family and is not affected.)
        source.Should().NotMatchRegex(
            "x:Key=\"[A-Za-z]+FontSize\">12</x:Double>",
            "the ramp never republishes the platform default as a font step");
    }

    [Fact]
    public void TheSpacingScale_CarriesBothShapesOfTheSameSteps()
    {
        var source = TokensThemes;

        foreach (var (space, inset, value) in new[]
                 {
                     ("SpaceXs", "InsetXs", "4"),
                     ("SpaceS", "InsetS", "8"),
                     ("SpaceM", "InsetM", "12"),
                     ("SpaceL", "InsetL", "16"),
                     ("SpaceXl", "InsetXl", "24"),
                 })
        {
            source.Should().Contain(
                $"<x:Double x:Key=\"{space}\">{value}</x:Double>",
                "the scale's {0} step serves the Spacing family as a double", value);
            source.Should().Contain(
                $"<Thickness x:Key=\"{inset}\">{value}</Thickness>",
                "the scale's {0} step serves Margin/Padding as a uniform thickness", value);
        }
    }

    [Fact]
    public void TheNumericStack_CarriesTheMonospacedFamily()
    {
        var source = TokensThemes;

        source.Should().Contain(
            "<FontFamily x:Key=\"NumericFont\">Consolas,Menlo,DejaVu Sans Mono,monospace</FontFamily>",
            "the HUD's monospaced precedent is the shared numeric stack");
    }

    [Fact]
    public void TheTokensDictionary_ShipsInThePackage()
    {
        var csproj = File.ReadAllText(Path.Combine(RepoRoot, "Livery.csproj"))
            .Replace("\r\n", "\n");

        csproj.Should().Contain(
            "<AvaloniaResource Include=\"Themes\\Tokens.axaml\" />",
            "the dictionary must be a compiled Avalonia resource or the host's ResourceInclude fails at runtime");
    }

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
