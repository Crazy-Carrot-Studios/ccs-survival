# CCS Framework — Modules

Optional, pluggable feature assemblies that extend the framework (inventory, crafting, UI, save system, etc.).

Each module should follow Runtime / Editor separation and define its own assembly definitions when implemented. Modules depend on Core; Core must not depend on modules.
