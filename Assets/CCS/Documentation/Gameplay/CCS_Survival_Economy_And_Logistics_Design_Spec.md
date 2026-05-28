# CCS Survival - Economy & Logistics Design Specification

**Document Type:** Internal Gameplay Design Specification  
**Project:** CCS Survival  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Draft (Prototype-Aligned)

---

# Purpose

Economy and logistics are core civilization systems in CCS Survival. They define how settlements grow beyond subsistence survival and develop distinct regional identities.

In this design, **distance**, **transport**, **quality**, **scarcity**, **specialization**, and **regional trade** are not background simulation details; they are the primary drivers of long-term progression, social dependency, and frontier power.

---

# Design Goals

- **Player-driven economy:** Most value should originate from player gathering, production, transport, and exchange.
- **Regional pricing:** Different regions should naturally produce different prices based on supply conditions and access.
- **Meaningful transport logistics:** Moving goods should cost time, risk, and planning, creating gameplay value.
- **Specialization and interdependence:** No single playstyle should efficiently do everything at scale.
- **Item quality matters:** Better craftsmanship and materials should have visible market and durability impact.
- **Frontier scarcity:** Early and remote territories should feel constrained by resources and infrastructure.
- **No instant global economy:** Economic access should be local/regional first, with expansion earned through systems.
- **No fast travel economy shortcuts:** Goods must be physically moved through the world.
- **Support solo specialists and group settlements:** Solo players can thrive in niche roles; groups excel through coordinated supply chains.

---

# Economic Philosophy

- Economy should emerge from player production rather than from unlimited NPC injection.
- Starter railroad town provides baseline stability so new players are not hard-locked by shortages.
- Settlements compete economically through infrastructure, quality output, and route control.
- Transport creates value by converting distance and risk into price opportunity.
- Distance matters for both sourcing and market reach.
- Infrastructure matters more than raw combat for sustained prosperity and settlement longevity.

---

# Currency & Trade

- **Universal currency baseline:** A shared currency provides a common value language across regions.
- **Barter possibility:** Direct item-for-item exchange remains valid for frontier contexts and trust-based trading.
- **Direct player trading:** Person-to-person trade supports immediate transactions and social economy.
- **Vendor transactions:** Player and NPC vendors provide structured buy/sell interactions.
- **Regional market variation:** Listings and prices are region-sensitive, not globally uniform.
- **Future contract systems (delayed):** Advanced order/contract mechanics are planned but out of early scope.

---

# Regional Pricing

- Prices differ by town/region based on local stock, demand, and transport friction.
- Shortages raise prices, especially for functional essentials and scarce craft inputs.
- Oversupply lowers prices and can pressure local producers to specialize or export.
- Transport cost affects value; hard-to-move goods gain premium in remote markets.
- Isolated settlements pay more for imports due to route risk and logistics overhead.
- Quality influences pricing heavily, with higher quality tiers retaining stronger value even in saturated markets.

---

# Item Quality System

Quality is a core axis of production identity and market differentiation.

## Quality Tiers

- Poor
- Common
- Fine
- Masterwork
- Legendary

## Quality Inputs

Quality outcomes should be influenced by:

- Material quality
- Crafter skill
- Station quality
- Recipe complexity
- Repair quality

Repaired items may lose quality depending on repair context and tool/material fidelity. Quality affects item durability, trade value, and producer reputation signals in the player economy.

---

# Professions & Interdependence

Primary economic roles include:

- Hunters
- Ranchers
- Blacksmiths
- Traders
- Farmers
- Prospectors
- Transporters/Logistics players

Design intent:

- Solo specialists can thrive by mastering a narrow role and trading effectively.
- Settlements scale through cooperation, where role coverage creates stability and margin.
- Economy depends on dependency chains (inputs -> processing -> transport -> retail -> upkeep).

---

# Logistics & Transportation

- No fast travel.
- Physical movement of goods matters.
- Carry weight limits constrain ad-hoc hauling.
- Backpacks provide personal mobility storage.
- Saddle bags extend mounted transport capability.
- Pack horses support mid-scale cargo movement.
- Wagons provide large-volume movement at higher vulnerability and route constraints.
- Future convoy gameplay expands coordinated hauling and escort patterns.

Moving goods is gameplay, not a menu shortcut. Routes create danger (ambush, theft, environmental delays) and opportunity (trade arbitrage, escort services, regional influence). Logistics is civilization progression because stable towns require reliable, defended supply throughput.

---

# Horse & Wagon Economy

- **Speed horse vs pack horse:** Speed horses prioritize mobility/response; pack horses prioritize cargo capacity.
- **Wagon transport for bulk trade:** Wagons are the backbone of high-volume inter-settlement commerce.
- **Stable systems:** Stable infrastructure supports breeding, storage, recovery, and transport uptime.
- **Future horse breeding economy:** Breeding can become a long-term specialization and trade vertical.
- **Transport specialization:** Dedicated logistics players can profit from hauling, route knowledge, and reliability.
- **Wagon vulnerability and escort gameplay:** High-value cargo should incentivize escorts, scouting, and route strategy.

---

# Settlement Economy

- Public markets provide baseline access for visitors and residents.
- Player-owned shops create identity, specialization, and persistent local services.
- Local taxes/fees fund infrastructure upkeep and civic protection.
- Infrastructure upkeep costs create ongoing economic sinks and governance decisions.
- Trade route importance determines import stability and export viability.
- Settlement specialization emerges from geography, labor composition, and infrastructure.
- Economic identity by region should be visible in available goods, quality profiles, and price patterns.

---

# Scarcity & Supply

- Weather impacts supply through crop reliability, travel windows, and production pace.
- Hunting pressure affects wildlife availability and shifts food/economic balance.
- Settlement growth increases demand and can strain unprepared logistics chains.
- Isolation creates shortages and raises strategic value of transport roles.
- Conflict disrupts markets by damaging routes, labor availability, and trust.
- Transportation disruption directly impacts economy through stockouts and pricing spikes.

---

# NPC Economy Role

- Starter railroad town serves as baseline economy and onboarding safety net.
- NPC vendors act as fallback supply when player economy is thin or unstable.
- NPC prices are intentionally higher to preserve player-trade relevance.
- NPC economy should never replace player economy as the dominant value source.
- Future NPC workers/hired hands should support player production loops rather than compete with them.

---

# Persistence & Offline Simulation

- Crop growth during elapsed-time simulation
- Crafting job completion
- Decay/upkeep costs
- Market drift
- Limited simulated economic progression while offline

Design requirement:

- Avoid fully simulating the entire world constantly.
- Use bounded elapsed-time calculations to preserve performance and predictability while maintaining world continuity.

---

# Prototype Scope

First version targets core loop validation:

- Starter town vendors
- Player buy/sell
- Simple regional pricing modifiers
- Item quality tiers
- Carry weight
- Pack horse storage
- Simple wagon inventory
- Basic player trading

---

# Delayed Scope

Defer to later milestones:

- Global contract systems
- Banking
- Player loans
- Advanced caravans
- Stockpile forecasting
- Dynamic taxes
- Black market economy depth
- Railroad logistics
- Large-scale trade simulation

---

# Open Questions

- Exact pricing formulas
- Quality multiplier values
- Wagon speed vs capacity balance
- Settlement tax tuning
- Offline economic simulation limits
- Item rarity rates
- Repair degradation formula
- Trade route visibility systems

---

# Design Identity Reminder

CCS Survival is a civilization and logistics frontier simulator — not a loot treadmill economy.

Infrastructure, transport, specialization, and reputation should matter more than raw item inflation.

---

# Notes for Technical Follow-up

Implementation details (data schema, authority model, persistence format, anti-exploit rules, and UI representation) should be captured in a dedicated technical design pass after prototype playtest validation.
