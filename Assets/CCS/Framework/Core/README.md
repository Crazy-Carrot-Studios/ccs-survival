# CCS Framework — Core

Authoritative foundation for the CCS framework.

## Runtime (`Runtime/`)

Player and build-safe code: systems, services, utilities, and shared data types. Compiled by `CCS.Core.Runtime`.

## Editor (`Editor/`)

Editor-only tooling: windows, custom inspectors, and editor utilities. Compiled by `CCS.Core.Editor`, references Runtime.

## Assembly

- `Runtime/Assembly/CCS.Core.Runtime.asmdef`
- `Editor/Assembly/CCS.Core.Editor.asmdef`

Do not place gameplay logic here. Feature code belongs in `../Modules/`.
