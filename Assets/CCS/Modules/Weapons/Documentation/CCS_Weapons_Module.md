# CCS Weapons Module



**Version:** 0.6.9  

**Scope:** Revolver M1879 world pickup, hitscan gameplay, equipment socket foundation, Equipment Fit Studio, revolver fit profile pack, and Master Test integration.



## Purpose



Provides the first weapon mechanic for CCS Survival: a test revolver with world pickup, aim, fire, reload, camera-center hitscan, debug tracer feedback, and a simple damage target. This module does not include full inventory, equipment hotbar, crafting, holster/equipped gun visuals, or weapon swapping.



## Runtime Components



| Component | Role |

|-----------|------|

| `CCS_RevolverController` | Local-owner revolver state, fire/reload, hitscan dispatch (requires pickup ownership) |

| `CCS_PlayerWeaponLoadout` | Bridge ownership until Inventory module (no attached gun visuals in v0.6.6) |

| `CCS_WeaponPickupInteractable` | World pickup grants revolver to local player loadout |

| `CCS_HitscanWeaponRaycaster` | Camera-center ray with spread |

| `CCS_TestDamageTarget` | Master Test damage receiver |

| `CCS_RevolverHudPresenter` | Test-only ammo/aim/reload HUD (hidden until pickup) |

| `CCS_RevolverFireFeedback` | Cosmetic fire visuals — tracer, flash, smoke; reload shell extraction |



## Profiles



| Asset | Path |

|-------|------|

| Test revolver definition | `Tests/Profiles/CCS_RevolverDefinition_Test.asset` |

| Revolver M1879 visual definition | `Content/RevolverM1879/CCS_RevolverM1879VisualDefinition.asset` |



## Revolver M1879 Visual Content (v0.6.6)



Reichsrevolver M1879 source assets are isolated into CCS-owned runtime assets under `Content/RevolverM1879/`.



v0.6.6 keeps **world revolver pickup only**. Character Controller v0.6.6 adds equipment socket metadata and zero-weight IK targets for future holster/hand attachment, but no gun meshes are parented to the player yet. Existing revolver gameplay, aim strafe, and upper-body animations remain active.



| Prefab | Purpose |

|--------|---------|

| `PF_CCS_RevolverM1879_WorldPickup` | Interactable scene pickup with CCS materials (`ModelRoot/RevolverVisual` only) |

| `PF_CCS_RevolverM1879_MaterializedVisual` | Builder-only materialized gun source |



- Gun pickup grants weapon ownership and removes the world pickup.

- No holster or hand gun visuals are attached to the player in v0.6.6.

- Equipment sockets (`CCS_EquipmentSocketAnchor`) exist for future visuals; validators fail if runtime equipment prefabs are attached.

- Equipment Fit Studio preview clones `ModelRoot/RevolverVisual` only — not the legacy `RevolverMesh` branch.

- Hitscan uses `CCS_WeaponAimResolver`: camera reticle (viewport center) selects aim point; muzzle/tracer/hit follow that point.
- When equipped visual is active, `FitGuides/MuzzlePoint` on `PF_CCS_RevolverM1879_VisualOnly` supplies tracer origin; player `MuzzlePoint` remains fallback.

- Vendor scripts/controllers from the imported package are not part of CCS runtime.



## Input



Weapon input actions live on the CharacterController Input Actions asset:



- **Aim** — mouse right button / gamepad left trigger (gated by `CCS_PlayerWeaponLoadout.HasRevolver`)

- **Fire** — mouse left button / gamepad right trigger (requires ownership + aiming)

- **Reload** — keyboard R / gamepad West (Square/X) (requires ownership + aiming)



## Master Test



- Test player prefab receives ownership loadout, revolver controller, HUD, and fire feedback via editor builders.

- Master Test scene receives exactly one `CCS_RevolverM1879_WorldPickup` forward/right of spawn, one red capsule damage target, plus existing interaction content.

- No loose vendor `ReichsrevolverM1879` scene objects.



## Camera



Third-person and aim camera distances remain owned by CharacterController (TP 3.0 / Aim 1.5). Weapons module does not modify camera rig values.

## v0.6.8 Revolver Fit Profiles + Aim Alignment + Camera Alignment

Runtime holster/equipped visuals use saved fit profiles via `CCS_PlayerEquipmentVisualController`. **Only** `RightHandEquipped` controls gun-in-hand placement. Gameplay aim uses `CCS_WeaponAimResolver`:

| Step | Behavior |
|------|----------|
| 1 | Camera reticle ray (viewport center) picks aim point — **gameplay source of truth** |
| 2 | Equipped visual `FitGuides/MuzzlePoint` (or fallback player `MuzzlePoint`) shoots toward aim point |
| 3 | Muzzle ray may hit nearby cover before camera target |
| 4 | Tracer travels from resolved muzzle origin to hit/aim point |

Equipped hierarchy (convergence root stays identity by default):

```text
CCS_HandSocket_Right
└─ CCS_RUNTIME_Revolver_EquippedAttachmentRoot   (saved RightHandEquipped profile)
   └─ CCS_RUNTIME_Revolver_AimConvergenceRoot      (identity unless experimental convergence enabled)
      └─ CCS_RUNTIME_Revolver_EquippedVisual       (zeroed local transform)
```

**Visual barrel convergence** (`CCS_RevolverVisualAimConvergence`) is **experimental and OFF by default**. Enabling it rotates the gun after the hand profile is applied and can break hand fit — do not use for v0.6.8 shipping feel.

**v0.6.8 visual feel fix:** tune `CCS_CharacterCameraProfile_AimOverShoulder` (right-shoulder offset, distance, FOV). Editor presets: **CCS → Character Controller → Camera → Aim Camera Presets**. Enable `debugAimCameraAlignment` on `CCS_RevolverController` for manual tuning (off by default).

### Upcoming candidate (not v0.6.8)

**One-Hand Revolver Aim + Arm Alignment (v0.6.9 / v0.7.0):** one-handed upper-body mask, right-arm aim pose, optional hand/grip IK, gun stays on hand profile while arms adapt to aim.

## v0.6.8 Fire Visuals

On fire (cosmetic only — damage remains aim-resolver hitscan):

| Visual | Source | Notes |
|--------|--------|-------|
| Bullet tracer | `HitscanResult.RayOrigin` → hit/aim point | `PF_CCS_RevolverM1879_BulletTracerVisual` with short `TrailRenderer` streak |
| Muzzle flash | Muzzle origin + shot direction | ~0.04–0.08s |
| Smoke puff | Muzzle origin | ~0.5–1.5s subtle puff |

**Bullet readability (v0.6.8):** spawned bullet visuals use a visual-only scale multiplier (default **2.0**) and a warm pale yellow/orange trail (default lifetime **0.08s**, width **0.035**). Trail material: `MAT_CCS_Revolver_BulletTrail.mat`. Gameplay raycasts and hit detection are unchanged.

**Revolver shell realism:** spent casings stay in the cylinder until reload. **No per-shot shell ejection.** On `RevolverReloadStarted`, `CCS_RevolverFireFeedback` spawns spent shell visuals near `FitGuides/ShellEjectPoint` (count = empty chambers). Reload shell visuals may use a visual-only scale multiplier (default **1.5**).

`CCS_RevolverFireFeedback` **Fire Visuals** inspector group exposes bullet/shell prefabs, scale multipliers, trail toggles, muzzle flash/smoke, and `debugFireVisuals` (off by default).

Visual-only revolver fit guides: `MuzzlePoint`, `CylinderPoint`, `ShellEjectPoint` under `FitGuides`.

Enable `debugFireVisuals` on `CCS_RevolverFireFeedback` for one concise log line per fire/reload (off by default).

Fit profiles live at `Assets/CCS/Modules/CharacterController/Profiles/EquipmentFitting/RevolverM1879/`. Tune via **Equipment Fit Studio** (Editor Mode only).

**v0.6.11 animation note:** default exploration uses third-person survival camera. RMB firearm aim switches the **local owner** to fixed-anchor `FirstPersonAim`. Weapon hand fit profiles are unchanged. Wild West one-handed revolver aim/fire clips drive the `RevolverUpperBody` layer only; legacy two-handed clips and the preview layer are removed from active runtime.

See [CCS Equipment Fit Studio](../../CharacterController/Documentation/CCS_Equipment_Fit_Studio.md).

## Networking



v0.6.5 supports solo/local-owner firing only. Future multiplayer must use server-authoritative validation for owner, cooldown, ammo, origin, range, and hit target before applying damage.



## Validation



Menu: **CCS → Weapons → Validate Weapons Module**



Batch: `CCS.Modules.Weapons.Editor.CCS_WeaponsValidationBatchEntry.RunFromBatchMode`



## Dependencies



- `CCS.Core.Runtime`

- `CCS.Project.Runtime`

- `CCS.Modules.CharacterController.Runtime` (input, camera, aim locomotion)

- `CCS.Modules.Interaction.Runtime` (world pickup interaction)



Weapon logic stays in this module; Framework is not modified.

