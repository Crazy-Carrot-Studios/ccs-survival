# CCS Survival

[![Unity 6](https://img.shields.io/badge/Unity-6-blue)](https://unity.com/)
[![Version](https://img.shields.io/badge/Version-2.5.1-green)](https://github.com/Crazy-Carrot-Studios/ccs-survival/releases)
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
| Version | **2.4.0** |
| Output | `Builds/CCS_Survival_2.4.0_Windows/` (gitignored) |
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

1. **Single source of truth:** `CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion` (currently **2.4.0**).
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
