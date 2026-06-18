# Survival Networking Authority

**Version:** 0.1.1  
**Status:** Direction document — **no networking package, no netcode** until a dedicated networking phase  
**Author:** James Schilz  
**Date:** 2026-05-24

Defines multiplayer-safe **authority and ownership** rules for **ccs-survival** before any networking stack is chosen or installed.

---

## Goals

- Design gameplay systems once with a clear **authority owner** per state type
- Avoid static globals that cannot replicate per connection or server instance
- Keep Core **network-agnostic**; net transport and spawn logic live in game modules later
- Support future co-op and MMO-scale survival without rewriting inventory or world rules

---

## Authority model (default)

| State category | Authority | Notes |
|----------------|-----------|--------|
| Player transform (world) | Server | Client may predict; server corrects |
| Player animation triggers (cosmetic) | Client | Visual only; no gameplay effect |
| Inventory contents | Server | All add/remove/equip validated server-side |
| Crafting completion | Server | Recipe validation and consumption on server |
| World containers / loot | Server | Open, take, and despawn rules server-owned |
| NPC / AI simulation | Server | AI tick on server; clients receive snapshots |
| UI / HUD | Client | Displays replicated or predicted state; never source of truth |
| Chat / social (future) | Server relay | Out of current rebuild scope |

**Single-player:** run the same authority paths with a local “server” context so multiplayer does not fork business logic.

---

## Runtime context ownership

Align with Core’s instance-owned model:

- One `CCS_RuntimeHost` (or derived game host) per **simulation context** (dedicated server, listen server, or offline local server)
- Registries (`ServiceRegistry`, `ModuleHost`, `EventDispatcher`) are **not** static singletons
- Connection-specific UI may use a separate lightweight client host if needed; gameplay services resolve from the simulation host only

---

## Replication direction (contracts first)

Before choosing Netcode for GameObjects, NGO + custom transport, or other packages:

1. **Identify replicated structs/DTOs** per module (inventory slot, equipment snapshot, craft queue).
2. **Define command/event names** as data (IDs), not hard-coded RPC sprawl in UI.
3. **Validate on authority** in module services, not in button handlers.

Suggested naming prefix for future net messages: `Survival.Net.<Module>.<Action>` — implementation deferred.

---

## Module design checklist (apply when coding starts)

- [ ] No `static` mutable gameplay state
- [ ] Mutations return `CCS_Result` and are callable from authority context
- [ ] Events describe **facts** (“ItemRemoved”) not requests (“TryRemoveItem”) on client
- [ ] Client requests go through a single **command ingress** per module (future)
- [ ] Deterministic IDs for items/entities (no `GetInstanceID` as save/net key)

---

## What not to do before networking phase

- Do not add Unity Netcode, Mirror, Photon, or other packages
- Do not add `NetworkBehaviour` to Core or module skeletons
- Do not split inventory into client-authoritative stacks “for prototyping”

---

## Bootstrap under multiplayer (future)

```text
Dedicated server:
  Load server bootstrap scene → RuntimeHost → server install plan → simulation modules

Client:
  Load client scene → RuntimeHost (client) → UI modules + presentation
  Connect → receive snapshots → update local view models
```

Install plans may differ between server and client builds; shared modules register the same IDs with different installer subsets.

---

## Related documents

- [Survival Framework Architecture Gate](../../Assets/CCS/Project/Documentation/Survival_Framework_Architecture_Gate.md)
- [Future Gameplay Module Guidelines](../Planning/Future_Gameplay_Module_Guidelines.md)
- [Survival Persistence Direction](Survival_Persistence_Direction.md)
- [CCS Core Platform Architecture](../../Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md)
