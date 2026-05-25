# CCS Phase One — Core Platform Completion Checklist

**Version:** 0.4.0  
**Status:** Phase One frozen baseline  
**Author:** James Schilz  
**Date:** 2026-05-24

Use this checklist to confirm the Core Platform is ready as a **forever-reusable** foundation and GitHub template.

---

## Version and release

| Check | Status (0.4.0) |
|-------|----------------|
| Root `README.md` version = **0.4.0** | Required |
| `ProjectSettings.bundleVersion` = **0.4.0** | Required |
| Release notes published | [CCS_Core_Platform_0.4.0_Release_Notes.md](Releases/CCS_Core_Platform_0.4.0_Release_Notes.md) |
| Git tag `v0.4.0-core-platform-baseline` | Required after push |

---

## Architecture policy (must remain true)

| Policy | Status |
|--------|--------|
| No singleton managers | Pass |
| No auto-discovery / scene scanning | Pass |
| No gameplay modules in Core | Pass |
| Manual registration only | Pass |
| Smoke tests diagnostics-gated | Pass |
| `CCS_Result` + `CCS_CoreErrorCode` for failures | Pass |

---

## Phase One systems delivered

| System | Path / type |
|--------|-------------|
| Runtime host bridge | `CCS_RuntimeHost` |
| Bootstrap runner | `CCS_BootstrapRunner` |
| Update loop | `CCS_RuntimeUpdateLoop` |
| Event dispatcher | `CCS_EventDispatcher` |
| Service registry | `CCS_ServiceRegistry` |
| Module registry + host | `CCS_ModuleRegistry`, `CCS_ModuleHost` |
| Module installer pipeline | `CCS_ModuleInstallerBase` |
| Module lifecycle states | `CCS_ModuleLifecycleState` |
| Uninstall flow | `CCS_ModuleHost.UninstallModule` |
| Core diagnostics | `CCS_CoreDiagnosticsReport` |
| Core validation | `CCS_CoreValidation` |
| Core error codes | `CCS_CoreErrorCode` |
| Module dependencies (metadata) | `CCS_ModuleDependency` |
| Manual install plans | `CCS_ModuleInstallPlan` |
| Smoke tests | `Core/Runtime/SmokeTests/` |

---

## Intentionally not included

- Gameplay modules (survival, MMO, inventory, etc.)
- Auto-discovery or automatic module install
- Singleton global managers
- Production dependency resolution graphs (metadata + preflight only)

---

## Play Mode validation

1. Open `SCN_CCS_Bootstrap`
2. Enable **Enable Runtime Diagnostics** on `PF_CCS_RuntimeHost`
3. Enter Play Mode
4. Confirm smoke test logs per [CCS Core Platform Architecture](CCS_Core_Platform_Architecture.md) (0.4.0 table)
5. No exceptions in Console

---

## Git hygiene

| Check | Notes |
|-------|-------|
| Do not commit `Library/`, `UserSettings/` | `.gitignore` |
| Do not commit incidental scene/RPM churn | Local Unity edits only |
| Working tree clean after milestone commits | Except intentional local churn |

---

## Template / fork next steps

1. Mark repository as GitHub template (optional)
2. Fork or duplicate for a new game project
3. Add gameplay under `Assets/CCS/Modules/` in the **game repo**, not Core
4. Keep `ccs-framework` as upstream for Core fixes

**Phase One is complete at 0.4.0.**
