# Changelog

All notable changes to the Livery package are documented here.
Versions are the published NuGet versions.

## [0.1.14] - 2026-11-14

- The icon buttons answer the pointer (T-232): the hover paints the button
  family's soft tint step (ButtonHoverTint) and the press the stronger step
  (ButtonPressedTint) on the template's content presenter — the Fluent button
  paints its state fill there, so the presenter is both the suppression and
  the tint target; the old opacity nudge (0.85 → 1.0) never read as feedback,
  so every borderless icon action (the settings' pair-transfer arrows, the
  chart's drawing tools, the toolbar glyphs) read as static marks. Keyboard
  focus draws the family's Highlight ring on the presenter (a 0-thickness
  button border paints nothing). The armed `.on` contrast stands on the kept
  0.85 rest dimming.
- The framed composition `Button.icon.framed`: the free-standing icon actions
  (the settings' pair transfers) gain the family's hairline outline at rest
  and answer the hover with the Highlight stroke — a mark-only button in a
  list context reads as decoration, not as a control.

## [0.1.13] - 2026-11-14

- The surface-family steps guarantee the contract floor (T-223): the shipped
  oled palette's #0a0a0a ground left the 3% raised well one grain short of
  the battery's 1.05 floor (#111111 over #0a0a0a, 1.0485) — a fixed step
  cannot promise visibility on the extreme palettes. The derivation's floor
  walk now raises the hairline's and the well's blend step by the 0.5% grain
  until the pair clears `SubtleTintStepMinimum`: the oled well lands at
  #131313, the shipped dark, midnight and light palettes' bytes stay exactly
  as pinned (their requests already clear), and the hover/press tints stay
  request-pure (the Accent is the theme's own interactive color — a no-op
  blend still fails the battery by name).

## [0.1.12] - 2026-11-14

- The control hairline and the raised wells (T-223): the derivation grows
  two Surface-family keys — `ControlHairline`, the resting button's
  boundary stroke (the plain `Border` role vanishes against the very cards
  and strips a button sits on — the stroke derives its own visible step),
  and `SurfaceHigh`, the input hosts' raised well fill (the combo/textbox
  wells no longer melt into the card plane). Both blend off the theme's
  own Surface toward its luminance pole (the stroke 7%, the well 3% — a
  dark theme's stroke brightens, a light theme's darkens); the shipped
  dark palette lands at #2a2a3d / #212134, the approved variant A's
  hand-picked `--hairline` / combo-well read.
- The kit's restyling follows: `Button`'s stroke and the text hosts'
  fill + stroke ride the new keys; the role-contract battery grows the
  pairs (the wells' text leg at 4.5:1 and the state-existence floors for
  the well and the hairline step).

## [0.1.11] - 2026-11-12

- The button kit (T-183): the generic button family goes quiet — the
  resting button is transparent over its hairline Border stroke, the
  pointer answers with the soft Accent tint the theme derivation
  publishes (`ButtonHoverTint`, a 12% Accent step over the theme's own
  Surface; pressed `ButtonPressedTint` at 22%) instead of the solid
  Accent flash. The derivation (`ButtonKeys`, the T-150
  publishes-as-brush canon) also scores the primary fill's readable
  foreground (`PrimaryForeground`, black vs white — against any fill at
  least one achromatic candidate clears 4.5:1, the T-152 shape); the
  `.primary` class marks the ONE main action per window (solid Accent,
  the scored ink, the emphasis ring on hover, the strip's dim step on
  press — the fill never leaves) and the `.quiet` class carries a Button
  whose chrome lives in its child card (the toast-row shape — chrome-free
  at every state, so the chrome never renders twice).
- The corner tokens: `CornerM` (4, the control rounding the
  button/text/combo family adopts) and `CornerL` (8, the card rounding the
  row cards ride) join the dictionary.
- The role contract battery (T-151) grew the derived rows: the text legs
  over both tints, the primary fill's scored foreground and the hover
  tint's state-existence floor (`SubtleTintStepMinimum` — a no-op blend
  fails by name, the soft tint deliberately sits below the WCAG non-text
  bar while the hairline stroke carries the boundary duty).

## [0.1.7] - 2026-11-11

- The panel header scaffold (T-160): the heads the panels share live here
  once — the `DockPanel.panel-header` composition (the title left, the
  actions right through a right-docked child) whose padding grammar rides
  the spacing scale's uniform inset, and the title treatment (bold, the
  ramp's SubtitleStrong step) published for the scaffold children and for
  bare captions and tab headers through the `TextBlock.panel-title` class.
  The kit styles read the tokens through DynamicResource (a Styles
  dictionary cannot StaticResource its sibling dictionary); the consumer
  merges Tokens.axaml above this include.
- The armed icon ink: `Button.icon.on` composes over `.icon` to read
  full-bright with the glyph stroking in the armed Highlight — the
  descendant rule reaches the Path content a Foreground setter cannot,
  and both rules ride after the `.icon` block so the opacity race settles
  in the armed state's favor.

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
