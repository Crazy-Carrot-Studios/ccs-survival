# Milestone 0.2.0 — Survival Bootstrap Scene + Empty Install Pipeline

**Version:** 0.2.0  
**Status:** Implementation milestone  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.1.0](Milestone_0.1.0_Survival_Project_Identity_Setup.md) (`v0.1.0-survival-identity`)

**Goal:** Bridge architecture documentation to a real gameplay startup pipeline without introducing gameplay debt.

---

## Architectural rule (studio-critical)

**Dependency direction is strictly downward:**

```text
Survival → Modules → Core
```

Core must **never** reference survival or gameplay modules. Survival owns bootstrap composition; modules install through survival sequencing later.

---

## Scope

### In scope

- [x] `CCS.Survival.Runtime` assembly (references `CCS.Core.Runtime` only)
- [x] Empty shell types: bootstrap, installer, diagnostics, runtime context
- [x] `PF_CCS_Survival_BootstrapRoot` prefab (host + survival bootstrap; **no** Core smoke bridge)
- [x] `SCN_CCS_Survival_Bootstrap` scene
- [x] Build Settings entry (index 1; Core bootstrap remains index 0)
- [x] Survival-owned diagnostics that read Core health without running Core smoke tests
- [x] Authority role stub on runtime context (`LocalAuthority` for single-player path)

### Explicitly out of scope

- [ ] Inventory, crafting, equipment, hotbar, UI gameplay
- [ ] Hunger/thirst or survival loops
- [ ] Networking package install
- [ ] Save/persistence implementation
- [ ] Gameplay module installers under `Assets/CCS/Modules/`
- [ ] Changes to `Assets/CCS/Framework/Core/` behavior
- [ ] Modifications to `SCN_CCS_Bootstrap` or Core smoke installers

---

## Deliverables

| Artifact | Path |
|----------|------|
| Runtime assembly | `Assets/CCS/Survival/Scripts/CCS.Survival.Runtime.asmdef` |
| Bootstrap | `Assets/CCS/Survival/Scripts/Bootstrap/CCS_SurvivalBootstrap.cs` |
| Installer shell | `Assets/CCS/Survival/Scripts/Installers/CCS_SurvivalInstaller.cs` |
| Diagnostics | `Assets/CCS/Survival/Scripts/Diagnostics/CCS_SurvivalDiagnostics.cs` |
| Runtime context | `Assets/CCS/Survival/Scripts/Runtime/CCS_SurvivalRuntimeContext.cs` |
| Bootstrap prefab | `Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |
| Bootstrap scene | `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| This milestone | `Documentation/Milestones/Milestone_0.2.0_Survival_Bootstrap_Scene_Empty_Install_Pipeline.md` |

---

## Startup pipeline (0.2.0)

```text
SCN_CCS_Survival_Bootstrap
  └── PF_CCS_Survival_BootstrapRoot
        ├── CCS_RuntimeHost (Core diagnostics OFF)
        └── CCS_SurvivalBootstrap [order 100]
              ├── CCS_SurvivalRuntimeContext.Initialize()
              ├── BootstrapRunner → CCS_SurvivalInstaller (empty)
              └── CCS_SurvivalDiagnostics (optional, survival-owned)
```

**Scene separation:**

| Scene | Purpose |
|-------|---------|
| `SCN_CCS_Bootstrap` | Core Platform validation only (smoke tests when Core diagnostics enabled) |
| `SCN_CCS_Survival_Bootstrap` | Game survival layer entry; future server/client/load entry |

---

## Diagnostics ownership

| Layer | Owner | Flag |
|-------|--------|------|
| Core smoke tests | Core bridge on `PF_CCS_RuntimeHost` | `enableRuntimeDiagnostics` on host |
| Survival health checks | `CCS_SurvivalDiagnostics` | `enableSurvivalDiagnostics` on bootstrap |

Survival bootstrap prefab keeps **Core diagnostics disabled** so Core smoke never runs in the game entry scene.

---

## Play Mode validation (expected logs)

Open `SCN_CCS_Survival_Bootstrap`, Play Mode, survival diagnostics enabled:

1. `[CCS Runtime Host] Runtime host initialized.`
2. `[CCS Survival Context] Survival runtime context initialized.`
3. `[CCS Survival Installer] Survival installer completed (empty install pipeline).`
4. `[CCS Survival Diagnostics] Core health OK. Modules=0, ...`
5. `[CCS Survival Bootstrap] Survival bootstrap completed.`

**Core bootstrap scene** (`SCN_CCS_Bootstrap`) behavior must remain unchanged when validated separately.

---

## Success criteria

- Survival assembly compiles with **only** `CCS.Core.Runtime` reference
- No gameplay modules registered after bootstrap (`RegisteredModuleCount == 0`)
- Core bootstrap scene and smoke pipeline unaffected
- Clear separation for future dedicated server/client bootstrap variants

---

## Suggested next milestone (0.3.0 — not started)

1. First gameplay module skeleton (`ccs.survival.character`) with installer registered from `CCS_SurvivalInstaller`
2. Documented manual install order list (code or ScriptableObject)
3. Optional: survival bootstrap scene as default game Play Mode entry (project policy decision)

---

## Related documents

- [Survival Gameplay Architecture](../Architecture/Survival_Gameplay_Architecture.md)
- [Survival Module Boundaries](../Architecture/Survival_Module_Boundaries.md)
- [Survival Networking Authority](../Architecture/Survival_Networking_Authority.md)
