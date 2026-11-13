namespace Livery.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Livery.Themes;
using Xunit;

/// <summary>
/// The control family's derivation pins (T-183, the surface-family
/// extension T-223): the soft-tint steps hold their approved alphas
/// (12% hover, 22% press — the variant A shape), the surface-family keys
/// lift toward the surface's own pole (7% hairline, 3% raised well), the
/// exact blended bytes mirror the shared blend's deterministic rounding,
/// the scored foreground always serves a readable pair over the primary
/// fill, and the all-or-nothing contract refuses a palette that cannot
/// serve both foundations — and the surface-family steps guarantee the
/// contract floor on the extreme grounds (the 0.5% grain walk, the oled
/// catch).
/// </summary>
public sealed class ButtonKeysTests
{
    private const string Accent = "Accent";
    private const string Surface = "Surface";

    private static IReadOnlyDictionary<string, string> Palette(string accent, string surface) =>
        new Dictionary<string, string>
        {
            [Accent] = accent,
            [Surface] = surface,
        };

    [Fact]
    public void TheHoverTint_BlendsOneSoftStepOverTheSurface()
    {
        // The midnight story: Surface #161b22 + Accent #1f6feb at 12% —
        // the exact bytes the shared blend's rounding produces.
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#161b22"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.HoverTintKey].Should().Be(Avalonia.Media.Color.Parse("#17253a"));
    }

    [Fact]
    public void ThePressedTint_BlendsTheStrongerStep()
    {
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#161b22"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.PressedTintKey].Should().Be(Avalonia.Media.Color.Parse("#182d4e"));
    }

    [Fact]
    public void TheHairline_LiftsOneStepTowardTheSurfacesOwnPole()
    {
        // The midnight story: Surface #161b22 is dark, so the resting
        // stroke lifts toward white — the exact bytes the shared blend's
        // rounding produces (7%).
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#161b22"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.HairlineKey].Should().Be(Avalonia.Media.Color.Parse("#262b31"));
    }

    [Fact]
    public void TheRaisedWell_BlendsTheRaisedStep()
    {
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#161b22"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.RaisedKey].Should().Be(Avalonia.Media.Color.Parse("#1d2229"));
    }

    [Fact]
    public void TheSurfaceSteps_HoldTheApprovedSteps()
    {
        ButtonKeys.HairlineStep.Should().Be(0.07, "the resting stroke is the approved variant A hairline step");
        ButtonKeys.RaisedStep.Should().Be(0.03, "the input wells ride the approved variant A raised fill");
    }

    [Fact]
    public void ALightSurface_SinksTheStepsTowardBlack()
    {
        // The pole branch: a light surface's stroke and well darken instead
        // of brightening — the step must read against the plane it sits
        // on, whatever the variant.
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#f3f4f6"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.HairlineKey].Should().Be(Avalonia.Media.Color.Parse("#e2e3e5"));
        derived[ButtonKeys.RaisedKey].Should().Be(Avalonia.Media.Color.Parse("#ecedef"));
    }

    [Theory]
    [InlineData("#000000")] // the true black ground: the pole is the only lift left
    [InlineData("#0a0a0a")] // the shipped oled palette's ground (the gate's catch)
    [InlineData("#ffffff")] // the true white ground: the light extreme
    public void TheExtremeGrounds_StillServeTheSurfaceFamilyFloors(string surface)
    {
        // The floor guarantee (T-223): a fixed step cannot promise visibility
        // on the extreme palettes — the shipped oled ground left the 3% well
        // one grain short of the contract floor (1.0485 < 1.05). The
        // derivation's floor walk raises the step until the pair clears, so
        // even the true black and white grounds ship a visible step.
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", surface), out _);

        derived.Should().NotBeNull();
        foreach (var key in new[] { ButtonKeys.HairlineKey, ButtonKeys.RaisedKey })
        {
            var lifted = derived![key];
            var ratio = Contrast.Ratio(
                $"#{lifted.R:X2}{lifted.G:X2}{lifted.B:X2}",
                surface);

            ratio.Should().BeGreaterThanOrEqualTo(
                PaletteContract.SubtleTintStepMinimum,
                "the {0} step over {1} clears the contract floor — the derivation guarantees it", key, surface);
        }
    }

    [Fact]
    public void TheOledGround_RidesTheWalksFirstGrain()
    {
        // The gate's catch: the oled palette's #0a0a0a ground left the 3%
        // well one grain short (#111111, 1.0485 < 1.05) — the walk's first
        // pass (0.035) is the answer, the smallest lift the guarantee ever
        // ships. The hairline's 7% request already clears over the same
        // ground, so the walk never touches it.
        ButtonKeys.FloorGrain.Should().Be(0.005, "the walk's grain is the minimum lift the guarantee ever ships");

        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#0a0a0a"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.RaisedKey].Should().Be(Avalonia.Media.Color.Parse("#131313"));
        derived[ButtonKeys.HairlineKey].Should().Be(
            Avalonia.Media.Color.Parse("#1b1b1b"),
            "the 7% request clears the floor over #0a0a0a, so the walk leaves it at the request");
    }

    [Fact]
    public void TheTrueBlackGround_LiftsTheWellTheMinimumStep()
    {
        // The black hole: no darker ground exists and the pole is the only
        // lift left — the well's 3% request lands #080808 (1.0486 < 1.05)
        // and the walk's first grain raises it to #090909; the hairline's
        // 7% request clears untouched.
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#000000"), out _);

        derived.Should().NotBeNull();
        derived![ButtonKeys.RaisedKey].Should().Be(Avalonia.Media.Color.Parse("#090909"));
        derived[ButtonKeys.HairlineKey].Should().Be(Avalonia.Media.Color.Parse("#121212"));
    }

    [Fact]
    public void TheDesignSteps_HoldTheApprovedAlphas()
    {
        ButtonKeys.HoverStep.Should().Be(0.12, "the soft hover tint is the approved variant A step");
        ButtonKeys.PressedStep.Should().Be(0.22, "the press answers with the stronger step");
    }

    [Theory]
    [InlineData("#2563eb", "#ffffff")] // the dark baseline: white reads best over the interactive blue
    [InlineData("#1f6feb", "#ffffff")]
    [InlineData("#3b82f6", "#000000")] // the light baseline: black reads best over the brighter blue
    [InlineData("#047857", "#ffffff")]
    [InlineData("#ffd700", "#000000")] // the gold: black reads best
    public void ThePrimaryForeground_ScoresTheReadableCandidate(string accent, string expected)
    {
        var derived = ButtonKeys.TryDerive(Palette(accent, "#161b22"), out _);

        derived![ButtonKeys.PrimaryForegroundKey].Should().Be(
            Avalonia.Media.Color.Parse(expected),
            "the scored argmax picks the readable achromatic ink over {0}", accent);
    }

    [Theory]
    [InlineData("#0a0a0a")]
    [InlineData("#1f6feb")]
    [InlineData("#3b82f6")]
    [InlineData("#22c55e")]
    [InlineData("#ef4444")]
    [InlineData("#ffd700")]
    [InlineData("#808080")]
    [InlineData("#f3f4f6")]
    public void ThePrimaryForeground_AlwaysClearsTheTextBar(string accent)
    {
        // The scoring's guarantee (the T-152 shape): against any single
        // fill at least one achromatic candidate clears 4.5:1 — the
        // scored pair can never ship an unreadable primary.
        var derived = ButtonKeys.TryDerive(Palette(accent, "#161b22"), out _);

        var foreground = derived![ButtonKeys.PrimaryForegroundKey];
        var ratio = Contrast.Ratio(
            $"#{foreground.R:X2}{foreground.G:X2}{foreground.B:X2}",
            accent);

        ratio.Should().BeGreaterThanOrEqualTo(
            PaletteContract.TextPairMinimum,
            "the scored foreground over {0} stays readable", accent);
    }

    [Fact]
    public void AMissingAccent_DerivesNothing()
    {
        var palette = new Dictionary<string, string> { [Surface] = "#161b22" };

        var derived = ButtonKeys.TryDerive(palette, out var failure);

        derived.Should().BeNull();
        failure.Should().Contain("'Accent'").And.Contain("all-or-nothing");
    }

    [Fact]
    public void AMissingSurface_DerivesNothing()
    {
        var palette = new Dictionary<string, string> { [Accent] = "#1f6feb" };

        var derived = ButtonKeys.TryDerive(palette, out var failure);

        derived.Should().BeNull();
        failure.Should().Contain("'Surface'");
    }

    [Fact]
    public void AnUnparsableAccent_DerivesNothing()
    {
        var derived = ButtonKeys.TryDerive(Palette("not-a-color", "#161b22"), out var failure);

        derived.Should().BeNull();
        failure.Should().Contain("'Accent'").And.Contain("not-a-color");
    }

    [Fact]
    public void TheFamily_CarriesExactlyTheFivePublishedKeys()
    {
        var derived = ButtonKeys.TryDerive(Palette("#1f6feb", "#161b22"), out _);

        derived!.Keys.Should().BeEquivalentTo(new[]
        {
            ButtonKeys.HoverTintKey,
            ButtonKeys.PressedTintKey,
            ButtonKeys.PrimaryForegroundKey,
            ButtonKeys.HairlineKey,
            ButtonKeys.RaisedKey,
        });
    }
}
