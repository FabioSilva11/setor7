# Setor 07 Interface Implementation Plan

> **For agentic workers:** Execute in the current confirmed project; preserve previous gameplay scenes. Use native implementation and a final independent review.

**Goal:** Apply both HTML references as editable Unity mock screens and deliver genuine rendered screenshots.
**Architecture:** Isolated uGUI presentation scene. Editor builder creates editable screen prefabs and a mock data asset. Runtime bootstrap only loads and reveals the visual prefab after layout readiness.
**Tech Stack:** Unity 6000.6, uGUI, embedded Unity UI Extensions, local licensed Lucide icons.
**Spec:** `docs/superpowers/specs/2026-11-05-interface-html-design.md`

## Constraints
- Use current HTML, not outdated review JPGs.
- No gameplay, commerce, network or reward actions.
- Preserve existing project and scenes; new startup scene is reversible through Build Settings.
- Palette #0b1014 / #f0f2eb / #839094 / #263238 / #c8f66b.

## Review focus
Loading must not dismiss before resources and layout are ready; input must be blocked while loading; mock buttons have no persistent callbacks; panels fit landscape aspect ratios; generated prefab references survive editor restart.

## Tasks
- [ ] Add loading/readiness and prefab integrity tests; observe missing presentation fail.
- [ ] Embed official UI Extensions and reuse Lucide sprite exports.
- [ ] Create native lobby and all HTML panels, serializable mock data, optional loading background.
- [ ] Generate prefab assets and startup scene; async load and layout gating.
- [ ] Run EditMode suite, PlayMode capture of each screen, inspect screenshots and review independently.

## Progress / rulings
- Confirmed target project via user reply. Work in place: target is not a Git checkout; preserve prior files and scenes.
- Layout is already specified by supplied HTML; no separate design approval needed for a faithful visual adaptation.
