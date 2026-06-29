# CCS Character Controller — Player Prefab Hierarchy Architecture

**Version:** 0.7.5 (Phase 3D — planning only)  
**Author:** James Schilz  
**Last updated:** 2026-06-25  
**Baseline:** v0.7.4 (`b3e8eef`) — animation rebuild architecture; v0.7.3 locomotion-only Animator preserved

## Purpose

Define the production-grade target hierarchy and script ownership for `PF_CCS_CharacterController_Player_Networked` before any hierarchy migration. This milestone is **architecture, documentation, audit contracts, and validation policy only**.

**Not in v0.7.5:** prefab hierarchy changes, component moves, `PF_CCS_Player_Visual` edits, animation import, CC4 import, Animator Controller rebuild, or gameplay logic changes.

## Canonical prefab

| Asset | Path |
|-------|------|
| Networked player | `Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab` |
| Player visual (nested today) | `Characters/Player/Prefabs/PF_CCS_Player_Visual.prefab` |
| Validation scene | `Scenes/Validation/SCN_CCS_CharacterController_Validation.unity` |

---

## Current problems (v0.7.4 baseline)

1. **Root script count is too high.** The networked player root carries **24 MonoBehaviours** plus `CharacterController`. This exceeds the future production budget and makes prefab review, Netcode ownership reasoning, and safe migration difficult.

2. **Root mixes unrelated concerns.** Core movement, Netcode authority, attributes, weapons bridges, camera, local UI, equipment, and presentation all share the root GameObject.

3. **Owner-only UI is embedded on every spawned player.** `AttributeHudRoot`, `InteractionPromptHudRoot`, and `WeaponHudRoot` are always present on the networked prefab. Future work should spawn or enable these only for the local owner.

4. **Double model nesting.** Current structure is `VisualRoot` → nested `PF_CCS_Player_Visual`. This creates two presentation roots and obscures the single CC4 swap point.

5. **Visual/presentation scripts are mixed with gameplay scripts.** Locomotion animator, interaction animator, revolver IK, and body aim follow live on `VisualRoot` while many gameplay bridges remain on root.

6. **External module bridges are not grouped as integrations.** Weapons (`CCS_RevolverController`, `CCS_PlayerWeaponLoadout`, `CCS_WeaponCarryStateController`, `CCS_PlayerEquipmentVisualController`), Attributes (`CCS_AttributeContainer`, `CCS_NetworkHealth`, …), and Interaction (`CCS_NetworkInteractionScanner`) appear as flat root components rather than explicit subsystem groups.

7. **Future CC4 import needs one model replacement point.** The current `VisualRoot` + nested visual prefab pattern must converge to a single `Model` / `ModelRoot` swap point without another big-bang rewrite.

---

## Target final hierarchy

Preferred future structure for `PF_CCS_CharacterController_Player_Networked`:

```text
PF_CCS_CharacterController_Player_Networked
├── AuthorityRoot / root components (minimal)
├── Systems
│   ├── Input
│   ├── Movement
│   ├── Camera
│   ├── Attributes
│   ├── Interaction
│   ├── Weapons
│   ├── Equipment
│   └── Presentation
├── CameraRig
│   ├── ThirdPersonFollowTarget
│   ├── FirstPersonAnchor
│   └── AimTarget
├── Model
│   └── CC4 / CC3 character prefab instance
├── EquipmentSockets
│   └── optional non-bone socket aliases if needed
├── WeaponMounts
│   ├── MuzzlePoint
│   ├── RightHandWeaponMount
│   └── LeftHandWeaponMount
├── Interaction
│   └── InteractionScanOrigin
├── LocalOnly
│   ├── LocalHudAnchor
│   ├── LocalReticleAnchor
│   └── LocalDebugHiddenByDefault
└── WorldPresentation
    └── NameplateRoot
```

### Model root rule (critical)

**Do not keep both:**

```text
VisualRoot
└── PF_CCS_Player_Visual
```

Final model structure must have **exactly one** model/presentation root:

```text
Model
└── CC3_Base_Plus / CC4 root / Animator / RigBuilder / mesh hierarchy
```

or:

```text
ModelRoot   (prefab instance root — single swap point for CC4)
```

`Model` / `ModelRoot` is the only approved swap point for CC4 replacement. Internal contents of `PF_CCS_Player_Visual` remain unchanged until a dedicated visual milestone approves edits.

---

## Root component rules

Root should contain only components that **truly require root placement** (Unity physics, Netcode `NetworkObject` co-location, or proven authority constraints).

### Target A — ideal root budget

| Component | Reason |
|-----------|--------|
| `Transform` | Required |
| `CharacterController` | Unity movement collider on authority object |
| `NetworkObject` | Netcode spawn identity |
| `CCS_ClientOwnerNetworkTransform` or `NetworkTransform` | Owner-synced transform |
| `CCS_NetworkPlayerController` | Owner gating for input/camera |
| `CCS_PlayerRuntimeFacade` / `CCS_PlayerCompositionRoot` | Future composition hub (read-only references) |
| `CCS_CharacterMotor` | Only if movement authority must remain on root |

**Ideal target:** ~6 MonoBehaviours (+ `CharacterController`), excluding future facade if Netcode batching proves unsafe.

### Target B — realistic Netcode-safe root

Root may retain **all `NetworkBehaviour` components that must share the root `NetworkObject`** until each has a validated independent `NetworkObject` strategy.

**Observed root `NetworkBehaviour` candidates (v0.7.4):**

- `CCS_ClientOwnerNetworkTransform`
- `CCS_NetworkPlayerController`
- `CCS_NetworkPlayerNameplate` (may move to `WorldPresentation` child if replication strategy allows)
- `CCS_NetworkInteractionScanner`
- `CCS_NetworkAttributeReplicator`
- `CCS_NetworkHealth`

**Rule:** Never move a `NetworkBehaviour` off the root unless it has its own correct `NetworkObject` strategy **and** hosting/spawn batches prove network spawn/ownership still works.

All **non-`NetworkBehaviour`** systems should migrate under `Systems/` children over time (v0.7.7+).

---

## Subsystem ownership (desired end state)

### Root / Authority

- `CharacterController`
- `NetworkObject`
- `CCS_ClientOwnerNetworkTransform`
- `CCS_NetworkPlayerController`
- Required root `NetworkBehaviour` components (until individually validated for child placement)
- Future `CCS_PlayerRuntimeFacade` / `CCS_PlayerCompositionRoot`

### Systems / Input

- `CCS_CharacterInputActionProvider`

### Systems / Movement

- `CCS_CharacterMotor`
- `CCS_CharacterControllerService`
- `CCS_CharacterAimLocomotionController`

### Systems / Camera

- `CCS_CharacterCameraController`
- `CCS_LocalFirstPersonHeadVisibility`
- `CCS_FirstPersonBodyCameraAnchor`
- `CCS_CharacterCameraFollowAnchor` (if it remains a component; today on `CameraFollowAnchor`)

### Systems / Attributes

- `CCS_AttributeContainer`
- `CCS_AttributeService`
- `CCS_NetworkAttributeReplicator`
- `CCS_StaminaController`
- `CCS_HealthRegenController`
- `CCS_NetworkHealth`
- `CCS_PlayerDeathScreenController` (if kept as player-local bridge)

### Systems / Interaction

- `CCS_NetworkInteractionScanner`
- Interaction target/source adapters
- `CCS_InteractionScanOriginGizmo` (editor/debug aid on `InteractionScanOrigin`)

### Systems / Weapons

- `CCS_RevolverController`
- `CCS_PlayerWeaponLoadout`
- `CCS_WeaponCarryStateController`
- `CCS_RevolverFireFeedback` (on `MuzzlePoint` / weapon mount)
- Weapon fire/reticle bridges

### Systems / Equipment

- `CCS_EquipmentSocketRegistry`
- `CCS_PlayerEquipmentVisualController`
- Bone/socket anchors (`CCS_EquipmentSocketAnchor` under rig)

### Model (presentation rig)

- `CCS_PlayerLocomotionAnimator`
- `CCS_PlayerInteractionAnimator`
- `CCS_RevolverArmReticleIK`
- `CCS_RevolverBodyAimFollowController`
- `Animator`, `RigBuilder`, IK constraints, `SkinnedMeshRenderer`s
- Model-local rig/pose helpers

### WorldPresentation

- `CCS_NetworkPlayerNameplate`
- `CCS_PlayerNameplateBillboard`
- World-space nameplate objects only (`NameplateRoot`)

### LocalOnly UI (owner-only future)

- `AttributeHudRoot` + `CCS_PlayerAttributeBarsHud`
- `InteractionPromptHudRoot` + `CCS_InteractionPromptPresenter`
- `WeaponHudRoot` + `CCS_RevolverHudPresenter`, `CCS_MuzzleDrivenReticleController`
- Local reticle UI
- Local-only debug widgets

**Policy:** Local-only HUD must not remain always-active on every network-spawned player. Future milestone (v0.7.9) moves to owner-only spawn/enable flow.

---

## Script architecture policy

1. Core gameplay scripts must not depend on visual hierarchy **names** (no `transform.Find("VisualRoot/...")` in runtime hot paths).
2. Visual scripts receive **serialized references** or a single composition context (`CCS_IPlayerCompositionRoot`).
3. Avoid `GetComponent` chains in `Update` / `FixedUpdate` / `LateUpdate`.
4. Cache references in `Awake` / `OnEnable`.
5. Prefer serialized references resolved by prefab builder/validator.
6. Avoid string hierarchy lookups except in **editor** builders/validators.
7. Runtime scripts must not know editor validation paths.
8. External module bridges must be explicit, grouped, and documented as integrations.
9. Owner-only systems must be gated by network ownership (`IsOwner`, `CCS_NetworkPlayerController`, or dedicated local bootstrap).
10. Presentation must be replaceable without changing gameplay contracts.
11. The model root must be swappable for CC4 (single `Model` / `ModelRoot` instance).
12. UI should be local-owner only unless intentionally world-space (nameplates).

---

## Future facade / composition root

**Status:** Interface-only in v0.7.5 (`CCS_IPlayerCompositionRoot`). No concrete facade wired on prefab.

### Purpose

- Central reference hub for player subsystems
- Exposes read-only references for builders and runtime consumers
- Validates required subsystem references (future validator)
- Avoids broad `GetComponent` usage across the prefab
- Separates root authority from child systems
- Helps prefab builders wire references after hierarchy migration

### Candidate name

- `CCS_PlayerRuntimeFacade` (runtime-facing), or
- `CCS_PlayerCompositionRoot` (composition/wiring emphasis)

### Potential references (full facade — post-migration)

| Reference | Expected type |
|-----------|---------------|
| Character collider | `CharacterController` |
| Netcode identity | `NetworkObject` |
| Input | `CCS_CharacterInputActionProvider` |
| Motor | `CCS_CharacterMotor` |
| Camera | `CCS_CharacterCameraController` |
| Attributes | `CCS_AttributeContainer` |
| Health | `CCS_NetworkHealth` |
| Interaction | `CCS_NetworkInteractionScanner` |
| Revolver | `CCS_RevolverController` |
| Equipment | `CCS_EquipmentSocketRegistry` |
| Model animator | `Animator` |
| Camera anchors | `Transform` (follow, FP, aim) |
| UI anchors | `Transform` (HUD, reticle) |
| Weapon mounts | `Transform` (muzzle, hands) |
| Model root | `Transform` |

v0.7.5 interface includes Character Controller module types and `Transform` anchors only; Weapons/Attributes concrete types will be added when asmdef and migration milestones allow.

---

## Builder and validator strategy (future)

Migration must be **builder-assisted**, not hand-edited only.

| Tool | Path | Status |
|------|------|--------|
| Hierarchy migration builder | `Editor/Builders/CCS_PlayerPrefabHierarchyMigrationBuilder.cs` | v0.7.6+ |
| Hierarchy validation utility | `Editor/Validation/CCS_PlayerPrefabHierarchyValidationUtility.cs` | v0.7.6+ |

### Future validation failures (policy — not enforced in v0.7.5)

- Duplicate `VisualRoot` / `PF_CCS_Player_Visual` nesting remains after implementation milestone
- Required subsystem groups missing (`Systems/Input`, `Systems/Movement`, …)
- Missing scripts > 0
- `NetworkManager` player prefab reference invalid
- Owner-only UI active for non-owner players
- Required references on composition root missing
- Model root missing `Animator`
- Model root missing `RigBuilder` when expected
- Root script count exceeds approved budget without documented exception
- External module bridge components not in expected subsystem group
- `CharacterController/Tests` folder returns
- Animation Fit Studio returns
- Equipment Fit Studio missing

v0.7.5 creates this policy only. **No hierarchy enforcement** until v0.7.6 validator foundation.

---

## Staged implementation roadmap

### v0.7.5 — architecture plan only (this milestone)

- Document target hierarchy, root budgets, subsystem ownership, script policy
- Generate hierarchy audit and architecture reports
- Add `CCS_IPlayerCompositionRoot` interface contract
- **No** prefab hierarchy changes, component moves, or visual prefab edits

### v0.7.6 — builder/validator foundation

- Create hierarchy validator (`CCS_PlayerPrefabHierarchyValidationUtility`)
- Create dry-run migration builder (`CCS_PlayerPrefabHierarchyMigrationBuilder`)
- No committed prefab rewrite unless dry-run report is clean

### v0.7.7 — non-network child grouping

- Move non-`NetworkBehaviour` systems into `Systems/*` children
- Preserve serialized references via builder
- Update builders/validators; run full batches and manual smoke

### v0.7.8 — model root simplification

- Remove duplicate `VisualRoot` / `PF_CCS_Player_Visual` nesting
- Establish single `Model` root
- Keep `PF_CCS_Player_Visual` internal hierarchy unchanged
- Prepare CC4 swap point

### v0.7.9 — local-owner UI separation

- Move owner-only HUD/reticle/interaction prompt to owner-spawned prefab or owner-enabled `LocalOnly` child
- World-space nameplate remains under `WorldPresentation`

### Later milestones

- CC4 import preparation
- Final CC4 model prefab replacement
- Weapon animation layer rebuild (per v0.7.4 animation architecture)
- Interaction animation rebuild

---

## Related documents

| Document | Purpose |
|----------|---------|
| `CCS_CharacterController_Module.md` | Living module overview |
| `CCS_CharacterController_Animation_Rebuild_Architecture.md` | Animation layers (Phase 3C) |
| `Logs/CharacterController/PrefabAudit/CCS_PlayerPrefab_HierarchyAudit_v0.7.5.md` | Generated current-state audit |
| `Logs/CharacterController/PrefabAudit/CCS_PlayerPrefab_HierarchyArchitecture_v0.7.5.md` | Generated migration report |

---

## v0.7.5 exit criteria

- Architecture doc committed under `Documentation/`
- Phase 3D validation batch passes
- Standard module batches pass (no accidental prefab/scene mutations)
- Root MonoBehaviour count documented (baseline: **24**)
- No prefab hierarchy changes in diff
- `PF_CCS_Player_Visual` unchanged
- Animator Controller and clips unchanged
- No CC4 or animation import
