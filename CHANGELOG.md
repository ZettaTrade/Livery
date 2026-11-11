# Changelog

All notable changes to the Livery package are documented here.
Versions are the published NuGet versions.

## [0.1.6] - 2026-11-11

- The type tokens (T-155): `Themes/Tokens.axaml` publishes the documented
  ramp (Caption 11 / Body 13 / SubtitleStrong 14 / Title 16 — the platform
  default of 12 stays implicit, so hosts carry no 12 literal), the spacing
  scale 4/8/12/16/24 in both shapes (the raw `Space*` doubles for the
  Spacing family, the uniform `Inset*` Thickness for Margin/Padding) and
  the `NumericFont` monospaced stack (the HUD's precedent) for the numeric
  surfaces. Static design constants read with StaticResource; the host
  merges the dictionary via
  `<ResourceInclude Source="avares://Livery/Themes/Tokens.axaml" />`.
- The pins hold the token shapes: the ramp values, the scale pairs, the
  numeric stack membership and the packaged resource inclusion.

## [0.1.5] - 2026-11-11

- The shared chrome kit (T-154): the vocabulary the consumer views once
  re-typed by hand lives here once — the `chip` style class (the rounded
  state badge: shape, the paper/testnet/live fills, the caption treatment
  riding the host's derived `ChipForeground`), the `dot` class for the
  state ellipses (connected/reconnecting/down/unsupervised over
  CandleUp/Highlight/Error/TextSecondary), the `Button.on, ToggleButton.on`
  armed-toggle class, the `Button.icon` strip-action class and the
  `InfoIcon` templated control — the circled "i" affordance, focusable
  with a keyboard-focus ring and automation-named by the consumer (the
  a11y step up: the pointer-only cue copies become keyboard-reachable).

## [0.1.4] - 2026-11-11

- The palette role contract (T-151): `PaletteContract` documents the role
  shape (Background/Surface/Border planar; Accent the interactive token —
  hover fill + focus stroke; Highlight the on-state token; TextPrimary the
  reading token; Buy/Sell the chip fills) and walks every palette through
  the WCAG battery: the hover fill and focus stroke at 3.0:1 (WCAG 1.4.11),
  text over the interactive fill at 4.5:1, the chip fills at 3.0:1. A
  broken palette fails naming the exact pair; a palette that cannot read
  its roles throws (never silently conforming). The generated sample
  theme drops the brand-red accent for the interactive blue the shipped
  dark baseline carries — the sample now already answers the battery.

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
