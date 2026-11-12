namespace Livery.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Livery.Themes;
using Xunit;

/// <summary>
/// The palette role contract's own facts (T-151): the battery API behaves
/// before any consumer pins it over real files. A healthy palette answers
/// all role pairs, a broken palette fails LOUDLY naming the exact pair it
/// broke (the gate must be able to point at the pair), and a palette that
/// cannot read its roles throws rather than silently passing. The palettes
/// here are inline stories of the shipped values (the pre-retune dark
/// accent, the midnight baseline) — the real files' battery lives where the
/// files live, in the consumer's suite.
/// </summary>
public sealed class PaletteContractTests
{
    /// <summary>The healthy baseline: the midnight palette's role shape
    /// (every shipped theme answers against this shape).</summary>
    private static readonly IReadOnlyDictionary<string, string> Healthy = new Dictionary<string, string>
    {
        ["Background"] = "#0d1117",
        ["Surface"] = "#161b22",
        ["Accent"] = "#1f6feb",
        ["TextPrimary"] = "#ffffff",
        ["Buy"] = "#22c55e",
        ["Sell"] = "#ef4444",
    };

    [Fact]
    public void AHealthyPalette_ServesTheWholeContract()
    {
        var violations = PaletteContract.Violations(new ThemeDefinition { Colors = new Dictionary<string, string>(Healthy) });

        violations.Should().BeEmpty("the midnight-shape palette answers every role pair");
    }

    [Fact]
    public void ABrokenPalette_FailsNamingTheExactPairs()
    {
        // The pre-retune dark accent: invisible as a hover fill AND as a
        // focus stroke — the review's finding, pinned as the battery's
        // own diagnostic shape.
        var broken = new Dictionary<string, string>(Healthy)
        {
            ["Accent"] = "#0f3460",
        };

        var violations = PaletteContract.Violations(new ThemeDefinition { Colors = broken });

        violations.Should().NotBeNull();
        violations!.Select(v => v.Pair).Should().Contain(
            "interactive hover fill over surface",
            "the pre-retune accent reads ~1.4:1 on its surface — the battery must name it");
        violations.Should().Contain(v => v.Pair == "focus stroke over background",
            "the same accent is invisible as a focus stroke");
        violations.Should().NotContain(v => v.Pair == "text over the interactive hover fill",
            "white text over the dark accent still reads — only the visual legs fail");
        var hover = violations!.Single(v => v.Pair.StartsWith("interactive hover"));
        hover.Ratio.Should().BeLessThan(hover.Minimum);
        hover.Minimum.Should().Be(PaletteContract.VisualPairMinimum);
    }

    [Fact]
    public void AChipFillFail_NamesTheChipPair()
    {
        var broken = new Dictionary<string, string>(Healthy)
        {
            ["Sell"] = "#8a2e2e",
        };

        var violations = PaletteContract.Violations(new ThemeDefinition { Colors = broken });

        var sell = violations!.Should().ContainSingle(v => v.Pair == "sell chip fill over surface").Which;
        sell.Background.Should().Be("#161b22");
        sell.Ratio.Should().BeLessThan(PaletteContract.VisualPairMinimum);
    }

    [Fact]
    public void AButtonTintNoOp_NamesTheStepRow()
    {
        // The T-183 derived row: an Accent equal to the Surface blends a
        // no-op hover tint — the state-existence floor fails by name, so a
        // palette whose hover vanishes into its ground cannot sneak
        // through the gate.
        var noOp = new Dictionary<string, string>(Healthy)
        {
            ["Accent"] = "#161b22",
        };

        var violations = PaletteContract.Violations(new ThemeDefinition { Colors = noOp });

        var step = violations.Should().ContainSingle(
            v => v.Pair == "the button hover tint is a visible step over its surface").Which;
        step.Ratio.Should().BeLessThan(PaletteContract.SubtleTintStepMinimum);
        step.Minimum.Should().Be(PaletteContract.SubtleTintStepMinimum);
    }

    [Fact]
    public void AMissingRole_ThrowsLoudly()
    {
        var holey = new Dictionary<string, string>(Healthy);
        holey.Remove("Accent");

        var act = () => PaletteContract.Violations(new ThemeDefinition { Colors = holey });

        act.Should().Throw<FormatException>().WithMessage("*'Accent'*cannot read*");
    }

    [Fact]
    public void AnUnparsableRole_ThrowsLoudly()
    {
        var corrupt = new Dictionary<string, string>(Healthy)
        {
            ["Accent"] = "not-a-color",
        };

        var act = () => PaletteContract.Violations(new ThemeDefinition { Colors = corrupt });

        act.Should().Throw<FormatException>().WithMessage("*not-a-color*#RRGGBB*");
    }

    [Fact]
    public void AColorslessDefinition_ThrowsLoudly()
    {
        var act = () => PaletteContract.Violations(new ThemeDefinition { Colors = null });

        act.Should().Throw<FormatException>().WithMessage("*no colors at all*");
    }
}
