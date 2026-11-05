# Solo and Gameplay Loading Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Connect Solo and JOGAR to existing Shooter through real resource loading after backing up the original project.

**Architecture:** Preserve uGUI. Bind explicit controls; persist only the loading Canvas across a Single async scene load, then start the existing host and wait for player/weapon readiness.

**Tech Stack:** Unity 6000.6.7f1, uGUI, Input System, Netcode, Unity Test Framework.

**Spec:** docs/superpowers/specs/2026-11-05-solo-loading-design.md

## Global Constraints

- Preserve original snapshot in private setor7 with tag pre-menu-integration.
- Solo available; Duo/Squad visible and inert. Preserve other menu behavior and visual assets.
- Render loading before work; reveal only when actual gameplay is ready.
- Reuse current checkout for the Unity project already open.

## Review Focus

- Repeated clicks create only one load/host.
- Spawn and weapon initialization finish after scene loading.
- Input and old lobby never leak through loading.
- Missing scene or failed connection leaves recoverable UI.
- Standalone and Android start in Interface and include Shooter.

### Task 1: Preserve baseline

- [x] Capture source and tag pre-menu-integration.
- [ ] Push fixed baseline to private setor7 and verify remote SHA.

### Task 2: Formation interaction

Files: InterfacePresentation.cs, InterfaceBuilder.cs, InterfaceGameplaySetup.cs, InterfacePresentationTests.cs.

Produces: PlayRequested event, BindInteractions(), SetLaunchInteractable(bool), explicit Button refs.

- [ ] Run actual prefab interaction test before runtime changes; expect Formation to remain closed.
- [ ] Bind/unbind idempotently, enable Solo/close, keep Duo/Squad inert.
- [ ] Upgrade prefab references without rebuilding warehouse/scene.
- [ ] Test Solo, close, inert allies, repeated binding and disabled launch.

### Task 3: Gameplay loading

Files: InterfaceBootstrap.cs, GameplayLaunchController.cs, UI asmdef, GameplayLaunchTests.cs, EditorBuildSettings.asset, Setor07AndroidBuild.cs.

Consumes: Task 2 event and launch state API.

- [ ] Add integration test: immediate loading, repeated click ignored, actual Shooter/player/weapon readiness.
- [ ] Capture RED, implement async loading and host readiness with persistent opaque Canvas.
- [ ] Add Input System UI receiver, input blocking and recoverable errors.
- [ ] Include Interface first and Shooter in build paths.
- [ ] Run EditMode and PlayMode behavior checks; expect zero failures.

### Task 4: Review and delivery

- [ ] Independent diff review, fix important findings, repeat affected checks.
- [ ] Verify formation, loading and gameplay in editor.
- [ ] Commit implementation separately, retain original snapshot/tag and report validation.
