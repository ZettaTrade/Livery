namespace Livery.Tests;

using System;
using System.IO;
using FluentAssertions;
using Livery.Themes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

/// <summary>
/// The theme singleton's dispose contract (T-165): a disposed manager must
/// stop claiming process-wide initialization. Every consumer resolves
/// through the one static, so a stale pointer would serve subscribes and
/// lookups from the torn-down manager forever - the T-144 test-suite
/// poison's exact route (a probe's dispose left Instance live and every
/// later widget attach died on the torn-down ThemeChanged subject; the
/// localization manager already carries the T-073 shape, this pins the
/// same contract on its theme twin). The facts mutate the static, so a
/// future singleton-touching class joins this one in a serialized
/// collection.
/// </summary>
public sealed class ThemeManagerDisposeTests
{
    /// <summary>
    /// A bare manager: the constructor registers the static Instance and
    /// touches no disk (themes load on InitializeAsync, which these facts
    /// never need - the temp path only satisfies the constructor's
    /// non-empty guard).
    /// </summary>
    private static ThemeManager NewManager() => new(
        NullLogger.Instance,
        Path.Combine(Path.GetTempPath(), $"livery-tests-themes-{Guid.NewGuid():N}"));

    [Fact]
    public void ADisposedManager_StopsClaimingProcessWideInitialization()
    {
        var manager = NewManager();
        ThemeManager.IsInitialized.Should().BeTrue("the constructor registers the static Instance");

        manager.Dispose();

        ThemeManager.IsInitialized.Should().BeFalse("Dispose clears the Instance pointer when it owns it");
        var access = () => _ = ThemeManager.Instance;
        access.Should().Throw<InvalidOperationException>().WithMessage("ThemeManager not initialized");
    }

    [Fact]
    public void ADispose_LeavesASubsequentManagerRegistrationAlone()
    {
        var first = NewManager();
        var second = NewManager(); // re-registers the static Instance
        _ = ThemeManager.Instance.Should().BeSameAs(second);

        first.Dispose(); // no longer owns the pointer

        ThemeManager.IsInitialized.Should().BeTrue("the older instance's dispose must not clear the newer registration");
        _ = ThemeManager.Instance.Should().BeSameAs(second);

        second.Dispose();
    }
}
