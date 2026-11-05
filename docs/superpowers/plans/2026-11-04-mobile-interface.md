# Mobile interface and lobby implementation plan

> **For agentic workers:** Use subagent-driven development. Independent tasks have exclusive file ownership; the main agent runs Unity validation after integration.

**Goal:** Fix the yellow movement overlay, make running usable on mobile, show the existing character in the lobby, and integrate the requested UI libraries.

**Architecture:** Preserve the approved UI Toolkit UXML/USS screens and native gameplay/network events. Render a visual-only character preview with no network, input or gameplay components. Use compatible Unity UI Extensions and a small licensed Lucide icon subset.

**Tech Stack:** Unity 6000.6.7f1, UI Toolkit, Universal RP 17.5, Unity UI Extensions, Lucide.

**Spec:** User request in this chat, approved Figma lobby and mobile controls.

## Global constraints

- Landscape only. Preserve the tested movement and look directions.
- No new inventory, matchmaking, social or combat logic.
- Use the existing character and native sprint event chain.
- Stop preview rendering when playing or when UI is disabled; release resources.
- Keep mobile rendering costs small. No paid dependencies.

## Review focus

- Moving on the joystick must never tint the full-screen look surface yellow.
- Running must work with movement and simultaneous camera input, and stop on release/focus loss.
- Character preview must not instantiate a network player or camera listener.
- Preview must survive lobby return and release its texture/camera on teardown.
- Library installation must resolve in this Unity version, with preserved icon license notices.

## Tasks

- [ ] Inspect the movement overlay and native sprint path. Add meaningful regression checks, implement mobile sprint behavior, preserve direction tests.
- [ ] Add an isolated lobby character preview, hook up lobby visibility, verify lifecycle and native host/client behavior.
- [ ] Integrate compatible UI Toolkit Extensions and Lucide icons; fix HUD clipping and pressed styles without changing approved layout.
- [ ] Inspect the diagnosed broken mobile rendering profile and repair the Android quality reference with a lightweight profile.
- [ ] Run Unity compilation and editor tests once after integration. Inspect lobby and gameplay UI visually where the installed tools permit it. Report verification and any device-validation limits accurately.
