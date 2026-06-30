using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CCS.Modules.AI;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.CharacterController.Netcode;
using CCS.Project;
using UnityEditor;
using UnityEditor.Compilation;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPlayerPrefabAuditUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Inventories player prefab components and writes Phase 2C audit reports (v0.7.1e).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Report-only classification. Does not remove or move prefab components.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public enum PlayerPrefabComponentClassification
    {
        RequiredRoot,
        RequiredRuntime,
        RequiredLocalOnly,
        RequiredNetwork,
        RequiredCamera,
        RequiredAnimation,
        RequiredInteraction,
        RequiredWeaponsBridge,
        RequiredEquipmentVisual,
        RequiredAttributes,
        RequiredUIBridge,
        TestOnly,
        DebugOnly,
        DiagnosticsOnly,
        SceneManagerCandidate,
        TestingManagerCandidate,
        DeprecatedCandidate,
        UnknownReview
    }

    public enum PlayerPrefabComponentLayer
    {
        Runtime,
        Tests,
        ExternalModule,
        UnityBuiltIn,
        EditorOnlyInvalid,
        Unknown
    }

    public enum PlayerPrefabComponentDisposition
    {
        Stay,
        MoveLater,
        NeedsReview
    }

    public sealed class PlayerPrefabComponentAuditEntry
    {
        public string PrefabPath;
        public string HierarchyPath;
        public string ComponentType;
        public string NamespaceName;
        public string AssemblyName;
        public string ScriptAssetPath;
        public string SerializedRole;
        public PlayerPrefabComponentLayer Layer;
        public PlayerPrefabComponentClassification Classification;
        public PlayerPrefabComponentDisposition Disposition;
        public bool IsOnRoot;
        public bool IsEnabled;
        public bool HasDebugTestNaming;
        public bool RequiredByMasterTestBatch;
        public bool RequiredByHostingBatch;
        public List<string> ReferencingScripts = new List<string>();
        public List<string> DependencyNotes = new List<string>();
    }

    public sealed class PlayerPrefabAuditSummary
    {
        public string GeneratedUtc;
        public string ReportAbsolutePath;
        public List<string> PrefabPathsAudited = new List<string>();
        public List<PlayerPrefabComponentAuditEntry> Entries = new List<PlayerPrefabComponentAuditEntry>();
        public int TotalMissingScripts;
        public int TotalDisabledComponents;
        public List<string> Warnings = new List<string>();
    }

    public static class CCS_CharacterControllerPlayerPrefabAuditUtility
    {
        public const string ReportRelativePath =
            "Logs/CharacterController/PlayerPrefabAudit/CCS_PlayerPrefab_ComponentAudit_v0.7.2.md";

        public const int FutureProductionRootMonoBehaviourTarget = 6;

        public const int RootMonoBehaviourWarningThreshold = 20;

        private static readonly string[] AuditedPrefabPaths =
        {
            CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,
            CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath,
            CCS_AIConstants.AIBanditPrefabPath,
            CCS_AIConstants.AIBanditModelEnemyAIPrefabPath,
        };

        private static readonly string[] DeprecatedPlayerPrefabPaths =
        {
            CCS_PlayerPrefabConstants.DeprecatedOfflinePlayerPrefabPath,
            CCS_PlayerPrefabConstants.DeprecatedTestsNetworkedPlayerPrefabPath,
            CCS_PlayerPrefabConstants.DeprecatedNetworkedPlayerDuplicatePrefabPath,
        };

        private static readonly string[] TestOnlyNameTokens =
        {
            "Test",
            "Debug",
            "Diagnostics",
            "OfflineBootstrap",
            "AttributeDebugInput",
            "DisplayProfile",
            "Placeholder",
            "Harness",
            "MasterTest",
            "Demo",
            "Prototype",
            "OnGUI",
            "Hotkey",
            "OneShotReport",
            "TestDamage",
        };

        private static readonly Dictionary<string, PlayerPrefabComponentClassification> KnownClassifications =
            new Dictionary<string, PlayerPrefabComponentClassification>(StringComparer.Ordinal)
            {
                { "CharacterController", PlayerPrefabComponentClassification.RequiredRoot },
                { "CCS_CharacterInputActionProvider", PlayerPrefabComponentClassification.RequiredRoot },
                { "CCS_CharacterMotor", PlayerPrefabComponentClassification.RequiredRoot },
                { "CCS_CharacterControllerService", PlayerPrefabComponentClassification.RequiredRoot },
                { "CCS_CharacterAimLocomotionController", PlayerPrefabComponentClassification.RequiredRoot },
                { "NetworkObject", PlayerPrefabComponentClassification.RequiredNetwork },
                { "CCS_ClientOwnerNetworkTransform", PlayerPrefabComponentClassification.RequiredNetwork },
                { "NetworkTransform", PlayerPrefabComponentClassification.RequiredNetwork },
                { "CCS_NetworkPlayerController", PlayerPrefabComponentClassification.RequiredNetwork },
                { "CCS_NetworkPlayerNameplate", PlayerPrefabComponentClassification.RequiredNetwork },
                { "CCS_LocalPlayerOfflineBootstrap", PlayerPrefabComponentClassification.TestOnly },
                { "CCS_TestPlayerAttributeDebugInput", PlayerPrefabComponentClassification.TestOnly },
                { "CCS_PlayerNameplateBillboard", PlayerPrefabComponentClassification.TestOnly },
                { "CCS_PlayerDisplayProfileApplicator", PlayerPrefabComponentClassification.TestOnly },
                { "CCS_CharacterCameraController", PlayerPrefabComponentClassification.RequiredCamera },
                { "CCS_CharacterCameraFollowAnchor", PlayerPrefabComponentClassification.RequiredCamera },
                { "CCS_FirstPersonBodyCameraAnchor", PlayerPrefabComponentClassification.RequiredCamera },
                { "CCS_LocalFirstPersonHeadVisibility", PlayerPrefabComponentClassification.RequiredCamera },
                { "CCS_PlayerLocomotionAnimator", PlayerPrefabComponentClassification.RequiredAnimation },
                { "CCS_PlayerInteractionAnimator", PlayerPrefabComponentClassification.RequiredInteraction },
                { "Animator", PlayerPrefabComponentClassification.RequiredAnimation },
                { "CCS_NetworkInteractionScanner", PlayerPrefabComponentClassification.RequiredInteraction },
                { "CCS_InteractionPromptHudPresenter", PlayerPrefabComponentClassification.RequiredInteraction },
                { "CCS_RevolverController", PlayerPrefabComponentClassification.RequiredWeaponsBridge },
                { "CCS_PlayerWeaponLoadout", PlayerPrefabComponentClassification.RequiredWeaponsBridge },
                { "CCS_WeaponCarryStateController", PlayerPrefabComponentClassification.RequiredWeaponsBridge },
                { "CCS_EquipmentSocketRegistry", PlayerPrefabComponentClassification.RequiredEquipmentVisual },
                { "CCS_PlayerEquipmentVisualController", PlayerPrefabComponentClassification.RequiredEquipmentVisual },
                { "CCS_EquipmentSocketMarker", PlayerPrefabComponentClassification.RequiredEquipmentVisual },
                { "CCS_AttributeContainer", PlayerPrefabComponentClassification.RequiredAttributes },
                { "CCS_AttributeService", PlayerPrefabComponentClassification.RequiredAttributes },
                { "CCS_NetworkAttributeReplicator", PlayerPrefabComponentClassification.RequiredAttributes },
                { "CCS_StaminaController", PlayerPrefabComponentClassification.RequiredAttributes },
                { "CCS_HealthRegenController", PlayerPrefabComponentClassification.RequiredAttributes },
                { "CCS_NetworkHealth", PlayerPrefabComponentClassification.RequiredAttributes },
                { "CCS_PlayerDeathScreenController", PlayerPrefabComponentClassification.RequiredUIBridge },
                { "CCS_AttributeBarsHudPresenter", PlayerPrefabComponentClassification.RequiredUIBridge },
                { "CCS_WeaponHudPresenter", PlayerPrefabComponentClassification.RequiredUIBridge },
            };

        private static readonly HashSet<string> MasterTestBatchRequiredTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "CCS_NetworkPlayerController",
            "CCS_ClientOwnerNetworkTransform",
            "NetworkObject",
            "CCS_CharacterInputActionProvider",
            "CCS_CharacterMotor",
            "CCS_CharacterCameraController",
            "CCS_NetworkInteractionScanner",
            "CCS_RevolverController",
            "CCS_NetworkHealth",
            "CCS_PlayerDeathScreenController",
        };

        private static readonly HashSet<string> HostingBatchRequiredTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "NetworkObject",
            "CCS_ClientOwnerNetworkTransform",
            "CCS_NetworkPlayerController",
            "CCS_NetworkPlayerNameplate",
            "CCS_CharacterInputActionProvider",
            "CCS_CharacterMotor",
        };

        public static PlayerPrefabAuditSummary RunAuditAndWriteReport(out CCS_SurvivalValidationResult validationResult)
        {
            PlayerPrefabAuditSummary summary = BuildAuditSummary();
            validationResult = ValidateAuditResults(summary);
            WriteMarkdownReport(summary, validationResult);
            return summary;
        }

        public static CCS_SurvivalValidationResult ValidatePhase2CAuditFoundation()
        {
            List<string> failures = new List<string>();
            ValidatePrefabPathsExist(failures);
            ValidatePlayerPrefabsLoadable(failures);
            ValidateNoMissingScriptsOnActivePrefabs(failures);
            ValidateNoEditorScriptsOnRuntimePrefabs(failures);
            ValidateNetworkManagerPlayerPrefabReference(failures);
            CCS_CharacterControllerPhase2BValidationUtility.ValidatePhase2BFoundation().AppendFailuresIfAny(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Character Controller player prefab audit foundation validated.");
        }

        private static PlayerPrefabAuditSummary BuildAuditSummary()
        {
            PlayerPrefabAuditSummary summary = new PlayerPrefabAuditSummary
            {
                GeneratedUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
                ReportAbsolutePath = GetReportAbsolutePath(),
            };

            Dictionary<string, List<string>> referenceCache = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            for (int i = 0; i < AuditedPrefabPaths.Length; i++)
            {
                string prefabPath = AuditedPrefabPaths[i];
                if (!File.Exists(prefabPath))
                {
                    summary.Warnings.Add("Prefab not found (skipped): " + prefabPath);
                    continue;
                }

                summary.PrefabPathsAudited.Add(prefabPath);
                ScanPrefab(prefabPath, summary, referenceCache);
            }

            for (int i = 0; i < DeprecatedPlayerPrefabPaths.Length; i++)
            {
                string deprecatedPath = DeprecatedPlayerPrefabPaths[i];
                if (File.Exists(deprecatedPath))
                {
                    summary.Warnings.Add("Deprecated player prefab still present: " + deprecatedPath);
                }
            }

            AppendRootBudgetWarnings(summary);
            AppendTestOnlyWarnings(summary);
            return summary;
        }

        private static void ScanPrefab(
            string prefabPath,
            PlayerPrefabAuditSummary summary,
            Dictionary<string, List<string>> referenceCache)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabRoot == null)
            {
                summary.Warnings.Add("Could not load prefab asset: " + prefabPath);
                return;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null)
            {
                summary.Warnings.Add("Could not open prefab contents: " + prefabPath);
                return;
            }

            try
            {
                Transform rootTransform = prefabContents.transform;
                Component[] components = prefabContents.GetComponentsInChildren<Component>(true);
                for (int i = 0; i < components.Length; i++)
                {
                    Component component = components[i];
                    if (component == null)
                    {
                        summary.TotalMissingScripts++;
                        continue;
                    }

                    if (component is Transform)
                    {
                        continue;
                    }

                    if (!IsComponentEnabled(component))
                    {
                        summary.TotalDisabledComponents++;
                    }

                    PlayerPrefabComponentAuditEntry entry = BuildEntry(
                        prefabPath,
                        rootTransform,
                        component,
                        referenceCache);
                    summary.Entries.Add(entry);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private static PlayerPrefabComponentAuditEntry BuildEntry(
            string prefabPath,
            Transform prefabRoot,
            Component component,
            Dictionary<string, List<string>> referenceCache)
        {
            Type componentType = component.GetType();
            string typeName = componentType.Name;
            string namespaceName = componentType.Namespace ?? string.Empty;
            string hierarchyPath = GetHierarchyPath(component.transform, prefabRoot);
            bool isOnRoot = component.transform == prefabRoot;
            MonoScript monoScript = component is MonoBehaviour behaviour
                ? MonoScript.FromMonoBehaviour(behaviour)
                : null;
            string scriptPath = monoScript != null ? AssetDatabase.GetAssetPath(monoScript) : string.Empty;
            string assemblyName = !string.IsNullOrEmpty(scriptPath)
                ? CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath) ?? string.Empty
                : string.Empty;

            PlayerPrefabComponentLayer layer = ResolveLayer(typeName, namespaceName, scriptPath, assemblyName);
            bool hasDebugTestNaming = HasDebugTestNaming(typeName, hierarchyPath, namespaceName);
            PlayerPrefabComponentClassification classification = ClassifyComponent(
                typeName,
                layer,
                hasDebugTestNaming,
                isOnRoot,
                hierarchyPath);
            PlayerPrefabComponentDisposition disposition = ResolveDisposition(classification, layer, isOnRoot);

            PlayerPrefabComponentAuditEntry entry = new PlayerPrefabComponentAuditEntry
            {
                PrefabPath = prefabPath,
                HierarchyPath = hierarchyPath,
                ComponentType = typeName,
                NamespaceName = namespaceName,
                AssemblyName = assemblyName,
                ScriptAssetPath = scriptPath,
                SerializedRole = InferSerializedRole(typeName),
                Layer = layer,
                Classification = classification,
                Disposition = disposition,
                IsOnRoot = isOnRoot,
                IsEnabled = IsComponentEnabled(component),
                HasDebugTestNaming = hasDebugTestNaming,
                RequiredByMasterTestBatch = MasterTestBatchRequiredTypes.Contains(typeName),
                RequiredByHostingBatch = HostingBatchRequiredTypes.Contains(typeName),
            };

            entry.ReferencingScripts.AddRange(FindReferencingScripts(typeName, referenceCache));
            AppendDependencyNotes(entry);
            return entry;
        }

        private static PlayerPrefabComponentLayer ResolveLayer(
            string typeName,
            string namespaceName,
            string scriptPath,
            string assemblyName)
        {
            if (string.IsNullOrEmpty(scriptPath) && typeName != "Transform")
            {
                return PlayerPrefabComponentLayer.UnityBuiltIn;
            }

            if (!string.IsNullOrEmpty(scriptPath)
                && (scriptPath.IndexOf("/Editor/", StringComparison.OrdinalIgnoreCase) >= 0
                    || assemblyName.IndexOf(".Editor", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return PlayerPrefabComponentLayer.EditorOnlyInvalid;
            }

            if (namespaceName.IndexOf(".Tests", StringComparison.Ordinal) >= 0
                || assemblyName.IndexOf(".Tests", StringComparison.Ordinal) >= 0)
            {
                return PlayerPrefabComponentLayer.Tests;
            }

            if (namespaceName.StartsWith("CCS.Modules.CharacterController", StringComparison.Ordinal))
            {
                return PlayerPrefabComponentLayer.Runtime;
            }

            if (namespaceName.StartsWith("CCS.Modules.", StringComparison.Ordinal))
            {
                return PlayerPrefabComponentLayer.ExternalModule;
            }

            if (namespaceName.StartsWith("UnityEngine", StringComparison.Ordinal)
                || namespaceName.StartsWith("Unity.Netcode", StringComparison.Ordinal)
                || namespaceName.StartsWith("Unity.Cinemachine", StringComparison.Ordinal)
                || namespaceName.StartsWith("Unity.", StringComparison.Ordinal)
                || namespaceName.StartsWith("TMPro", StringComparison.Ordinal))
            {
                return PlayerPrefabComponentLayer.UnityBuiltIn;
            }

            return PlayerPrefabComponentLayer.Unknown;
        }

        private static PlayerPrefabComponentClassification ClassifyComponent(
            string typeName,
            PlayerPrefabComponentLayer layer,
            bool hasDebugTestNaming,
            bool isOnRoot,
            string hierarchyPath)
        {
            if (KnownClassifications.TryGetValue(typeName, out PlayerPrefabComponentClassification known))
            {
                if (known == PlayerPrefabComponentClassification.TestOnly)
                {
                    return known;
                }

                if (hasDebugTestNaming && typeName.IndexOf("Debug", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return PlayerPrefabComponentClassification.DebugOnly;
                }

                return known;
            }

            if (layer == PlayerPrefabComponentLayer.Tests || hasDebugTestNaming)
            {
                if (typeName.IndexOf("Debug", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return PlayerPrefabComponentClassification.DebugOnly;
                }

                if (typeName.IndexOf("Diagnostic", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return PlayerPrefabComponentClassification.DiagnosticsOnly;
                }

                return PlayerPrefabComponentClassification.TestOnly;
            }

            if (typeName.IndexOf("Debug", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PlayerPrefabComponentClassification.TestingManagerCandidate;
            }

            if (hierarchyPath.IndexOf("Hud", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Hud", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Presenter", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PlayerPrefabComponentClassification.RequiredUIBridge;
            }

            if (typeName.IndexOf("Animator", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Rig", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Constraint", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PlayerPrefabComponentClassification.RequiredAnimation;
            }

            if (isOnRoot)
            {
                return PlayerPrefabComponentClassification.RequiredRuntime;
            }

            return PlayerPrefabComponentClassification.UnknownReview;
        }

        private static PlayerPrefabComponentDisposition ResolveDisposition(
            PlayerPrefabComponentClassification classification,
            PlayerPrefabComponentLayer layer,
            bool isOnRoot)
        {
            switch (classification)
            {
                case PlayerPrefabComponentClassification.TestOnly:
                case PlayerPrefabComponentClassification.DebugOnly:
                case PlayerPrefabComponentClassification.DiagnosticsOnly:
                case PlayerPrefabComponentClassification.TestingManagerCandidate:
                    return PlayerPrefabComponentDisposition.MoveLater;
                case PlayerPrefabComponentClassification.SceneManagerCandidate:
                    return PlayerPrefabComponentDisposition.MoveLater;
                case PlayerPrefabComponentClassification.DeprecatedCandidate:
                    return PlayerPrefabComponentDisposition.MoveLater;
                case PlayerPrefabComponentClassification.UnknownReview:
                    return PlayerPrefabComponentDisposition.NeedsReview;
                default:
                    if (isOnRoot && classification != PlayerPrefabComponentClassification.RequiredRoot
                        && classification != PlayerPrefabComponentClassification.RequiredNetwork)
                    {
                        return PlayerPrefabComponentDisposition.NeedsReview;
                    }

                    return layer == PlayerPrefabComponentLayer.EditorOnlyInvalid
                        ? PlayerPrefabComponentDisposition.NeedsReview
                        : PlayerPrefabComponentDisposition.Stay;
            }
        }

        private static void AppendDependencyNotes(PlayerPrefabComponentAuditEntry entry)
        {
            if (entry.Disposition != PlayerPrefabComponentDisposition.MoveLater
                && entry.Disposition != PlayerPrefabComponentDisposition.NeedsReview)
            {
                return;
            }

            if (entry.RequiredByMasterTestBatch)
            {
                entry.DependencyNotes.Add("Required by Master Test batch validation/builder paths.");
            }

            if (entry.RequiredByHostingBatch)
            {
                entry.DependencyNotes.Add("Required by Hosting batch / NetworkManager player prefab wiring.");
            }

            if (entry.ReferencingScripts.Count > 0)
            {
                entry.DependencyNotes.Add("Referenced by: " + string.Join(", ", entry.ReferencingScripts.Take(8)));
            }

            if (entry.Classification == PlayerPrefabComponentClassification.TestingManagerCandidate
                || entry.Classification == PlayerPrefabComponentClassification.DebugOnly)
            {
                entry.DependencyNotes.Add("Replacement path: gate debug toggles through CCS_CharacterControllerDiagnosticsManager.");
            }

            if (entry.Classification == PlayerPrefabComponentClassification.TestOnly && entry.IsOnRoot)
            {
                entry.DependencyNotes.Add("Candidate to move off root after scene/bootstrap replacement is proven.");
            }

            if (entry.Layer == PlayerPrefabComponentLayer.EditorOnlyInvalid)
            {
                entry.DependencyNotes.Add("Editor-only script must not ship on runtime prefabs.");
            }
        }

        private static List<string> FindReferencingScripts(
            string typeName,
            Dictionary<string, List<string>> referenceCache)
        {
            if (referenceCache.TryGetValue(typeName, out List<string> cached))
            {
                return cached;
            }

            List<string> references = new List<string>();
            string[] scriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < scriptFiles.Length; i++)
            {
                string relativePath = "Assets" + scriptFiles[i].Substring(Application.dataPath.Length).Replace('\\', '/');
                if (relativePath.IndexOf("/Editor/", StringComparison.OrdinalIgnoreCase) >= 0
                    && relativePath.IndexOf("Validation", StringComparison.OrdinalIgnoreCase) < 0
                    && relativePath.IndexOf("Builder", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                string source = File.ReadAllText(scriptFiles[i]);
                if (source.IndexOf(typeName, StringComparison.Ordinal) >= 0)
                {
                    references.Add(relativePath);
                }

                if (references.Count >= 12)
                {
                    break;
                }
            }

            referenceCache[typeName] = references;
            return references;
        }

        private static bool HasDebugTestNaming(string typeName, string hierarchyPath, string namespaceName)
        {
            if (MatchesTestOnlyToken(typeName))
            {
                return true;
            }

            if (namespaceName.IndexOf(".Tests", StringComparison.Ordinal) >= 0
                && (typeName.IndexOf("Test", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("Debug", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("Diagnostic", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            if (string.IsNullOrEmpty(hierarchyPath))
            {
                return false;
            }

            string[] segments = hierarchyPath.Split('/');
            for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
            {
                if (MatchesTestOnlyToken(segments[segmentIndex]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesTestOnlyToken(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < TestOnlyNameTokens.Length; i++)
            {
                string token = TestOnlyNameTokens[i];
                if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string InferSerializedRole(string typeName)
        {
            if (typeName.IndexOf("Network", StringComparison.Ordinal) >= 0)
            {
                return "Network replication / ownership";
            }

            if (typeName.IndexOf("Hud", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Presenter", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "UI bridge / HUD presenter";
            }

            if (typeName.IndexOf("Animator", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Animation driver";
            }

            if (typeName.IndexOf("Camera", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Camera rig / follow";
            }

            if (typeName.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Revolver", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Weapons bridge";
            }

            if (typeName.IndexOf("Attribute", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Health", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Stamina", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Attributes bridge";
            }

            if (typeName.IndexOf("Interaction", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Interaction bridge";
            }

            return "Gameplay component";
        }

        private static string GetHierarchyPath(Transform target, Transform prefabRoot)
        {
            if (target == null)
            {
                return string.Empty;
            }

            List<string> segments = new List<string>();
            Transform current = target;
            while (current != null)
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

        private static void AppendRootBudgetWarnings(PlayerPrefabAuditSummary summary)
        {
            for (int i = 0; i < summary.PrefabPathsAudited.Count; i++)
            {
                string prefabPath = summary.PrefabPathsAudited[i];
                int rootCount = summary.Entries.Count(entry =>
                    entry.PrefabPath == prefabPath
                    && entry.IsOnRoot
                    && entry.ComponentType != "Transform");
                if (rootCount >= RootMonoBehaviourWarningThreshold)
                {
                    summary.Warnings.Add(
                        prefabPath + " root MonoBehaviour count is high (" + rootCount + "). Future production target: "
                        + FutureProductionRootMonoBehaviourTarget + ".");
                }
            }
        }

        private static void AppendTestOnlyWarnings(PlayerPrefabAuditSummary summary)
        {
            int testOnlyOnRoot = summary.Entries.Count(entry =>
                entry.IsOnRoot
                && (entry.Classification == PlayerPrefabComponentClassification.TestOnly
                    || entry.Classification == PlayerPrefabComponentClassification.DebugOnly
                    || entry.Classification == PlayerPrefabComponentClassification.DiagnosticsOnly));
            if (testOnlyOnRoot > 0)
            {
                summary.Warnings.Add(
                    "Found " + testOnlyOnRoot + " test/debug/diagnostics component(s) on player prefab root.");
            }
        }

        private static CCS_SurvivalValidationResult ValidateAuditResults(PlayerPrefabAuditSummary summary)
        {
            List<string> failures = new List<string>();
            ValidatePrefabPathsExist(failures);
            ValidateNoMissingScriptsOnActivePrefabs(failures);
            ValidateNoEditorScriptsOnRuntimePrefabs(failures);
            CCS_CharacterControllerPhase2DValidationUtility.ValidatePhase2DSeparation().AppendFailuresIfAny(failures);
            ValidateNetworkManagerPlayerPrefabReference(failures);

            if (summary.TotalMissingScripts > 0)
            {
                failures.Add("Player prefab audit found " + summary.TotalMissingScripts + " missing script slot(s).");
            }

            if (summary.PrefabPathsAudited.Count == 0)
            {
                failures.Add("No player prefabs were audited.");
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            if (summary.Warnings.Count > 0)
            {
                return CCS_SurvivalValidationResult.Warn(
                    "Player prefab audit completed with warnings: " + string.Join(" ", summary.Warnings));
            }

            return CCS_SurvivalValidationResult.Pass("Player prefab component audit completed.");
        }

        private static void ValidatePrefabPathsExist(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath),
                "Missing active networked player prefab at " + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath),
                "Missing Kevin player model prefab at " + CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.AIBanditModelEnemyAIPrefabPath),
                "Missing EnemyAI bandit model prefab at " + CCS_AIConstants.AIBanditModelEnemyAIPrefabPath);
        }

        private static void ValidatePlayerPrefabsLoadable(List<string> failures)
        {
            for (int i = 0; i < AuditedPrefabPaths.Length; i++)
            {
                string path = AuditedPrefabPaths[i];
                if (!File.Exists(path))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                AppendIfMissing(failures, prefab != null, "AssetDatabase could not load player prefab: " + path);
            }
        }

        private static void ValidateNoMissingScriptsOnActivePrefabs(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !PrefabHasMissingScripts(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath),
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath + " contains missing scripts.");
        }

        private static void ValidateNoEditorScriptsOnRuntimePrefabs(List<string> failures)
        {
            if (!File.Exists(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath))
            {
                return;
            }

            PlayerPrefabAuditSummary summary = BuildAuditSummary();
            bool hasEditorOnly = summary.Entries.Any(entry =>
                entry.PrefabPath == CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath
                && entry.Layer == PlayerPrefabComponentLayer.EditorOnlyInvalid);
            AppendIfMissing(
                failures,
                !hasEditorOnly,
                "Editor-only scripts are attached to " + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath + ".");
        }

        public static CCS_SurvivalValidationResult ValidateNetworkManagerPlayerPrefabReference()
        {
            List<string> failures = new List<string>();
            ValidateNetworkManagerPlayerPrefabReference(failures);
            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("NetworkManager player prefab reference validated.");
        }

        private static void ValidateNetworkManagerPlayerPrefabReference(List<string> failures)
        {
            if (!File.Exists(CCS_NetcodeConstants.NetworkManagerPrefabPath))
            {
                failures.Add("Missing NetworkManager prefab at " + CCS_NetcodeConstants.NetworkManagerPrefabPath);
                return;
            }

            GameObject networkManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkManagerPrefabPath);
            if (networkManagerPrefab == null)
            {
                failures.Add("Could not load NetworkManager prefab asset.");
                return;
            }

            NetworkManager networkManager = networkManagerPrefab.GetComponent<NetworkManager>();
            AppendIfMissing(failures, networkManager != null, "NetworkManager prefab missing NetworkManager component.");
            if (networkManager == null)
            {
                return;
            }

            GameObject playerPrefab = networkManager.NetworkConfig.PlayerPrefab;
            AppendIfMissing(failures, playerPrefab != null, "NetworkManager PlayerPrefab reference is null.");
            if (playerPrefab == null)
            {
                return;
            }

            string playerPath = AssetDatabase.GetAssetPath(playerPrefab);
            AppendIfMissing(
                failures,
                playerPath == CCS_NetcodeConstants.NetworkedPlayerPrefabPath,
                "NetworkManager PlayerPrefab must reference " + CCS_NetcodeConstants.NetworkedPlayerPrefabPath
                + " (found " + playerPath + ").");
        }

        private static bool PrefabHasMissingScripts(string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                return true;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (contents == null)
            {
                return true;
            }

            try
            {
                Component[] components = contents.GetComponentsInChildren<Component>(true);
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            return false;
        }

        private static void WriteMarkdownReport(
            PlayerPrefabAuditSummary summary,
            CCS_SurvivalValidationResult validationResult)
        {
            string reportPath = summary.ReportAbsolutePath;
            string reportDirectory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Player Prefab Component Audit — v0.7.2");
            builder.AppendLine();
            builder.AppendLine("**Generated:** " + summary.GeneratedUtc);
            builder.AppendLine("**Validation:** " + (validationResult.IsSuccess ? "PASS" : "FAIL")
                + (validationResult.IsWarning ? " (warnings)" : string.Empty));
            builder.AppendLine();
            builder.AppendLine("## 1. Summary");
            builder.AppendLine();
            builder.AppendLine("- Active networked player prefab is the canonical spawn target for validation and hosting.");
            builder.AppendLine("- Production prefab path: `" + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath + "`.");
            builder.AppendLine("- This milestone classifies components only. No prefab hierarchy rewrite was performed.");
            builder.AppendLine("- Validation scene uses `CCS_CharacterControllerDiagnosticsManager` on `CCS_DiagnosticsManager`.");
            builder.AppendLine();
            builder.AppendLine("| Metric | Value |");
            builder.AppendLine("|--------|-------|");
            builder.AppendLine("| Prefabs audited | " + summary.PrefabPathsAudited.Count + " |");
            builder.AppendLine("| Total component entries | " + summary.Entries.Count + " |");
            builder.AppendLine("| Missing scripts | " + summary.TotalMissingScripts + " |");
            builder.AppendLine("| Disabled components | " + summary.TotalDisabledComponents + " |");
            builder.AppendLine("| Future production root MonoBehaviour target | " + FutureProductionRootMonoBehaviourTarget + " |");
            builder.AppendLine();
            if (summary.Warnings.Count > 0)
            {
                builder.AppendLine("**Warnings:**");
                for (int i = 0; i < summary.Warnings.Count; i++)
                {
                    builder.AppendLine("- " + summary.Warnings[i]);
                }

                builder.AppendLine();
            }

            builder.AppendLine("## 2. Prefabs audited");
            builder.AppendLine();
            for (int i = 0; i < summary.PrefabPathsAudited.Count; i++)
            {
                builder.AppendLine("- `" + summary.PrefabPathsAudited[i] + "`");
            }

            builder.AppendLine();
            builder.AppendLine("Deprecated paths (must remain absent):");
            for (int i = 0; i < DeprecatedPlayerPrefabPaths.Length; i++)
            {
                builder.AppendLine("- `" + DeprecatedPlayerPrefabPaths[i] + "` → "
                    + (File.Exists(DeprecatedPlayerPrefabPaths[i]) ? "PRESENT (review)" : "absent (expected)"));
            }

            builder.AppendLine();
            builder.AppendLine("## 3. Component counts by prefab");
            builder.AppendLine();
            for (int i = 0; i < summary.PrefabPathsAudited.Count; i++)
            {
                string prefabPath = summary.PrefabPathsAudited[i];
                int total = summary.Entries.Count(entry => entry.PrefabPath == prefabPath);
                int rootCount = summary.Entries.Count(entry => entry.PrefabPath == prefabPath && entry.IsOnRoot);
                int childCount = total - rootCount;
                builder.AppendLine("### `" + Path.GetFileName(prefabPath) + "`");
                builder.AppendLine("- Total MonoBehaviour/component entries: **" + total + "**");
                builder.AppendLine("- Root entries: **" + rootCount + "**");
                builder.AppendLine("- Child entries: **" + childCount + "**");
                builder.AppendLine();
            }

            builder.AppendLine("## 4. Root component counts");
            builder.AppendLine();
            for (int i = 0; i < summary.PrefabPathsAudited.Count; i++)
            {
                string prefabPath = summary.PrefabPathsAudited[i];
                IEnumerable<PlayerPrefabComponentAuditEntry> rootEntries = summary.Entries
                    .Where(entry => entry.PrefabPath == prefabPath && entry.IsOnRoot)
                    .OrderBy(entry => entry.ComponentType);
                builder.AppendLine("### `" + Path.GetFileName(prefabPath) + "` root components");
                builder.AppendLine();
                builder.AppendLine("| Type | Classification | Layer | Disposition |");
                builder.AppendLine("|------|----------------|-------|-------------|");
                foreach (PlayerPrefabComponentAuditEntry entry in rootEntries)
                {
                    builder.AppendLine("| `" + entry.ComponentType + "` | " + entry.Classification + " | "
                        + entry.Layer + " | " + entry.Disposition + " |");
                }

                builder.AppendLine();
            }

            AppendClassificationSection(builder, summary, "5. Components by classification", entry => true);
            AppendClassificationSection(
                builder,
                summary,
                "6. TestOnly candidates",
                entry => entry.Classification == PlayerPrefabComponentClassification.TestOnly);
            AppendClassificationSection(
                builder,
                summary,
                "7. DebugOnly / DiagnosticsOnly candidates",
                entry => entry.Classification == PlayerPrefabComponentClassification.DebugOnly
                    || entry.Classification == PlayerPrefabComponentClassification.DiagnosticsOnly);
            AppendClassificationSection(
                builder,
                summary,
                "8. TestingManagerCandidate components",
                entry => entry.Classification == PlayerPrefabComponentClassification.TestingManagerCandidate);
            AppendClassificationSection(
                builder,
                summary,
                "9. SceneManagerCandidate components",
                entry => entry.Classification == PlayerPrefabComponentClassification.SceneManagerCandidate);
            AppendClassificationSection(
                builder,
                summary,
                "10. Required components that must not move yet",
                entry => entry.Disposition == PlayerPrefabComponentDisposition.Stay
                    && (entry.RequiredByMasterTestBatch || entry.RequiredByHostingBatch));
            AppendClassificationSection(
                builder,
                summary,
                "11. External module bridge components",
                entry => entry.Layer == PlayerPrefabComponentLayer.ExternalModule);

            builder.AppendLine("## 12. Missing scripts / null references");
            builder.AppendLine();
            builder.AppendLine("- Missing script slots: **" + summary.TotalMissingScripts + "**");
            builder.AppendLine("- Disabled components: **" + summary.TotalDisabledComponents + "**");
            builder.AppendLine();

            builder.AppendLine("## 13. Recommended Phase 2D actions");
            builder.AppendLine();
            builder.AppendLine("1. Route remaining per-component debug booleans through `CCS_CharacterControllerDiagnosticsManager` toggles.");
            builder.AppendLine("2. Reduce root MonoBehaviour count toward future production target (" + FutureProductionRootMonoBehaviourTarget + ") without a big-bang prefab rewrite.");
            builder.AppendLine("3. Evaluate moving external module bridges off root in Phase 2E after batch + Play Mode validation.");
            builder.AppendLine();

            builder.AppendLine("## 14. Guardrails for future component removal");
            builder.AppendLine();
            builder.AppendLine("- Do not remove live components until batch + manual smoke test prove the replacement path.");
            builder.AppendLine("- Do not alter animator controller, production clips, or player visual hierarchy without a dedicated milestone.");
            builder.AppendLine("- Equipment Fit Studio remains the production equipment fit workflow.");
            builder.AppendLine("- Generated audit reports stay under `Logs/` and are not committed.");
            builder.AppendLine();

            AppendDetailedEntryAppendix(builder, summary);
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
        }

        private static void AppendClassificationSection(
            StringBuilder builder,
            PlayerPrefabAuditSummary summary,
            string title,
            Func<PlayerPrefabComponentAuditEntry, bool> predicate)
        {
            builder.AppendLine("## " + title);
            builder.AppendLine();
            IEnumerable<IGrouping<PlayerPrefabComponentClassification, PlayerPrefabComponentAuditEntry>> groups =
                summary.Entries.Where(predicate).GroupBy(entry => entry.Classification).OrderBy(group => group.Key);
            bool wroteAny = false;
            foreach (IGrouping<PlayerPrefabComponentClassification, PlayerPrefabComponentAuditEntry> group in groups)
            {
                wroteAny = true;
                builder.AppendLine("### " + group.Key + " (" + group.Count() + ")");
                builder.AppendLine();
                foreach (PlayerPrefabComponentAuditEntry entry in group.OrderBy(item => item.PrefabPath).ThenBy(item => item.HierarchyPath))
                {
                    builder.AppendLine("- `" + entry.ComponentType + "` @ `" + entry.HierarchyPath + "` (" + entry.Layer + ", "
                        + entry.Disposition + ")");
                }

                builder.AppendLine();
            }

            if (!wroteAny)
            {
                builder.AppendLine("_None._");
                builder.AppendLine();
            }
        }

        private static void AppendDetailedEntryAppendix(StringBuilder builder, PlayerPrefabAuditSummary summary)
        {
            builder.AppendLine("## Appendix — Full component inventory");
            builder.AppendLine();
            builder.AppendLine("| Prefab | Hierarchy | Type | Namespace | Assembly | Layer | Class | Root | Enabled | Notes |");
            builder.AppendLine("|--------|-----------|------|-----------|----------|-------|-------|------|---------|-------|");
            foreach (PlayerPrefabComponentAuditEntry entry in summary.Entries.OrderBy(item => item.PrefabPath).ThenBy(item => item.HierarchyPath))
            {
                string notes = entry.DependencyNotes.Count > 0
                    ? string.Join("; ", entry.DependencyNotes)
                    : entry.SerializedRole;
                builder.AppendLine("| `" + Path.GetFileName(entry.PrefabPath) + "` | `" + entry.HierarchyPath + "` | `"
                    + entry.ComponentType + "` | `" + entry.NamespaceName + "` | `" + entry.AssemblyName + "` | "
                    + entry.Layer + " | " + entry.Classification + " | " + (entry.IsOnRoot ? "yes" : "no") + " | "
                    + (entry.IsEnabled ? "yes" : "no") + " | " + EscapeMarkdown(notes) + " |");
            }
        }

        private static string EscapeMarkdown(string value)
        {
            return value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string GetReportAbsolutePath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, ReportRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static bool IsComponentEnabled(Component component)
        {
            if (component == null)
            {
                return false;
            }

            if (component is Behaviour behaviour)
            {
                return behaviour.enabled;
            }

            return component.gameObject.activeInHierarchy;
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        private static void AppendFailuresIfAny(this CCS_SurvivalValidationResult result, List<string> failures)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }
    }
}
