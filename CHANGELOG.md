# Changelog

All notable changes to the Livery package are documented here.
Versions are the published NuGet versions.

## [0.1.3] - 2026-11-11

- The brush-family publish fix (T-150, riding a real consumer scene): the
  derived `SystemControl*Brush` members must publish as brushes, not
  Colors. Avalonia 12's property store enforces value-type identity per
  effectively bound entry — a stock template resolves the key with a
  brush first (the hosted Fluent dictionary's own value), so a raw Color
  arriving on the next resources change threw
  `InvalidCastException` inside the effective-value store. The accent
  family keeps publishing Colors (the stock values there are Colors too).

## [0.1.2] - 2026-11-11

- The derived Fluent system keys (T-150): ThemeManager now publishes the
  stock control templates' own families (`SystemAccentColor*`, the
  `SystemControl*Brush` ramps, the chrome and highlight steps) from the
  applied theme's palette, so a user JSON theme recolors the stock paths
  together with the palette roles. The derivation is all-or-nothing on
  the foundation keys (Accent, Background, Surface, TextPrimary) and
  formula-pure; a palette missing a foundation key publishes nothing,
  leaving the previous theme's values in place — the palette keys' own
  omission semantics.

## [0.1.1] - 2026-11-10

- The two-repo bump rehearsal (T-130): a version-only release exercising
  the cross-repo path end to end - the version-bumped release here, then
  the one-line consuming bump in ZettaTrade. No API or style changes.

## [0.1.0] - 2026-11-10

- First standalone package: extracted from the ZettaTrade monorepo
  (`src/Livery`) with full history.
- Added NuGet packaging metadata (MIT, repository URL, packaged README) and
  the GitHub Packages publish workflow (tag-driven, version-guarded).
