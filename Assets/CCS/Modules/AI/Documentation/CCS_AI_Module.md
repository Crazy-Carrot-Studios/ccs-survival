# CCS AI Module

**Version:** 0.7.0

## Purpose

Introduces the first network-aware hostile AI foundation for `ccs-survival`: a simple server-authoritative bandit that senses targets, chases, fires revolver hitscan shots, and takes replicated damage through shared `CCS_IDamageable` contracts.

## Runtime Scope (v0.7.0)

- `CCS_AIBanditProfile`: tuning profile for sensing, chase, and combat cadence.
- `CCS_AIBanditBrain`: lightweight state machine (`Idle`, `Chasing`, `Attacking`, `Dead`).
- `CCS_AITargetSensor` + `CCS_AILineOfSightSensor`: nearest-target and LOS checks.
- `CCS_AIMotorController`: flat XZ `CharacterController.Move` steering.
- `CCS_AIWeaponController`: AI shot dispatch through `CCS_WeaponShotResolver` in `AIAimTarget` mode.
- `CCS_AIBanditController`: server/offline authority orchestrator.
- `CCS_AIBanditNameplate`: world-space health slider + `AI_Bandit` text billboard.
- `CCS_AIBanditSpawner`: single spawn in Master Test for server/offline sessions.

## Shared Combat Bridge

To avoid circular dependencies between Weapons and AI:

- `CCS_IDamageable`, `CCS_DamageInfo`, `CCS_DamageSourceType` live in `Attributes`.
- `CCS_NetworkHealth` implements `CCS_IDamageable` and replicates health/death as server authority.
- `CCS_RevolverController` now applies damage via `GetComponentInParent<CCS_IDamageable>()`.

## Editor Utilities

- `CCS_AIBanditPrefabBuilder`: duplicates canonical networked test player into `PF_CCS_AI_Bandit_Networked`, strips player-only scripts, adds AI stack.
- `CCS_AIBanditMasterTestBuilder`: ensures `CCS_AIBanditSpawner` is present and wired in Master Test.
- `CCS_AIBanditValidationUtility`: Milestone B13 foundation checks.
- `CCS_AIBanditBatchEntry.RunFromBatchMode`: one-shot setup + validate entrypoint.

## Netcode Registration

`Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_NetcodeTestConstants.cs` now includes:

- `Assets/CCS/Modules/AI/Content/Prefabs/PF_CCS_AI_Bandit_Networked.prefab`

in `RequiredNetworkPrefabPaths`.

## Notes

- Chest-height AI aiming default is `1.45m`.
- Debug logs are gated by local booleans in AI runtime components.
- `CCS_RevolverUpperBodyAnimator` exposes external AI aim control through `SetRevolverAimHeldExternal(...)`.
