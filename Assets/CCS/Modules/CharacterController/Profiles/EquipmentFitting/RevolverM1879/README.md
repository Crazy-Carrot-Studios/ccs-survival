# Revolver M1879 Fit Profiles (v0.6.8)

Profile data only — runtime holstered/equipped visuals are intentionally deferred.

## IDs

| Field | Value |
|-------|-------|
| weaponId | `ccs.weapon.revolver.m1879` |
| characterRigId | `ccs.character.testplayer.cc3_base_plus` |

## Assets

| Asset | Socket / Purpose |
|-------|------------------|
| `CCS_RevolverM1879_RightHipHolster_Fit.asset` | `CCS_HolsterSocket_RightHip` — side holster preview |
| `CCS_RevolverM1879_RightHandEquipped_Fit.asset` | `CCS_HandSocket_Right` — equipped grip preview |
| `CCS_RevolverM1879_AimIKPose.asset` | Aim IK foundation (`revolver.aim.basic`) |
| `CCS_RevolverM1879_RightHandGripPose.asset` | Hand pose foundation (`revolver.right_hand.trigger_ready`) |

## Tuning rules

- Preview revolver stays **zeroed** under the socket (`0,0,0` / identity / `1,1,1`).
- Tune **socket/profile values only** — do not move the weapon prefab root.
- Clear preview after tuning. Do not save preview objects to scene or prefab.
- IK preview weights must return to **0** before closing Fit Studio.

## Tool

Open **CCS → Character Controller → Equipment → Equipment Fit Studio**.
