# CCS Survival — Module Roadmap

**Milestone baseline:** 0.9.2 — Primitive Tool & Weapon Foundation  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Item/tool/weapon classifications **defined** at **0.9.2**. Combat, wildlife, and durability deferred.

---

## Version sequence (foundation grouping)

| Version | Milestone |
|---------|-----------|
| **0.3.5 / 0.3.5a** | Survival framework quality gate (pre-gameplay) |
| **0.3.6** | Development / Framework Support Foundation |
| **0.3.7** | Survival Core (Health, Stamina, Hunger, Thirst, Fatigue, Temperature architecture) |
| **0.3.7a** | Module folder structure cleanup (`Assets/CCS/Modules/`) |
| **0.3.7b** | Foundation cleanup — commit default profile, remove temporary editor tooling |
| **0.3.8** | Character Controller (`Assets/CCS/Modules/CharacterController/`) |
| **0.3.8a** | Batch validation runner + Survival Core editor asmdef fix |
| **0.3.9** | Interaction |
| **0.4.0** | Inventory |
| **0.4.1** | Equipment |
| **0.4.1a** | Equipment carry capacity expansion |
| **0.4.1b** | Prototype scene build verification |
| **0.4.1c** | README & repository presentation cleanup |
| **0.4.1d** | Repository sanitization check |
| **0.4.2** | UI/HUD foundation |
| **0.4.2a** | HUD readability and anchor pass |
| **0.4.3** | HUD runtime wiring pass |
| **0.5.0** | Crafting module foundation |
| **0.5.1** | World resource module foundation |
| **0.5.2** | Resource harvesting integration |
| **0.5.3** | Crafting gameplay integration |
| **0.6.0** | Save / load foundation |
| **0.6.1** | Save / load debug controls |
| **0.6.2** | Inventory & equipment persistence |
| **0.7.0** | Time of day foundation |
| **0.7.1** | Weather foundation |
| **0.7.2** | Environment effects foundation |
| **0.7.3** | Survival Core environment integration |
| **0.7.4** | Clothing & equipment environmental modifiers |
| **0.7.5** | Shelter & environmental protection foundation |
| **0.8.0** | Building foundation |
| **0.8.3** | Building snapping foundation |
| **0.8.2** | Building construction costs and placement validation |
| **0.8.1** | Building placement foundation |
| **0.8.4** | Building persistence restore |
| **0.8.5** | Building shelter integration |
| **0.9.0** | Character controller gameplay integration (player prefab, input, interaction, stamina) |
| **0.9.1** | Starter loadout and primitive progression (knife, branches, hand crafting placeholders) |
| **0.9.2** | Primitive tool and weapon foundation (archetypes, tiers, bone tools, equipment verification) |
| **0.9.2a** | Bootstrap ground collider hotfix (playable floor in bootstrap test scene) |
| **0.9.3** | Wildlife resource foundation (carcass harvesting, Bone/Hide/Sinew/Raw Meat drops) |
| **0.9.4** | Campfire & cooking foundation (campfire placement, Raw Meat → Cooked Meat, hunger restore) |
| **0.9.5** | Consumables & hunger usage (passive drain, thresholds, F consume, HUD feedback) |
| **0.9.6** | Sleep & bedroll foundation (time advance, fatigue restore, shelter modifier) |
| **0.9.7** | Passive wildlife AI (wander, idle, flee; living rabbit/deer placeholders) |
| **0.9.8** | Primitive combat foundation (melee hunting, wildlife health, carcass spawn on kill) |
| **0.9.9** | Resource gathering foundation (SmallTree/Rock/Bush nodes, stick/stone/wood/fiber rewards) |
| **1.0.0** | Campfire + cooking foundation (fuel-backed rabbit/venison recipes, full gather-hunt-cook-eat loop) |
| **1.0.1** | Death, respawn & save foundation (unified save file, starvation/dehydration death, respawn point) |
| **1.0.2** | Manual playtest harness (bootstrap checklist HUD, event-driven step completion, F7/F10/F11/F12) |
| **1.0.3** | Manual playtest pass + fixes (bootstrap route verified, gather/death/equip/build blockers resolved) |
| **1.1.0** | Building progression foundation (tier-1 primitive shelter, recipes, placement rules, shelter playtest step) |
| **1.1.1** | Crafting progression + workstation foundation (hand / campfire / workbench recipes, test workbench, playtest step) |

Later milestones continue from **1.1.2+** (durability, ranged weapons, predators).

---

## 1.1.1 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_CraftingProgressionProfile` with hand, FirePit, and workbench recipes | **Complete** |
| `CCS_CraftingRecipeService` with progression events | **Complete** |
| `CCS_TestWorkbench` + campfire station interactables | **Complete** |
| Playtest **Craft at workbench** step | **Complete** |
| Validation 0 warnings / 0 errors | **Pending batch** |
| Version **1.1.1** build | **Pending batch** |
| Production crafting UI | **Deferred** |

---

## 1.1.0 definition of done

| Criterion | Status |
|-----------|--------|
| Tier-1 primitive pieces (foundation, wall, doorway, floor, roof) | **Complete** |
| `CCS_BuildingRecipeService` with costs and placement authorization | **Complete** |
| Playtest **Build shelter** step (foundation + wall + roof) | **Complete** |
| Validation 0 warnings / 0 errors | **Pending batch** |
| Version **1.1.0** build | **Pending batch** |
| Production building UI | **Deferred** |

---

## 1.0.3 definition of done

| Criterion | Status |
|-----------|--------|
| Full bootstrap manual route documented and exercised (code-path + fixes) | **Complete** |
| Gather wood/stick detection fixed | **Complete** |
| F7 reliable death trigger | **Complete** |
| F6 equip spear + B place foundation dev helpers | **Complete** |
| Validation 0 warnings / 0 errors | **Complete** |
| Version **1.0.3** build | **Complete** |
| Production equip/build UI | **Deferred** |

---

## 1.0.2 definition of done

| Criterion | Status |
|-----------|--------|
| Playtesting module under `Assets/CCS/Modules/Playtesting/` | **Complete** |
| `CCS_PlaytestService` with bootstrap checklist and module event hooks | **Complete** |
| `CCS_PlaytestHud` on bootstrap prefab/scene (F10/F11/F12, F7 test death) | **Complete** |
| Default profile with all required step types | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **1.0.2** | **Complete** |
| Automated gameplay bot, production UI polish | **Deferred** |

---

## 1.0.1 definition of done

| Criterion | Status |
|-----------|--------|
| SaveSystem module under `Assets/CCS/Modules/SaveSystem/` | **Complete** |
| `CCS_SaveService` writes `CCS_Survival_Save.json` with player, needs, inventory, world state | **Complete** |
| PlayerDeath module under `Assets/CCS/Modules/PlayerDeath/` | **Complete** |
| Hunger/thirst depletion triggers respawn at bootstrap spawn point | **Complete** |
| `CCS_SaveStartupLoader` loads save on play or applies starter loadout | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **1.0.1** | **Complete** |
| Death UI, multiple save slots, cloud save | **Deferred** |

---

## 1.0.0 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_CookingStation` and `CCS_CookingInteractable` on bootstrap campfire | **Complete** |
| Fuel-backed rabbit and venison recipes | **Complete** |
| Species-specific raw/cooked meat items in inventory catalog | **Complete** |
| `CCS_CookingService.TryStartCooking(station, recipeId)` with fuel consumption | **Complete** |
| Cooked food restores more hunger than raw meat | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **1.0.0** | **Complete** |
| Cooking UI, fuel burn simulation, recipe picker | **Deferred** |

---

## 0.9.9 definition of done

| Criterion | Status |
|-----------|--------|
| Gathering module under `Assets/CCS/Modules/Gathering/` | **Complete** |
| `CCS_GatheringService` registers nodes and grants inventory rewards | **Complete** |
| SmallTree, Rock, and Bush bootstrap nodes with configured rewards | **Complete** |
| `CCS_GatheringInteractable` uses existing interaction input | **Complete** |
| Optional node respawn after depletion | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.9** | **Complete** |
| Tool requirements, gather UI, biome spawning | **Deferred** |

---

## 0.9.8 definition of done

| Criterion | Status |
|-----------|--------|
| Combat module under `Assets/CCS/Modules/Combat/` | **Complete** |
| `CCS_CombatService` camera-forward SphereCast melee | **Complete** |
| Knife (10 dmg / 2 m) and spear (20 dmg / 3 m) on items | **Complete** |
| `CCS_WildlifeDamageable` on living rabbit (20 HP) and deer (50 HP) | **Complete** |
| Kill spawns carcass; living agent removed; harvest retained | **Complete** |
| `CCS_PlayerCombatDriver` on PrimaryAction input | **Complete** |
| HUD hit/kill notifications | **Complete** |
| Bootstrap spear → rabbit hunt path | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.8** | **Complete** |
| Humanoid enemies, predators, animations, multiplayer combat | **Deferred** |

---

## 0.9.7 definition of done

| Criterion | Status |
|-----------|--------|
| Wildlife AI under `Assets/CCS/Modules/Wildlife/Runtime/AI/` | **Complete** |
| `CCS_WildlifeAgent` wander/idle/alert/flee state machine | **Complete** |
| Transform movement without NavMesh or Rigidbody | **Complete** |
| Player proximity flee for rabbit and deer | **Complete** |
| Bootstrap `CCS_TestRabbit` and `CCS_TestDeer` living placeholders | **Complete** |
| Carcass harvest systems remain available | **Complete** |
| Optional upper-right HUD wildlife debug readout | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.7** | **Complete** |
| Combat, damage, death, predators, animations | **Deferred** |

---

## 0.9.6 definition of done

| Criterion | Status |
|-----------|--------|
| Sleep module under `Assets/CCS/Modules/Sleep/` | **Complete** |
| `CCS_SleepService` advances time and restores fatigue | **Complete** |
| Bedroll item, equipment, and Hide/Fiber hand recipe | **Complete** |
| Bedroll required for sleep; shelter penalty when unsheltered | **Complete** |
| Bootstrap `CCS_SleepTestArea` with rest point interactable | **Complete** |
| HUD notifications for sleep success/failure | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.6** | **Complete** |
| Dreams, enemy interruption, death, sleep UI | **Deferred** |

---

## 0.9.5 definition of done

| Criterion | Status |
|-----------|--------|
| Passive hunger drain via `hungerDrainPerSecond` on Survival Core profile | **Complete** |
| Hunger states Normal / Low / Critical / Empty via `CCS_HungerStateUtility` | **Complete** |
| Basic Food (+15) and Cooked Meat (+40) consumable restore values | **Complete** |
| `CCS_ConsumableFoodService` cooldown, fullness checks, Cooked Meat priority | **Complete** |
| F key consume binding (temporary developer gameplay binding) | **Complete** |
| HUD hunger state label and threshold/consume notifications | **Complete** |
| No starvation health damage or death | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.5** | **Complete** |
| Starvation damage, buffs, cooking UI, spoilage | **Deferred** |

---

## 0.9.4 definition of done

| Criterion | Status |
|-----------|--------|
| Cooking module under `Assets/CCS/Modules/Cooking/` | **Complete** |
| Campfire placement via Building framework (Campfire Kit → Campfire) | **Complete** |
| `CCS_CampfireInteractable` light + cook interactions | **Complete** |
| Raw Meat → Cooked Meat cooking loop (5s default) | **Complete** |
| Basic Food + Cooked Meat hunger restore via Survival Core | **Complete** |
| Bootstrap `CCS_CampfireTestArea` near wildlife test area | **Complete** |
| HUD notifications: Campfire Lit, Cooking Started/Complete, Ate Cooked Meat | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.4** | **Complete** |
| Fuel systems, cooking UI, health restore | **Deferred** |

---

## 0.9.3 definition of done

| Criterion | Status |
|-----------|--------|
| Wildlife module under `Assets/CCS/Modules/Wildlife/` | **Complete** |
| Carcass harvest via `CCS_HarvestableWildlife` + `CCS_WildlifeHarvestService` | **Complete** |
| Drops: Bone, Hide, Sinew, Raw Meat | **Complete** |
| Bootstrap test carcasses in `SCN_CCS_Survival_Bootstrap` | **Complete** |
| HUD notifications for wildlife harvest events | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.3** | **Complete** |
| AI, combat, predators, spawning | **Deferred** |

---

## 0.9.2a definition of done

| Criterion | Status |
|-----------|--------|
| Bootstrap scene ground has solid collider (`CCS_BootstrapTestGround`) | **Complete** |
| Ground collider enabled and not trigger | **Complete** |
| Player spawn above ground collider | **Complete** |
| Validation fails when bootstrap scene lacks playable ground | **Complete** |
| Version **0.9.2a** | **Complete** |

---

## 0.9.2 definition of done

| Criterion | Status |
|-----------|--------|
| Tool archetypes: Knife, Hatchet, Pick, Shovel (+ Bone variants) | **Complete** |
| Weapon archetypes: Knife, Spear (+ Bow/Club enum placeholders) | **Complete** |
| Tool tier system: Primitive → Bone → Stone → Iron → Steel | **Complete** |
| Resource placeholders: Bone, Sinew, Hide | **Complete** |
| Bone + Branch crafting recipes for bone tools | **Complete** |
| Equipment definitions and equip verification harness | **Complete** |
| Harvest resolves equipped tools (MainHand + Tool slots) | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.2** | **Complete** |
| Combat, wildlife, durability loss | **Deferred** |

---

## 0.9.1 definition of done

| Criterion | Status |
|-----------|--------|
| Starter loadout profile (`CCS_DefaultStarterLoadoutProfile`) | **Complete** |
| Starter items: Knife, Basic Food, Coin, Branch, Spear, Bow Stave, Arrow Shaft, Campfire Kit | **Complete** |
| Knife tool identity satisfies `RequiredToolType.Knife` harvest checks | **Complete** |
| Test tree harvest: Knife → Branch x2–4 | **Complete** |
| Primitive hand recipes (Spear, Bow Stave, Arrow Shafts, Campfire Kit) | **Complete** |
| `CCS_StarterLoadoutService` with empty-inventory guard | **Complete** |
| Bootstrap composition + HUD inventory summary lines | **Complete** |
| Rock formation sourcing documented (implementation deferred) | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.9.1** | **Complete** |
| Combat, animals, vendors, final economy | **Deferred** |

---

## Folder policy

| Path | Purpose |
|------|---------|
| `Assets/CCS/Modules/` | Gameplay modules — one folder per feature |
| `Assets/CCS/Survival/` | Project shell — bootstrap, scenes, profiles, composition, roadmap docs |
| `Assets/CCS/Framework/` | Reusable Core Platform |

Do **not** add gameplay modules under `Assets/CCS/Survival/Runtime/<ModuleName>/`.

## Roadmap policy

- Manual module registration through `CCS_SurvivalInstaller` (explicit install order).
- Module IDs use reverse-DNS prefix `ccs.survival.*`.
- Editor validation uses **registrable validators** via `CCS_SurvivalValidationPipeline` — modules add validators; menus do not hard-code checks.
- Scene bootstrap profiles declare **Required Services**, **Required Scene Objects**, and **Optional Scene Objects** — modules append entries without changing bootstrap architecture.
- No gameplay mechanics in Core; survival gameplay stays in this repository.

---

## Recommended gameplay module order (after 0.3.6)

| Order | Version (target) | Module area | Module ID (planned) |
|------:|------------------|-------------|---------------------|
| — | **0.3.6** | **Development / Framework Support** | `ccs.survival.development` (support layer) — **Complete** |
| — | 0.3.0 | Character skeleton | `ccs.survival.character` — **Installed** |
| 1 | **0.3.7** | **Survival Core** | `ccs.survival.core` — **Complete** (foundation) |
| 2 | **0.3.8** | **Character Controller** | `ccs.survival.movement` — **Complete** (foundation) |
| 3 | **0.3.9** | **Interaction** | `ccs.survival.interaction` — **Complete** (foundation) |
| 4 | **0.4.0** | **Inventory** | `ccs.survival.inventory` — **Complete** (foundation) |
| 5 | **0.4.1** | **Equipment** | `ccs.survival.equipment` — **Complete** (foundation) |
| 6 | **0.4.2 / 0.4.2a / 0.4.3** | **UI / HUD** | `ccs.survival.ui` — **Foundation + runtime wiring complete** |
| 7 | **0.5.0 / 0.5.3** | **Crafting** | `ccs.survival.crafting` — **Foundation + gameplay integration complete** |
| 8 | **0.5.1 / 0.5.2** | **World Resources** | `ccs.survival.world.resources` — **Foundation + harvest integration complete** |
| 9 | **0.6.0 – 0.6.2** | **Save / Load** | `ccs.survival.saveload` — **Foundation + inventory/equipment persistence complete** |
| 10 | **0.7.0** | **Time Of Day** | `ccs.survival.timeofday` — **Foundation complete** |
| 11 | **0.7.1** | **Weather** | `ccs.survival.weather` — **Foundation complete** |
| 12 | **0.7.2** | **Environment Effects** | `ccs.survival.environment` — **Foundation complete** |
| 13 | **0.7.3** | **Survival Core Environment Integration** | `ccs.survival.core` — **Environment integration complete** |
| 14 | **0.7.4** | **Clothing & Equipment Environmental Modifiers** | `ccs.survival.equipment` + `ccs.survival.environment` — **Complete** |
| 15 | **0.7.5** | **Shelter & Environmental Protection** | `ccs.survival.shelter` — **Complete** |
| 16 | **0.8.0** | **Building** | `ccs.survival.building` — **Foundation complete** |
| 17 | **0.8.1** | **Building Placement** | `ccs.survival.building` — **Placement foundation complete** |
| 18 | **0.8.2** | **Building Construction Costs** | `ccs.survival.building` — **Inventory build costs and validation complete** |
| 19 | **0.8.3** | **Building Snapping** | `ccs.survival.building` — **Snap foundation complete** |
| 20 | **0.8.4** | **Building Persistence Restore** | `ccs.survival.building` — **Placed instance save/load restore complete** |
| 21 | **0.8.5** | **Building Shelter Integration** | `ccs.survival.building` + `ccs.survival.shelter` — **Building shelter contributions complete** |
| 18 | 0.4.x | Loot / Spawn | `ccs.survival.loot` |
| 19 | 0.5.x | Combat | `ccs.survival.combat` |
| 20 | 0.5.x | AI / Wildlife | `ccs.survival.ai` |
| 22 | 0.8.6+ | Building structural rules | `ccs.survival.building` |
| 22 | 0.5.x | Quests / Objectives | `ccs.survival.quests` |
| 23 | 0.5.x | Audio | `ccs.survival.audio` |
| 24 | 0.5.x | Settings finalization | `ccs.survival.settings` (player-facing preferences UI) |

**Rationale:** UI immediately after Inventory lets every subsequent system be validated on-screen as it is built.

---

## Testing workflows (not gameplay modules)

| Folder | Purpose |
|--------|---------|
| `Runtime/Development/Testing/Traversal/` | Automated traversal route tests |
| `Runtime/Development/Testing/Simulation/` | Survival simulation / vitals smoke |
| `Runtime/Development/Testing/Inventory/` | Inventory smoke tests |
| `Runtime/Development/Testing/SaveLoad/` | Save/load round-trip tests |

Controlled by `CCS_SurvivalTestToggleProfile` and `CCS_SurvivalTestRuntimeFlags`. No automation in **0.3.6**.

---

## 0.3.6 definition of done

| Criterion | Status |
|-----------|--------|
| Diagnostics foundation (Info / Warning / Error severity) | **Complete** |
| Validation framework (registrable validators + central pipeline) | **Complete** |
| Testing framework (folders + toggle profile reserved categories) | **Complete** |
| Settings foundation | **Complete** |
| Scene bootstrap foundation (required/optional services + scene objects) | **Complete** |
| Documentation updated | **Complete** |
| Version **0.3.6** | **Complete** |
| Git committed and pushed | **Verify** |
| Unity compiles with zero errors | **Verify in Editor** |
| Working tree clean | **Verify** |

---

## Next milestone

**0.8.6 — Building Structural Rules** (support checks and advanced snap constraints)

---

## 0.8.5 definition of done

| Criterion | Status |
|-----------|--------|
| Shelter contribution fields on building definitions | **Complete** |
| `CCS_BuildingShelterContribution` model | **Complete** |
| `GetShelterContributions()` / `RecalculateShelterContributions()` | **Complete** |
| Shelter service max-radius building integration | **Complete** |
| Building → Shelter → Environment flow | **Complete** |
| Integration test harness + HUD lines | **Complete** |
| Version **0.8.5** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.8.4 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_BuildingSaveData` v3 with placed instance records | **Complete** |
| Snap occupancy and placement order persistence | **Complete** |
| `CCS_BuildingDefinitionLookup` for restore | **Complete** |
| `RestoreState()` recreates instances, snap points, and visuals | **Complete** |
| `CCS_BuildingInstanceVisualFactory` shared by placement and restore | **Complete** |
| `CCS_BuildingPersistenceTestHarness` save/clear/load verification | **Complete** |
| HUD saved/restored building count lines | **Complete** |
| Version **0.8.4** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.8.3 definition of done

| Criterion | Status |
|-----------|--------|
| Snap point types and definition authoring | **Complete** |
| Runtime snap points with occupancy on placed instances | **Complete** |
| Snap compatibility utility with explicit rules | **Complete** |
| `FindBestSnapMatch`, `UpdatePreviewWithSnap`, `PlaceCurrentPieceUsingSnap` | **Complete** |
| Harness foundation → wall → roof snap sequence | **Complete** |
| HUD snap target and placement validity lines | **Complete** |
| Version **0.8.3** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.8.2 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_BuildingCostEntry` on piece definitions | **Complete** |
| `CCS_BuildingPlacementValidationUtility` validate + consume + rollback | **Complete** |
| `TryPlaceCurrentPiece()` inventory-aware placement flow | **Complete** |
| HUD placement success/failure notifications | **Complete** |
| Harness resource seeding and `TryPlaceCurrentPiece()` cycling | **Complete** |
| Bootstrap definition build costs (Wood/Stone/Fiber) | **Complete** |
| Version **0.8.2** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.8.1 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_BuildingInstance` placed structure model | **Complete** |
| `CCS_BuildingPlacementService` with build mode API | **Complete** |
| `CCS_BuildingPlacementPreview` development preview | **Complete** |
| `CCS_BuildingPlacementTestHarness` automated placement | **Complete** |
| Bootstrap `CCS_BuildingTestArea` hierarchy | **Complete** |
| Placed instance save records in `CCS_BuildingSaveData` | **Complete** |
| HUD placement debug lines | **Complete** |
| Version **0.8.1** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.8.0 definition of done

| Criterion | Status |
|-----------|--------|
| Building module under `Assets/CCS/Modules/Building/` | **Complete** |
| `CCS_BuildingPieceDefinition` and piece types | **Complete** |
| `CCS_BuildingService` with events and save/load | **Complete** |
| Test definition assets (foundation, wall, roof) | **Complete** |
| Save restore order: building after environment | **Complete** |
| HUD building definition count display | **Complete** |
| Validation pipeline updates | **Complete** |
| Version **0.8.0** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.7.5 definition of done

| Criterion | Status |
|-----------|--------|
| Shelter module under `Assets/CCS/Modules/Shelter/` | **Complete** |
| `CCS_ShelterService` with events and save/load | **Complete** |
| `CCS_ShelterVolume` trigger volume foundation | **Complete** |
| Environment Effects shelter binding and protection chain | **Complete** |
| Save restore order: shelter before environment | **Complete** |
| Bootstrap test volume and development harness | **Complete** |
| HUD shelter debug display | **Complete** |
| Validation pipeline updates | **Complete** |
| Version **0.7.5** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.7.4 definition of done

| Criterion | Status |
|-----------|--------|
| Equipment definition environmental modifier fields | **Complete** |
| `CCS_EquipmentEnvironmentalModifierSnapshot` and aggregation | **Complete** |
| `CCS_EnvironmentEffectsService` reads equipment modifiers | **Complete** |
| Raw vs effective values on environment snapshot | **Complete** |
| Survival Core uses effective environment values | **Complete** |
| Bootstrap environment HUD resistance/effective display | **Complete** |
| Test equipment assets (Warm Hat, Heavy Coat, Waterproof Boots) | **Complete** |
| Validation pipeline updates | **Complete** |
| Version **0.7.4** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.7.3 definition of done

| Criterion | Status |
|-----------|--------|
| Survival Core profile environment tuning fields | **Complete** |
| `CCS_SurvivalEnvironmentInfluence` model and events | **Complete** |
| `CCS_SurvivalCoreService` reads Environment snapshots | **Complete** |
| Temperature, fatigue, and thirst pressure applied | **Complete** |
| Bootstrap influence HUD panel | **Complete** |
| No Health modification or death systems | **Complete** |
| Validation pipeline updates | **Complete** |
| Version **0.7.3** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.7.2 definition of done

| Criterion | Status |
|-----------|--------|
| Environment Effects module under `Assets/CCS/Modules/EnvironmentEffects/` | **Complete** |
| Global environment service with temperature, wetness, and exposure | **Complete** |
| Default environment effects profile asset | **Complete** |
| One-way Time Of Day and Weather integration | **Complete** |
| Save/load integration via `CCS_ISaveable` | **Complete** |
| Bootstrap HUD environment display | **Complete** |
| Validation pipeline registration | **Complete** |
| Version **0.7.2** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.7.1 definition of done

| Criterion | Status |
|-----------|--------|
| Weather module under `Assets/CCS/Modules/Weather/` | **Complete** |
| Global weather service with transitions and events | **Complete** |
| Default weather profile asset | **Complete** |
| One-way Time Of Day integration | **Complete** |
| Save/load integration via `CCS_ISaveable` | **Complete** |
| Bootstrap HUD weather display | **Complete** |
| Validation pipeline registration | **Complete** |
| Version **0.7.1** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.7.0 definition of done

| Criterion | Status |
|-----------|--------|
| TimeOfDay module under `Assets/CCS/Modules/TimeOfDay/` | **Complete** |
| Global game clock service with phases and events | **Complete** |
| Default time-of-day profile asset | **Complete** |
| Save/load integration via `CCS_ISaveable` | **Complete** |
| Bootstrap HUD time display | **Complete** |
| Validation pipeline registration | **Complete** |
| Version **0.7.0** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.6.2 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_PlayerInventoryService` implements `CCS_ISaveable` | **Complete** |
| `CCS_PlayerEquipmentService` implements `CCS_ISaveable` | **Complete** |
| Versioned save payloads (`saveDataVersion`) | **Complete** |
| Registry load order: inventory before equipment | **Complete** |
| Save debug panel registration indicators | **Complete** |
| Persistence test harness (harvest/craft/equip/save/load) | **Complete** |
| Version **0.6.2** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.6.1 definition of done

| Criterion | Status |
|-----------|--------|
| Manual save/load/delete debug controller | **Complete** |
| Minimal bootstrap HUD debug panel | **Complete** |
| Slot listing and last result display | **Complete** |
| Save path display via path utility | **Complete** |
| Validation covers debug components and path utility | **Complete** |
| Version **0.6.1** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.6.0 definition of done

| Criterion | Status |
|-----------|--------|
| Save/load module under `Assets/CCS/Modules/SaveLoad/` | **Complete** |
| `CCS_ISaveable`, registry, JSON save service | **Complete** |
| Default save/load profile asset | **Complete** |
| `CCS_SaveLoadService` registered on bootstrap | **Complete** |
| Development test saveable component | **Complete** |
| Version **0.6.0** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.5.3 definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_CraftingService` registered on bootstrap gameplay host | **Complete** |
| Test recipes and craft output items created | **Complete** |
| Craft validates capacity before consuming ingredients | **Complete** |
| Craft rollback on grant failure | **Complete** |
| HUD crafting notifications + inventory refresh | **Complete** |
| Development crafting test harness | **Complete** |
| Version **0.5.3** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.5.2 definition of done

| Criterion | Status |
|-----------|--------|
| Harvestable resources implement interaction contract | **Complete** |
| Harvest adds drops through `CCS_PlayerInventoryService` | **Complete** |
| HUD inventory summary refreshes on harvest | **Complete** |
| Harvest/depletion/respawn HUD notifications | **Complete** |
| Resource harvest/respawn services registered on bootstrap | **Complete** |
| Bootstrap test nodes + development harness | **Complete** |
| Version **0.5.2** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.5.1 definition of done

| Criterion | Status |
|-----------|--------|
| World Resources module under `Assets/CCS/Modules/WorldResources/` | **Complete** |
| Resource, drop, tool, and node type definitions | **Complete** |
| Harvest and respawn services + harvestable component | **Complete** |
| Default world resource profile asset | **Complete** |
| Bootstrap test tree/rock/plant placeholders | **Complete** |
| World resource validation registered | **Complete** |
| No final art / terrain / save / resource UI | **Complete** |
| Version **0.5.1** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.5.0 definition of done

| Criterion | Status |
|-----------|--------|
| Crafting module under `Assets/CCS/Modules/Crafting/` | **Complete** |
| Recipe, ingredient, result ScriptableObject definitions | **Complete** |
| Station types (Hand, FirePit, Workbench, Forge, Apothecary) | **Complete** |
| `CCS_CraftingService` with inventory integration | **Complete** |
| Crafting events and profile | **Complete** |
| Default crafting profile asset | **Complete** |
| Crafting validation registered | **Complete** |
| No crafting UI / world stations / save-load | **Complete** |
| Version **0.5.0** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.4.3 definition of done

| Criterion | Status |
|-----------|--------|
| Gameplay services registered on bootstrap startup | **Complete** |
| HUD resolves services from runtime registry | **Complete** |
| Live survival, interaction, inventory, equipment summaries | **Complete** |
| Notification bridge for inventory/equipment/interaction events | **Complete** |
| UI remains read-only (no menus/drag-drop) | **Complete** |
| Version **0.4.3** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.4.2a definition of done

| Criterion | Status |
|-----------|--------|
| HUD anchored out of center gameplay view | **Complete** |
| Readable survival bars, summaries, notifications | **Complete** |
| Plain-text interaction prompt (no glyphs) | **Complete** |
| Layout tuning via HUD profile | **Complete** |
| Version **0.4.2a** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.4.2 definition of done

| Criterion | Status |
|-----------|--------|
| UI module under `Assets/CCS/Modules/UI/` | **Complete** |
| Read-only `CCS_HudPresentationService` | **Complete** |
| HUD presenters + `PF_CCS_HUD_Root` | **Complete** |
| Default HUD profile asset | **Complete** |
| Bootstrap scene HUD instance | **Verify** |
| UI validation registered | **Complete** |
| No full inventory menu / drag-drop / crafting UI | **Complete** |
| Version **0.4.2** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Windows build verification | **Verify** |

---

## 0.4.1d definition of done

| Criterion | Status |
|-----------|--------|
| Removed obsolete public references (product themes, tooling language) | **Complete** |
| Architecture docs use framework-first language | **Complete** |
| Version **0.4.1d** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |

---

## 0.4.1c definition of done

| Criterion | Status |
|-----------|--------|
| Root README rewritten (framework-focused, no obsolete references) | **Complete** |
| UPM git URL install section | **Complete** |
| Updated folder layout and module table | **Complete** |
| Validation and build verification sections | **Complete** |
| Version **0.4.1c** | **Complete** |
| Batch validations 0 warnings / 0 errors | **Verify** |

---

## 0.4.1b definition of done

| Criterion | Status |
|-----------|--------|
| One Main Camera in `SCN_CCS_Survival_Bootstrap` | **Complete** |
| Build verification ground reference object | **Complete** |
| Survival bootstrap first in Build Settings | **Complete** |
| Windows development build succeeds | **Verify** |
| Batch validations 0 warnings / 0 errors | **Verify** |
| Build output not committed (`Builds/`) | **Complete** |
| Version **0.4.1b** | **Complete** |
| UI / HUD | **Deferred** |
| Cinemachine / gameplay camera | **Deferred** |
| Final player prefab wiring | **Deferred** |

---

## 0.4.1a definition of done

| Criterion | Status |
|-----------|--------|
| Carry-related slots (`Back`, `Satchel`, `Bedroll`) | **Complete** |
| Equipment capacity modifier fields on definitions | **Complete** |
| Aggregate modifiers on snapshot + service queries | **Complete** |
| `CCS_InventoryCapacityModifierSnapshot` placeholder in Inventory | **Complete** |
| Inventory does not reference Equipment directly | **Complete** |
| Validation for non-negative capacity modifiers | **Complete** |
| Version **0.4.1a** | **Complete** |
| Bootstrap composition wiring to Inventory | **Deferred** |
| UI display of resolved capacity | **Deferred** |

---

## 0.4.1 definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/Equipment/` layout | **Complete** |
| Slot types, definitions, durability, equipped item data | **Complete** |
| Player equipment service + events | **Complete** |
| Inventory item definition references (no duplicate ownership) | **Complete** |
| No UI/combat/visual/weapon gameplay coupling | **Complete** |
| Default profile under `Assets/CCS/Survival/Profiles/Equipment/` | **Complete** |
| Validation registered on pipeline | **Complete** |
| CS0618 bootstrap validation warning fixed | **Complete** |
| Version **0.4.1** | **Complete** |
| Bootstrap installer / scene wiring | **Deferred** |

---

## 0.4.0 definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/Inventory/` layout | **Complete** |
| Item definitions, stacks, slots, snapshots | **Complete** |
| Container with merge/split + player inventory service | **Complete** |
| No UI/equipment/crafting/storage/save coupling | **Complete** |
| Default profile under `Assets/CCS/Survival/Profiles/Inventory/` | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.4.0** | **Complete** |
| Bootstrap installer / scene wiring | **Deferred** |

---

## 0.3.9 definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/Interaction/` layout | **Complete** |
| `CCS_IInteractable` + `CCS_InteractableBase` | **Complete** |
| Forward-raycast scanner + service + events | **Complete** |
| No inventory/crafting/equipment/save/quest coupling | **Complete** |
| Default profile under `Assets/CCS/Survival/Profiles/Interaction/` | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.3.9** | **Complete** |
| Bootstrap installer / scene wiring | **Deferred** |

---

## 0.3.8a definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_SurvivalValidationBatchUtility` + `HasWarnings()` | **Complete** |
| Batchmode exit 0 pass / 1 on warnings or errors | **Complete** |
| Validation menus skip dialogs in batchmode | **Complete** |
| Survival Core stat constructor + Editor asmdef GUID fix | **Complete** |
| Version **0.3.8a** | **Complete** |

---

## 0.3.8 definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/CharacterController/` layout | **Complete** |
| Unity `CharacterController` motor (no Rigidbody) | **Complete** |
| Input abstraction + camera-look foundation | **Complete** |
| Stamina drain **request** event (no Survival Core calls) | **Complete** |
| Default profile under `Assets/CCS/Survival/Profiles/CharacterController/` | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.3.8** | **Complete** |
| Bootstrap installer / scene prefab wiring | **Deferred** |

---

## 0.3.7a definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/SurvivalCore/` layout | **Complete** |
| Module asmdefs (`CCS.Modules.SurvivalCore.*`) | **Complete** |
| Namespaces `CCS.Modules.SurvivalCore` | **Complete** |
| `SurvivalDiagnosticsLogCategory` compile fix | **Complete** |
| Documentation + version **0.3.7a** | **Complete** |

---

## 0.3.7b definition of done

| Criterion | Status |
|-----------|--------|
| Default profile committed under `Assets/CCS/Survival/Profiles/SurvivalCore/` | **Complete** |
| Profile creation menu removed | **Complete** |
| Development testing convenience menus removed | **Complete** |
| Validation pipeline + menus retained | **Complete** |
| Version **0.3.7b** | **Complete** |

---

## 0.3.7 definition of done

| Criterion | Status |
|-----------|--------|
| Stat architecture (6 stats) | **Complete** |
| Profiles + default asset tool | **Complete** |
| `CCS_SurvivalCoreService` + events | **Complete** |
| Validation registered on pipeline | **Complete** |
| Documentation | **Complete** |
| Version **0.3.7** | **Complete** |
| Bootstrap installer wiring | **Deferred** |

---

## Related

- [Survival Core Module](../../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md)
- [Interaction Module](../../Modules/Interaction/Documentation/CCS_Interaction_Module.md)
- [Character Controller Module](../../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
- [Gameplay modules index](../../Modules/README.md)
- [Development Framework Support](CCS_Survival_Development_Framework_Support.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
