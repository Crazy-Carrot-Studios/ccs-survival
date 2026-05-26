# CCS Survival — Gameplay Systems Breakdown

**Document type:** Implementation-facing systems and module planning  
**Project:** ccs-survival (Crazy Carrot Studios)  
**Status:** Planning reference — not implementation spec  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Upstream:** [Gameplay Constitution](CCS_Survival_Gameplay_Constitution.md)

---

## 1. Purpose

This document **translates the [Gameplay Constitution](CCS_Survival_Gameplay_Constitution.md) into concrete gameplay systems** that engineering can module, save, and phase.

It defines:

- **Module candidates** — proposed `ccs.survival.*` boundaries and ownership
- **Dependency boundaries** — what may call what (services/events, not hard coupling)
- **Save responsibilities** — which module owns which persistent data slices
- **Prototype phasing** — what ships in the first playable loop vs what waits

**Progression north star (from constitution):** player advancement is **infrastructure and influence**, not level/gear score alone. Module order reflects that: survive → carry → produce → claim → reputation → trade → settle → world pressure → logistics → environment → specialize → persist.

**Related technical docs:**

- [Survival Gameplay Architecture](../../../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
- [Survival Persistence Direction](../../../../Documentation/Architecture/Survival_Persistence_Direction.md)
- [Survival Networking Authority](../../../../Documentation/Architecture/Survival_Networking_Authority.md)
- [Future Gameplay Module Guidelines](../Future_Gameplay_Module_Guidelines.md)

---

## 2. System Priority Order

Recommended **first gameplay system implementation order**. Each layer assumes the previous layers expose stable service contracts.

| Order | System | Module ID (proposed) | Rationale |
|-------|--------|----------------------|-----------|
| 1 | **Survival** | `ccs.survival.survival` | Core needs (hunger, health, exposure hooks) — everything else assumes a living character |
| 2 | **Inventory** | `ccs.survival.inventory` | Carry weight, containers, pickup — crafting and economy need item flow |
| 3 | **Crafting & Production** | `ccs.survival.crafting` | Stations, recipes, timed jobs — converts resources into goods and infrastructure inputs |
| 4 | **Claims & Ownership** | `ccs.survival.claims` | Land/build permissions and raid rules — anchors persistence and PvP stakes |
| 5 | **Reputation & Law** | `ccs.survival.reputation` | Witnesses, outlaw bands, death persistence modifiers — consequences for PvP and theft |
| 6 | **Economy** | `ccs.survival.economy` | Pricing, shops, quality markets — needs items, claims, and reputation context |
| 7 | **Settlement / Town** | `ccs.survival.settlement` | Influence radius, required buildings, growth metrics — civilization layer |
| 8 | **Wildlife & Hunting** | `ccs.survival.wildlife` | Predators, harvest, danger — feeds survival and economy |
| 9 | **Horses & Transport** | `ccs.survival.transport` | Speed/pack horses, wagons — logistics and regional trade |
| 10 | **Weather** | `ccs.survival.weather` | Exposure, storms — amplifies survival and travel risk |
| 11 | **Professions** | `ccs.survival.professions` | Specialization, XP/skill hooks, role permissions — cross-cuts crafting/economy/ranching |
| 12 | **Save / Persistence** | `ccs.survival.save` | Orchestration, schema versioning, elapsed-time simulation commit — spans all modules |

**Already in foundation (skeleton only):** `ccs.survival.character` — movement, avatar, authority hooks; expands in parallel with Survival needs, not after Settlement.

---

## 3. Module Breakdown Table

### Survival (`ccs.survival.survival`)

| Field | Detail |
|-------|--------|
| **Purpose** | Character vital needs: hunger, thirst (if used), stamina hooks, poison/disease status, death entry points |
| **Primary owned data** | Need levels, status effects, last-fed timestamps, death cause metadata |
| **Likely dependencies** | `character`, `inventory` (consumables), `weather` (exposure modifiers) |
| **Save / persistence** | `survival.json` — need state, active effects, last simulation tick |
| **Prototype scope** | Hunger drain, basic damage/heal, starvation death, consumable use |
| **Delayed scope** | Complex disease trees, detailed medical simulation, hardcore permadeath modes |

### Inventory (`ccs.survival.inventory`)

| Field | Detail |
|-------|--------|
| **Purpose** | Containers, pickup/drop, stack rules, carry weight, item instances |
| **Primary owned data** | Container snapshots, item instance IDs, quantity, durability instance data |
| **Likely dependencies** | `character` (pickup range), item definition content (SO profiles) |
| **Save / persistence** | `inventory.json` — all container graphs keyed by stable container IDs |
| **Prototype scope** | Player inventory, ground loot, weight limit, basic stack split |
| **Delayed scope** | Nested containers, mail/trade escrow, warehouse indexing at MMO scale |

### Crafting & Production (`ccs.survival.crafting`)

| Field | Detail |
|-------|--------|
| **Purpose** | Recipe validation, station jobs, timed production, material quality inputs |
| **Primary owned data** | Active job queue per station, known recipes, station ownership link |
| **Likely dependencies** | `inventory`, `claims` (station placement), `professions` (skill gates) |
| **Save / persistence** | `crafting.json` — job queues with elapsed-time completion timestamps |
| **Prototype scope** | 2–3 station types, timed jobs, recipe unlock by items |
| **Delayed scope** | Large recipe graphs, factory chains, player-designed blueprints |

### Claims & Ownership (`ccs.survival.claims`)

| Field | Detail |
|-------|--------|
| **Purpose** | Land claims, structure ownership, permission roles, raid eligibility flags |
| **Primary owned data** | Claim boundaries, owner authority IDs, permission matrix, structure registry |
| **Likely dependencies** | `character` (authority ID), `reputation` (raid modifiers), `settlement` (town overlay) |
| **Save / persistence** | `claims.json` — claim records with stable claim IDs (not scene paths) |
| **Prototype scope** | Single personal claim, build permission, basic raid flag |
| **Delayed scope** | Multi-layer clan/town/faction hierarchy, siege windows, war declarations |

### Reputation & Law (`ccs.survival.reputation`)

| Field | Detail |
|-------|--------|
| **Purpose** | Reputation score/bands, witness events, outlaw state, bounty ledger, death persistence rules |
| **Primary owned data** | Reputation value, band, recent crimes, active bounties, witness log summaries |
| **Likely dependencies** | `character` (authority ID), `claims` (loss on death), `economy` (shop access), `settlement` (town permissions) |
| **Save / persistence** | `reputation.json` — per-authority reputation and crime history |
| **Prototype scope** | Score + 3 bands, witness flag on PvP/theft, death property retention modifier |
| **Delayed scope** | Full bounty hunter loop, legal warfare, complex court/trial systems |

### Economy (`ccs.survival.economy`)

| Field | Detail |
|-------|--------|
| **Purpose** | Regional pricing, NPC/player shops, quality premiums, tax hooks |
| **Primary owned data** | Shop inventories, price tables, regional modifiers, transaction log (bounded) |
| **Likely dependencies** | `inventory`, `crafting` (quality), `transport` (distance cost), `settlement` (market zones) |
| **Save / persistence** | `economy.json` — shop state, regional tables, player vendor listings |
| **Prototype scope** | Starter town vendors, player sell/buy, simple regional price multiplier |
| **Delayed scope** | Full supply/demand simulation, player auction houses, cross-region trade routes |

### Settlement / Town (`ccs.survival.settlement`)

| Field | Detail |
|-------|--------|
| **Purpose** | Town growth, influence radius, required infrastructure, decay/collapse |
| **Primary owned data** | Settlement ID, growth stage, building checklist, influence map, decay timers |
| **Likely dependencies** | `claims`, `economy`, `reputation`, `crafting` (required buildings) |
| **Save / persistence** | `settlement.json` — settlement records + building completion state |
| **Prototype scope** | Growth metric, starter town anchor, 3–5 required building types |
| **Delayed scope** | Player elections, advanced governance, NPC population simulation |

### Wildlife & Hunting (`ccs.survival.wildlife`)

| Field | Detail |
|-------|--------|
| **Purpose** | Spawn/harvest rules, predator threat, hunting loot, snake/poison sources |
| **Primary owned data** | Population buckets (region), respawn timers, harvested node state |
| **Likely dependencies** | `survival`, `inventory`, `weather` (activity modifiers) |
| **Save / persistence** | `wildlife.json` — regional population/depletion state (not per-animal instance IDs at prototype) |
| **Prototype scope** | Huntable deer/rabbit, one predator type, snake poison item chain |
| **Delayed scope** | Migration, ecosystems, breeding sim, rare legendary hunts |

### Horses & Transport (`ccs.survival.transport`)

| Field | Detail |
|-------|--------|
| **Purpose** | Mounts, pack capacity, wagons, travel time logistics |
| **Primary owned data** | Owned mount records, wagon inventory links, trait stubs |
| **Likely dependencies** | `inventory`, `claims` (stable placement), `economy` (trade runs) |
| **Save / persistence** | `transport.json` — mount/wagon records keyed by stable transport IDs |
| **Prototype scope** | Speed horse + pack horse archetypes, basic wagon inventory |
| **Delayed scope** | Breeding, trait genetics, rail interfaces, convoy escort gameplay |

### Weather (`ccs.survival.weather`)

| Field | Detail |
|-------|--------|
| **Purpose** | Regional weather state, exposure, storm events, survival modifiers |
| **Primary owned data** | Current weather per region, forecast seed, exposure accumulation |
| **Likely dependencies** | `survival` (cold/heat), `transport` (travel blocking), `wildlife` (activity) |
| **Save / persistence** | `weather.json` — regional weather + last advance timestamp for elapsed-time sim |
| **Prototype scope** | Clear/rain/cold states, exposure drain, simple shelter check |
| **Delayed scope** | Season cycles, blizzards, crop-affecting climate, lightning wildfires |

### Professions (`ccs.survival.professions`)

| Field | Detail |
|-------|--------|
| **Purpose** | Role specialization, skill progression, profession-gated recipes and permissions |
| **Primary owned data** | Profession levels, unlocked perks, active role selection |
| **Likely dependencies** | `crafting`, `economy`, `wildlife`, `transport`, `settlement` |
| **Save / persistence** | `professions.json` — per-authority profession progress |
| **Prototype scope** | 2–3 professions (e.g. hunter, smith, trader), simple level gates |
| **Delayed scope** | Full profession trees, masterwork titles, teaching/apprentice systems |

### Save / Persistence (`ccs.survival.save`)

| Field | Detail |
|-------|--------|
| **Purpose** | Save orchestration, schema versioning, atomic commit, elapsed-time catch-up |
| **Primary owned data** | Save metadata, module schema versions, last world tick |
| **Likely dependencies** | All gameplay modules (read/write snapshots) |
| **Save / persistence** | `meta.json` + orchestrated module files (see [Persistence Direction](../../../../Documentation/Architecture/Survival_Persistence_Direction.md)) |
| **Prototype scope** | Single-player + co-op host save/load, module snapshot ordering |
| **Delayed scope** | Cloud saves, MMO shard persistence, migration tooling at scale |

### Character (existing skeleton — `ccs.survival.character`)

| Field | Detail |
|-------|--------|
| **Purpose** | Avatar, authority binding, movement/input (future), camera hooks |
| **Primary owned data** | Authority ID link, avatar binding, locomotion state (future) |
| **Likely dependencies** | Core runtime only at foundation; gameplay modules consume authority |
| **Save / persistence** | `character.json` — authority/profile references, not Transform paths |
| **Prototype scope** | Already skeleton-installed; expand with survival needs integration |
| **Delayed scope** | Full controller, animation combat stance, networked prediction |

---

## 4. Dependency Philosophy

### Core stays gameplay-agnostic

`Assets/CCS/Framework/Core/` provides host, modules, services, events, and validation — **never** survival rules, items, or reputation formulas. Gameplay lives in `Assets/CCS/Modules/` or `Assets/CCS/Survival/Runtime/<Feature>/` per [Module Boundaries](../../../../Documentation/Architecture/Survival_Module_Boundaries.md).

### Prefer services and events over hard coupling

| Pattern | Use for |
|---------|---------|
| **Service interfaces** (`CCS_ISurvivalService` descendants) | Query/adjust state across modules (inventory count, reputation band) |
| **Events** (`CCS_EventDispatcher`) | Notifications (player died, craft completed, claim raided) |
| **Direct module references** | Avoid except within same feature folder during install |

Install order is **manual** on `CCS_SurvivalInstaller`; dependency metadata is preflight validation only.

### Save-stable IDs are mandatory

Persistent gameplay objects use reverse-DNS IDs (`ccs.survival.*`, authority IDs, profile IDs, claim IDs) — **never** Unity asset paths, scene hierarchy paths, or `GetInstanceID()`. See foundation validation utilities in `CCS.Survival.Runtime`.

### MMO-compatible later, co-op first

- **Now:** host-authoritative co-op; single logical authority per session
- **Later:** shard authority without rewriting module boundaries — authority/avatar contracts already separated (v0.3.3)
- **Rule:** no netcode package in foundation assemblies; networking adapts to authority in gameplay layers

---

## 5. First Prototype Implementation Path

Practical **build order** for the first playable vertical slice (each step should be saveable and testable):

| Phase | Deliverable | Modules touched |
|-------|-------------|-----------------|
| **A** | Survival needs (hunger, damage, death) | `survival`, `character` |
| **B** | Inventory + carry weight | `inventory` |
| **C** | Basic crafting stations (campfire, workbench) | `crafting` |
| **D** | Land claim prototype (place flag, build zone) | `claims` |
| **E** | Reputation score + band + death retention modifier | `reputation` |
| **F** | Starter town vendor economy (buy/sell) | `economy` |
| **G** | Simple settlement growth metric (building checklist) | `settlement` |
| **H** | Wildlife hunting loop (spawn, kill, loot) | `wildlife` |
| **I** | Basic horse transport (mount + pack slots) | `transport` |
| **J** | Weather exposure (rain/cold + shelter) | `weather` |
| **K** | Persistence pass (save/load all module snapshots + elapsed-time) | `save` + all |

**Professions** can begin lightly at phase C (recipe gates) and expand after economy (F).

**Exit criteria:** host + one client can survive a session, claim land, craft goods, trade at starter town, lose/gain reputation on death, and reload with state intact.

---

## 6. Delayed Systems

Explicitly **out of first prototype** (documented in constitution; repeated here for engineering scope):

| System | Delay reason |
|--------|--------------|
| **MMO networking** | Co-op host model must prove loops first |
| **Full modular building** | Blueprint + station crafting first |
| **Advanced politics** | Needs settlement + reputation maturity |
| **Advanced AI schedules** | Town crowds and army sim after combat/economy |
| **Full migration ecosystems** | Wildlife prototype uses regional buckets first |
| **Player-operated railroads** | Starter town rail as lore/content only |
| **Massive procedural world generation** | Handcrafted frontier region first |

---

## 7. Open Questions

Unresolved design decisions — resolve in dedicated specs before implementation hard-codes behavior:

| Topic | Question |
|-------|----------|
| **Co-op ownership** | Exact rules for shared claims, shared storage, and host migration on disconnect |
| **Raid windows** | Time-limited raid windows vs always-on raid eligibility outside safe zones |
| **War declarations** | Requirements for town-vs-town war (reputation, cost, notice period, defender rights) |
| **Item quality formula** | How input quality, skill, station tier, and RNG combine into output quality |
| **Reputation thresholds** | Exact band breakpoints and per-action delta values |
| **Crop growth timing** | Real-time vs accelerated sim vs offline elapsed-time growth curves |
| **Animal breeding inheritance** | Trait blending, mutation rules, and generational caps |
| **Wagon physics vs abstraction** | Full physics wagon vs inventory-only abstract convoy for prototype |

**Recommended next specs (documentation milestones):**

1. [ ] Gameplay Loop Specification  
2. [ ] Reputation & Law Design Spec  
3. [ ] Settlement & Territory Spec  
4. [ ] Economy & Logistics Spec  

---

## Document maintenance

When a module ships, update its row in **Section 3** with actual module ID, save filename, and service interfaces. When prototype scope closes, move rows from prototype to delayed or mark **shipped**.

**Constitution link:** [CCS_Survival_Gameplay_Constitution.md](CCS_Survival_Gameplay_Constitution.md)
