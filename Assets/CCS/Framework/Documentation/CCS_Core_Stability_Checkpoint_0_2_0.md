# CCS Core Framework — Stability Checkpoint 0.2.0

**Version:** 0.2.0  
**Status:** First stable architecture checkpoint for CCS Core runtime foundation  
**Author:** James Schilz  
**Date:** 2026-05-24

This document marks the current CCS Core runtime foundation as the first stable architecture checkpoint. Milestone 0.2.0 is documentation and validation only — no new runtime systems were introduced.

**Note (0.3.0):** Checkpoint **0.2.0** remains the stable Core runtime foundation. Milestone **0.3.0** begins module contract architecture (`CCS_IModule`, installers, metadata) on top of this stable base without changing the 0.2.0 runtime checkpoint scope.

---

## Stable foundation summary

The following Core runtime capabilities are established and considered stable at **0.2.0**:

| Capability | Location / type | Status |
|------------|-----------------|--------|
| **Runtime host** | `CCS_RuntimeHost` — production-safe Unity bridge | Stable |
| **Diagnostics toggle** | `CCS_RuntimeHost.EnableRuntimeDiagnostics` | Stable |
| **Smoke test bridge** | `CCS_RuntimeSmokeTestBridge` — diagnostics-gated | Stable |
| **Service registry** | `CCS_IServiceRegistry` / `CCS_ServiceRegistry` | Stable |
| **Event dispatcher** | `CCS_IEventDispatcher` / `CCS_EventDispatcher` | Stable |
| **Runtime update loop** | `CCS_RuntimeUpdateLoop` | Stable |
| **Bootstrap runner** | `CCS_IBootstrapInstaller` / `CCS_BootstrapRunner` | Stable |
| **Bootstrap scene** | `SCN_CCS_Bootstrap` (Build Settings index 0) | Stable |
| **Runtime host prefab** | `PF_CCS_RuntimeHost` | Stable |
| **Git hygiene** | `.gitignore`, no generated Unity cache in repo | Established |
| **Unity metadata workflow** | Sibling `.meta` files committed when Unity generates them | Established |

---

## Verified Play Mode pipeline

Play Mode validation uses `SCN_CCS_Bootstrap` with `PF_CCS_RuntimeHost` (and optional `CCS_RuntimeSmokeTestBridge` on the same GameObject).

### Execution flow

When runtime diagnostics are **enabled** on `CCS_RuntimeHost`:

```
RuntimeHost (Awake)
  → BootstrapRunner
    → SmokeTestInstaller (via CCS_RuntimeSmokeTestBridge)
      → RuntimeUpdateLoop (registers smoke test system)
        → Tick (Update / FixedUpdate / LateUpdate)
          → Clean Shutdown (OnDestroy)
```

### Step-by-step

1. **RuntimeHost** — `CCS_RuntimeHost.Awake` creates `CCS_RuntimeUpdateLoop`, `CCS_ServiceRegistry`, `CCS_EventDispatcher`, and `CCS_BootstrapRunner`. No validation logic runs inside the host.
2. **Diagnostics gate** — `CCS_RuntimeSmokeTestBridge` (execution order 100) checks `EnableRuntimeDiagnostics`. If false, the bridge returns silently.
3. **BootstrapRunner** — When diagnostics are enabled, the bridge registers `CCS_RuntimeSmokeTestInstaller` and calls `BootstrapRunner.Run(runtimeHost)`.
4. **SmokeTestInstaller** — Registers `CCS_RuntimeSmokeTestSystem` on the host’s update loop.
5. **RuntimeUpdateLoop** — Unity lifecycle drives `Tick`, `FixedTick`, and `LateTick` for registered updatables.
6. **Tick** — Smoke test system confirms one update-loop tick during Play Mode.
7. **Clean Shutdown** — `OnDestroy` clears update loop, service registry, event dispatcher, and bootstrap runner.

### Expected diagnostics logs (enabled)

When `enableRuntimeDiagnostics` is **true** and the smoke test bridge has `enableDebugLogs` enabled:

```text
[CCS SmokeTest] Runtime smoke test initialized
[CCS SmokeTest] Smoke test installer completed
[CCS SmokeTest] Runtime update loop tick confirmed
```

### Expected behavior (diagnostics disabled)

When `enableRuntimeDiagnostics` is **false**:

- No smoke test logs
- No Console errors
- Production-safe runtime host behavior unchanged

---

## Production boundary rules

These rules apply to all work after checkpoint **0.2.0**:

1. **`CCS_RuntimeHost` must remain production-safe** — Orchestration and subsystem exposure only. No smoke tests, no gameplay logic, no validation orchestration inside the host.
2. **Smoke tests must stay diagnostics-gated** — Validation runs only through `CCS_RuntimeSmokeTestBridge` when `EnableRuntimeDiagnostics` is true.
3. **Validation systems must not pollute core production logic** — Diagnostic bridges are optional MonoBehaviour layers, not part of the production host contract.
4. **Future gameplay systems** must install through established contracts:
   - **Bootstrap** — `CCS_IBootstrapInstaller` registered with `CCS_BootstrapRunner`
   - **Services** — `CCS_IService` via `CCS_ServiceRegistry`
   - **Events** — `CCS_IEvent` via `CCS_EventDispatcher`
   - **Update** — `CCS_IUpdatable` / `CCS_IFixedUpdatable` / `CCS_ILateUpdatable` via `CCS_RuntimeUpdateLoop`

---

## Validation checklist (0.2.0)

- [ ] Unity compiles without errors
- [ ] Open `SCN_CCS_Bootstrap`
- [ ] Play Mode with diagnostics **enabled** — three smoke test logs, no errors
- [ ] Play Mode with diagnostics **disabled** — no smoke logs, no errors
- [ ] `git status --short` clean after milestone commit

---

## Related documentation

- [CCS Script Standards](CCS_Script_Standards.md)
- [Framework Documentation README](README.md)
- Root [README.md](../../../README.md)
