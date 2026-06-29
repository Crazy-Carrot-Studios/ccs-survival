using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerPrefabHierarchyArchitectureReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.5 player prefab hierarchy audit and architecture reports to Logs.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerPrefabHierarchyArchitectureReportBuilder
    {
        public const string HierarchyAuditReportRelativePath =
            "Logs/CharacterController/PrefabAudit/CCS_PlayerPrefab_HierarchyAudit_v0.7.5.md";

        public const string ArchitectureReportRelativePath =
            "Logs/CharacterController/PrefabAudit/CCS_PlayerPrefab_HierarchyArchitecture_v0.7.5.md";

        private const string PlayerVisualPrefabGuid = "f4a8c2e1b9d3476580a1b2c3d4e5f6a7";

        public static string WriteAllReports()
        {
            HierarchyScanResult scan = ScanNetworkedPlayerPrefab();
            string auditPath = WriteHierarchyAuditReport(scan);
            string architecturePath = WriteArchitectureMigrationReport(scan);
            Debug.Log(
                "[Player Prefab Hierarchy Architecture] Wrote audit: "
                + auditPath
                + " architecture: "
                + architecturePath);
            return architecturePath;
        }

        private static HierarchyScanResult ScanNetworkedPlayerPrefab()
        {
            HierarchyScanResult result = new HierarchyScanResult
            {
                PrefabPath = CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,
            };

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(result.PrefabPath);
            if (prefabAsset == null)
            {
                result.Errors.Add("Could not load prefab at " + result.PrefabPath);
                return result;
            }

            string prefabContentsPath = AssetDatabase.GetAssetPath(prefabAsset);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabContentsPath);
            try
            {
                Transform root = prefabContents.transform;
                result.RootName = root.name;
                CollectRootComponents(root, result);
                CollectDirectChildren(root, result);
                CollectSubsystemMapping(root, result);
                DetectModelNesting(root, result);
                CollectOwnerOnlyUiCandidates(root, result);
                CollectNetworkBehaviourCandidates(root, result);
                CollectNonNetworkMoveCandidates(root, result);
                CollectExternalBridgeCandidates(root, result);
                CountGameObjectsAndBehaviours(root, result);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }

            return result;
        }

        private static void CollectRootComponents(Transform root, HierarchyScanResult result)
        {
            Component[] components = root.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    result.MissingScriptsOnRoot++;
                    continue;
                }

                if (component is Transform)
                {
                    continue;
                }

                string typeName = component.GetType().Name;
                result.RootComponents.Add(typeName);
                if (component is NetworkBehaviour)
                {
                    result.RootNetworkBehaviours.Add(typeName);
                }
            }
        }

        private static void CollectDirectChildren(Transform root, HierarchyScanResult result)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                List<string> scripts = child.GetComponents<MonoBehaviour>()
                    .Where(behaviour => behaviour != null)
                    .Select(behaviour => behaviour.GetType().Name)
                    .ToList();
                result.DirectChildren.Add(new DirectChildEntry
                {
                    Name = child.name,
                    ScriptNames = scripts,
                });
            }
        }

        private static void CollectSubsystemMapping(Transform root, HierarchyScanResult result)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string scriptName = behaviour.GetType().Name;
                if (!scriptName.StartsWith("CCS_") && scriptName != "NetworkObject")
                {
                    continue;
                }

                string path = GetHierarchyPath(behaviour.transform, root);
                string subsystem = ResolveSubsystem(scriptName, path);
                result.SubsystemMappings.Add(new SubsystemMappingEntry
                {
                    ScriptName = scriptName,
                    HierarchyPath = path,
                    Subsystem = subsystem,
                    IsOnRoot = behaviour.transform == root,
                    IsNetworkBehaviour = behaviour is NetworkBehaviour,
                });
            }
        }

        private static void DetectModelNesting(Transform root, HierarchyScanResult result)
        {
            Transform visualRoot = root.Find("VisualRoot");
            if (visualRoot == null)
            {
                result.ModelNestingNotes.Add("No VisualRoot child found.");
                return;
            }

            result.HasVisualRoot = true;
            for (int i = 0; i < visualRoot.childCount; i++)
            {
                Transform child = visualRoot.GetChild(i);
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (source != null)
                {
                    string sourcePath = AssetDatabase.GetAssetPath(source);
                    string sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
                    if (sourceGuid == PlayerVisualPrefabGuid || source.name.Contains("PF_CCS_Player_Visual"))
                    {
                        result.HasNestedPlayerVisual = true;
                        result.ModelNestingNotes.Add(
                            "Double nesting detected: VisualRoot -> "
                            + child.name
                            + " (source: "
                            + sourcePath
                            + ").");
                    }
                }
            }

            if (!result.HasNestedPlayerVisual)
            {
                result.ModelNestingNotes.Add("VisualRoot present but nested PF_CCS_Player_Visual instance not confirmed by GUID.");
            }
        }

        private static void CollectOwnerOnlyUiCandidates(Transform root, HierarchyScanResult result)
        {
            string[] candidateNames =
            {
                "AttributeHudRoot",
                "InteractionPromptHudRoot",
                "WeaponHudRoot",
            };

            for (int i = 0; i < candidateNames.Length; i++)
            {
                Transform child = root.Find(candidateNames[i]);
                if (child != null)
                {
                    result.OwnerOnlyUiCandidates.Add(candidateNames[i]);
                }
            }
        }

        private static void CollectNetworkBehaviourCandidates(Transform root, HierarchyScanResult result)
        {
            NetworkBehaviour[] networkBehaviours = root.GetComponents<NetworkBehaviour>();
            for (int i = 0; i < networkBehaviours.Length; i++)
            {
                NetworkBehaviour behaviour = networkBehaviours[i];
                if (behaviour != null)
                {
                    result.NetworkBehaviourRootStayCandidates.Add(behaviour.GetType().Name);
                }
            }

            if (root.GetComponent<NetworkObject>() != null)
            {
                result.NetworkBehaviourRootStayCandidates.Insert(0, "NetworkObject");
            }
        }

        private static void CollectNonNetworkMoveCandidates(Transform root, HierarchyScanResult result)
        {
            MonoBehaviour[] rootBehaviours = root.GetComponents<MonoBehaviour>();
            for (int i = 0; i < rootBehaviours.Length; i++)
            {
                MonoBehaviour behaviour = rootBehaviours[i];
                if (behaviour == null || behaviour is NetworkBehaviour)
                {
                    continue;
                }

                result.NonNetworkMoveCandidates.Add(behaviour.GetType().Name);
            }
        }

        private static void CollectExternalBridgeCandidates(Transform root, HierarchyScanResult result)
        {
            HashSet<string> bridgeScripts = new HashSet<string>
            {
                "CCS_RevolverController",
                "CCS_PlayerWeaponLoadout",
                "CCS_WeaponCarryStateController",
                "CCS_PlayerEquipmentVisualController",
                "CCS_AttributeContainer",
                "CCS_AttributeService",
                "CCS_NetworkAttributeReplicator",
                "CCS_StaminaController",
                "CCS_HealthRegenController",
                "CCS_NetworkHealth",
                "CCS_NetworkInteractionScanner",
                "CCS_PlayerDeathScreenController",
            };

            MonoBehaviour[] rootBehaviours = root.GetComponents<MonoBehaviour>();
            for (int i = 0; i < rootBehaviours.Length; i++)
            {
                MonoBehaviour behaviour = rootBehaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (bridgeScripts.Contains(typeName))
                {
                    result.ExternalBridgeCandidates.Add(typeName);
                }
            }
        }

        private static void CountGameObjectsAndBehaviours(Transform root, HierarchyScanResult result)
        {
            result.TotalGameObjects = root.GetComponentsInChildren<Transform>(true).Length;
            result.TotalMonoBehaviours = root.GetComponentsInChildren<MonoBehaviour>(true).Count(behaviour => behaviour != null);
        }

        private static string ResolveSubsystem(string scriptName, string hierarchyPath)
        {
            if (scriptName.Contains("Input"))
            {
                return "Systems/Input";
            }

            if (scriptName.Contains("Motor") || scriptName.Contains("ControllerService") || scriptName.Contains("AimLocomotion"))
            {
                return "Systems/Movement";
            }

            if (scriptName.Contains("Camera") || scriptName.Contains("FirstPerson") || scriptName.Contains("HeadVisibility"))
            {
                return "Systems/Camera";
            }

            if (scriptName.Contains("Attribute") || scriptName.Contains("Health") || scriptName.Contains("Stamina") || scriptName.Contains("DeathScreen"))
            {
                return "Systems/Attributes";
            }

            if (scriptName.Contains("Interaction"))
            {
                return "Systems/Interaction";
            }

            if (scriptName.Contains("Revolver") || scriptName.Contains("Weapon") || scriptName.Contains("Muzzle"))
            {
                return "Systems/Weapons";
            }

            if (scriptName.Contains("Equipment"))
            {
                return "Systems/Equipment";
            }

            if (scriptName.Contains("LocomotionAnimator") || scriptName.Contains("InteractionAnimator") || scriptName.Contains("ReticleIK") || scriptName.Contains("BodyAim"))
            {
                return "Model";
            }

            if (scriptName.Contains("Nameplate"))
            {
                return hierarchyPath.Contains("NameplateRoot") ? "WorldPresentation" : "Root/Authority";
            }

            if (scriptName.Contains("Network"))
            {
                return "Root/Authority";
            }

            return "Review";
        }

        private static string WriteHierarchyAuditReport(HierarchyScanResult scan)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Player Prefab Hierarchy Audit (v0.7.5)");
            builder.AppendLine();
            builder.AppendLine("**Source:** `" + scan.PrefabPath + "`");
            builder.AppendLine("**Generated:** " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine("**Baseline tag:** v0.7.4");
            builder.AppendLine();
            builder.AppendLine("Permanent generated report. Replaces temporary TEMP hierarchy audit.");
            builder.AppendLine();

            if (scan.Errors.Count > 0)
            {
                builder.AppendLine("## Errors");
                for (int i = 0; i < scan.Errors.Count; i++)
                {
                    builder.AppendLine("- " + scan.Errors[i]);
                }

                builder.AppendLine();
            }

            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- **Root GameObject:** `" + scan.RootName + "`");
            builder.AppendLine("- **Root MonoBehaviour count:** " + scan.RootComponents.Count);
            builder.AppendLine("- **Root missing scripts:** " + scan.MissingScriptsOnRoot);
            builder.AppendLine("- **Total GameObjects:** " + scan.TotalGameObjects);
            builder.AppendLine("- **Total MonoBehaviours:** " + scan.TotalMonoBehaviours);
            builder.AppendLine("- **Has VisualRoot:** " + scan.HasVisualRoot);
            builder.AppendLine("- **Has nested PF_CCS_Player_Visual:** " + scan.HasNestedPlayerVisual);
            builder.AppendLine();

            builder.AppendLine("## Root components");
            builder.AppendLine();
            for (int i = 0; i < scan.RootComponents.Count; i++)
            {
                builder.AppendLine("- `" + scan.RootComponents[i] + "`");
            }

            builder.AppendLine();
            builder.AppendLine("## Direct children");
            builder.AppendLine();
            for (int i = 0; i < scan.DirectChildren.Count; i++)
            {
                DirectChildEntry child = scan.DirectChildren[i];
                builder.Append("- `" + child.Name + "`");
                if (child.ScriptNames.Count > 0)
                {
                    builder.Append(" — scripts: ");
                    builder.Append(string.Join(", ", child.ScriptNames.Select(name => "`" + name + "`")));
                }

                builder.AppendLine();
            }

            builder.AppendLine();
            builder.AppendLine("## Model nesting notes");
            builder.AppendLine();
            for (int i = 0; i < scan.ModelNestingNotes.Count; i++)
            {
                builder.AppendLine("- " + scan.ModelNestingNotes[i]);
            }

            builder.AppendLine();
            builder.AppendLine("## CCS subsystem mapping (current attachment)");
            builder.AppendLine();
            builder.AppendLine("| Script | Path | Subsystem | Root | NetworkBehaviour |");
            builder.AppendLine("|--------|------|-----------|------|------------------|");
            for (int i = 0; i < scan.SubsystemMappings.Count; i++)
            {
                SubsystemMappingEntry entry = scan.SubsystemMappings[i];
                builder.AppendLine(
                    "| `"
                    + entry.ScriptName
                    + "` | `"
                    + entry.HierarchyPath
                    + "` | "
                    + entry.Subsystem
                    + " | "
                    + entry.IsOnRoot
                    + " | "
                    + entry.IsNetworkBehaviour
                    + " |");
            }

            string reportPath = ResolveReportPath(HierarchyAuditReportRelativePath);
            WriteReportFile(reportPath, builder);
            return reportPath;
        }

        private static string WriteArchitectureMigrationReport(HierarchyScanResult scan)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Player Prefab Hierarchy Architecture Report (v0.7.5)");
            builder.AppendLine();
            builder.AppendLine("**Milestone:** Phase 3D — planning only (no prefab changes)");
            builder.AppendLine("**Generated:** " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine("**Architecture doc:** `Assets/CCS/Modules/CharacterController/Documentation/CCS_PlayerPrefab_Hierarchy_Architecture.md`");
            builder.AppendLine();

            builder.AppendLine("## Current state");
            builder.AppendLine();
            builder.AppendLine("- Root MonoBehaviour count: **" + scan.RootComponents.Count + "** (target ideal ~6, realistic Netcode-safe budget documented in architecture doc)");
            builder.AppendLine("- Root `NetworkBehaviour` count: **" + scan.RootNetworkBehaviours.Count + "**");
            builder.AppendLine("- Direct children: **" + scan.DirectChildren.Count + "**");
            builder.AppendLine();

            AppendBulletList(builder, "Direct children", scan.DirectChildren.Select(child => child.Name).ToList());
            AppendBulletList(builder, "Owner-only UI candidates", scan.OwnerOnlyUiCandidates);
            AppendBulletList(builder, "NetworkBehaviour root-stay candidates", scan.NetworkBehaviourRootStayCandidates);
            AppendBulletList(builder, "Non-NetworkBehaviour move candidates (root)", scan.NonNetworkMoveCandidates);
            AppendBulletList(builder, "External module bridge candidates (root)", scan.ExternalBridgeCandidates);
            AppendBulletList(builder, "Model nesting", scan.ModelNestingNotes);

            builder.AppendLine("## Proposed final hierarchy");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine("PF_CCS_CharacterController_Player_Networked");
            builder.AppendLine("├── AuthorityRoot / minimal root components");
            builder.AppendLine("├── Systems/ (Input, Movement, Camera, Attributes, Interaction, Weapons, Equipment, Presentation)");
            builder.AppendLine("├── CameraRig");
            builder.AppendLine("├── Model/  (single CC4 swap point — replaces VisualRoot + nested visual)");
            builder.AppendLine("├── EquipmentSockets");
            builder.AppendLine("├── WeaponMounts");
            builder.AppendLine("├── Interaction");
            builder.AppendLine("├── LocalOnly/  (owner-only HUD — v0.7.9)");
            builder.AppendLine("└── WorldPresentation/NameplateRoot");
            builder.AppendLine("```");
            builder.AppendLine();

            builder.AppendLine("## Target root policy");
            builder.AppendLine();
            builder.AppendLine("**Target A (ideal):** CharacterController, NetworkObject, owner NetworkTransform, CCS_NetworkPlayerController, future facade, CCS_CharacterMotor if required.");
            builder.AppendLine();
            builder.AppendLine("**Target B (Netcode-safe):** retain root NetworkBehaviours until child NetworkObject strategy is proven by hosting batches.");
            builder.AppendLine();
            builder.AppendLine("**Rule:** never move NetworkBehaviour off root without validated NetworkObject strategy.");
            builder.AppendLine();

            builder.AppendLine("## Staged migration roadmap");
            builder.AppendLine();
            builder.AppendLine("| Version | Scope |");
            builder.AppendLine("|---------|-------|");
            builder.AppendLine("| v0.7.5 | Architecture plan only |");
            builder.AppendLine("| v0.7.6 | Validator + dry-run migration builder |");
            builder.AppendLine("| v0.7.7 | Move non-NetworkBehaviour systems under Systems/ |");
            builder.AppendLine("| v0.7.8 | Single Model root; remove VisualRoot double nesting |");
            builder.AppendLine("| v0.7.9 | Local-owner UI separation |");
            builder.AppendLine("| Later | CC4 import, animation layer rebuild |");
            builder.AppendLine();

            builder.AppendLine("## Implementation risks");
            builder.AppendLine();
            builder.AppendLine("1. Breaking serialized references during child reparenting.");
            builder.AppendLine("2. Netcode ownership/regression if NetworkBehaviours move prematurely.");
            builder.AppendLine("3. Owner-only UI accidentally disabled for local player.");
            builder.AppendLine("4. Model swap point ambiguity if VisualRoot nesting is not removed.");
            builder.AppendLine("5. External module bridges losing GetComponent assumptions.");
            builder.AppendLine("6. Batch/builder drift if migration is hand-edited.");
            builder.AppendLine();

            builder.AppendLine("## v0.7.5 confirmations");
            builder.AppendLine();
            builder.AppendLine("- No prefab hierarchy changes in this milestone.");
            builder.AppendLine("- PF_CCS_Player_Visual unchanged.");
            builder.AppendLine("- Animator Controller unchanged.");
            builder.AppendLine("- Animation clips unchanged.");
            builder.AppendLine("- No CC4 import.");
            builder.AppendLine("- No animation import.");

            string reportPath = ResolveReportPath(ArchitectureReportRelativePath);
            WriteReportFile(reportPath, builder);
            return reportPath;
        }

        private static void AppendBulletList(StringBuilder builder, string title, IList<string> items)
        {
            builder.AppendLine("### " + title);
            builder.AppendLine();
            if (items == null || items.Count == 0)
            {
                builder.AppendLine("- (none)");
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    builder.AppendLine("- `" + items[i] + "`");
                }
            }

            builder.AppendLine();
        }

        private static string GetHierarchyPath(Transform target, Transform prefabRoot)
        {
            if (target == prefabRoot)
            {
                return prefabRoot.name;
            }

            List<string> segments = new List<string>();
            Transform current = target;
            while (current != null && current != prefabRoot.parent)
            {
                segments.Add(current.name);
                if (current == prefabRoot)
                {
                    break;
                }

                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void WriteReportFile(string reportPath, StringBuilder builder)
        {
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
        }

        private sealed class HierarchyScanResult
        {
            public string PrefabPath;
            public string RootName;
            public List<string> Errors = new List<string>();
            public List<string> RootComponents = new List<string>();
            public List<string> RootNetworkBehaviours = new List<string>();
            public List<DirectChildEntry> DirectChildren = new List<DirectChildEntry>();
            public List<SubsystemMappingEntry> SubsystemMappings = new List<SubsystemMappingEntry>();
            public List<string> ModelNestingNotes = new List<string>();
            public List<string> OwnerOnlyUiCandidates = new List<string>();
            public List<string> NetworkBehaviourRootStayCandidates = new List<string>();
            public List<string> NonNetworkMoveCandidates = new List<string>();
            public List<string> ExternalBridgeCandidates = new List<string>();
            public bool HasVisualRoot;
            public bool HasNestedPlayerVisual;
            public int MissingScriptsOnRoot;
            public int TotalGameObjects;
            public int TotalMonoBehaviours;
        }

        private sealed class DirectChildEntry
        {
            public string Name;
            public List<string> ScriptNames = new List<string>();
        }

        private sealed class SubsystemMappingEntry
        {
            public string ScriptName;
            public string HierarchyPath;
            public string Subsystem;
            public bool IsOnRoot;
            public bool IsNetworkBehaviour;
        }
    }
}
