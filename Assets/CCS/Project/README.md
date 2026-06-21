# CCS Project — Composition Shell

## What Project owns

- Bootstrap composition prefab and install sequencing (`CCS_SurvivalBootstrap`, `CCS_SurvivalInstaller`)
- Project runtime context, validation, and diagnostics contracts
- Project-level documentation and architecture references
- Editor project audit (`CCS.Project.Editor`)
- Character module skeleton (transitional until moved to `Assets/CCS/Modules/`)

Project-wide scenes live under `Assets/CCS/Scenes/` (bootstrap, Master Test, multiplayer hosting).

## What Project does not own

- Gameplay module implementations → `Assets/CCS/Modules/`
- Core platform code → `Assets/CCS/Framework/`
- Module-owned data → inside each module's `Content/` and `Profiles/`

## Runtime assembly

`Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef` → `CCS.Core.Runtime` only.  
Namespace: `CCS.Project`.

## Documentation

- [Documentation index](Documentation/README.md)
- [Architecture Gate](Documentation/Survival_Framework_Architecture_Gate.md)
- [Scene Bootstrap Standards](Documentation/Survival_Scene_Bootstrap_Standards.md)
- [Modules folder purpose](../Modules/README.md)
