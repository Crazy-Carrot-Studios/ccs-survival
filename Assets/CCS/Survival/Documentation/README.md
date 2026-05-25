# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.1.0 — Survival Project Identity Setup  
**Author:** James Schilz  
**Date:** 2026-05-24

This folder is the Unity-visible index for survival-specific documentation. Authoritative copies of architecture and milestones live at the **repository root** so they are easy to find outside the Editor.

---

## Repository documentation (primary)

| Topic | Path (from repo root) |
|-------|------------------------|
| Project overview | `README.md` |
| Gameplay architecture | `Documentation/Architecture/Survival_Gameplay_Architecture.md` |
| Module boundaries | `Documentation/Architecture/Survival_Module_Boundaries.md` |
| Networking authority | `Documentation/Architecture/Survival_Networking_Authority.md` |
| Persistence direction | `Documentation/Architecture/Survival_Persistence_Direction.md` |
| Milestone 0.1.0 | `Documentation/Milestones/Milestone_0.1.0_Survival_Project_Identity_Setup.md` |

---

## Core Platform (protected — read before editing)

| Topic | Path |
|-------|------|
| Core architecture | `Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md` |
| Upstream workflow | `Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md` |
| Script standards | `Assets/CCS/Framework/Documentation/CCS_Script_Standards.md` |

Do not add survival gameplay logic under `Assets/CCS/Framework/Core/`.

---

## Sibling folders

| Folder | Purpose |
|--------|---------|
| `Assets/CCS/Survival/Scripts/` | Reserved for future cross-cutting game code (empty at 0.1.0) |
| `Assets/CCS/Modules/` | Gameplay feature modules (`ccs.survival.*`) |
| `Assets/CCS/Framework/` | Vendored CCS Core Platform |

---

## Milestone 0.1.0 rule

**Documentation only.** No systems, installers, networking packages, or persistence code in this milestone.
