# CCS Survival — Gameplay Constitution

**Document type:** Foundational gameplay direction and design constitution  
**Project:** ccs-survival (Crazy Carrot Studios)  
**Status:** Authoritative planning reference — not implementation spec  
**Author:** James Schilz  
**Date:** 2026-05-24

This document defines the **agreed gameplay philosophy and systems direction** for CCS Survival. It guides prototype scope, feature prioritization, and long-term architecture alignment. Technical module boundaries live in [Survival Gameplay Architecture](../../../../Documentation/Architecture/Survival_Gameplay_Architecture.md) and [Framework Architecture Guide](../Framework_Architecture_Guide.md).

---

## Game Identity

CCS Survival is a **persistent frontier civilization survival simulator**.

Players survive hostile wilderness, establish claims, develop professions, participate in a player-driven economy, and grow reputation within a living frontier. Success is measured not only by personal survival but by **civilization-building**: settlements, infrastructure, commerce, law, and territorial influence.

The fantasy is **player-driven world building** on a dangerous frontier — not a theme-park MMO on day one. The world reacts to player choices, reputation, and economic activity. Towns rise, economies form, outlaws emerge, and wilderness remains a credible threat.

**Design north star:** meaningful persistence, systemic consequence, and long-horizon settlement growth — with co-op hosted worlds as the first playable target.

---

## Core Gameplay Pillars

The following pillars define what CCS Survival *is*. Features should map to at least one pillar; features that serve none are out of scope.

| Pillar | Summary |
|--------|---------|
| **Survival** | Hunger, weather, predators, poison, scarcity, and isolation make preparation essential. |
| **Settlement Building** | Players and communities construct towns, infrastructure, and civilization — not just personal bases. |
| **Reputation** | A sliding-scale social contract: lawful players retain more on death; outlaws sacrifice security for freedom. |
| **Economy** | Regional, player-driven markets shaped by quality, distance, supply, and specialization. |
| **Law vs Outlaw** | Witnesses, bounties, permissions, and consequences — anti-griefing through systems, not hand-waving. |
| **Professions** | Specialized roles (rancher, smith, trader, etc.) create interdependence and trade loops. |
| **Logistics & Transportation** | Wagons, pack animals, and distance make moving goods as important as producing them. |
| **Hunting & Ranching** | Wildlife and livestock as resource ecosystems — taming, breeding, and ranching professions. |
| **PvP & Territory** | Claims, raids, and territorial conflict with reputation-weighted stakes — not unrestricted chaos. |
| **Dynamic Frontier Simulation** | The world continues between sessions via elapsed-time simulation; wilderness and settlements evolve. |

---

## Prototype Scope

### First prototype — in scope

The **first playable prototype** targets a focused frontier slice with full systemic depth over raw map size.

| Area | Prototype intent |
|------|------------------|
| **Session model** | Single-player and **co-op hosted worlds** (listen-server / host-authoritative model) |
| **Persistence** | **Persistent saves** — character, claims, economy state, reputation, settlement progress |
| **World** | **Handcrafted frontier region** — curated geography, POIs, and balance over procedural sprawl |
| **Anchor content** | **Starter railroad town** — NPC seed civilization with commerce, law baseline, and growth hooks |
| **Survival** | Core needs: hunger, weather exposure, predators, poison, supply scarcity |
| **Land claims** | Player and group territory with permissions and raid rules |
| **Professions** | Foundational profession loop (gather → process → trade → specialize) |
| **Economy** | Regional pricing, quality tiers, player shops, transport cost |
| **Reputation** | Sliding-scale death persistence and world reaction to lawful/outlaw status |
| **Settlement growth** | Town influence radius, required buildings, infrastructure-driven expansion |

**Prototype success criteria:** a small group can survive, claim land, join the economy, affect reputation, and grow a settlement toward self-sufficiency — with saves that survive restarts.

### Intentionally delayed — later phases

These systems are **aligned with the vision** but **not first-prototype blockers**:

| System | Rationale for delay |
|--------|---------------------|
| **MMO infrastructure** | Requires sharded authority, matchmaking, and ops scale — after co-op proves loops |
| **Railroads (player-operated)** | Starter town may reference rails; full rail logistics come after core economy |
| **Massive procedural worlds** | Handcrafted region first; procedural expansion after simulation and save models stabilize |
| **Advanced politics** | Elections, governance trees, and faction diplomacy after reputation and town permissions work |
| **Large-scale AI simulation** | Town NPC crowds and army-scale simulation after core combat and economy |
| **Advanced modular construction** | Blueprint construction first; deep modular building kits after core crafting and durability |

---

## Reputation System

Reputation is a **sliding-scale death persistence philosophy** — not a cosmetic honor score.

### Core principles

- **High reputation (lawful):** On death, players **retain most owned property** — structures, stored goods, and claim security remain largely intact. The world treats them as invested citizens.
- **Low reputation (outlaw):** On death, players **lose far more** — weaker claim protection, greater loot vulnerability, harsher economic and social penalties.
- **World reaction:** NPCs, town access, prices, law enforcement intensity, and witness behavior scale with reputation bands.
- **Player choice:** Outlaw path is viable for PvP-focused players but carries **systemic cost**, not arbitrary punishment.

### Law systems

- **Witness systems:** Crimes observed by players or NPCs feed reputation and bounty state.
- **Bounty systems:** Outlaws accumulate actionable bounties — incentivizing hunter gameplay without free-for-all griefing.
- **Town permissions:** Lawful settlements restrict outlaw access to commerce, crafting, and safe zones.
- **Anti-griefing philosophy:** PvP and raiding exist within **claim rules, reputation stakes, and witness consequences** — not unchecked destruction of new players outside agreed conflict zones.

Reputation changes through **observable actions** (theft, murder, trade fairness, bounty completion, town contribution) — not invisible grind.

---

## Settlement & Town Philosophy

Settlements are **player-created civilization** growing from wilderness roots.

### Starter railroad town

- A **handcrafted NPC railroad town** seeds the region: baseline law, starter commerce, and narrative anchor.
- Players are not isolated in a void — they enter an **existing frontier economy** with room to compete, cooperate, or conflict.

### Town growth model

| Concept | Direction |
|---------|-----------|
| **Influence radius** | Towns exert a logical zone affecting permissions, NPC activity, and economic density |
| **Infrastructure-driven growth** | Towns expand through **required buildings** (trade post, sheriff office, clinic, stable, etc.) — not arbitrary plot spam |
| **Permissions** | Role-based access: build, craft, tax, defend, invite — scoped to town/clan governance |
| **Public commerce / private ownership** | Shared market spaces vs player-owned shops and storage — clear rules for both |
| **Collapse and decay** | Abandoned towns and neglected structures **decay** — persistence includes failure, not only success |

Player towns can rival or exceed the starter hub over time; the starter town is a **floor**, not a ceiling.

---

## Economy Philosophy

The economy is **player-driven**, **regional**, and **logistics-aware**.

| Principle | Detail |
|-----------|--------|
| **Regional pricing** | Distance and local supply affect price — no global instant auction house at prototype |
| **Quality-based markets** | Material and craft **quality tiers** differentiate products; masters command premium prices |
| **Transport logistics** | Moving goods costs time, risk, and capacity — wagons and pack animals matter |
| **Specialization** | Professions create dependency chains; generalists survive, specialists thrive |
| **Supply and demand** | Scarcity events (predators, weather, raids, over-harvest) ripple through regional markets |

Money and goods flow through **player actions**, not infinite NPC faucets. The starter town provides **bootstrap liquidity**; long-term economy health depends on player production and trade.

---

## Survival Philosophy

Survival is **credible pressure**, not a timer mini-game.

| Element | Direction |
|---------|-----------|
| **No fast travel** | Distance is meaningful; logistics and planning replace teleport shortcuts |
| **Dangerous wilderness** | Leaving settled zones increases risk — predators, weather, poison, and scarcity |
| **Starvation** | Food is not optional background; preservation and hunting matter |
| **Weather** | Exposure, storms, and seasonal pressure affect travel and shelter decisions |
| **Predators** | Wildlife threatens the unprepared; hunting feeds economy and survival |
| **Snakes / poison** | Environmental hazards require medicine, preparation, and knowledge |
| **Lack of supplies** | Frontier isolation — restocking requires travel, trade, or production |
| **Isolation** | Co-op mitigates loneliness; solo play emphasizes self-reliance and risk |

**Preparation beats reaction.** Players who plan routes, pack food, craft medicine, and secure shelter outperform those who sprint into the wild.

---

## Building & Ownership

Construction follows **blueprint-style placement**, **station-based crafting**, and **durability-aware ownership**.

| System | Direction |
|--------|-------------|
| **Blueprint construction** | Structures placed from validated blueprints with material costs |
| **Station-based crafting** | Production tied to physical stations (forge, loom, kitchen) — not menu-only crafting |
| **Timed production** | Jobs run over real and simulated elapsed time |
| **Material quality** | Input quality affects output quality — ties to economy and professions |
| **Structure durability** | Buildings degrade; neglect and combat damage require **repair systems** |
| **Territory claims** | Land claims define build permissions and raid rules |
| **Clan / town ownership** | Group ownership layers on personal claims for settlements |
| **Raiding & warfare** | Permitted within rules — reputation, claim windows, and defender/offender stakes |

Ownership is **persistent and consequential**. What you build can be defended, lost, repaired, or decay — aligned with reputation band.

---

## Animal & Horse Systems

Animals are **economic assets** and **logistics tools**, not cosmetic mounts.

### Initial horse archetypes (prototype direction)

| Archetype | Role |
|-----------|------|
| **Speed Horse** | Fast travel and pursuit — lower carry capacity |
| **Pack Horse** | Logistics and trade runs — slower, higher carry capacity |

### Future expansion (post-prototype)

- **Breeding systems** — trait inheritance and bloodline optimization
- **Breed expansion** — new breeds introduced through content updates, not day-one roster bloat
- **Trait-driven animals** — stats, temperament, and specialty modifiers as systemic data

### Livestock plans

| Species | Role |
|---------|------|
| **Cattle** | Meat, hide, milk chains; ranching profession anchor |
| **Pigs** | Food production, waste/byproduct loops |
| **Chickens** | Eggs, small-scale protein, homestead baseline |

### Taming and ranching

- **Taming / capturing philosophy:** Wild animals require skill, tools, time, and risk — not instant clicks
- **Ranching as profession ecosystem:** Feed, shelter, breeding, slaughter, and transport integrate with economy and settlement demand

---

## Technical Philosophy

Gameplay direction aligns with **framework-first, simulation-first, MMO-ready** engineering — without building MMO infrastructure in the prototype.

| Principle | Detail |
|-----------|--------|
| **Systemic gameplay first** | Rules emerge from systems (economy, reputation, decay) — not one-off scripts |
| **Simulation-focused architecture** | World state advances through **elapsed-time calculations** when players are offline |
| **Single-player / co-op first** | Host-authoritative sessions prove loops before shard-scale networking |
| **Future MMO-compatible architecture** | Authority boundaries, stable IDs, modular saves — see persistence and networking direction docs |
| **Framework-first design** | CCS Core provides platform; survival modules own gameplay — no gameplay in Core |

Persistent simulation means: crops grow, stations finish jobs, structures decay, and markets drift while players are away — within bounded, testable rules.

Implementation follows **explicit module registration**, **save-stable identity**, and **profile-driven setup** documented in the survival foundation milestones (v0.3.x). Gameplay modules must not bypass these guardrails.

---

## Document maintenance

| Change type | Action |
|-------------|--------|
| New pillar or prototype scope shift | Update this constitution + milestone note |
| Feature implementation | Reference pillar mapping in module design docs |
| Conflict with architecture docs | Architecture wins for **code boundaries**; this doc wins for **player-facing intent** |

**Related documents:**

- [Survival Gameplay Architecture](../../../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
- [Survival Persistence Direction](../../../../Documentation/Architecture/Survival_Persistence_Direction.md)
- [Survival Networking Authority](../../../../Documentation/Architecture/Survival_Networking_Authority.md)
- [Future Gameplay Module Guidelines](../Future_Gameplay_Module_Guidelines.md)

---

*This constitution describes direction. Implementation status lives in milestone docs and runtime code — foundation milestones through v0.3.5a contain **no gameplay mechanics**.*
