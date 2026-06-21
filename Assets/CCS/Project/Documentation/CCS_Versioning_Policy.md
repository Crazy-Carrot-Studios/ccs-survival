# CCS Survival — Versioning Policy

**Current version:** `0.6.0`

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
