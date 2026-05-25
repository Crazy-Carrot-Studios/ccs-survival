# CCS Core Platform — Template Readiness Checklist

**Version:** 0.4.0  
**Status:** Phase One frozen — template-ready baseline  
**Author:** James Schilz  
**Date:** 2026-05-24

Use this checklist before publishing the CCS repository as a **GitHub template** or before branching a new game project (survival, MMO, or other genres) from the Core Platform baseline.

**Authoritative baseline:** [CCS Core Platform Architecture](CCS_Core_Platform_Architecture.md) (0.4.0)

**Phase One completion:** [CCS Phase One Core Platform Completion Checklist](CCS_Phase_One_Core_Platform_Completion_Checklist.md)

---

## Template scope

| In scope (template Core) | Out of scope (game projects add later) |
|--------------------------|----------------------------------------|
| `Assets/CCS/Framework/Core/` | Gameplay module implementations |
| `Assets/CCS/Framework/Documentation/` | Inventory, character controller, crafting, etc. |
| `Assets/CCS/Framework/Tests/` roots | Game-specific content under `Assets/CCS/Modules/` |
| Bootstrap scene + runtime host prefab | Production game scenes beyond bootstrap validation |

---

## Folder structure cleanliness

| Check | Status (0.3.9) | Notes |
|-------|----------------|-------|
| Core runtime code under `Framework/Core/Runtime/` | Pass | Single `CCS.Core.Runtime` assembly area |
| Core docs under `Framework/Core/Documentation/` | Pass | Platform architecture + this checklist |
| Framework standards under `Framework/Documentation/` | Pass | Script standards, stability checkpoint |
| Smoke tests under `Core/Runtime/SmokeTests/` only | Pass | No smoke tests in production host code |
| Module contracts under `Core/Runtime/Modules/` | Pass | Interfaces, base, registry, host |
| No duplicate legacy test trees | Pass | Authoritative smoke path is `Core/Runtime/SmokeTests/` |
| Placeholder `Assets/CCS/Modules/` folders exist but contain **no** `.cs` gameplay code | Pass | Folders are structural placeholders for future games |

---

## Architecture policy checks

| Check | Status (0.3.9) | Notes |
|-------|----------------|-------|
| **No singleton managers** | Pass | Subsystems are instance-owned by `CCS_RuntimeHost` |
| **No auto-discovery** | Pass | No reflection scanning or scene-wide installer discovery |
| **No gameplay content in Core** | Pass | All Core `.cs` files are framework infrastructure only |
| **Manual module registration** | Pass | `CCS_ModuleRegistry` + explicit installers |
| **Manual service registration** | Pass | `CCS_ServiceRegistry` by interface type |
| **CCS_Result for mutating operations** | Pass | Registry/install/uninstall use `CCS_Result` |
| **Diagnostics gated** | Pass | `EnableRuntimeDiagnostics` on runtime host |

---

## Smoke test isolation

| Check | Status (0.3.9) | Notes |
|-------|----------------|-------|
| Smoke tests compile inside `CCS.Core.Runtime` | Pass | Required for host/installer references |
| `CCS_RuntimeSmokeTestBridge` is optional MonoBehaviour | Pass | Not part of production host contract |
| Bridge respects `EnableRuntimeDiagnostics` | Pass | No logs when diagnostics disabled |
| Module smoke uses `CCS_ModuleInstallerBase` | Pass | Install, duplicate preflight, uninstall validated |
| Smoke namespace `CCS.Core.Tests` | Pass | Clear test-only boundary |
| Production `CCS_RuntimeHost` has no smoke orchestration | Pass | Validation-agnostic host |

---

## Documentation readiness

| Check | Status (0.3.9) | Notes |
|-------|----------------|-------|
| Core Platform architecture documented | Pass | `CCS_Core_Platform_Architecture.md` |
| Script standards documented | Pass | `Framework/Documentation/CCS_Script_Standards.md` |
| Root README points to Core baseline | Pass | Version synced with Unity Player |
| Framework README links Core docs | Pass | Cross-links to Core Documentation |
| Install / duplicate preflight / uninstall documented | Pass | In platform architecture doc |
| Stability checkpoint 0.2.0 preserved | Pass | Historical runtime checkpoint reference |

---

## Versioning and Git hygiene

| Check | Status (0.3.9) | Notes |
|-------|----------------|-------|
| Framework version in root `README.md` | Pass | Must match milestone (0.3.9) |
| Unity `bundleVersion` synced | Pass | `ProjectSettings/ProjectSettings.asset` |
| `.gitignore` excludes `Library/`, `Temp/`, `UserSettings/` | Pass | Standard Unity template |
| `.gitignore` excludes `.vscode/`, `*.csproj`, `*.sln` | Pass | Local IDE churn ignored |
| **Do not commit** local Unity scene/asset churn | Required | e.g. `SCN_CCS_Bootstrap.unity`, `Mobile_RPAsset.asset`, `SceneTemplateSettings.json` unless intentionally changed for the template |
| Unity `.meta` GUIDs are **32** lowercase hex characters | Required | Invalid GUIDs break compilation |

---

## Play Mode validation (template baseline)

Run before tagging a template release:

1. Open `SCN_CCS_Bootstrap` with diagnostics **enabled** on `PF_CCS_RuntimeHost`.
2. Confirm install succeeds (module + runtime smoke).
3. Confirm duplicate install blocked **before** installer hooks (preflight only).
4. Confirm uninstall clears registry and lifecycle is `Uninstalled`.
5. Confirm missing and duplicate uninstall fail gracefully (warnings, no exceptions).
6. Disable diagnostics — confirm **no** smoke logs and no errors.
7. Exit Play Mode — confirm clean subsystem shutdown logs.

---

## GitHub template duplication path

When using this repo as a **GitHub template**:

1. Click **Use this template** → Create new repository.
2. Clone the new repository (do not develop directly on the template repo).
3. Rename Unity product / company fields as needed.
4. Set `bundleVersion` and README framework version for the new game’s first milestone.
5. Keep `Assets/CCS/Framework/Core/` intact as the shared platform baseline.
6. Add game modules under `Assets/CCS/Modules/` or `Assets/CCS/Framework/Modules/` per game architecture plan.
7. Wire game installers into bootstrap explicitly (manual registration).
8. Keep smoke tests behind diagnostics or move to a dedicated test assembly in the game repo if desired.

---

## Future extraction path (other games / survival / MMO)

The 0.3.9 Core Platform is designed so game projects can:

1. **Branch from template** — New repo inherits Core + documentation baseline.
2. **Add modules incrementally** — Each module: `CCS_ModuleBase` + `CCS_ModuleInstallerBase` + manual registry.
3. **Share platform updates** — Cherry-pick or subtree-merge `Framework/Core/` improvements back from the canonical CCS framework repo.
4. **Scale to MMO** — Instance-owned registries and hosts support future server/client/host contexts without global singletons (dedicated hosts per context in later milestones).
5. **Survival / simulation projects** — Same bootstrap and module pipeline; gameplay systems stay out of Core.

Recommended game-repo layout after branch:

```text
Assets/CCS/
  Framework/Core/          ← platform baseline (from template)
  Framework/Documentation/
  Framework/Modules/       ← optional shared game framework modules
  Modules/                 ← game feature modules (inventory, networking, etc.)
  Shared/                  ← game assets
```

---

## Pre-publish checklist (summary)

- [ ] All items in this document reviewed for current milestone
- [ ] `git status --short` shows no unintended Unity local churn
- [ ] Play Mode validation passed with diagnostics on and off
- [ ] Root README and `bundleVersion` = **0.3.9**
- [ ] [CCS Core Platform Architecture](CCS_Core_Platform_Architecture.md) reviewed
- [ ] Template repository description and topics set on GitHub (optional)
- [ ] Default branch is `main` and clean

---

## Sign-off

| Field | Value |
|-------|-------|
| Milestone | 0.3.9 — Core Template Readiness Pass |
| Core Platform baseline | 0.3.8 architecture + 0.3.9 template checklist |
| Gameplay modules in Core | None |
| Ready for template use | After checklist + Play Mode validation complete |
