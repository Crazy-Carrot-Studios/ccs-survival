# CCS Equipment Fit Studio

**Version:** 0.6.7  
**Type:** Editor-only tuning tool  
**Menu:** `CCS → Character Controller → Equipment → Equipment Fit Studio`

## What this tool does

Equipment Fit Studio helps you tune equipment sockets and IK targets on the test player **without** changing gameplay code. You can:

- Pick a player in the scene
- Pick one of the six equipment sockets
- Spawn a temporary preview revolver under that socket
- Orbit, pan, and zoom a live preview camera inside the editor window
- Nudge socket or IK target transforms
- Capture live values and save them to CCS ScriptableObject profiles
- Rebuild and validate when you are done

The preview item is **editor-only**. It is never saved to scenes or prefabs.

## Plain-English rules

1. **Do not move the weapon prefab to make it fit.** Move the socket.
2. **Keep the preview item zeroed.** Local position 0,0,0 — rotation identity — scale 1,1,1.
3. **Save to profiles.** Socket values go to `CCS_EquipmentSocketDefinition` assets.
4. **Run validation** after saving and rebuilding.
5. **Green means safe. Yellow means check it. Red means fix before saving.**

## How to open it

1. Open Unity.
2. Open `SCN_CCS_CharacterController_MasterTest` (or a scene with the test player).
3. Use menu: **CCS → Character Controller → Equipment → Equipment Fit Studio**.

## How to select a player

- Drag the spawned test player into the **Player** field, or
- Click **Find Player** to auto-find a player with `CCS_EquipmentSocketRegistry`.

## How to select a socket

Use the **Socket** dropdown. Supported sockets:

| Socket ID | Purpose |
|-----------|---------|
| `CCS_HolsterSocket_RightHip` | Right hip holster |
| `CCS_HolsterSocket_LeftHip` | Left hip offhand |
| `CCS_HandSocket_Right` | Main hand |
| `CCS_HandSocket_Left` | Offhand / support |
| `CCS_BackSocket_LongGun_A` | Primary back item |
| `CCS_BackSocket_LongGun_B` | Secondary back item |

## How to spawn a preview item

1. Select a socket.
2. Click **Spawn Preview Item**.
3. The preview uses `ModelRoot/RevolverVisual` from the world pickup prefab (visuals only — no gameplay scripts).
4. Check the status line: preview transform must stay zeroed.

## How to use the preview camera

- **Left drag** — orbit
- **Middle drag** or **Shift + left drag** — pan
- **Mouse wheel** — zoom
- Preset buttons: Frame, Full Body, Right Hand, Left Hand, hips, back, trigger close-up, muzzle view

## How to nudge position and rotation

Use the **Socket Tuner** or **IK Target Tuner** tabs:

- Type numbers directly, or
- Use small/large nudge buttons for position and rotation

Only the **socket** or **IK target** moves. The preview item stays at zero under the socket.

## How to capture and save

1. Tune the socket or IK target.
2. Click **Capture Live Values** to see pending diffs.
3. Open **Save / Validate** tab.
4. Click **Save To Socket Definition** or **Save To Attachment Fit Profile** or **Save To IK Pose Profile**.
5. Confirm the save dialog — the tool does not silently overwrite profiles.

## How to rebuild and validate

On the **Save / Validate** tab:

- **Apply Profile To Player** — reapplies socket layout to the selected player
- **Rebuild / Apply** — runs CCS builders and cleans preview objects
- **Validate** — runs Equipment Fit Studio validation

Batch validation (close Unity Editor first):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\james\OneDrive\Documents\GitHub\ccs-survival" `
  -executeMethod CCS.Project.Editor.CCS_ProjectMasterTestBatchEntry.RunFromBatchMode `
  -logFile "C:\Users\james\OneDrive\Documents\GitHub\ccs-survival\Logs\master-test-equipment-fit-studio-batch.log"
```

## What not to do

- Do not save preview objects (`CCS_EDITOR_PREVIEW_ITEM_DO_NOT_SAVE`) into scenes or prefabs.
- Do not enable IK weights in normal gameplay — preview weights reset when the window closes.
- Do not attach holstered or equipped gun visuals to the player in v0.6.7.
- Do not move the weapon prefab root to fit the character — tune the socket instead.

## Profile assets

| Asset | Path |
|-------|------|
| Fit Studio settings | `Profiles/EquipmentFitting/CCS_EquipmentFitStudioSettings.asset` |
| Default IK pose profile | `Profiles/EquipmentFitting/IK/CCS_WeaponIKPoseProfile_DefaultRevolver.asset` |
| Socket definitions | `Profiles/EquipmentSockets/Sockets/*.asset` |
| Attachment fit profiles | `Profiles/EquipmentFitting/*.asset` (created on save) |
| Hand pose definitions | `Profiles/EquipmentFitting/HandPoses/*.asset` |

## World pickup preview source

Preview visuals clone **`ModelRoot/RevolverVisual`** from `PF_CCS_RevolverM1879_WorldPickup` only. The legacy top-level `RevolverMesh` branch is not used.

## Related

- [CCS Character Controller Module](CCS_CharacterController_Module.md)
- [CCS Weapons Module](../../Weapons/Documentation/CCS_Weapons_Module.md)
