# CCS Survival

**Version 0.7.10a** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, diagnostics, hosting/netcode tooling (v0.7.6 Kevin player visual) |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |
| **Weapons** | Revolver M1879 world pickup, hitscan, fit profile pack (v0.6.16 reticle-aligned shots) |
| **AI** | Network AI bandit combat foundation + v0.7.1 polish (v0.7.1) |

## Current milestone

**0.7.10a** — **Revolver hand socket preview hotfix:** fixes diagnostics **Force Revolver Hand Socket Preview** so the revolver visibly attaches to Kevin’s `CCS_HandSocket_Right`; clarifies socket vs IK hierarchy; diagnostics manager now targets the player equipment visual controller directly. No gameplay ownership/ammo/damage/fire changes.

**0.7.10** — **Revolver hand socket preview toggle:** adds diagnostics **Force Revolver Hand Socket Preview** (visual-only right-hand socket attachment for offset testing); **Force Revolver Aim Setup Pose** remains separate (animation + visual). No gameplay weapon ownership, ammo, damage, or fire changes. No new animation layers or fire/reload/interaction/dual-revolver animation work.

**0.7.9** — **Validation cleanup and aim setup pose toggle:** moves `CCS_TestWeaponDamageTarget` to CharacterController Prototyping; removes legacy `CCS_TestDetectionCube`; adds diagnostics **Force Revolver Aim Setup Pose** (animation + right-hand visual preview, presentation-only); removes `CapsuleVisual`/`VisualGlasses` from production player prefab. No gameplay aim/fire changes.

**0.7.8** — **Single revolver aim upper-body layer:** adds masked `SingleRevolverUpperBody` draw/hold/holster presentation using Wild West clips; gameplay aim/fire remains on `CCS_RevolverController`. No fire/reload/interaction/dual revolver animations.

**0.7.7** — **EnemyAI default AI bandit visual:** replaces bandit `VisualRoot`/`PF_CCS_Player_Visual` with single `Model` root and `PF_CCS_AI_Bandit_Model_EnemyAI`; deletes legacy player visual when unreferenced; cleans Unity 6 CS0618 editor warnings. Kevin remains player visual. Camila not wired. Locomotion-only Animator preserved.

**0.7.6** — **Kevin default player visual:** replaces nested `VisualRoot`/`PF_CCS_Player_Visual` with single `Model` root and `PF_CCS_Player_Model_Kevin`; rebuilds equipment sockets for Kevin humanoid rig. EnemyAI and Camila imported but not wired. Locomotion-only Animator preserved. No weapon/interaction animation rebuild.

**0.7.5** — **Player prefab hierarchy architecture (Phase 3D):** documents target hierarchy, root component budgets, subsystem ownership, Netcode-safe root rules, single Model root plan, and owner-only UI separation roadmap; adds `CCS_IPlayerCompositionRoot` interface contract. No prefab hierarchy changes in v0.7.5.

**0.7.4** — **Animation rebuild architecture (Phase 3C):** documents future locomotion/weapon/interaction/additive layer plan; adds `CCS_CharacterAnimationParameterIds`, `CCS_CharacterWeaponAnimationMode`, and `CCS_ICharacterAnimationPresenter`; centralizes locomotion parameter hashes. v0.7.3 locomotion-only Animator preserved. No animation import, no CC4 import, no Animator Controller rebuild.

**0.7.3** — **Locomotion-only Animator reset (Phase 3B):** resets player Animator Controller to Base Layer locomotion only; removes aim/revolver/interaction animation layers and player animation bridge components; gameplay aiming, shooting, pickup, and interaction locks remain. No animation import, no CC4 import, no new weapon animation set.

**0.7.2** — **Productionize Character Controller architecture (Phase 3A):** removes `CharacterController/Tests/`; moves networked player prefab to `Prefabs/Player/PF_CCS_CharacterController_Player_Networked`; validation scene to `Scenes/Validation/SCN_CCS_CharacterController_Validation`; adds `Prototyping/` for blockout environment assets; renames Testing Manager → Diagnostics Manager (`CCS_DiagnosticsManager`). No Animator reset or animation import.

**0.7.1f** — **Safe Test-Only Component Separation (Phase 2D):** migrates Master Test to `CCS_CharacterControllerDiagnosticsManager` directly; moves offline bootstrap and damage diagnostics input off the player prefab root into scene-level replacements. No intended gameplay behavior changes; no animation import yet.

**0.7.1e** — **Player Prefab Component Audit (Phase 2C):** adds editor audit utility + batch entry to inventory/classify test player prefab components; documents future component reduction without prefab rewrite. No gameplay behavior changes; no player prefab cleanup yet.

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
| Master Test (primary) | `Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity` |
| Interaction module | **CCS → Interaction → Validate Interaction Module** |
| Weapons module | **CCS → Weapons → Validate Weapons Module** |
| Character Controller Master Test | **CCS → Character Controller → Scene → Setup And Validate Master Test Scene** |
| Equipment Fit Studio | **CCS → Character Controller → Equipment → Equipment Fit Studio** |
| Animation Fit Studio | Removed in v0.7.1c — use Equipment Fit Studio |
| Attributes module | **CCS → Attributes → Validate Attributes Module** |
| Bootstrap smoke | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |

Legacy ground-only preview removed in v0.7.2. Primary validation scene: `Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity`

## Requirements

Unity 6 · URP · Input System · Netcode for GameObjects · Cinemachine 3.1
