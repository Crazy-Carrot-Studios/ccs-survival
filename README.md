# CCS Survival

[![Unity 6](https://img.shields.io/badge/Unity-6-blue)](https://unity.com/)
[![Version](https://img.shields.io/badge/Version-4.8.0-green)](https://github.com/Crazy-Carrot-Studios/ccs-survival/releases)
[![License](https://img.shields.io/badge/License-Proprietary-lightgrey)](#)

Modular survival gameplay framework for Unity 6 — built by **Crazy Carrot Studios** for reusable AAA-style survival prototypes.

**Repository:** https://github.com/Crazy-Carrot-Studios/ccs-survival  
**Upstream Core:** [ccs-framework](https://github.com/Crazy-Carrot-Studios/ccs-framework) (vendored under `Assets/CCS/Framework/`)

---

## Description

**CCS Survival** is a production-oriented, module-driven survival framework. It ships playable bootstrap integration, validation tooling, persistence, and a manual playtest harness — without locking the codebase to a single commercial title.

Install via Unity Package Manager (**Add package from git URL**):

```text
https://github.com/Crazy-Carrot-Studios/ccs-survival.git
```

---

## Current Version

**4.8.0** — NPC Settlement Affiliation Foundation

Persistent settlement, business, workforce, and region affiliation metadata for placeholder NPCs through `CCS_NpcAffiliationService` and `CCS_NpcAffiliationProfile`. Workers receive settlement + workforce affiliations; representatives receive settlement + business affiliations. Default loyalty 50 (0–100, metadata only). State persists on `CCS_SettlementSimulationState.npcAffiliationStates`. Labels show settlement ownership; debug HUD shows affiliation and loyalty. Playtest group **NPC Affiliations** with **Ctrl+Alt+F** shortcut.

**NPC Affiliation Loop:**

```text
Population Creates NPC → NPC Assigned Settlement → NPC Assigned Business/Workforce → NPC Becomes Part Of Community
```

**4.7.0** — NPC Activity State Foundation

Lightweight visible activity states for placeholder NPCs through `CCS_NpcActivityService` and `CCS_NpcActivityProfile`. Activities derive from schedule blocks (Working, Serving, Resting, Sleeping, Leisure, Idle) with **Traveling** override while movement status is `TravelingToWork` / `TravelingHome`. Dev labels and optional primitive cube indicators show current activity. State persists on `CCS_SettlementSimulationState.npcActivityStates`. No animations, dialogue, pathfinding, or final UI. Playtest group **NPC Activity** with **Ctrl+Alt+A** shortcut.

**NPC Activity Loop:**

```text
Schedule Selects Block → Movement Selects Destination → Activity Reflects Current Behavior → Settlement Feels More Alive
```

**4.6.0** — NPC Schedule State Foundation

Profile-driven daily schedule blocks for placeholder NPCs and service representatives through `CCS_NpcScheduleService` and `CCS_NpcScheduleProfile`. Role mappings select Worker or Service Representative schedules. Movement (`CCS_NpcMovementService`) resolves destinations from schedule blocks (housing, workplace/service point, settlement center) with profile work/home hour fallback when the schedule service is unavailable. State persists on `CCS_SettlementSimulationState.npcScheduleStates`. No NavMesh, advanced AI, dialogue, quests, or final NPC UI. Playtest group **NPC Schedule** with **Ctrl+Alt+S** shortcut.

**NPC Schedule Loop:**

```text
NPC Has Role → Role Selects Schedule → Schedule Selects Destination → Movement Sends NPC There → Future Routines Ready
```

**4.5.0** — NPC Movement Foundation

Transform-based placeholder movement driven by schedule blocks and time-of-day evaluation through `CCS_NpcMovementService`. Workforce placeholders travel between workplace and housing anchors; service representatives use service points. State persists on `CCS_SettlementSimulationState.npcMovementStates`. Playtest group **NPC Movement** with **Ctrl+Alt+M** shortcut.

**4.4.0** — Settlement Housing Foundation

Settlement-owned housing contributes **population capacity** through `CCS_SettlementHousingService` and primitive labeled markers (`CCS_SettlementHousingAnchor`). Total capacity = base population capacity + active housing capacity. Housing types: Worker Cabin (+10), Farmhouse (+12), Boarding House (+20), Mining Barracks (+25). Activation gates by growth stage (Outpost vs TradingPost). State persists on `CCS_SettlementSimulationState.housingStates`. Optional `homeHousingId` placeholder on NPC identity. No player housing, schedules, pathfinding, or final art. Playtest group **Settlement Housing** with **Ctrl+Alt+H** shortcut.

**Settlement Housing Loop:**

```text
Population Grows → Housing Capacity Matters → Housing Markers Show Settlement Life → Future NPC Homes / Schedules Ready
```

**4.3.0** — NPC Service Representatives Foundation

Active settlement businesses assign **named NPC service representatives** through `CCS_NpcServiceRepresentativeService`. Representatives use population placeholder actors (or synced anchors near service points) with **name + title** labels. Interaction routes through existing `CCS_SettlementServiceRouteResolver` (vendor, bank, industry, contract board). Service point cubes remain fallback. State persists on `CCS_SettlementSimulationState.npcServiceRepresentativeStates`. No AI, dialogue, schedules, quests, or pathfinding. Playtest group **NPC Service Representatives** with **Ctrl+Alt+R** shortcut.

**Service Representative Loop:**

```text
Business Activates → Representative Assigned → Player Talks To Named NPC → Existing Service Opens → Town Feels Human
```

**4.1.0** — NPC Identity and Role Foundation

Population placeholder actors receive **stable names and roles** through `CCS_NpcIdentityService`, `CCS_NpcIdentityProfile`, and `CCS_NpcRuntimeBridge`. Identities persist on `CCS_SettlementSimulationState.npcIdentityStates` via world simulation save/load. Labels show dev-readable text (e.g. `Elias Carter — Miner`). No AI, dialogue, schedules, quests, or pathfinding. Playtest group **NPC Identity** with **Ctrl+Shift+E** shortcut.

**NPC Identity Loop:**

```text
Population Grows → Placeholder Workers Appear → Workers Receive Names + Roles → Settlements Feel More Human
```

**4.0.0** — NPC Population Placeholder Foundation

Settlement **workforce population** drives idle primitive placeholder actors (`CCS_PopulationPresenceAnchor`, `CCS_PopulationPlaceholderActor`, `CCS_PopulationPresenceLabel`) capped by category counts from `CCS_SettlementPopulationSnapshot`. No AI, dialogue, schedules, or final character art. Playtest group **Population Presence** with **Ctrl+Shift+X** shortcut.

**Population Presence Loop:**

```text
Supply Settlement → Population Grows → Workforce Count Increases → Placeholder Workers Appear → Settlement Feels Alive
```

**3.9.0** — Settlement Visual Growth Foundation

Settlement **growth stages** drive visible primitive world markers through `CCS_SettlementVisualGrowthAnchor`, `CCS_SettlementVisualGrowthMarker`, and `CCS_SettlementVisualGrowthLabel`. Outpost markers are active by default; TradingPost markers activate when growth advances. Visual state derives from `CCS_SettlementGrowthSnapshot` (save/load through world simulation). Business presence markers remain independent. Playtest group **Settlement Visual Growth** with **Ctrl+Shift+Z** shortcut.

**Settlement Visual Growth Loop:**

```text
Complete Contracts → Settlement Grows → Stage Markers Activate → World Visibly Changes
```

**3.8.0** — Visible Business Presence Foundation

Active businesses gain **visible placeholder presence** in the bootstrap world through `CCS_BusinessPresenceAnchor`, `CCS_BusinessPresenceMarker`, and `CCS_BusinessPresenceLabel` components. Markers derive state from business simulation (`CCS_BusinessService` / `CCS_BusinessSnapshot`) and update on `BusinessActivated` / `BusinessDeactivated`. Linked settlement service points tint from presence status without duplicating access gating. Playtest group **Business Presence** with **Ctrl+Shift+V** shortcut.

**3.7.0** — Frontier Businesses Foundation

Settlements gain a simulation-level **business framework** (`CCS_BusinessProfile`, `CCS_BusinessService`, `CCS_BusinessValidationUtility`) that activates General Store, Stable, Gunsmith, Bank, Contract Office, Farm Supply, Mining Supplier, and Lumber Yard businesses from population, prosperity, growth stage, and optional reputation thresholds. Business activation persists on `CCS_SettlementSimulationState.businessStates` via world simulation save/load. Service points respect simulation activation through `CCS_BusinessRuntimeBridge`. Playtest group **Businesses** with **Ctrl+Shift+J** shortcut.

**Business Loop:**

```text
Population → Businesses Open → Services Expand → Prosperity Improves → Settlement Grows
```

**3.6.0** — Population Foundation

Settlements gain living **population simulation** through `CCS_SettlementPopulationProfile`, workforce categories (Farmers, Ranchers, Miners, Lumber Workers, Merchants, Laborers), growth rate, stability, and capacity on `CCS_SettlementSimulationState` (persisted via world simulation save/load). Growth scales from contract completion, prosperity, food supply health, and settlement reputation tier. **Settlement growth** gates now include population: Outpost **0+**, Trading Post **50+** with prosperity **35+** and **1+** completed contracts. Playtest group **Population** with **Ctrl+Shift+K** shortcut (homestead supply crate kit moved to **Ctrl+Alt+K**).

**Population Loop:**

```text
Supply Settlement → Population Grows → Workforce Expands → Production Improves → Settlement Develops
```

**3.5.0** — Route Risk and Freight Bonus Foundation

Trade route definitions add **risk rating** (`CCS_TradeRouteRiskLevel`: Safe, Low, Moderate active; Dangerous/Severe placeholders), **base/distance freight multipliers**, and wagon/route-condition placeholders. **FreightDelivery** contracts with `linkedTradeRouteId` scale trade-dollar rewards via `CCS_TradeRouteRewardModifierUtility` (base × route × risk, clamped non-negative); local contracts unchanged. Completion results and contract debug HUD expose base/final reward breakdown. Higher-risk routes grant conservative bonus destination reputation. Route risk is profile data; **usage counts** remain runtime-persisted from 3.4.0. Playtest group **Route Risk / Freight** with **Ctrl+Shift+Q** shortcut.

**Route Risk Freight Loop:**

```text
Accept Freight → Assess Route Risk → Load Wagon → Deliver Goods → Earn Risk-Adjusted Reward
```

**3.4.0** — Trade Routes and Freight Contracts

Trade route metadata includes **route difficulty**, **discovery/active** runtime state, and **usage counts** (`CCS_TradeRouteService`, save/load snapshots). **FreightDelivery** contracts link origin and destination settlement boards; completion prefers **wagon cargo** with safe failure when goods are missing. Outbound regional freight: Pine Ridge (lumber, charcoal), Broken Creek (corn, wheat), Iron Ridge (iron ore, coal) → Trading Post; Trading Post → camps mixed-supply placeholders. Playtest group **Trade Routes / Freight** with **Ctrl+Shift+F** shortcut.

**Freight Loop:**

```text
Produce Regional Goods → Load Wagon → Travel Route → Deliver To Destination → Increase Prosperity + Reputation
```

**3.3.0** — Multi-Settlement Foundation

Four independent frontier settlements (**Frontier Trading Post**, **Pine Ridge Camp**, **Broken Creek Farmstead**, **Iron Ridge Mining Camp**) with per-settlement world simulation, reputation, growth, and contract boards. Regional contract boards prioritize Timber, Agriculture, and Mining specialties. Metadata-only **trade routes** (`CCS_TradeRouteDefinition`, `CCS_TradeRouteProfile`, `CCS_TradeRouteSnapshot`) link settlements without transport simulation. Independent discovery — finding one settlement does not reveal others. Playtest group **Multi-Settlement** with **Ctrl+Shift+N** shortcut.

**Frontier Settlement Network:**

```text
Discover Settlement → Accept Regional Contract → Improve Prosperity + Reputation → Independent Save/Load
```

**3.2.0** — Settlement Growth Foundation

Generic **settlement growth** stages (`CCS_SettlementGrowthStage`, `CCS_SettlementGrowthDefinition`, `CCS_SettlementGrowthProfile`, `CCS_SettlementGrowthUtility`) evaluated from prosperity, food/industrial supply health, completed contracts, and region placeholder. **Outpost** and **TradingPost** are active; **FrontierTown** and **EstablishedTown** are placeholders. `CCS_WorldSimulationService` evaluates growth after contract completion, supply updates, prosperity recalc, and save/load restore. Frontier Trading Post starts as **Outpost** and can advance to **TradingPost** (prosperity ≥ 35, food supply ≥ 25%, completed contracts ≥ 1). Debug HUD shows growth stage and next-stage progress; location primitive color shifts on upgrade. Playtest group **Settlement Growth** with **Ctrl+Shift+G** shortcut. No NPC AI, quests, factions, procedural towns, or final town art.

**Settlement Growth Loop:**

```text
Complete Contracts → Improve Supply + Prosperity → Settlement Growth Progress → New Growth Stage → Future Services / Expansion
```

**3.0.0** — Frontier Contracts Foundation

Generic **Contracts** module: settlement delivery jobs (`CCS_ContractDefinition`, `CCS_ContractProfile`, `CCS_ContractService`) with item requirements, trade dollar / reputation / prosperity rewards, `ContractBoard` settlement routing, debug contract panel, save/load contract snapshots, and playtest harness group with **Ctrl+Shift+C** shortcut. Conservative starter catalog for general store, stable, gunsmith, and trading post boards. No factions, quests, law, NPC AI, or final contract UI.

**Frontier Contract Loop:**

```text
Discover Settlement → Accept Contract → Deliver Goods → Earn Rewards → Improve Settlement Prosperity / Supply
```

**2.8.0** — Service Access and Price Modifier Foundation

Settlement **service access rules** (`CCS_ServiceAccessRule`, `CCS_ServiceAccessProfile`, `CCS_ServiceAccessEvaluationUtility`) evaluate reputation tier/value before service activation. **Vendor buy price modifiers** (`CCS_ReputationPriceModifierUtility`) apply conservative tier discounts/markups from settlement trust; `CCS_VendorTransactionResult` reports base/final price and modifier. Missing reputation service falls back safely to modifier 1.0. Debug HUD shows access results and price breakdown. Playtest steps and **Ctrl+Shift+Y** shortcut cover access/modifier save-load stability. No factions, quests, law, NPC AI, or final UI.

**Frontier Trust Service Loop:**

```text
Trade / Pay Obligations → Settlement Trust Improves → Better Service Terms → Future Service Access Rules
```

**2.7.0** — Reputation and Service Trust Foundation

Generic **Reputation** module: settlement-scoped trust standings (`CCS_ReputationDefinition`, `CCS_ReputationProfile`, `CCS_ReputationService`), conservative event deltas from vendor sells, loan repay, upkeep pay/fail, and settlement discovery, save/load reputation snapshots, settlement service query/events, dev reputation debug HUD, and playtest harness steps. Scopes include Settlement (active), Region/Service placeholders, and FutureFaction/Global hooks. No factions, quests, law, vendor lockouts, or final reputation UI yet.

**Frontier Trust Loop:**

```text
Discover Settlement → Trade / Pay Obligations → Settlement Trust Changes → Future Service Access / Discounts / Law Hooks
```

**2.6.0** — Loans and Debt Foundation

Generic **loan/debt** extension on the Banking module: `CCS_LoanDefinition`, `CCS_LoanProfile`, borrow/repay through `CCS_BankingService`, Frontier Small Loan content (500 principal / 550 repayment Trade Dollars), bank-then-wallet repayment, save/load loan snapshots, Bank debug HUD loan panel with **Shift+L** / **Shift+P** hotkeys, and playtest harness steps. No compound interest, foreclosure, credit score, final bank UI, or NPC bankers yet.

**Frontier Loan Loop:**

```text
Need Capital → Borrow From Bank → Buy Supplies / Expand Claim → Earn Income → Repay Loan
```

**2.5.1** — Upkeep Release Cleanup

Release cleanup for the 2.5.0 Tax and Upkeep Foundation milestone: final `Upkeep.meta` tracking, upkeep release-safety validation (register, save/load, bank/wallet payment, safe failure, reconcile), and tag alignment. **v2.5.0** remains the feature milestone tag; **v2.5.1** points to the clean release tree on `main`.

**2.5.0** — Tax and Upkeep Foundation

Generic **Upkeep** module: recurring costs for land claims and future owned assets, Frontier Homestead Claim Tax definition, bank-then-wallet payment (no overdraft), save/load upkeep entries, Land Office debug HUD upkeep summary, and playtest harness steps. No debt, loans, foreclosure, or final tax UI yet.

**Frontier Upkeep Loop:**

```text
Claim Land → Earn Money → Deposit Savings → Pay Claim Tax / Upkeep → Maintain Legal Frontier Presence
```

**2.4.0** — Banking and Land Office Foundation

Generic **Banking** module: open frontier savings account, deposit/withdraw Trade Dollars via `CCS_CurrencyService`, save/load bank balance, Bank and Land Office settlement service points, and debug banking HUD with land claim summary. No loans, taxes, interest, debt, or final bank/deed UI yet.

**Frontier Finance Loop:**

```text
Earn Trade Dollars → Deposit Savings → Claim Land → Register Frontier Presence → Prepare For Taxes / Loans / Expansion
```

**2.3.0** — Land Ownership Foundation

Generic **Land** module: buy Homestead Claim Deed, preview claim radius, confirm placement, associate nearby structures, camp `landClaimId` tracking, and save/load claim state. No taxes, banks, or deeds UI yet.

**Land Claim Loop:**

```text
Earn Money → Buy Homestead Claim Deed → Claim Land → Build Inside Claim → Establish Legal Frontier Presence
```

**2.2.0** — Farming Foundation

Generic **Farming** module: place farm plots, plant seeds, timer-based crop growth, harvest food, vendor buy/sell, World Simulation **Food** supply, and save/load plot state. Primitive crop prefabs only (Corn, Beans, Potatoes, Wheat).

**Farming Loop:**

```text
Buy Seeds → Place Farm Plot → Plant Crop → Grow → Harvest Food → Sell / Supply Settlement
```

**2.1.2** — Play Mode Smoke + Bootstrap Scene Polish

Manual Play Mode smoke checklist on `SCN_CCS_Survival_Bootstrap`, bootstrap zone organization with world-space labels, persistence harness log-once waiting states, and console-clean defaults (`enableHarness` off unless explicitly enabled).

**2.1.1** — Input Asset Verification + Fishing Runtime Safety

Verifies `CCS_Survival_InputActions` wiring on `PF_CCS_Player`, documents Input System ownership, and fixes fishing spot startup `NullReferenceException` via null-safe `CCS_FishingRuntimeBridge` and deferred `CCS_FishingSpot` registration (matches `CCS_SleepSpot`).

**2.1.0** — Ranching Foundation

Generic **Ranching** module supports livestock ownership, ranch structure placement, timer-based egg/milk production, economy buy/sell, and world simulation Food supply updates. Bootstrap content includes chicken, goat, cow, pig placeholders, coop/pen/trough structures, and unified save persistence.

**Ranching Loop:**

```text
Buy Livestock
      ↓
Place Ranch Structure
      ↓
Produce Eggs / Milk
      ↓
Sell Goods
      ↓
Increase Settlement Food Supply
```

**2.0.0** — Frontier World Simulation Foundation

Generic **WorldSimulation** module tracks settlement supply, demand, production, and prosperity plus region resource potential metadata. Player vendor trades adjust frontier trading post supplies; prosperity derives from food, supply, and production ratios. Simulation state persists in unified save. Bootstrap profile links Frontier Trading Post to Pine Ridge Forest, Broken Creek, Iron Ridge Mine, and Frontier Trading Post Region.

**Frontier World Simulation Loop:**

```text
Gather Resources
      ↓
Trade Goods
      ↓
Settlement Supply Changes
      ↓
Prosperity Changes
      ↓
Frontier Evolves
```

**1.9.0** — Frontier Region Foundation

Generic **Regions** module organizes frontier world areas with discovery volumes, current-region tracking, settlement ownership metadata, and resource tags. Bootstrap regions: Pine Ridge Forest, Broken Creek, Iron Ridge Mine, and Frontier Trading Post Region. Region discoveries persist in unified save.

**1.8.1** — Settlement Services Polish + Blacksmith Routing

Settlement service routing cleanup with structured activation results (vendor, industry, placeholder, disabled, unavailable). **Blacksmith** at `CCS_TestTradingPost` routes to industry service summary — forge processes, recipes, and Primitive Forge requirement — without auto-craft or duplicate vendor logic. Availability flags on service points; playtest steps for blacksmith routing verification.

**1.8.0** — Frontier Settlement Expansion

Generic **Settlements** module with discovery tracking, service point interaction, and vendor routing through existing economy vendors. Bootstrap **CCS_TestTradingPost** includes General Store, Stable, Gunsmith, and Blacksmith placeholder. Settlement discovery persists in unified save.

**1.7.2** — Playtest Harness Cleanup + Input Action Consolidation

Dev hotkeys consolidated through `CCS_DevHotkeyUtility` / `CCS_KeyboardInputUtility` (New Input System only). Playtest HUD checklist grouped by survival domain. Validation scans ban legacy `UnityEngine.Input` and obsolete API usage.

**1.7.1** — Bootstrap Version Safety Cleanup

All legacy milestone bootstrap scripts now call `CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(...)` so re-running older bootstraps cannot downgrade `ProjectSettings.bundleVersion`. Validators use minimum-version checks instead of brittle exact pins. See **Bootstrap version policy** under Development Notes.

**1.7.0** — Prospecting and Mining Expansion

**Prospecting** frontier loop: iron/coal veins, stone outcrops, clay deposits, salvage mine debris, prospecting spot and abandoned mine placeholders, pick tier rules (primitive vs iron), industry iron refining, wagon bulk-haul weight hints, vendor buy paths for mining goods, gathering node save persistence, and playtest mining loop (Ctrl+Shift+M shortcut).

**Prospecting loop:** Find deposit → mine ore/coal → haul with wagon → refine at homestead → sell or craft better gear.

**1.6.0** — Firearm Foundation

Generic **Firearms** module with **Frontier Revolver, Rifle, and Shotgun**, ammunition crafting at the primitive forge, gunsmith economy, reload/fire active-item flow, wildlife hunting via existing combat raycast, firearm save/load, and playtest loop (Ctrl+Shift+G shortcut).

**1.5.2** — Wagon Foundation

Generic **Vehicles** module with **Frontier Wagon** ownership, summon/park, horse hitch follow, 24-slot wagon cargo, Frontier Stable wagon deed, vehicle save/load, and playtest wagon loop (Ctrl+Shift+W shortcut).

**1.5.1** — Horse Foundation

Generic **Mounts** module with **Frontier Horse** ownership, riding, saddlebag storage, Frontier Stable economy, camp horse presence, mount save/load, `Horse` camera placeholder, and playtest horse loop.

**1.5.0** — Frontier Industry Foundation

Generic **Industry** module for resource processing (wood → lumber, wood → charcoal, iron ore → refined iron) at Saw Table, Charcoal Kiln, and Primitive Forge workstations. Blacksmith forge recipes (tool heads, nails, horseshoe placeholder), iron tool upgrades, **IndustrialHomestead** camp tier, economy trade paths, industry save jobs, and playtest industry loop.

**1.4.1** — Frontier Homestead Foundation

Profile-driven camp tier ladder (TemporaryCamp → FrontierCamp → FrontierHomestead), placeable frontier storage (Supply Crate, Trapper Chest) and Frontier Workbench, camp ownership metadata on snapshots, General Store homestead kits, and playtest homestead progression with save/load tier restore.

**1.4.0** — Frontier Shelter Expansion

Frontier shelter kits (Lean-To, Tarp, Trapper), placeable shelter flow, camp tier tracking (shelter + campfire + bedroll → TemporaryCamp), camp/shelter save persistence, General Store cordage/canvas sales, and playtest frontier camp loop.

**1.3.4** — Cooking + Food Preservation Expansion

Campfire cook recipes for fish and all frontier meats, smoke-only jerky/dried fish preservation, consumable hunger tuning, General Store trail food trade, and playtest cooking loop (raw → cook → eat → preserve → sell).

**1.3.3** — Frontier Trapping Foundation

Placeable simple trap (preview + confirm), timer capture rolls for rabbit/turkey, knife harvest via wildlife harvest service, trap save/load persistence, and playtest trapping loop (craft → place → trigger → harvest → sell).

**1.3.2** — Frontier Hunting Foundation

Bow raycast hunting, knife wildlife harvest (skin/butcher drop tables), passive wildlife death/carcass state, General Store buys hunting trade goods (hide, meat, feather, fat, bone), and playtest hunting loop (bow → kill → harvest → sell).

**1.3.1** — Vendor Trading Polish + Tool Acquisition

Expanded General Store catalog, polished debug vendor HUD, safer buy/sell transactions, and hatchet trade progression (fish/salvage → dollars → hatchet → wood harvest). Knife-only starter unchanged.

**1.3.0** — Frontier Economy Foundation — currency/vendor framework, Trade Dollars, save-backed wallet, initial trade loop.

**1.2.6** — Frontier Starter Progression Rework — knife, camp, water, bow, fishing, traps, salvage, shelter, tools, and trade placeholders.

---

## Project Status

Playable modular survival prototype foundation.

**Current focus:** prototype gameplay systems, validation, persistence, and playtest harness.

---

## Core Features

| System | Status |
|--------|--------|
| Character Controller | Third-person Cinemachine 3.1 + CharacterController locomotion |
| Interaction | Forward-ray interactables |
| Inventory | Stack-based inventory foundation |
| Equipment | Wearable modifiers + primitive equipped visuals (socket rig) |
| Active Item | Service-driven select/use; weapons → combat, tools → gathering/harvest, fishing pole → fishing |
| Fishing | Service-driven water spots, catch tables, inventory rewards (no minigame yet) |
| Crafting | Recipes + workstation progression |
| World Resources / Gathering | Practical source types, harvest methods, multi-drop yields |
| Building | Placement, snapping, persistence |
| Cooking / Campfire | Campfire cooking foundation |
| Combat | Primitive combat foundation |
| Passive Wildlife | Passive wildlife AI foundation |
| Sleep / Bedroll | Sleep spots + bedroll flow |
| Storage | Container storage foundation |
| Save / Load | World + player persistence |
| Time of Day | Global game clock |
| Weather | Global weather state |
| Environment Effects | Temperature, wetness, exposure |
| Shelter | Environmental protection volumes |
| HUD / Playtest Harness | Runtime HUD + guided playtest steps |

---

## Latest Milestones

| Version | Milestone |
|---------|-----------|
| **1.4.1** | Frontier Homestead Foundation |
| **1.4.0** | Frontier Shelter Expansion |
| **1.3.4** | Cooking + Food Preservation Expansion |
| **1.3.3** | Frontier Trapping Foundation |
| **1.3.2** | Frontier Hunting Foundation |
| **1.3.1** | Vendor Trading Polish + Tool Acquisition |
| **1.3.0** | Frontier Economy Foundation |
| **1.2.6** | Frontier Starter Progression Rework |
| **1.2.5** | Fishing Foundation |
| **1.2.4** | Frontier Resource Framework Audit |
| **1.2.3** | Primitive Tool Use Routing Foundation |
| **1.2.2** | Active Item Slot + Use Flow Foundation |
| **1.2.1** | Held Item Pose + Socket Cleanup |
| **1.2.0** | Primitive Equipment Visual Foundation |
| **1.1.5** | AAA Third-Person Controller Feel Polish |
| **1.1.4** | Third-Person Controller Feel + README Polish |
| **1.1.3** | Sleep + Bedroll Foundation |
| **1.1.2** | Storage Container Foundation |
| **1.1.1** | Crafting Progression + Workstation Foundation |
| **1.1.0** | Building Progression Foundation |
| **1.0.3** | Manual Playtest Pass + Fixes |

---

## Architecture

```text
Survival → Modules → Core
```

| Layer | Role |
|-------|------|
| **Framework/Core** | Reusable CCS platform (runtime host, services, events) |
| **Modules** | Gameplay systems (`ccs.survival.*` module IDs) |
| **Survival** | Project composition — scenes, prefabs, profiles, validation |

**Rule:** Core must never reference survival or gameplay modules.

---

## Repository Layout

```text
Assets/CCS/
├── Framework/Core/          # Upstream-aligned platform
├── Modules/                 # Gameplay feature modules
│   ├── CharacterController/
│   ├── Interaction/
│   ├── Inventory/
│   ├── Crafting/
│   ├── Building/
│   ├── Combat/
│   ├── Wildlife/
│   ├── Sleep/
│   ├── Storage/
│   └── …
└── Survival/                # Project shell
    ├── Scenes/              # SCN_CCS_Survival_Bootstrap.unity
    ├── Prefabs/Player/      # PF_CCS_Player
    ├── Profiles/
    ├── Runtime/
    └── Documentation/
```

---

## Validation & Quality Gate

**Policy:** zero warnings, zero errors.

| Check | Entry |
|-------|--------|
| Batch compile | Unity opens project without compile errors |
| Survival validation | `CCS.Survival.Editor.Development.CCS_SurvivalValidationMenu.RunSurvivalValidation` |
| Module validators | Per-module `Validate*` batch methods |
| Bootstrap scene | Scene bootstrap validation utility |
| Windows build | `CCS_SurvivalBuildVerificationBuildRunner.ExecuteBatch` |

Batch commands and log paths: [Assets/CCS/Modules/README.md](Assets/CCS/Modules/README.md)

Third-person camera prefab setup (1.1.4+):

```text
CCS.Survival.Editor.Development.CCS_PlayerThirdPersonCameraBootstrapSetup.ExecuteBatch
```

---

## Latest Verified Build

| Item | Value |
|------|--------|
| Version | **4.8.0** |
| Output | `Builds/CCS_Survival_4.8.0_Windows/` (gitignored) |
| Scene | `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` |

Details: [Build verification](Assets/CCS/Survival/Documentation/CCS_Survival_Build_Verification.md)

---

## Development Notes

| Topic | Detail |
|-------|--------|
| Unity | Unity 6 |
| Camera | **Cinemachine 3.1** third-person follow (default) |
| Locomotion | `CharacterController` (animator root motion **OFF**) |
| Look | Reduced mouse sensitivity; safe pitch clamp |
| Architecture | Modular service-driven composition via bootstrap host |
| Bootstrap version policy | Milestone bootstraps must use `CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion)` — never hard-code `Regex.Replace(..., "bundleVersion: X.Y.Z")`. Update `CurrentMilestoneVersion` when cutting a release. Foundation validation scans for stale hard-coded writes. |
| Input policy | Gameplay reads `Assets/CCS/Survival/Input/CCS_Survival_InputActions.inputactions` via `CCS_CharacterInputActionProvider`. Dev-only hotkeys use `CCS_DevHotkeyUtility` / `CCS_KeyboardInputUtility`. **Legacy `UnityEngine.Input` is banned.** |

### Bootstrap version policy

1. **Single source of truth:** `CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion` (currently **4.8.0**).
2. **Bootstrap writes:** Every `*BootstrapSetup.cs` that touches `ProjectSettings.bundleVersion` must call `EnsureBundleVersionAtLeast(...)` so older scripts only bump forward, never downgrade.
3. **Validators:** Check `bundleVersion >= CurrentMilestoneVersion` via `AddBundleVersionValidationIssue`. Do not pin exact milestone strings that break on the next release.
4. **Log strings:** Historical milestone labels in `Debug.Log` or playtest copy may stay unchanged (e.g. wagon bootstrap still logs `1.5.2`).
5. **New milestones:** Bump `CurrentMilestoneVersion`, build runner output folder, README, then run full validation and Windows build verification.

Utility: `Assets/CCS/Survival/Editor/Development/Bootstrap/CCS_SurvivalBootstrapVersionUtility.cs`

---

## Git Hygiene

Do not commit local Unity churn:

- `Library/`, `Temp/`, `Logs/`, `UserSettings/`
- `Builds/` (development build output)
- Incidental `ProjectSettings` edits unless intentional

See `.gitignore`.

---

## Author / Studio

**James Schilz** — Crazy Carrot Studios
