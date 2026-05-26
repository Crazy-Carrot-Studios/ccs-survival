# CCS Survival — Gameplay Loop Specification

**Document type:** Player experience flow and progression pacing  
**Project:** ccs-survival (Crazy Carrot Studios)  
**Status:** Design specification — not implementation spec  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Upstream:** [Gameplay Constitution](CCS_Survival_Gameplay_Constitution.md) · [Systems Breakdown](CCS_Survival_Gameplay_Systems_Breakdown.md)

---

## Purpose

This document defines the **intended player gameplay flow** from first spawn through long-term settlement and frontier civilization play.

It answers: *what does the player do minute-to-minute, hour-to-hour, and week-to-week?* — and how does pressure, reward, and identity evolve over that arc?

**Progression is not traditional level scaling.** Players do not “level up” into generic power spikes. Advancement comes from:

| Axis | Player expression |
|------|-------------------|
| **Survival** | Staying alive under hunger, weather, predators, and scarcity |
| **Logistics** | Moving goods, securing routes, using horses and wagons |
| **Reputation** | Lawful standing vs outlaw risk — and what the world does in response |
| **Territory** | Claims, homesteads, settlement influence, and defense |
| **Professions** | Specialized excellence that others depend on |
| **Civilization building** | Infrastructure, markets, towns, and long-horizon settlement growth |

The emotional arc moves from **vulnerable newcomer** to **frontier stakeholder** — someone who shapes the map through what they build, trade, and defend.

---

## Core Gameplay Loop Summary

The high-level loop that repeats at every scale of play:

```text
Survive → Gather → Craft → Trade → Claim → Specialize → Build → Defend → Expand Civilization
```

| Phase | Player focus |
|-------|----------------|
| **Survive** | Needs, exposure, predators, medicine, shelter |
| **Gather** | Resources from wilderness and salvage |
| **Craft** | Tools, food, stations, construction materials |
| **Trade** | Convert surplus into security and specialization |
| **Claim** | Secure land and permissions — stakes on the map |
| **Specialize** | Profession identity and interdependence |
| **Build** | Homesteads, stations, town infrastructure |
| **Defend** | Reputation-weighted PvP, raids, bounties, settlement security |
| **Expand civilization** | Influence, economy, logistics networks, frontier identity |

**Evolution of pressure:** Early play is **survival-forward** — every decision is about staying alive today. Mid play introduces **logistics and economy** — distance, weight, and price matter. Late sandbox play is **infrastructure-forward** — towns, routes, reputation, and territorial influence define success.

Survival pressure never disappears; it **changes shape** — from starving in the brush to losing a wagon convoy to an outlaw ambush.

---

## Player Spawn & Onboarding

### Spawn context

- Players spawn **near the starter railroad town** — visible civilization anchor, not empty wilderness isolation.
- **Starter supplies** provide a short grace window (basic food, bandage, crude tool) — enough to learn, not enough to skip the loop.
- **Beginner survival pressure** begins immediately: hunger clock, exposure risk if wandering, predator audio/visual hints at the treeline.
- **Protected starter civilization:** core town streets and vendor areas enforce law baseline — theft and murder have witness/reputation consequences.
- **Introduction to trade and wilderness risk:** NPC vendors and signage orient players — “town is safety; treeline is opportunity and danger.”

### Emotional goal

> The player should feel **vulnerable but hopeful**.

Vulnerable: the wilderness can kill them, supplies run out, night is colder.  
Hopeful: the railroad town exists, people trade here, a path to homestead and profession is visible.

Onboarding does **not** front-load tutorials for every system. It **stages discovery** through pressure and proximity (see time bands below).

---

## First 5 Minutes

**Design intent:** Orientation, immediacy, first agency.

| Beat | Intended gameplay |
|------|-------------------|
| **Gather basics** | Wood, stone, fiber from nearby nodes — tactile “I can interact with the world” |
| **Locate water** | Stream/well discovery — survival literacy |
| **Avoid predators** | Audio cue, distant wolf/deer — teach threat without mandatory death |
| **Survival awareness** | Hunger indicator, simple temperature/exposure hint |
| **First primitive tools** | Sharp stone / crude axe — unlock faster gather |
| **Railroad town orientation** | See tracks, vendors, other players (co-op) — mental map anchor |

**Pacing:** Fast feedback loops. Every action has visible result. No long crafting timers yet.

**Risk level:** Low near town; rises with distance and night.

---

## First 30 Minutes

**Design intent:** First camp, first meal, first economic touch.

| Beat | Intended gameplay |
|------|-------------------|
| **Hunting small wildlife** | Rabbit/small game — first protein source, bow/spear moment |
| **Basic camp creation** | Campfire, bed roll, crude shelter — “I have a place” |
| **First food preparation** | Cook raw meat — hunger relief with quality jump |
| **Weather exposure awareness** | Rain/cold pushes shelter use |
| **First trade/vendor interaction** | Sell pelts, buy salt/tools — economy is real |
| **Inventory & carry weight** | Full pockets force choices — logistics seed planted |
| **Meaningful wilderness travel** | Trip beyond sight of town — tension + reward |

**Pacing:** Players should complete a **full survive → hunt → cook → trade** cycle before feeling ready to claim land.

**Risk level:** Medium outside town; mistakes are recoverable with town fallback.

---

## First 2 Hours

**Design intent:** Homestead intent, profession direction, economy participation.

| Beat | Intended gameplay |
|------|-------------------|
| **Temporary homestead** | Claim flag or lease plot — persistence milestone |
| **Early profession direction** | Hunter vs smith vs trader lean — recipe/vendor affinity |
| **Higher-quality materials** | Iron ore, hardwood, hides — craft tier step-up |
| **Crafting stations** | Workbench, forge, cooking station — timed production |
| **Storage management** | Chests, weight planning — loss on death becomes meaningful |
| **First economy participation** | Player shop or repeat vendor runs — surplus has value |
| **First horse goals** | Save for speed or pack horse — travel fantasy unlocked |
| **Risk vs reward exploration** | Distant nodes, predator zones, outlaw skirt — player-chosen danger |

**Pacing:** Session ends with player **owning something on the map** and **needing the town** for something they cannot self-provide.

**Risk level:** Player-selected; reputation begins to matter for death outcomes.

---

## Profession Progression

Professions are **identity and interdependence**, not class locked at character creation.

| Profession | Fantasy | Loop contribution |
|------------|---------|-----------------|
| **Hunter** | Wilderness mastery, meat/hides | Feeds survival and economy |
| **Rancher** | Livestock, breeding, bulk goods | Supply chains for town |
| **Blacksmith** | Tools, weapons, station upgrades | Enables other professions |
| **Trader** | Routes, arbitrage, wagons | Connects regions |
| **Farmer** | Crops, preservation, stability | Food security for settlements |
| **Prospector** | Rare ore, remote risk | High-value inputs for craft |

### Solo vs group

- **Solo players** specialize through **focused excellence** — one person can be the town’s best smith, but cannot be every role at once.
- **Groups** scale through **cooperation and infrastructure** — shared claim, shared storage, dedicated roles, settlement defense rotations.

Profession progression unlocks **recipes, station tiers, vendor trust, and settlement permissions** — not flat +10% damage.

---

## Survival Pressure Escalation

Pressure **stacks and shifts** rather than replacing earlier threats.

| Stage | Pressure sources |
|-------|------------------|
| **Early** | Starvation, exposure, minor predators, inventory limits |
| **Mid** | Weather events, supply shortages, travel distance, carry weight |
| **Late** | Hostile players, outlaw activity, raid risk, bounty hunters |
| **Persistent** | Long-distance logistics, wagon vulnerability, seasonal scarcity |

**Design rule:** Each new pressure **connects to a system solution** — hunger → hunt/farm/trade; raid risk → walls/allies/reputation; distance → horses/wagons.

Avoid “survival for survival’s sake” with no payoff. Pressure should drive **decisions**, not only frustration.

---

## Settlement Gameplay

Settlements are the **true long-term progression system** — more important than individual gear score.

| Layer | Gameplay |
|-------|----------|
| **Claims** | Personal/group land rights — build and store |
| **Homesteads** | Stations, storage, defenses — player base |
| **Town infrastructure** | Required buildings unlock growth stage |
| **Influence radius** | Civilization density — permissions, markets, NPC activity |
| **Player markets** | Shops, stalls, supply contracts |
| **Wagons / logistics** | Bulk move between homestead and town |
| **Settlement defense** | Walls, guards, allies, raid windows (TBD) |
| **Town specialization** | Mining town vs ranch town vs trade hub |

**Progression metric:** infrastructure completed, trade volume, population stage, territorial influence — not character level.

Players should feel: *“We built this town’s economy”* — not *“I hit level 50.”*

---

## Long-Term Sandbox Gameplay

After the prototype vertical slice, the sandbox sustains play through **systems interaction**:

| Loop | Description |
|------|-------------|
| **Territorial influence** | Towns and clans compete for region control |
| **Economy control** | Supply routes, pricing, monopolies, shortages |
| **Town growth** | New buildings, decay if neglected, collapse possible |
| **Profession mastery** | Rare goods, legendary crafts, teaching others |
| **Logistics networks** | Wagon roads, horse breeding, escort contracts |
| **Law vs outlaw conflict** | Bounties, witness networks, town bans, raids |
| **Player-driven frontier civilization** | The map reflects player history — not static quests |

**Session types:** Some players log in to hunt; others to run a shop; others to defend a raid — same world, different roles.

---

## Emotional Design Goals

| Emotion | How the game evokes it |
|---------|------------------------|
| **Isolation** | Wilderness distance, limited fast travel, solo vulnerability |
| **Danger** | Predators, weather, outlaws, raid stakes |
| **Survival tension** | Hunger clock, exposure, medicine scarcity |
| **Pride of ownership** | Claim, homestead, shop, town contribution |
| **Reputation** | Lawful respect vs outlaw notoriety — world reacts |
| **Frontier immersion** | Railroad town, open range, period-appropriate friction |
| **Civilization from wilderness** | Watching a settlement grow from camp to hub |
| **Meaningful risk/reward** | Death and loss matter — scaled by reputation band |

Avoid emotional whiplash: town safety and wilderness danger should feel **consistent**, not arbitrary.

---

## Prototype Vertical Slice Goal

The **first playable vertical slice** must prove the loop — not every system at full depth.

A player (solo or co-op host) should be able to:

| Capability | Proves |
|------------|--------|
| Survive wilderness pressure | Hunger, weather, predators, medicine |
| Establish a small claim | Territory persistence |
| Hunt and trade | Wildlife + economy loop |
| Interact with the economy | Starter town vendors, buy/sell |
| Experience reputation systems | Band change, death retention difference |
| Begin profession specialization | Gated recipes or vendor trust |
| Contribute to settlement growth | Building checklist / growth metric |

**Without requiring:** MMO infrastructure, procedural worlds, player railroads, advanced politics, or full modular construction.

**Slice geography:** handcrafted frontier region + starter railroad town — systemic depth over map size.

---

## Scope Protection Notes

Guardrails for design and implementation — refer to this section when scoping milestones.

| Rule | Rationale |
|------|-----------|
| **Avoid feature creep** | Ship the loop before adding parallel fantasies |
| **Systemic depth over map size** | One rich region beats empty procedural expanse |
| **Prototype the loop before advanced simulation** | Elapsed-time crop genetics can wait; hunt→trade→claim cannot |
| **Civilization > raw combat volume** | Town growth and logistics are the identity — not kill counts |
| **Do not become “cowboy Rust”** | Persistence and settlement matter; not only raid-meta PvP |
| **No fast travel** | Distance preserves logistics and tension |
| **Reputation must have teeth** | Outlaw path is viable but costly; lawful path is secure |

When a feature does not serve **Survive → … → Expand Civilization**, defer it.

---

## Related documents

| Document | Role |
|----------|------|
| [Gameplay Constitution](CCS_Survival_Gameplay_Constitution.md) | What the game is |
| [Systems Breakdown](CCS_Survival_Gameplay_Systems_Breakdown.md) | How systems split for engineering |
| [Future Gameplay Module Guidelines](../Future_Gameplay_Module_Guidelines.md) | Module implementation rules |

**Planned follow-ups:** Reputation & Law Design Spec · Settlement & Territory Spec · Economy & Logistics Spec

---

*Implementation status: foundation runtime (v0.3.x) contains no gameplay mechanics. This spec guides first gameplay milestones.*
