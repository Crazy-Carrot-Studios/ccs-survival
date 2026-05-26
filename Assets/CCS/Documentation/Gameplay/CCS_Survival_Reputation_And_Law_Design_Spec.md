# CCS Survival — Reputation & Law Design Spec

**Document type:** Gameplay design specification — social control and consequence  
**Project:** ccs-survival (Crazy Carrot Studios)  
**Status:** Design specification — not implementation spec  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Module (proposed):** `ccs.survival.reputation`  
**Upstream:** [Gameplay Constitution](CCS_Survival_Gameplay_Constitution.md) · [Systems Breakdown](CCS_Survival_Gameplay_Systems_Breakdown.md) · [Gameplay Loop Specification](CCS_Survival_Gameplay_Loop_Specification.md)

---

## Purpose

**Reputation is the core social-control and consequence system** for CCS Survival.

It answers: *what happens when players hurt each other, steal, raid, or build civilization?* — without removing PvP or outlaw fantasy.

Reputation connects:

- **World reaction** (town access, prices, NPC behavior, witness reporting)
- **Death persistence** (how much property survives death — the signature sliding-scale rule)
- **Economy** (bounties, fines, black markets, lawful trade trust)
- **Territory** (raid legality, war declarations, claim protection)
- **Long-term identity** (lawful builder vs notorious outlaw)

This is not a cosmetic honor score. It is **enforceable frontier law** expressed through systems.

---

## Design Goals

| Goal | Intent |
|------|--------|
| **Prevent griefing without removing PvP** | Violence exists inside rules — witnesses, bounties, claims, war declarations |
| **Make outlaw gameplay viable but costly** | Outlaw path is playable; security and death outcomes punish recklessness |
| **Reward lawful settlement building** | High reputation protects investment in towns, stations, and storage |
| **Connect death persistence to reputation** | Death hurts everyone; it hurts outlaws **far more** — anti-griefing by design |
| **Support bounties, witnesses, lawmen, towns, war** | Each subsystem has a documented role (this spec) |

**Anti-pattern:** unrestricted murder and base erase with no witness trail.  
**Anti-pattern:** PvP so penalized that frontier conflict disappears.

---

## Reputation Scale

**Recommended range:** `-1000` to `+1000` (integer score with band mapping).

Bands drive UI, permissions, death modifiers, and world reaction — not raw number display to players (optional debug).

| Band | Score range | Label | Summary |
|------|-------------|-------|---------|
| 5 | +750 to +1000 | **Revered** | Exemplary citizen; maximum death protection and town trust |
| 4 | +300 to +749 | **Trusted** | Lawful builder/trader; strong protection and vendor access |
| 3 | -299 to +299 | **Neutral** | Unknown or mixed history; moderate consequences |
| 2 | -300 to -749 | **Wanted** | Active criminal risk; bounties, restricted towns, harsh death loss |
| 1 | -750 to -1000 | **Notorious Outlaw** | Frontier villain; maximum loss, black market reliance, hunt priority |

### Band behavior (directional)

- **Revered / Trusted:** Eligible for law roles, best prices, safest storage on death, witness credibility bonus.
- **Neutral:** Default new character band after onboarding grace (TBD); mistakes are recoverable.
- **Wanted / Notorious:** Bounty eligible, town bans likely, black market affinity, severe death retention.

Score changes through **crime events** and **lawful deeds** (see below). Caps and decay rates are tuning parameters (open questions).

---

## Death Persistence Rules

**Philosophy:** Death is meaningful; **reputation determines how much of your investment survives**.

This is **anti-griefing** and **long-term consequence** design — not punishment for playing.

| Band (summary) | Property / stored goods retention (directional) |
|----------------|--------------------------------------------------|
| **Highest reputation** | Retain **most** owned property — structures, chest contents, claim core |
| **Neutral** | Retain **moderate** amount — homestead damaged, partial storage loss |
| **Worst reputation** | Risk losing **nearly everything** outside equipped/on-person rules |

### What “property” includes (conceptual)

- Claim structures and placed stations (subject to raid rules)
- Stored inventory in owned containers
- Wagons/mounts in owned stables (TBD)
- **Not** typically: equipped hotbar, minimal starter grace items (prototype TBD)

### Design intent

- **Lawful settlers** can die in conflict but **rebuild** — encourages town investment.
- **Outlaws** accept **high churn** — raid economy and intimidation loops with real downside.
- **Griefing a Revered player** still kills them, but does not erase a week of town contribution in one ambush (within tuned %).

Exact percentages are **open questions** — must be playtested with economy and raid cadence.

---

## Crime Events

Crime events are **discrete reputation deltas** with severity, witnesses, and context flags.

| Event | Typical reputation impact | Notes |
|-------|---------------------------|-------|
| **Murder** | Large negative | Player or NPC kill outside duel/war |
| **Theft** | Medium negative | Container pickpocket, shop steal |
| **Assault** | Small–medium negative | Non-lethal player damage |
| **Structure damage** | Medium negative | Griefing buildings outside war |
| **Raiding** | Large negative (if undeclared) | See [Raid Legality](#raid-legality) |
| **Trespassing** | Small negative | Ignored claim warnings |
| **Killing outlaws** | Positive or neutral | Bounty/legal kill |
| **Defending property** | Positive | Verified defender context |
| **Declared war kills** | Reduced or neutral penalty | War flag active |
| **Duels** | Neutral if valid | See [Duels](#duels) |

Events should log: `authorityId`, `victimId`, `location`, `witnessIds`, `severity`, `contextFlags`.

---

## Witness System

Crimes are not always instant global knowledge — **witnesses** make enforcement credible.

### Witness types

| Type | Role |
|------|------|
| **NPC witnesses** | Town guards, shopkeepers, travelers — baseline law in starter region |
| **Player witnesses** | Other players in range — social enforcement, report UI |
| **Town witnesses** | Settlement-boundary sensors + guard posts — aggregated reports |

### Detection concept

- **Line-of-sight / proximity** — witness must plausibly perceive event (not omniscient anti-cheat replacement; complements server authority).
- **Reporting crimes** — witness triggers report → reputation delta applied → bounty generation (if applicable).
- **Delayed reporting** — messenger/delay simulates frontier distance (optional prototype: instant in town, delayed in wilds).

### False reports (delayed scope)

- False accusation, frame jobs, and evidence disputes — **not prototype**.
- Prototype: trusted witness types only; player reports require proximity + cooldown.

---

## Bounty System

Bounties convert **wanted status** into **economy and PvP loops**.

| Rule | Detail |
|------|--------|
| **Generation** | Crimes above threshold create or increase bounty on `authorityId` |
| **Value** | Scales with severity, repetition, band depth — open formula |
| **Bounty hunters** | Lawful players collect by delivering proof/kill at town — legal income |
| **Killing wanted players** | Does **not** harm lawful hunter reputation when target is Wanted+ |
| **Economy circulation** | Payout from town pool / fines — sinks and sources tuned with economy spec |

**Notorious Outlaw** band should feel **hunted** — map pressure without instant teleporting guards everywhere.

---

## Lawful vs Outlaw Gameplay

### Lawful benefits

| Benefit | Description |
|---------|-------------|
| **Town access** | Markets, crafting halls, clinics, stables |
| **Vendor trust** | Better buy/sell, credit lines (later), contract eligibility |
| **Safer death persistence** | Highest retention bands |
| **Lower taxes/prices** | Settlement policy hook (TBD with economy) |
| **Law roles** | Sheriff, deputy, militia captain eligibility |

### Outlaw benefits

| Benefit | Description |
|---------|-------------|
| **Black markets** | Stolen goods, contraband, no questions (delayed economy depth) |
| **Hideouts** | Reduced witness radius; off-map stashes (TBD rules) |
| **Stolen goods economy** | Higher margins, higher risk |
| **Intimidation / robbery loops** | PvP fantasy with bounty counter-pressure |
| **High-risk high-reward** | Short-term gains, long-term loss on death |

Both paths must be **viable fantasies** — not trap choices.

---

## Town Law

### Starter railroad town

- **Protected law zone** — murder/theft/structure damage heavily penalized, strong NPC witness coverage.
- **New player sanctuary** — not immunity from wilderness, but core streets are learnable safe space.
- **Cannot be war target** — see [War Declaration Rules](#war-declaration-rules).

### Player towns

- Define **local rules within bounds** — curfew, tax, market fees, militia recruitment.
- **Sheriff / law roles** — player-appointed or earned (prototype: simple role flag).
- **Town bans** — outlaw bands blocked from commerce; appeal via reputation recovery.
- **Market restrictions** — contraband lists, reputation gates.
- **Abuse of power** — sheriffs who murder or steal face same crime events + role removal + reputation crash.

Town law **extends** frontier rules; it does not replace global reputation.

---

## War Declaration Rules

Clan/town conflict needs **formal war** so PvP is legible — not endless murder with war excuse.

### Why formal war

- Protects neutral players and new homesteaders
- Creates narrative beats (notice, duration, peace)
- Reduces reputation penalties for **soldiers in declared conflict**
- Prevents “we’re at war” as griefing cover

### Recommended requirements

| Requirement | Purpose |
|-------------|---------|
| **Valid cause or cost** | Declaration costs gold/supplies/prestige — prevents spam |
| **Notice period** | 24–72h in-game (tunable) — defenders prepare |
| **Duration** | War auto-expires unless renewed — avoids permanent grief |
| **Reduced penalties during war** | Kills/structure damage between belligerents only |
| **Outside war rules = crime** | Attacking neutrals or starter town is full penalty |
| **Starter town non-target** | Hard exclusion zone |

Belligerent lists: `settlementId` / `clanId` pairs stored in save (`reputation.json` or `settlement.json` cross-ref).

---

## Duels

**Fair gunfights** — consensual PvP without murder stigma.

| Rule | Detail |
|------|--------|
| **Accepted challenge** | Both parties confirm; optional stakes |
| **Defined location** | Duel ring or marked outskirts — witness scope known |
| **No murder penalty** | Valid duel flag suppresses murder crime |
| **Winner reward** | Stake pot, reputation flair (small), not loot-all (TBD) |
| **Loser death rules** | Death persistence still applies — duels are risky |
| **Unfair duels** | Ambush under duel pretext → witness system treats as murder |

Reputation impact: minor positive for honored participation; negative for duel scamming.

---

## Raid Legality

| Scenario | Legality |
|----------|----------|
| **Undeclared raid** on active claim | **Criminal** — full reputation + bounty + witness |
| **Declared war raid** | **Reduced penalties** between belligerents only |
| **Abandoned / decayed claims** | **Legal or low-penalty** looting — decay system ties to settlement spec |
| **Starter region** | **Protected** — raids treated as maximum crime |
| **New player grace** | Optional brief claim raid protection (open question) |

Raids must **not** erase Revered player towns in one offline night without war notice — tune retention + raid windows.

---

## Recovery & Redemption

Reputation **can move** — outlaw is a state, not always a permanent class.

| Recovery action | Effect (directional) |
|-----------------|----------------------|
| **Lawful actions over time** | Slow passive drift toward neutral |
| **Paying fines** | Instant partial restore at town courthouse |
| **Completing bounties** | Positive delta + payout |
| **Defending towns** | Event bonus during raids |
| **Trade contribution** | Tax/tariff contribution hooks (with economy) |
| **Long-term outlaw redemption** | Possible but **hard** — requires sustained lawful play + fines |

**Notorious → Revered** should be a **season arc**, not one afternoon of chores.

---

## Prototype Scope

First version (`ccs.survival.reputation` prototype) ships:

| Feature | Prototype behavior |
|---------|-------------------|
| **Numeric reputation score** | -1000 to +1000 stored per authority |
| **5 bands** | Revered / Trusted / Neutral / Wanted / Notorious |
| **Simple crime events** | Murder, theft, assault — fixed deltas |
| **Witness trigger** | NPC + player proximity flag (instant report) |
| **Bounty value** | Single bounty integer on Wanted+ |
| **Death retention modifier** | Band → % retention table (tuned minimum viable) |
| **Starter town law zone** | Hard-coded protected volume + witness coverage |

**Integrates with:** claims (ownership), economy (vendor access), save orchestration.

---

## Delayed Scope

Explicitly **not** in first reputation prototype:

| System | Reason to delay |
|--------|-----------------|
| False accusation / frame jobs | Needs evidence and UI |
| Courts / trials | Narrative and session overhead |
| Complex evidence chains | Investigation gameplay |
| Advanced sheriff elections | Governance spec dependency |
| Prison gameplay | Content and escape loops |
| Full black market economy | Economy spec + contraband items |

---

## Open Questions

Resolve before locking implementation constants:

| Topic | Question |
|-------|----------|
| **Reputation point values** | Per-event delta table for murder/theft/assault/defense |
| **Death retention percentages** | Per-band % for structures, storage, wagons |
| **Bounty payout formula** | Base × severity × repetition; funding source |
| **War declaration costs** | Gold, influence, cooldown, minimum settlement size |
| **Duel reward formula** | Stakes cap, reputation bonus, loot rules |
| **Outlaw hideout protection** | Witness suppression radius, raidability, discovery |
| **Neutral starting score** | 0 vs small positive after tutorial |
| **Co-op shared reputation** | Party/clan aggregate vs individual only |
| **Offline raid + retention** | How retention interacts with elapsed-time sim |

---

## Related documents

| Document | Role |
|----------|------|
| [Gameplay Constitution](CCS_Survival_Gameplay_Constitution.md) | Reputation pillar and death philosophy |
| [Systems Breakdown](CCS_Survival_Gameplay_Systems_Breakdown.md) | Module `ccs.survival.reputation`, save file |
| [Gameplay Loop Specification](CCS_Survival_Gameplay_Loop_Specification.md) | When reputation enters player pacing |
| [Settlement & Territory Spec](CCS_Survival_Settlement_And_Territory_Spec.md) | *Planned* — war, claims, town law zones |

---

*Implementation status: foundation runtime (v0.3.x) contains no reputation mechanics. This spec guides first `ccs.survival.reputation` milestone.*
