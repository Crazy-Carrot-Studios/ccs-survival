# Survival Persistence Direction

**Version:** 0.1.0  
**Status:** Direction document — **no save/load implementation** at this milestone  
**Author:** James Schilz  
**Date:** 2026-05-24

Defines how **ccs-survival** will approach player and world persistence without writing save system code in milestone 0.1.0.

---

## Goals

- Server-authoritative saves for multiplayer-compatible survival
- Versioned save **schemas** per module, not one monolithic blob
- Clear separation: Core may offer generic I/O contracts upstream; survival owns **data shapes**
- Recoverable migrations when inventory or crafting rules change

---

## Scope layers

| Layer | Owner | 0.1.0 status |
|-------|--------|----------------|
| **Core save abstraction** (file/cloud interface, result types) | ccs-framework when needed | Not implemented here |
| **Game save orchestration** | `ccs.survival.save` module (future) | Documented only |
| **Module snapshots** | Each gameplay module | Documented only |
| **Content** | ScriptableObject definitions | Existing folders; no save wiring |

---

## Save partition model (planned)

```text
SaveGame_v{schemaVersion}/
  meta.json              # game version, timestamp, character id
  character.json         # ccs.survival.character snapshot
  inventory.json         # ccs.survival.inventory snapshot
  equipment.json         # ccs.survival.equipment snapshot
  crafting.json          # queues, known recipes (if persisted)
  world.json             # optional world/chunk state (later)
```

Each file is owned by the module that defines the schema. The save module **orchestrates** read/write order and atomic commit — it does not embed every field inline.

---

## Authority alignment

Persistence follows [Survival Networking Authority](Survival_Networking_Authority.md):

- **Writes** originate from server authority (or local server in single-player)
- Clients do not push save files directly to shared storage in multiplayer
- UI triggers save **requests**; orchestration validates and commits

---

## Identity keys

Do not use Unity `GetInstanceID` or scene object names as persistent IDs.

| Entity | Planned ID style |
|--------|------------------|
| Item definition | Stable string ID (`item.western.bandage`) |
| Item stack instance | GUID or ulong assigned at creation (server) |
| Character | Persistent player/character GUID |
| World chunk | Spatial key (implementation TBD) |

Product flavor (*Reckoning*, western) appears in **content IDs**, not in framework module names.

---

## Migration strategy (planned)

1. Each module exposes `SchemaVersion` integer.
2. Save orchestration reads `meta.json` global version.
3. Per-module migrators upgrade snapshot JSON in order.
4. Failed migration returns `CCS_Result` failure; do not partially apply inventory without rollback plan.

---

## Module responsibilities (future)

| Module | Persists |
|--------|----------|
| `ccs.survival.character` | Position, stats, progression hooks |
| `ccs.survival.inventory` | Containers, stacks, instance IDs |
| `ccs.survival.equipment` | Equipped slots |
| `ccs.survival.crafting` | Known recipes, station state (if any) |
| `ccs.survival.save` | Orchestration, atomic write, encryption hook (optional) |

---

## What not to do at 0.1.0

- Do not implement `SaveSystem` runtime scripts
- Do not wire `PlayerPrefs` for inventory
- Do not edit existing `Assets/CCS/Modules/SaveSystem/` code
- Do not add third-party save assets without an architecture review

---

## Testing direction (future)

- Edit Mode: schema serialize/deserialize round-trip per module
- Play Mode: save → mutate → load → assert authority-consistent state
- Multiplayer: server save while clients disconnected (integration milestone)

---

## Related documents

- [Survival Gameplay Architecture](Survival_Gameplay_Architecture.md)
- [Survival Module Boundaries](Survival_Module_Boundaries.md)
- [Survival Networking Authority](Survival_Networking_Authority.md)
