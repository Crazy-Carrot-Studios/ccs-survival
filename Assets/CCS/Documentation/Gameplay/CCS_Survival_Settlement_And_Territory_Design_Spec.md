# CCS Survival - Settlement & Territory Design Specification

**Document Type:** Internal Gameplay Design Specification  
**Project:** CCS Survival  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Draft (Prototype-Aligned)

---

# Purpose

Settlements and territory are the true long-term progression system for CCS Survival. Moment-to-moment survival keeps players alive, but land control, civic infrastructure, and local economic power define lasting player impact. This system is intended to turn wilderness into living, player-shaped regions where conflict, cooperation, prosperity, and collapse all emerge from world rules rather than scripted events.

---

# Design Goals

- **Civilization emerging from wilderness:** Regions begin as untamed frontier and only become stable through sustained player labor and organization.
- **Player-created towns:** Town identity, services, and social structure are formed by players, not pre-authored static hubs (outside the starter safe zone).
- **Infrastructure-based progression:** Growth is earned through functional buildings, utilities, and logistics, not menu-level unlocks alone.
- **Territorial influence:** Land ownership and influence radius establish practical control over permissions, trade density, and law.
- **Public commerce/private ownership:** Settlements support public-facing services while preserving private control over homes, plots, and businesses.
- **Decay and collapse:** Neglected settlements should degrade naturally, reopening land and creating a dynamic world lifecycle.
- **No random base spam:** Claim and distance rules prevent uncontrolled clutter and preserve readable, intentional world development.

---

# Territory Hierarchy

Territory exists on a structured gradient from unclaimed frontier to organized civic space:

1. **Wilderness**  
   Unclaimed and law-light. Full environmental risk, no ownership protections, limited guaranteed services.

2. **Personal Claim**  
   Small owner-defined building footprint for individual progression and early storage/security.

3. **Homestead**  
   Expanded personal living area with basic production and family/co-op viability.

4. **Ranch Claim**  
   Larger agrarian/livestock-focused parcel optimized for food, hides, and mounted logistics.

5. **Business Plot**  
   Commercial parcel intended for player-operated storefronts and service buildings tied to settlement economy.

6. **Town Territory**  
   Shared civic influence zone created by a valid town center and infrastructure milestones.

7. **Outlaw Hideout**  
   Parallel illicit territory type focused on concealment, smuggling support, and high risk/reward.

8. **Starter Railroad Town Safe Zone**  
   NPC-administered protected territory that anchors onboarding, law baseline, and early economy.

---

# Starter Railroad Town

The starter railroad town is a fixed, NPC-controlled safe zone with non-player governance:

- **NPC-controlled:** Core facilities and law enforcement are system-owned.
- **Safe zone:** New players are protected from open aggression and foundational progression blockers.
- **No player building:** Construction and land claims are disallowed inside protected boundaries.
- **Higher prices than player towns:** NPC convenience carries a premium to incentivize player-run settlement economies.
- **Onboarding hub:** Tutorial flow, basic vendors, and first social touchpoints are centered here.
- **Law baseline:** Crime expectations and consequences are introduced consistently through this zone.
- **Economy bootstrap:** Guaranteed minimum goods/services reduce early hard-lock scenarios.

---

# Claim System

Personal and structured claims are the fundamental anti-spam and ownership enforcement mechanism.

- **Claim placement rules:** Claims require clear terrain validation, distance checks, and no overlap with restricted zones.
- **Owner authority ID:** Every claim binds to a persistent owner authority identifier used for permissions, audits, and recovery.
- **Claim radius:** Each claim type has a defined radius budget governing placeable structures and interaction rights.
- **Build permissions:** Owner can assign build tiers (owner-only, trusted list, role-based in town context).
- **Storage permissions:** Containers and stockpiles inherit claim-level defaults with optional per-container overrides.
- **Raid vulnerability:** Claim type, region law state, and conflict status determine whether structural damage/entry is legal.
- **Abandonment rules:** Extended inactivity or unpaid upkeep transitions claims into vulnerable, then reclaimable state.
- **Reputation effects on claim protection:** High-crime or notorious owners may lose passive protections and gain shorter grace windows.

---

# Town Creation

Town founding is intentional, costly, and spatially constrained.

- **Town center placement:** Founder places a validated town center object in eligible frontier land.
- **Town name:** Unique, moderated naming with reserved-word filtering and cooldown on renamed identities.
- **Influence radius:** Initial civic radius starts modest and expands only through verified infrastructure.
- **Founder permissions:** Founder starts with governor authority, including role assignment and policy defaults.
- **Initial required resources:** Founding consumes a significant package (construction materials, tools, food/water reserves).
- **Minimum distance from starter town and other towns:** Hard spatial limits prevent immediate adjacency and preserve regional identities.
- **No instant city creation:** Large-scale services and influence must be earned over time through staged growth.

---

# Town Growth Stages

Recommended progression stages define expansion pacing and gameplay texture.

## 1) Camp

- **Required infrastructure:** Town center, basic shelter cluster, water access point, minimal food storage.
- **Influence radius growth:** Initial baseline radius only.
- **Unlocked services:** Limited public rest, basic trade board, simple storage permissions.
- **Risks:** High vulnerability to raids, shortages, and weather disruption.
- **Economy effects:** Primarily self-sufficiency; negligible market pull.

## 2) Homestead Cluster

- **Required infrastructure:** Multiple active dwellings, improved storage, stable supply node.
- **Influence radius growth:** Small radius increase around town center.
- **Unlocked services:** Resident assignment, expanded storage tiers, early craft exchange.
- **Risks:** Governance overhead begins; weak law enforcement coverage.
- **Economy effects:** Local barter market begins to form.

## 3) Outpost

- **Required infrastructure:** General store equivalent, stable, basic workshop (smithing tier 1).
- **Influence radius growth:** Moderate expansion enabling surrounding claim integration.
- **Unlocked services:** Public storefront access, basic service taxation, traveler utility services.
- **Risks:** Attractive raid target due to concentrated goods.
- **Economy effects:** Regional trade relevance starts; supply volatility remains high.

## 4) Settlement

- **Required infrastructure:** Sheriff office, clinic/doctor support, expanded water and food systems.
- **Influence radius growth:** Significant increase tied to public-service uptime.
- **Unlocked services:** Law role operations, wanted handling hooks, medical recovery support.
- **Risks:** Internal role conflict and upkeep burden increase.
- **Economy effects:** Stable multi-vendor marketplace with stronger price signals.

## 5) Frontier Town

- **Required infrastructure:** Hotel/lodging, advanced smithing, ranch/farm network, perimeter defenses.
- **Influence radius growth:** Broad civic radius with secondary service pockets.
- **Unlocked services:** Visitor economy scaling, larger event/trade hosting, structured civic roles.
- **Risks:** Political disputes and organized raid pressure.
- **Economy effects:** Becomes a trade-route anchor with export potential.

## 6) Established Town

- **Required infrastructure:** Full service stack, resilient logistics, defense layers, sustained civic staffing.
- **Influence radius growth:** Maximum designed radius within regional cap.
- **Unlocked services:** High-trust commerce, robust law and emergency response, advanced specialization.
- **Risks:** Strategic target status, upkeep complexity, dependence on trade security.
- **Economy effects:** Regional pricing influence and durable market depth.

---

# Infrastructure Requirements

Infrastructure defines functional legitimacy for growth and unlocks.

**Required core buildings (stage-gated):**

- General store
- Stable
- Blacksmith/Gunsmith
- Sheriff office
- Hotel
- Clinic/Doctor
- Food storage
- Water source
- Ranch/Farm structures

**Optional but high-impact structures:**

- Defensive structures (walls, reinforced gates, watch positions)
- Specialized workshops and processing stations
- Extended warehouse capacity
- Public utility improvements

Growth checks should validate both **presence** and **operational state** (fuel, stock, repair condition, staffing role assignment where required).

---

# Permissions & Roles

Role-driven permissions support public utility without removing ownership control.

- **Visitor:** Can enter public spaces and use explicitly public services.
- **Resident:** Lives in town territory with limited private access and participation rights.
- **Citizen/Member:** Core community participant with broader access and voting/eligibility hooks (future systems).
- **Builder:** Can place/modify approved structures in authorized zones.
- **Shop Owner:** Operates designated business plots and shop storage/pricing controls.
- **Sheriff/Law Role:** Can enforce local law actions within permitted mechanics.
- **Council/Manager:** Oversees operations, role assignments, zoning, and selected policy levers.
- **Founder/Governor:** Top authority; final override on governance configuration (within system constraints).

Public access should default to economy-critical interactions (market browsing, lodging, basic transit services), while member-only privileges protect strategic assets (high-value storage, governance controls, advanced production chains).

---

# Town Economy

Towns are intended as player-driven economic engines with controlled systemic scaffolding.

- **Public market access:** Non-members can buy/sell through approved public channels.
- **Player-owned shops:** Business plots support persistent storefront identity and inventory control.
- **Town taxes/fees:** Configurable local fees fund upkeep and defensive/public service costs.
- **Quality-based pricing:** Better-crafted goods and reliable services command premium pricing.
- **Supply/demand hooks:** Local scarcity and production capacity influence market valuations.
- **Starter town price floor:** NPC town sets a baseline convenience price to prevent total market collapse.
- **Trade route importance:** Connectivity and safety of routes materially affect town prosperity.

---

# Influence Radius

Influence radius is the core regional control expression for each town.

- **Influence affects permissions:** Role rules and build restrictions are applied based on territory inclusion.
- **NPC activity:** Patrols, vendor behavior, and service density scale with town maturity and influence.
- **Market density:** Trade volume and vendor concentration increase with stronger, stable influence.
- **Law enforcement:** Legal response and crime visibility are stronger in high-control zones.
- **Settlement growth:** Radius expansion gates new claims, civic services, and expansion eligibility.
- **Claim overlap rules:** Conflicting claims must resolve through explicit precedence and conflict-state rules.
- **Regional identity:** Influence boundaries define practical cultural/economic regions in the world.

---

# Decay & Collapse

To keep the world dynamic, settlements must decline when not maintained.

- **Inactivity decay:** Long inactivity begins staged degradation of claim/town protections.
- **Structure durability loss:** Buildings lose condition over time without maintenance resources.
- **Unpaid upkeep:** Town services and protection tiers downgrade when upkeep is unmet.
- **Food shortage:** Sustained shortages reduce service uptime and increase abandonment risk.
- **Abandoned towns:** Governance rights and protected permissions expire after abandonment thresholds.
- **Collapse back to frontier land:** Territory reverts to wilderness/frontier state after full collapse.
- **Ruins if appropriate:** Select structures may persist as salvageable ruins to preserve world history.
- **Reclaiming land after collapse:** Cleared rules allow new founders/claimants to legally retake territory.

---

# Raiding & Defense

Raid systems must create tension without invalidating progression.

- **Walls/doors durability:** Defensive elements have tiered durability and repair loops.
- **Breaking doors/walls:** Physical breach tools enable entry under valid raid conditions.
- **Explosives later:** Early prototype supports direct breach tools; explosive meta is delayed.
- **Declared war raids vs criminal raids:** Legal conflict state changes rule set, penalties, and target validity.
- **Reputation consequences:** Illegal raids carry escalating reputation and bounty consequences.
- **Starter town protected:** NPC safe zone is never raid-eligible.
- **New player protection philosophy:** Early progression windows should discourage veteran predation and grief loops.

---

# Clan / Co-op Ownership

Settlement systems must support groups without making solo play obsolete.

- **Towns as clan/group structures:** Town ownership and governance can be assigned to organized groups.
- **Shared claims:** Group-authorized claims allow coordinated expansion and specialization.
- **Shared storage permissions:** Role-based storage tiers support safe collaboration.
- **Co-op host world behavior:** Ownership authority is consistent with host persistence model and explicit member rights.
- **Solo homesteads remain viable:** Individual claims/homesteads remain progression-capable outside large-town play.

---

# Outlaw Hideouts

Outlaw territories run parallel to lawful settlement progression.

- **Parallel outlaw territory:** Distinct territory logic supports criminal identity and anti-law playstyles.
- **No lawful business ownership:** Outlaws cannot operate normal lawful commerce within sanctioned town systems.
- **Hidden storage:** Concealed caches emphasize stealth and risk mitigation.
- **Black market future:** Future expansion can enable illicit vendor networks and contraband circulation.
- **Vulnerable if discovered:** Discovery creates raid/legal pressure and potential forced displacement.
- **Reputation and bounty risk:** Persistent outlaw activity compounds long-term pursuit and economic restriction.

---

# Prototype Scope

First implementation should target a narrow, testable vertical feature slice:

- Personal claim
- Simple town center
- Influence radius
- 3-5 required buildings
- Basic permissions
- Durability/decay placeholder
- Starter town safe zone
- Save/load support

---

# Delayed Scope

The following systems are intentionally deferred:

- Advanced elections
- Complex taxation
- Full siege windows
- Advanced modular construction
- NPC population simulation
- Large-scale politics
- Roads/railroads
- Automated caravans

---

# Open Questions

- Exact claim sizes
- Town distance rules
- Upkeep cost formula
- Decay timer length
- War declaration requirements
- Max town radius
- How ruins should behave
- Co-op host migration rules

---

# Notes for Implementation Planning

This document establishes gameplay design intent and progression structure. Runtime implementation details (data models, authoritative networking contracts, save schema, and server rule enforcement) should be captured in follow-up technical design docs after prototype validation.
