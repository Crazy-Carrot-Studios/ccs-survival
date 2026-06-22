# CCS Revolver M1879 Content

Reichsrevolver M1879 vendor source assets are isolated into CCS-owned runtime assets under this folder.

## v0.6.5 scope

World pickup only. Holstered/equipped visual attachment is intentionally deferred.

## Prefabs

| Prefab | Purpose |
|--------|---------|
| `PF_CCS_RevolverM1879_MaterializedVisual` | CCS-owned materialized gun source (builder only) |
| `PF_CCS_RevolverM1879_WorldPickup` | Scene pickup — grants weapon ownership |

## Runtime wiring

- Pickup grants ownership via `CCS_PlayerWeaponLoadout`.
- Existing revolver gameplay, aim strafe, and upper-body animations remain active.
- Hitscan remains gameplay authority on `CCS_RevolverController`.
- Vendor scripts/controllers are not part of CCS runtime.

Rebuild via **CCS → Weapons → Validate Weapons Module** or Master Test batch setup.
