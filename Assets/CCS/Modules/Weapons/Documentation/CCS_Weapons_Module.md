# CCS Weapons Module



**Version:** 0.6.7  

**Scope:** Revolver M1879 world pickup, hitscan gameplay, equipment socket foundation, Equipment Fit Studio, and Master Test integration.



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

| `CCS_RevolverFireFeedback` | Debug tracer from player `MuzzlePoint` |



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

- Hitscan remains gameplay authority; tracer uses the player placeholder `MuzzlePoint`.

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

