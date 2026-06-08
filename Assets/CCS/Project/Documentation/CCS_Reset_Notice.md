# CCS Survival — Hard Reset Notice

**Reset date:** 2026-06-07  
**Reset target SHA:** `e1002fe661f3613685d4a90d9521a403f819fede` (tag `v0.3.5a`)  
**Archive branch:** `archive/full-survival-before-hard-reset`

## Reason

Restart from the completed survival **framework baseline** (milestone 0.3.5a — quality gate, no gameplay modules) and rebuild gameplay modules in controlled, isolated steps.

## Folder normalization (2026-06-07)

`Assets/CCS/Survival/` renamed to `Assets/CCS/Project/`. Assembly renamed to `CCS.Project.Runtime` / namespace `CCS.Project`. Empty `Assets/CCS/Database/` removed.

**Ownership:** `Framework/` = Core · `Modules/` = gameplay · `Shared/` = cross-module · `Project/` = bootstrap/composition · `Tests/` = harnesses.

## Rule going forward

Each future gameplay module must include a **working test prefab** before proceeding to the next module. Module-owned data stays inside the module folder.

## Preserved history

All prior survival gameplay and module work remains on the archive branch for reference. It is not part of `main` history after this reset.
