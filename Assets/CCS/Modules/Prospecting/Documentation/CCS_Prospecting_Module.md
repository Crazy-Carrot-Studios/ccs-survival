# CCS Prospecting Module (1.7.0)

Frontier prospecting and mining expansion for CCS Survival.

## Scope

- Ore vein, coal vein, stone outcrop, clay deposit, salvage mine debris
- Prospecting spot and abandoned mine entrance placeholders (no caves / ownership / explosives)
- Pick tier gating: primitive pick for basic outcrops, iron pick for rich veins
- Industry iron ore → refined iron integration
- Wagon bulk-haul weight/value placeholders
- Economy vendor buy paths for mining materials

## Prospecting Loop

```
Find Deposit
    ↓
Mine Ore / Coal (pick tier + WrongTool when under-tier)
    ↓
Haul With Wagon (dense ore/coal weights)
    ↓
Refine At Homestead (iron ore → refined iron)
    ↓
Sell Or Craft Better Gear
```

## Bootstrap

Batch entry: `CCS.Modules.Prospecting.Editor.CCS_ProspectingFoundationBootstrapSetup.ExecuteBatch`

## Playtest

Steps 103–110; shortcut **Ctrl+Shift+M**.
