# CCS AI Module

**Version:** 0.7.1

## Purpose

Introduces the first network-aware hostile AI foundation for `ccs-survival`: a simple server-authoritative bandit that senses targets, pathfinds, fires revolver hitscan shots, and takes replicated damage through shared `CCS_IDamageable` contracts.

## v0.7.1 — AI bandit polish and hosting fixes

- `CCS_AIBanditNameplate`: `AI_Bandit_Nameplate` hierarchy with `HealthBar_Slider` above `NameText` (`AI_Bandit`), client-local camera billboard.
- `CCS_AIMotorController`: NavMesh-first movement with `CharacterController` fallback.
- `CCS_AINavigationMasterTestBuilder`: bakes Master Test `NavMeshSurface_MasterTest` and marks environment static.
- `CCS_NetworkHealth` / `CCS_IDamageable.IsDamageReady`: spawn-safe damage gating (no pre-spawn NetworkVariable writes).
- AI prefab builder strips legacy player nameplate/HUD and wires AI stack + `NavMeshAgent`.

## Runtime Scope (v0.7.0+)

- `CCS_AIBanditProfile`: tuning profile for sensing, chase, and combat cadence.
- `CCS_AIBanditBrain`: state machine (`Idle`, `AcquireTarget`, `MoveToRange`, `DrawWeapon`, `Aim`, `Fire`, `Cooldown`, `Dead`).
- `CCS_AITargetSensor` + `CCS_AILineOfSightSensor`: nearest-target and LOS checks.
- `CCS_AIMotorController`: NavMeshAgent destination steering with flat XZ fallback.
- `CCS_AIWeaponController`: AI shot dispatch through `CCS_WeaponShotResolver` in `AIAimTarget` mode.
- `CCS_AIBanditController`: server/offline authority orchestrator.
- `CCS_AIBanditNameplate`: world-space health slider + `AI_Bandit` text billboard.
- `CCS_AIBanditSpawner`: single spawn in Master Test for server/offline sessions.

## Shared Combat Bridge

To avoid circular dependencies between Weapons and AI:

- `CCS_IDamageable`, `CCS_DamageInfo`, `CCS_DamageSourceType` live in `Attributes`.
- `CCS_NetworkHealth` implements `CCS_IDamageable` and replicates health/death as server authority.
- `CCS_RevolverController` applies damage via `GetComponentInParent<CCS_IDamageable>()` when `IsDamageReady`.

## Editor Utilities

- `CCS_AIBanditPrefabBuilder`: builds `PF_CCS_AI_Bandit_Networked`, strips player-only scripts, adds AI stack + nameplate + NavMeshAgent.
- `CCS_AINavigationMasterTestBuilder`: ensures/bakes Master Test navigation surface.
- `CCS_AIBanditMasterTestBuilder`: ensures `CCS_AIBanditSpawner` is present and wired in Master Test.
- `CCS_AIBanditValidationUtility`: B13 + v0.7.1 polish checks.
- `CCS_AIBanditBatchEntry.RunFromBatchMode`: one-shot setup + validate entrypoint.

## Netcode Registration

`Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_NetcodeTestConstants.cs` includes:

- `Assets/CCS/Modules/AI/Content/Prefabs/PF_CCS_AI_Bandit_Networked.prefab`

in `RequiredNetworkPrefabPaths`.

`CCS_NetworkPrefabReferenceGuard` repairs player, pickup, and AI bandit prefab list entries from serialized fallbacks.

## Notes

- Chest-height AI aiming default is `1.45m`.
- Debug logs are gated by local booleans in AI runtime components.
- `CCS_RevolverUpperBodyAnimator` exposes external AI aim control through `SetRevolverAimHeldExternal(...)`.
- Ambient playlist plays on hosting scene only (not Master Test gameplay).
