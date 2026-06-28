# CCS Equipment Fit Studio

**Version:** 0.6.16  
**Type:** Editor-only tuning tool  
**Menu:** `CCS → Character Controller → Equipment → Equipment Fit Studio`

## Relationship to Animation Fit Studio (removed v0.7.1c)

**Equipment Fit Studio is kept.** It is a separate production workflow from the removed **Animation Fit Studio** tooling.

| Tool | Status | Purpose |
|------|--------|---------|
| **Equipment Fit Studio** | **Kept** | Socket and IK target tuning; saves fit profiles for production weapons |
| **Animation Fit Studio** | **Removed (v0.7.1c)** | Obsolete animation audition / Humanoid pose writeback editor — not part of runtime |

Equipment Fit Studio does **not** replace animation clip editing, AimPitch blend tuning, or controller wiring. It supports production equipment/socket fitting only.

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

## v0.6.8 Editor-Only Fit Studio Revamp

Equipment Fit Studio is an **Editor Mode only** profile-tuning tool. Play Mode is for **runtime verification** after saving — not for tuning or saving profiles in the main window.

### Fit Target first

The first control is **Fit Target**:

| Fit Target | Socket | Profile | Pose | Camera focus |
|------------|--------|---------|------|----------------|
| **Holstered Item** | `CCS_HolsterSocket_RightHip` | `CCS_RevolverM1879_RightHipHolster_Fit.asset` | Neutral | Right Hip |
| **Equipped Item** | `CCS_HandSocket_Right` | `CCS_RevolverM1879_RightHandEquipped_Fit.asset` | Revolver Aim | Upper body / right hand / weapon |

Changing Fit Target auto-loads preview player, pose, profile values, weapon preview, and camera framing.

### Window layout

- Header: **CCS Equipment Fit Studio | Editor Mode Only | Profile Tuning**
- Top bar: Fit Target, Weapon/Item, Profile Asset, status (loaded from SO / clean or dirty)
- Left guide panel: four cards (Select Fit Target → Auto Load Preview → Adjust Transform → Save Profile)
- Center: large orbit/pan/zoom preview viewport with camera presets
- Right: **Attachment / Profile Offset** fields and nudge controls
- Bottom bar: Load Preview, Reset to Profile, Reset to Default, Validate, **Save Profile**

Opens at **1450×820** (minimum **1200×700**) for docked and smaller monitors.

### Play Mode (minimal)

If Fit Studio is opened during Play Mode, it shows only:

```text
Equipment Fit Studio works in Editor Mode only.
Exit Play Mode to edit equipment fit profiles.
Use Play Mode only to test saved profiles in-game.
```

No tuning controls, save controls, or runtime profile details are shown.

### Preview camera

Mouse controls inside the preview viewport:

- Left drag: orbit
- Middle drag: pan
- Scroll wheel: zoom
- F key (viewport focused): frame current target

Camera preset controls live **outside** the preview image in a compact toolbar:

- Fit Target Default, Frame, Reset Camera
- Optional preset dropdown (Target Default, Full Body, Upper Body, Right Hand, Right Hip, Weapon Close-Up)

No preset button grid is drawn inside the viewport.

### Preview rules

- Preview visual stays **zeroed** (`0,0,0` / identity / `1,1,1`) under the **preview attachment root**
- Editable values are the **attachment/profile offset** on the preview attachment root (not the visual child)
- Socket anchor stays at its definition; profile position/rotation/scale apply to the attachment root via the same applicator used at runtime
- Preview camera: left-drag orbit, middle/Alt+drag pan, scroll zoom, right-drag look, **F** frame focus

## v0.6.15 Weapon Rotation Basis / Roll Axis Fix

Equipment Fit Studio now exposes **weapon-space Pitch / Yaw / Roll** controls so artists can fit holstered and equipped revolvers without guessing Unity Euler X/Y/Z.

### Weapon Rotation Controls (preferred)

In the right **Attachment / Profile Offset** panel:

| Control | Effect |
|---------|--------|
| **Pitch + / -** | Muzzle up/down (local X) |
| **Yaw + / -** | Muzzle left/right (local Y) |
| **Roll / Side Tilt + / -** | Side tilt around weapon forward / barrel axis |

Rotation nudge step sizes: **1°** (small), **5°** (medium), **15°** (large).

**Weapon Forward Axis** selects which local axis is treated as barrel forward for roll (default **Local -Z** for the revolver visual). If roll still looks like yaw, change this setting and enable axis visualization.

### Profile Euler (advanced)

Numeric **Profile Euler (Display)** fields remain for save/load compatibility. Prefer Pitch/Yaw/Roll buttons for fitting. The tool keeps separate edit state (`pendingLocalRotation` / `pendingDisplayEuler`) to avoid Euler read/write drift during IMGUI repaints.

### Axis visualization and diagnostics

Optional toggles (Scene view):

- **Show Socket Local Axes** — red X, green Y, blue Z at socket anchor
- **Show Weapon Local Axes** — attachment root and preview visual
- **Show Weapon Forward / Barrel Axis** — yellow/white forward line (and muzzle when available)

Collapsed **Axis Diagnostics** shows quaternion/Euler readout, selected forward axis, world forward direction, and last axis action.

**Axis Hard Tests** (`Test Pitch/Yaw/Roll +15`, `Reset Axis Test`) help verify each control produces a distinct motion. A warning appears if roll appears aligned with yaw.

### Transform hierarchy (editor preview)

```text
Socket Anchor
└─ Preview Attachment Root   ← profile position/rotation/scale applied here
   └─ Preview Visual          ← always zeroed (CCS_EDITOR_PREVIEW_ITEM)
```

### Safe startup

Fit Studio **OnEnable** defers heavy asset ensure/builder work to the next editor frame so restoring a saved layout with Fit Studio open does not hang Unity startup.

### Save workflow

1. Select Fit Target
2. Preview auto-loads (or click **Load Preview**)
3. Adjust offset in the right panel
4. **Save Profile** — writes to the mapped SO, reloads from disk, verifies match

Holstered Item must never save to the right-hand profile; Equipped Item must never save to the hip profile.

### Play Mode (read-only in Fit Studio)

If Fit Studio is opened during Play Mode, tuning and save controls are hidden. Use Play Mode to verify:

1. Pick up revolver → holster uses saved hip profile
2. Hold RMB → equipped uses saved hand profile
3. Release RMB → holster returns

### IK and one-hand mask (deferred)

IK is **not** part of the main fitting workflow. Production IK weights remain **0**.

The current revolver aim source pose is **two-handed**. Final one-handed revolver animation mask is the next animation milestone — tuning proceeds in the current aim pose without blocking save.

### Runtime visual bridge

`CCS_PlayerEquipmentVisualController` applies saved profiles at runtime using `PF_CCS_RevolverM1879_VisualOnly.prefab`.

Equipped visual `FitGuides/MuzzlePoint` supplies the gameplay tracer origin when aiming. v0.6.8 adds cosmetic fire visuals (tracer/flash/smoke) and reload-only spent shell extraction — **no per-shot casing ejection** (revolver realism). v0.6.15 keeps third-person as the default exploration camera; RMB firearm aim uses **Aim Over Shoulder** only (first-person aim removed from active Master Test flow). Reticle/hitscan follow the active camera via `CCS_WeaponAimResolver` with hybrid center + clamped muzzle drift.

### Cleanup

Temporary objects (`CCS_EDITOR_FIT_PREVIEW_PLAYER_DO_NOT_SAVE`, preview item, preview camera, test fit attachments) are removed on window close, Load Preview, Validate, Play Mode transitions, and batch/builders. None may be saved to scene or prefab.

## Upcoming: One-Hand Revolver Aim Mask (next animation milestone)

**Goal:** duplicate/create a revolver upper-body avatar mask that **excludes the left arm**:

- Keep spine, chest, right shoulder, right arm, right hand active
- Exclude left shoulder, left arm, left hand
- Use for revolver aim/fire/reload preview and runtime
- Keep the current two-handed animation path for future rifles, shotguns, bows, and two-hand stances

Planned asset name: `CCS_Revolver_UpperBody_RightArm.mask` with optional preview/runtime option `RevolverOneHandUpperBody`.

**Not implemented in v0.6.8** — avoids risking the working runtime aim/fire/reload path.

## v0.6.16 Animation Fit Studio (simplified FullDraw workflow)

- **Target:** Runtime Aim Idle — FullDraw
- **Clip curve mode:** Humanoid Muscle Curves
- **Save:** Save Runtime FullDraw + Reimport (writes controller-used `CCS_WW_Revolver_AimIdle_FullDraw.anim` in place)
- Legacy FitTest / AimPitch main-workflow labels removed; axis calibration remains under Advanced / Diagnostics only.

## v0.6.15 Animation Fit Studio (test tool)

Equipment Fit Studio fit is **separate** from animation pose tuning:

- Do **not** adjust weapon fit profiles to fix first-person arm distortion.
- Runtime equipped visual parents under **runtime equipped attachment root** (zeroed visual child), matching Fit Studio hierarchy.
- Enable `debugRuntimeFitParity` on `CCS_PlayerEquipmentVisualController` to log one-shot parity while aiming.
- Visual aim convergence remains **off** by default.

**Animation Fit Studio (editor test only):**

- Menu: `CCS → Character Controller → Animations → Animation Fit Studio`
- **Pose Target dropdown:** `Final Aim — FullDraw` (default) | `Aimed Walk — RH` — no full clip picker
- **Pose Frame dropdown:** `Stable Aim Hold` (FullDraw default, 65% clip) | `Start` | `Middle` | `End` | `Custom`
- **Default on open:** FullDraw + Stable Aim Hold; preview loads into aim pose, not regular idle
- Window title: `Animation Fit Studio — <SelectedTargetClip>`
- Layout matches Equipment Fit Studio: left guide, center preview viewport, right pose controls, bottom action bar
- Preview hierarchy: `CCS_HandSocket_Right → attachment root (profile) → zeroed revolver visual`
- Uses `CCS_RevolverM1879_RightHandEquipped_Fit.asset` when present
- FitTest naming: `<SourceClipName>_FitTest.anim` → **`CCS_WW_Revolver_AimIdle_FullDraw_FitTest.anim`** under `WildWest/Edited/`
- **Create / Load FitTest Clip** creates or loads the FitTest duplicate automatically — no file picker
- **Runtime target candidate:** Saved FitTest clip is now runtime target candidate: **`CCS_WW_Revolver_AimIdle_FullDraw_FitTest.anim`**
- **Save FitTest Pose** does **not** directly modify Animator Controller — controller wiring is a separate builder pass
- **Runtime wiring (v0.6.15):** `Revolver_AimIdle_FullDraw` on `RevolverUpperBody` uses the edited FitTest clip when present; source Wild West clips remain unchanged as fallbacks

**Recommended promotion flow (manual, after approval):**

1. Edit and save `<SourceClipName>_FitTest.anim` in Animation Fit Studio
2. Run Master Test setup / animation isolation builder (wires FitTest to `Revolver_AimIdle_FullDraw`)
3. Validate Play Mode on `SCN_CCS_CharacterController_MasterTest`
4. James approves Play Mode result
5. Optional: duplicate/promote to production name: `CCS_Revolver_AimIdle_RH_FirstPerson.anim`
6. Commit only after manual acceptance

**Wild West vendor source:** `Assets/YashMakesGames` is optional. Master Test validates CCS-owned clips under `Content/Animations/Revolver/WildWest/` when the vendor pack is absent.
