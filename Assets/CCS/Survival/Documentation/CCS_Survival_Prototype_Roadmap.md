# CCS Survival — Prototype Roadmap

**Document Type:** Internal Production Roadmap  
**Project:** CCS Survival  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Active

---

## Purpose

This document defines the production roadmap for the first playable prototype of the CCS Survival project.

The goal is to transform the established gameplay design pillars into an executable prototype plan with clear milestone order, system dependencies, scope boundaries, testing expectations, and exit criteria.

---

## Prototype Vision

The prototype will prove whether the core fantasy works:

**Can a player survive, establish a homestead, interact with civilization, and create meaningful frontier stories inside a beautiful but unforgiving wilderness?**

The prototype is not intended to prove the full MMO vision.

It is intended to prove the survival foundation.

---

## Core Production Philosophy

Build the game as:

**Singleplayer-first, multiplayer-conscious, with hosted co-op as a stretch target.**

The prototype should support clean architecture for future multiplayer, but should not require full networking before the survival loop is proven.

Minimum multiplayer ambition:

**1 host + 1 joined player.**

If hosted co-op becomes too costly during early development, the project remains valid as a singleplayer prototype.

---

## Prototype Identity

The prototype should feel like:

**A grounded frontier survival simulation where civilization is valuable because the wilderness is beautiful, dangerous, and difficult to survive alone.**

Core emotional goals:

- peaceful
- dangerous
- awe-inspiring
- mysterious
- immersive
- untamed
- alive

---

## Prototype V1 Must Prove

Prototype V1 is successful when the player can:

- survive multiple in-game days
- manage hunger and thirst
- survive weather exposure
- gather natural resources
- hunt wildlife
- cook food
- suffer injury or death
- establish a persistent homestead
- store resources
- interact with a central safe settlement
- experience economic usefulness from civilization
- feel inventory and carry-weight pressure
- create emergent survival stories naturally

---

## Phase 0 — Foundation

**Status:** mostly complete / in progress.

Includes:

- CCS framework foundation
- module host direction
- documentation stack
- character controller foundation
- inventory framework direction
- survival design pillars
- prototype architecture decisions

**Exit criteria:**

- core gameplay identity is documented
- module-based architecture direction is clear
- prototype roadmap exists
- first implementation milestone can begin safely

---

## Phase 1 — Survival Core

**Goal:** Build the minimum survival loop.

**Systems:**

- hunger
- thirst
- health
- stamina
- temperature exposure
- basic injury
- basic death and respawn
- basic item consumption
- basic gathering

**Done means:**

- player can become hungry/thirsty
- player can eat/drink
- player can take damage
- player can die
- player can respawn
- survival pressure is understandable but not annoying

**Deferred:**

- advanced disease
- deep medical simulation
- complex death penalties
- reputation-scaled death consequences

---

## Phase 2 — Inventory, Weight, and Gathering

**Goal:** Make survival resource management meaningful.

**Systems:**

- inventory
- item stacks
- carry weight
- storage containers
- resource nodes
- animal harvesting
- water collection
- basic tools

**Done means:**

- player can gather resources
- player can become overburdened
- player must make inventory decisions
- storage has clear value
- resources support survival and crafting

**Deferred:**

- deep item quality
- large item catalog
- complex equipment systems

---

## Phase 3 — Campcraft and Homestead

**Goal:** Allow the player to establish a persistent survival base.

**Systems:**

- campfire
- cooking
- shelter placement
- prebuilt cabin or lean-to
- bed/sleep/save point
- storage placement
- basic crafting station

**Done means:**

- player can place a simple homestead
- shelter protects from weather
- campfire enables cooking
- storage supports longer-term survival
- homestead feels emotionally valuable

**Deferred:**

- modular building
- advanced construction
- structural damage
- settlement growth

---

## Phase 4 — Weather and Wilderness Pressure

**Goal:** Make the world beautiful but unforgiving.

**Systems:**

- weather states
- storms
- temperature pressure
- shelter protection
- visibility changes
- basic wildlife danger

**Done means:**

- weather changes player behavior
- shelter matters
- storms create tension
- wilderness feels immersive
- survival pressure comes from the world, not just UI meters

**Deferred:**

- advanced seasons
- full climate simulation
- deep disease/weather interactions

---

## Phase 5 — Wildlife and Hunting

**Goal:** Make animals useful, dangerous, and believable.

**Systems:**

- prey animals
- predator animals
- territory behavior
- fleeing behavior
- hunting
- harvesting
- cooking inputs

**Done means:**

- animals are not just enemies
- hunting supports survival
- predators create real danger
- wildlife makes the world feel alive

**Deferred:**

- full ecosystem simulation
- migration systems
- breeding/population simulation

---

## Phase 6 — Central Safe Settlement

**Goal:** Create contrast between wilderness and civilization.

**Systems:**

- one safe town
- vendors
- guards
- basic lodging
- basic law zone
- simple buy/sell economy
- supply stabilization

**Done means:**

- town feels safer than wilderness
- town has economic value
- player wants to return after surviving
- civilization feels useful but not mandatory

**Deferred:**

- multiple towns
- deep economy
- advanced NPC schedules
- full settlement simulation

---

## Phase 7 — Reputation and Law Foundation

**Goal:** Create early social consequences.

**Systems:**

- lawful/outlaw state
- faction reputation
- event memory flags
- basic guard reactions
- basic vendor trust modifiers

**Done means:**

- harmful actions can mark the player negatively
- helpful actions can be remembered
- reputation affects civilization response
- lawfulness has mechanical value

**Deferred:**

- deep individual NPC memory
- advanced diplomacy
- complex criminal justice systems

---

## Phase 8 — Hosted Co-op Feasibility Pass

**Goal:** Test whether the prototype can support one host and one joined player.

**Minimum target:**

- host creates world
- second player joins
- both players can move
- both players can survive
- basic shared world state works
- placed objects are visible to both
- inventory/world interactions are authority-safe

**Done means:**

- 1 host + 1 client can share a basic survival session

**Fallback:**

If this delays core progress too much, networking remains architecture-ready but implementation returns later.

**Deferred:**

- dedicated servers
- MMO infrastructure
- large player counts
- server persistence
- PvP systems

---

## Phase 9 — AI Simulation Test Harness

**Goal:** Use AI agents to stress-test survival systems.

AI agents should be able to simulate:

- gathering
- eating
- drinking
- storing items
- hauling resources
- traveling to town
- hunting behavior tests
- weather exposure tests
- starvation tests
- economy supply tests

**Done means:**

- AI can repeatedly exercise core systems
- test logs show survival loops working or failing
- systems can be validated without only relying on manual testing

This is a development/testing layer, not a final gameplay feature at first.

---

## Phase 10 — Prototype Polish Pass

**Goal:** Make the prototype feel like a real game slice.

Includes:

- UI clarity
- feedback polish
- sound placeholders
- basic animation pass
- interaction prompts
- death/respawn feedback
- weather feedback
- town atmosphere
- wilderness ambience

**Done means:**

- prototype feels coherent
- core loop is understandable
- survival experience has emotional impact
- the first "wow moment" exists

---

## First Wow Moment

The target wow moment is:

**Returning to civilization after barely surviving the wilderness.**

The player should feel:

- relief
- safety
- accomplishment
- attachment to supplies
- appreciation for civilization
- desire to prepare better next time

---

## Systems To Stub Early

These should exist architecturally but remain shallow:

- advanced economy
- settlement growth
- tribal diplomacy
- deep NPC memory
- professions
- craftsmanship quality
- horses and wagons
- caravans
- multiplayer
- advanced weather
- advanced disease
- regional faction politics

---

## Systems Not To Build Early

Avoid these until the survival prototype proves itself:

- full MMO infrastructure
- massive world scale
- hundreds of items
- procedural everything
- advanced modular building
- full dynamic economy
- full autonomous civilization simulation
- cinematic campaign
- deep quest system
- large-scale PvP
- complex animation pipelines

---

## Prototype Complete Means

The prototype is complete when:

A player can survive in the wilderness, establish a homestead, interact with civilization, experience meaningful survival pressure, and naturally create frontier stories through weather, hunger, wildlife, logistics, law, and scarcity.

The prototype does not need:

- MMO systems
- full economy
- multiple towns
- advanced professions
- deep diplomacy
- massive content scale
- polished final art

---

## Final Production Rule

**Depth over breadth.**

The prototype should have fewer systems that interact well instead of many shallow systems that do not create meaningful gameplay.

---

## Related Documentation

- [Gameplay Constitution](../../Documentation/Gameplay/CCS_Survival_Gameplay_Constitution.md)
- [Gameplay Loop Specification](../../Documentation/Gameplay/CCS_Survival_Gameplay_Loop_Specification.md)
- [Settlement & Territory Design Spec](../../Documentation/Gameplay/CCS_Survival_Settlement_And_Territory_Design_Spec.md)
- [Economy & Logistics Design Spec](../../Documentation/Gameplay/CCS_Survival_Economy_And_Logistics_Design_Spec.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
