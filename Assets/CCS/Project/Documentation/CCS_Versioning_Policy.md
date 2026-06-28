# CCS Survival — Versioning Policy

**Current version:** `0.7.1d`

## Purpose

After the controlled hard reset, `ccs-survival` uses a fresh **0.x.x rebuild scheme**. All `0.x.x` versions are internal beta/prototype/rebuild milestones. **`1.0.0` is reserved for the first public alpha-ready release** and must not be used until a controlled playable alpha slice exists.

**Note:** The original phase map below is a planning guide. The project also uses **pragmatic patch tags** on the current integration line (e.g. `0.5.4` Interaction, `0.5.5` audit) when multiple modules ship together on `main`.

---

## Version map (planning guide)

| Range | Phase | Scope |
|-------|-------|-------|
| **0.0.x** | Project architecture | Folder cleanup, rebuild planning, versioning baseline |
| **0.1.x** | Architecture normalization | Folder ownership, module placeholders, bootstrap baseline |
| **0.2.x** | Character | Character controller + camera |
| **0.3.x** | Attributes | Attribute model, health, replication |
| **0.4.x** | Character polish | Locomotion, stamina, netcode harness refinements |
| **0.5.x** | Integrated test slice | Character visuals + interaction integration on Master Test |
| **0.6.x+** | Next modules | Inventory, equipment, persistence (when implemented) |
| **0.9.x** | Vertical slice | First controlled playable survival loop |
| **1.0.0** | Public alpha | First alpha-ready release (not before 0.9.x criteria met) |

### Current integration line (actual tags)

| Version | Milestone |
|---------|-----------|
| `0.5.0`–`0.5.3` | Character visual / locomotion integration |
| **`0.5.4`** | **Interaction Pickup and Door Flow** |
| **`0.5.5`** | **Project audit and interaction cleanup** |
| **`0.5.6`** | **Isolate player animation clips** |
| **`0.6.0`** | **Revolver shooting foundation** |
| **`0.6.7`** | **Equipment Fit Studio foundation** |
| **`0.6.8`** | **Revolver fit profile tuning + Fit Studio UI polish + fire visuals** |
| **`0.6.9`** | **Third-person default + first-person weapon aim** |
| **`0.6.10`** | **Wild West one-handed revolver aim default** |
| **`0.6.11`** | **Hard replace two-handed revolver aim with Wild West one-handed** |
| **`0.6.12`** | **BodyAware-only first-person aim camera** |
| **`0.6.14`** | **Local self head layer mask for separated head renderers** |
| **`0.6.15`** | **Local self headless first-person body fallback (combined CC3 body)** |
| **`0.6.16`** | **Simplified third-person revolver aim cleanup** |
| **`0.7.0`** | **Network AI bandit combat foundation** |
| **`0.7.1b`** | **Character Controller cleanup plan (Phase 1, documentation only)** |
| **`0.7.1d`** | **Testing Manager foundation + editor menu reduction (Phase 2B)** |
| **`0.7.1c`** | **Remove Animation Fit Studio tooling (Phase 2A, editor/docs only)** |
| **`0.7.1a`** | **AI health bar fill direction hotfix** |
| **`0.7.1`** | **AI bandit polish and hosting fixes** |

---

## Semver rules (game repo)

| Component | Meaning |
|-----------|---------|
| **Major** (`1.x.x`) | Public release tier. `1.0.0` = first alpha. |
| **Minor** (`0.N.x`) | Module/rebuild phase (see version map). |
| **Patch** (`0.N.P`) | Incremental safe milestone or hotfix **within** the current phase |

### Patch examples (0.2.x character phase)

| Version | Milestone |
|---------|-----------|
| `0.2.0` | Character controller foundation |
| `0.2.1` | Character test prefab |
| `0.2.2` | Camera/input polish |
| `0.2.3` | Character validation/build fix |

Each gameplay module must include a **working test prefab** before advancing to the next minor phase.

---

## Unity alignment

- Set `ProjectSettings` → Player → **Version** (`bundleVersion`) to match the tagged milestone.
- README **Current Project Version** must match `bundleVersion` at each release tag.

---

## Git tag policy

### Tag format

```text
v<major>.<minor>.<patch>
```

Examples: `v0.0.3`, `v0.1.0`, `v0.1.1`, `v0.2.0`, `v0.2.1`, `v0.2.2`

### Rules

1. Tag **only** `main` at a validated, committed milestone.
2. One tag per released version — do not move or reuse tags.
3. Hotfixes increment **patch** within the current minor phase (`v0.2.1`, `v0.2.2`).
4. Advancing to a new module phase increments **minor** and resets patch to `0` (`v0.3.0`).
5. **Do not reuse** tags from the pre-reset timeline (`v1.x`–`v5.x` and legacy `v0.3.5a` scheme). Those belong to `archive/full-survival-before-hard-reset` only.
6. **`v1.0.0` is blocked** until alpha criteria are documented and met in the `0.9.x` vertical slice.

### Alpha gate (`1.0.0` — future)

Do not tag `v1.0.0` until all of the following are true:

- Controlled playable survival vertical slice complete (`0.9.x`)
- Bootstrap + core modules compile cleanly with zero console errors
- Documented test prefab per integrated module
- Manual playtest pass recorded for the alpha slice scope

---

## Release history

### `0.7.1d` — Testing Manager and Editor Menu Reduction (Phase 2B)

- Added `CCS_CharacterControllerTestingManager` as central Master Test debug switchboard
- Removed runtime OnGUI overlays from production animation/camera scripts; moved diagnostics to Tests-only reporters
- Removed obsolete editor menu wrappers (Master Test setup, hosting setup, camera presets); batch entries remain
- No gameplay behavior changes; player prefab cleanup deferred

### `0.7.1c` — Remove Animation Fit Studio Tooling (Phase 2A)

- Removed entire `Editor/AnimationFitStudio/` stack and obsolete FullDraw nudge menu/batch hook
- Consolidated Character Controller documentation; removed temporary Phase 01 cleanup plan
- Redirected `CCS_AnimationInventoryReporter` output to `Logs/CharacterController/AnimationInventory/`
- Editor/documentation cleanup only — no gameplay behavior changes, no prefab/controller/clip edits

### `0.7.1b` — Character Controller Cleanup Plan (Phase 1)

- Added temporary Phase 01 cleanup plan documenting Animation Fit Studio removal scope, editor menu reduction, Testing Manager direction, and runtime script classification
- Documentation-only milestone — no gameplay behavior changes, no script deletions, no prefab/controller/clip edits

### `0.7.1a` — AI Health Bar Fill Direction Hotfix

- Fixed AI bandit world-space health bar to drain left-to-right from the player camera view
- Switched health fill to right-anchored rect layout with `Image` horizontal fill origin on the right
- Added validation for fill direction, fill amount clamping, and non-mirrored nameplate scale

### `0.7.1` — AI Bandit Polish and Hosting Fixes

- Fixed `AI_Bandit` nameplate layout (health bar above name), camera-facing billboard, and legacy player nameplate removal
- Added Master Test NavMesh surface (`CCS_AINavigationRoot`) and `NavMeshAgent` on AI bandit prefab
- Fixed `CCS_NetworkHealth` offline initialization and `IsDamageReady` spawn-safe damage gating
- Extended `CCS_NetworkPrefabReferenceGuard` with AI bandit fallback repair and stale entry removal
- Moved ambient playlist (`CCS Western Game 2`, `CCS_Western_Theme 7`) to hosting scene only; disabled Master Test gameplay music

### `0.7.0` — Network AI Bandit Combat Foundation

- Added `Assets/CCS/Modules/AI` runtime/editor/documentation/content scaffolding and asmdefs
- Added AI bandit state machine, sensing, motor, weapon firing, controller, nameplate, and master-test spawner
- Added shared combat contracts (`CCS_IDamageable`, `CCS_DamageInfo`, `CCS_DamageSourceType`) and replicated `CCS_NetworkHealth`
- Registered AI prefab path in netcode required prefab list and setup utility flow

### `0.5.5` — Project Audit and Interaction Cleanup

- Documentation aligned to v0.5.4 interaction flow (removed toggle-cube references)
- Runtime debug log cleanup; manual interaction animation test path removed
- Project-level audit validator (`CCS → Project → Run Project Audit`)
- Documented intentional CharacterController → Interaction/Attributes test-player coupling

### `0.5.4` — Interaction Pickup and Door Flow

- Pickup and WalkThroughDoor interactable kinds with executor routing
- Forward interaction volume and closest-point line-of-sight validation
- Press [E] prompt when interaction-ready; movement lock during animations
- PickUp_RH and WalkThroughDoor_RH animator triggers
- Master Test pickup cube and building door targets

### `0.2.0` — Character Controller Foundation

- First rebuilt gameplay module: `Assets/CCS/Modules/CharacterController/`
- Module-owned Input Actions, movement/camera profiles, test prefab, debug HUD, validation
- Cinemachine 3.1 third-person camera foundation
- No test scene included (manual test scene step deferred)

### `0.1.1` — Historical Documentation Cleanup

- Removed pre-reset `Documentation/Milestones/` records with stale `Assets/CCS/Survival/` paths
- Removed superseded repo-level architecture duplicates
- Retained repo-level networking and persistence direction docs under `Documentation/Architecture/`
- Active architecture rules remain in `Assets/CCS/Project/Documentation/`

### `0.1.0` — Architecture Normalization

- Target structure: `Framework/`, `Modules/`, `Shared/`, `Project/`, `Tests/`
- `Assets/CCS/Project/` owns bootstrap, composition, scenes, and project documentation
- Module placeholders use `Content/`, `Profiles/`, `Documentation/` subfolders
- Assembly: `CCS.Project.Runtime` / namespace `CCS.Project`
- No global `Database/` folder

### `0.0.3` — Controlled Rebuild Baseline

- Framework baseline restored from archive reference
- Versioning policy established for controlled module rebuild

---

## Related

- [CCS Reset Notice](../../../Documentation/CCS_Reset_Notice.md)
- [Future Gameplay Module Guidelines](../../../Documentation/Planning/Future_Gameplay_Module_Guidelines.md)
- [README.md](../../../README.md)
