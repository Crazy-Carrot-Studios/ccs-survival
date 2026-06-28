# CCS Survival

**Version 0.7.1d** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, Master Test harness (v0.6.16 simplified third-person revolver aim) |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |
| **Weapons** | Revolver M1879 world pickup, hitscan, fit profile pack (v0.6.16 reticle-aligned shots) |
| **AI** | Network AI bandit combat foundation + v0.7.1 polish (v0.7.1) |

## Current milestone

**0.7.1d** — **Testing Manager and editor menu reduction (Phase 2B):** centralizes Master Test debug toggles in `CCS_CharacterControllerTestingManager`; removes runtime OnGUI overlays; converts setup/camera editor menus to batch-only. No gameplay behavior changes; no player prefab cleanup yet.

**0.7.1c** — **Remove Animation Fit Studio tooling (Phase 2A):** deleted obsolete animation audition/pose editor stack; consolidated module documentation; animation inventory reports now write to `Logs/`. Editor/documentation cleanup only — no gameplay behavior changes.

**0.7.1b** — **Character Controller cleanup plan (Phase 1):** documentation-only milestone adding temporary Phase 01 cleanup plan — audits Animation Fit Studio removal, editor menu reduction, Testing Manager direction, and runtime classification. No gameplay behavior changes.

**0.7.1a** — **AI health bar fill direction hotfix:** AI bandit world-space health bar now drains left-to-right from the player camera view (right-anchored fill with `Image.fillOrigin = Right`).

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
| Equipment Fit Studio | **CCS → Character Controller → Equipment → Equipment Fit Studio** |
| Animation Fit Studio | **CCS → Character Controller → Animations → Animation Fit Studio** |
| Attributes module | **CCS → Attributes → Validate Attributes Module** |
| Bootstrap smoke | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |

Legacy ground-only preview (retained, not primary): `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_Test.unity`

## Requirements

Unity 6 · URP · Input System · Netcode for GameObjects · Cinemachine 3.1
