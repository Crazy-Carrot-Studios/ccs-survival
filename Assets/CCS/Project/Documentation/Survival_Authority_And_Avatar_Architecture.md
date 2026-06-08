# Survival Authority and Avatar Architecture

## Overview

Survival separates **authority** (ownership, decisions, persistence identity) from **avatar** (scene representation). This boundary keeps save data, input routing, camera targets, and network ownership from hard-coupling to scene objects.

The contracts are defined in Project foundation assemblies without networking or save implementations. Gameplay modules implement authority and avatar on top of these interfaces.

## Architecture

### Authority layer

`CCS_ISurvivalAuthority` defines who owns decisions and stable identity.

| Concern | Authority owns |
|---------|----------------|
| Identity | Stable `AuthorityId` for save and network keys |
| Decisions | Input intent, simulation ownership signals |
| Persistence | Authoritative identity for save schemas |
| Network readiness | `IsNetworkAuthorityReady` signal without netcode references |

### Avatar layer

`CCS_ISurvivalAvatar` defines scene representation only.

| Concern | Avatar owns |
|---------|-------------|
| Presence | `Transform` root, spawn and possession flags |
| Visuals | Body root, animator attachment, equipment sockets |
| Camera | Camera target references |
| Identity | `AvatarId` as instance identifier — not persistence owner |

### Binding

`CCS_SurvivalAuthorityAvatarBinding` links `AuthorityId` ↔ `AvatarId` with an optional `BindingId`. Binding validates relationship integrity without performing spawn, save, or network IO.

```text
Authority (ownership)  ←→  Binding  ←→  Avatar (representation)
```

### Core types

| Type | Path |
|------|------|
| `CCS_ISurvivalAuthority` | `Runtime/Character/Authority/` |
| `CCS_ISurvivalAvatar` | `Runtime/Character/Avatar/` |
| `CCS_SurvivalAuthorityAvatarBinding` | `Runtime/Character/Avatar/` |
| `CCS_SurvivalAuthorityAvatarValidationUtility` | `Runtime/Character/Avatar/` |
| `CCS_SurvivalIdentityUtility` | `Runtime/Character/Identity/` |

## Rules

### Identity prefixes

| Identity | Prefix |
|----------|--------|
| Authority | `ccs.survival.authority.*` |
| Avatar | `ccs.survival.avatar.*` |
| Binding | `ccs.survival.binding.*` |

### Format requirements

- Lowercase reverse-DNS: `a-z`, `0-9`, `.`, `-`
- Non-empty strings
- No slashes, spaces, or Unity-internal identifiers

### Forbidden identity sources

| Source | Status |
|--------|--------|
| `GetInstanceID()` | Forbidden |
| GameObject names | Forbidden |
| Scene paths | Forbidden |
| Asset paths (`Assets/...`) | Forbidden |
| Transform hierarchy paths | Forbidden |

### Ownership rules

- Save identity belongs to authority and profile IDs, not scene objects.
- Avatar is scene representation only — authority owns persistence.
- Input and decisions route through authority; camera and visuals target avatar.
- Replication targets authority ownership; avatar is the replicated view.
- Never collapse authority ID and avatar ID into a single string.

### Multiplayer compatibility

Authority contracts expose readiness signals without referencing a networking package. Network implementations live in gameplay assemblies and adapt to authority ownership — not avatar instance IDs.

## Validation

`CCS_SurvivalAuthorityAvatarValidationUtility` validates:

- Authority ID format and prefix
- Avatar ID format and prefix
- Binding ID format and prefix
- Authority–avatar pair consistency when both are assigned

Bootstrap does not require runtime authority or avatar instances. Validation of assigned bindings runs at setup time.

## Usage

Possession flow at setup:

```text
1. Create authority implementation with stable AuthorityId
2. Create avatar implementation with stable AvatarId
3. Bind via CCS_SurvivalAuthorityAvatarBinding
4. Validate binding before bootstrap completes
```

Systems that consume this split:

| System | Target layer |
|--------|--------------|
| Player controller | Authority (intent) |
| Save/load | Authority + profile IDs |
| Camera rig | Avatar root/target |
| Equipment visuals | Avatar sockets |
| Replication | Authority ownership |

## Related Documentation

- [Survival Validation Standards](Survival_Validation_Standards.md)
- [Survival Runtime Foundation](Survival_Runtime_Foundation.md)
- [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md)
- [Survival Framework Architecture Gate](Survival_Framework_Architecture_Gate.md)
