# CCS Core Platform 0.4.0 — Release Notes

**Tag:** `v0.4.0-core-platform-baseline`  
**Author:** James Schilz  
**Date:** 2026-05-24

Phase One of the CCS Core Platform is **complete**. This release freezes a reusable baseline with no gameplay code.

---

## Completed systems

- **Runtime host** — `CCS_RuntimeHost` Unity bridge, subsystem ownership, shutdown cleanup
- **Bootstrap** — Manual installer registration and `CCS_BootstrapRunner`
- **Update loop** — Tick / FixedTick / LateTick for `CCS_IUpdatable` systems
- **Events** — `CCS_EventDispatcher` with typed subscribe/dispatch
- **Services** — `CCS_ServiceRegistry` manual registration by interface type
- **Modules** — Contracts, `CCS_ModuleBase`, installer base, registry, host, lifecycle states
- **Uninstall** — `UninstallModule` with registry unregister after successful hooks
- **Diagnostics** — `CCS_CoreDiagnosticsReport` (on-demand, read-only)
- **Validation** — `CCS_CoreValidation` centralized helpers
- **Error codes** — `CCS_CoreErrorCode` on `CCS_Result`
- **Dependencies** — `CCS_ModuleDependency` metadata + install preflight (no auto-install)
- **Install plans** — `CCS_ModuleInstallPlan` explicit ordered installs
- **Smoke tests** — Diagnostics-gated Play Mode validation in `SCN_CCS_Bootstrap`

---

## Architectural policies

| Policy | Description |
|--------|-------------|
| No singleton managers | Instance-owned subsystems only |
| No auto-discovery | No reflection or scene scanning |
| Manual registration | Installers, services, modules explicit |
| Diagnostics gating | Smoke tests only when `EnableRuntimeDiagnostics` is true |
| CCS_Result outcomes | Failures return results; no exception-driven control flow |

---

## Intentionally not included

- Gameplay modules (survival, MMO, crafting, etc.)
- Automatic module dependency resolution or install ordering
- Networked multiplayer implementation
- Editor tooling for module manifests (future game repos)

---

## Smoke test expectations

With diagnostics enabled on `PF_CCS_RuntimeHost` in `SCN_CCS_Bootstrap`:

1. Runtime smoke: init, installer, update tick
2. Module install lifecycle and registry
3. Duplicate install blocked (`ModuleAlreadyInstalled` / `Failed` duplicate instance)
4. Dependency install fails without parent; succeeds with parent
5. Install plan: duplicate entry blocked; bad dependency order blocked; successful smoke install via plan
6. Uninstall + missing/duplicate uninstall graceful failures
7. Diagnostics report after uninstall (zero registered modules)

---

## Duplicating for new games

1. **Template/fork** this repository (`ccs-framework`)
2. Keep `Assets/CCS/Framework/Core/` as the shared core — do not add gameplay there
3. Implement game modules under `Assets/CCS/Modules/` in the game project
4. Compose bootstrap installers manually per client/server/headless mode

---

## Recommended next phase

Start **game-specific** survival or MMO modules in a **separate branch or repository** forked from this baseline. Do not expand Core with gameplay systems.

---

## Version path (Phase One)

| Version | Milestone |
|---------|-----------|
| 0.3.10 | Final validation pass (pre-freeze hygiene) |
| 0.3.11 | Core diagnostics foundation |
| 0.3.12 | Core validation helpers + error codes |
| 0.3.13 | Module dependency metadata |
| 0.3.14 | Manual module install plans |
| 0.3.15 | Freeze preparation documentation |
| **0.4.0** | **Core Platform baseline freeze** |
