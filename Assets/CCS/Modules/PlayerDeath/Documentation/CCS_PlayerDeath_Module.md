# CCS Survival — Player Death Module

**Milestone:** 1.0.1 — Death, Respawn & Save Foundation  
**Module ID:** `ccs.survival.playerdeath`  
**Namespace:** `CCS.Modules.PlayerDeath` (editor: `CCS.Modules.PlayerDeath.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-01  

---

## Purpose

Handle **starvation and dehydration failure** without UI:

- Monitor hunger and thirst through `CCS_SurvivalCoreService`  
- On death: freeze movement, log cause, reset needs, respawn at bootstrap spawn point  

---

## Key types

| Type | Role |
|------|------|
| `CCS_PlayerDeathProfile` | Respawn need values, default spawn id, debug logging |
| `CCS_PlayerDeathService` | Death detection, respawn orchestration |
| `CCS_PlayerRespawnPoint` | World spawn transform + optional spawn id |
| `CCS_PlayerDeathRuntimeBridge` | Resolves service from runtime host |

---

## Death conditions (1.0.1)

- Hunger at or below minimum  
- OR thirst at or below minimum  

---

## Events

| Event | When |
|-------|------|
| `PlayerDied` | Hunger/thirst depletion triggers death handling |
| `PlayerRespawned` | Player moved to spawn and needs restored |

---

## Bootstrap

`CCS_SaveBootstrapSetup` also creates `CCS_PlayerRespawnPoint_Bootstrap` in the survival bootstrap scene.

Default spawn id: `ccs.survival.spawn.bootstrap`

---

## Validation

Validator ID: `ccs.survival.validation.playerdeath`
