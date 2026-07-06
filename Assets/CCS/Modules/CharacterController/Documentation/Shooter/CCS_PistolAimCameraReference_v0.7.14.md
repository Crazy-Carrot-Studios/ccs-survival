# CCS Pistol Aim Camera Reference v0.7.14

Reference-inspired design targets for CCS aim camera tuning. Apply through **CCS camera profile only** — do not import external camera controllers.

## Current CCS locked baseline

Profile: `CCS_CharacterCameraProfile_AimOverShoulder`

| Field | Current CCS value |
|-------|-------------------|
| Camera distance | 1.06 |
| Shoulder offset X | 0.65 |
| Tracking height | 1.48 |
| FOV | 56 |
| Aim blend duration | 0.45 s |

## Reference-inspired test targets

Derived from external shooter reference inspection (over-shoulder aim state):

| Field | Reference observation | CCS test range | Notes |
|-------|----------------------|----------------|-------|
| Aim camera distance | ~1.23 | **1.15 – 1.25** | Tighter than current 1.06 |
| Shoulder offset | ~0.34 | **0.30 – 0.40** | Much less than current 0.65 |
| Height | ~1.6 | **1.55 – 1.62** | Slightly above current 1.48 |
| FOV | ~35 | **38 – 42** | Major gap vs current 56 |

## Implementation rules

- Keep `CCS_CharacterCameraFollowAnchor` and existing obstacle avoidance.
- Tune `CCS_CharacterCameraProfile_AimOverShoulder` values only after pistol pose smoke on Kevin.
- Do not replace CCS camera controller architecture.
- Do not copy external camera scripts or state list assets into `ccs-survival`.

## Activation (CCS-owned)

- Aim presentation gate drives aim camera profile selection.
- `IsAiming` presentation state from diagnostics / equip visual flow — not vendor input stack.
