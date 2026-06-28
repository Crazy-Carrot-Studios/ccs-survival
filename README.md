# CCS Survival

**Version 0.8.0** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, production player prefab architecture (v0.8.0) |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |
| **Weapons** | Revolver M1879 world pickup, hitscan, fit profile pack (v0.6.16 reticle-aligned shots) |
| **AI** | Network AI bandit combat foundation + v0.7.1 polish (v0.7.1) |

## Current milestone

**0.8.0** — **Player production prefab architecture:** splits production (`PF_CCS_Player_Networked_Runtime`) from Master Test harness, adds `CCS_PlayerRuntimeFacade`, owner-gated `PlayerLocalUI`, component classification/validation, and preserves v0.7.2 animator layer isolation.

**0.7.3** — **Player animator runtime reconnect:** fixes RevolverUpperBody/Interaction layer weight at runtime, reconnects AC motion clips by resolved name, adds optional Animator diagnostics, and strengthens motion/playback validation.

**0.7.2** — **Player animator layer cleanup:** Base Layer is locomotion-only; `RevolverUpperBody` owns masked revolver aim and aim-strafe; `Interaction` owns pickup/door animations with `PickUp_RH` / `WalkThroughDoor_RH` triggers; batch validation guards layer structure.

**0.7.1** — **AI bandit polish and hosting fixes:** fixed `AI_Bandit` nameplate layout/billboard, added Master Test NavMesh pathfinding for bandit pursuit, fixed `CCS_NetworkHealth` offline spawn timing (`IsDamageReady`), extended network prefab guard for AI bandit fallbacks, moved ambient playlist to multiplayer hosting scene only (no Master Test music).

**0.7.0** — **Network AI bandit combat foundation:** adds `Assets/CCS/Modules/AI` runtime/editor/documentation/content scaffolding, server-authoritative bandit state machine, target sensing/LOS, simple XZ chase motor, AI revolver firing via `CCS_WeaponShotResolver` `AIAimTarget`, world-space bandit health nameplate, Master Test spawner, prefab builder/batch validation flow, shared `CCS_IDamageable` combat contracts, and `CCS_NetworkHealth` for replicated health/death.

**0.6.16** — Simplified third-person revolver aim cleanup.

**0.6.15** — Local self headless first-person body fallback; Equipment Fit Studio weapon-space rotation; Master Test recording ambience.

Profiles live at `Assets/CCS/Modules/CharacterController/Profiles/EquipmentFitting/RevolverM1879/`.

## Validation and playtest

| Action | Entry point |
|--------|-------------|
| Project audit (docs, asmdefs, legacy leftovers) | **CCS → Project → Run Project Audit** |
| Master Test (primary) | `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity` |
| Interaction module | **CCS → Interaction → Validate Interaction Module** |
| Weapons module | **CCS → Weapons → Validate Weapons Module** |
| Character Controller Master Test | **CCS → Character Controller → Scene → Setup And Validate Master Test Scene** |
| Player production prefab architecture | **CCS → Character Controller → Player Architecture → Build Production + Test Harness Prefabs** |
| Equipment Fit Studio | **CCS → Character Controller → Equipment → Equipment Fit Studio** |
| Animation Fit Studio | **CCS → Character Controller → Animations → Animation Fit Studio** |
| Attributes module | **CCS → Attributes → Validate Attributes Module** |
| Bootstrap smoke | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |

Legacy ground-only preview (retained, not primary): `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_Test.unity`

## Requirements

Unity 6 · URP · Input System · Netcode for GameObjects · Cinemachine 3.1
