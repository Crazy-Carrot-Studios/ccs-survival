using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverRightHandFitProfileValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.10b right-hand revolver fit profile workflow.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverRightHandFitProfileValidationUtility
    {
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string RightHandFitEditorMenusPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioRightHandFitEditorMenus.cs";

        private const string EquipmentVisualSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";

        private const string ApplicatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Equipment/Fitting/CCS_WeaponAttachmentFitProfileApplicator.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        public static CCS_SurvivalValidationResult ValidateRevolverRightHandFitProfile()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateFitProfileAsset(failures);
            ValidateEquipmentVisualOffsetParent(failures);
            ValidateApplicatorHelpers(failures);
            ValidateEquipmentFitStudioSupport(failures);
            ValidateHandSocketOnPlayerPrefab(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Revolver right-hand fit profile validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateFitProfileAsset(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath),
                "Missing CCS_RevolverM1879_RightHandEquipped_Fit asset.");

            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            AppendIfMissing(failures, profile != null, "Could not load right-hand equipped fit profile.");
            if (profile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                profile.SocketLocalScale.x > 0f
                    && profile.SocketLocalScale.y > 0f
                    && profile.SocketLocalScale.z > 0f,
                "Right-hand fit profile scale must be valid (> 0).");
            AppendIfMissing(
                failures,
                profile.SocketId == CCS_EquipmentConstants.HandSocketRightId,
                "Right-hand fit profile socketId must be CCS_HandSocket_Right.");
        }

        private static void ValidateEquipmentVisualOffsetParent(List<string> failures)
        {
            if (!File.Exists(EquipmentVisualSourcePath))
            {
                failures.Add("Missing CCS_PlayerEquipmentVisualController.");
                return;
            }

            string source = File.ReadAllText(EquipmentVisualSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("EnsureRightHandAttachmentOffsetRoot")
                    && source.Contains("RightHandRevolverAttachmentOffsetObjectName"),
                "Equipment visual controller must create CCS_RightHandRevolverAttachmentOffset for right-hand preview.");

            AppendIfMissing(
                failures,
                source.Contains("ApplyRightHandEquippedFitProfile")
                    && source.Contains("ShowDiagnosticsEquippedPreview"),
                "Diagnostics preview must apply right-hand fit profile through shared offset parent path.");

            AppendIfMissing(
                failures,
                source.Contains("ResetDirectVisualChildToIdentity"),
                "Equipment visual controller must reset visual child transform to identity under offset parent.");

            AppendIfMissing(
                failures,
                !source.Contains("localPosition = new Vector3(0.0909511")
                    && !source.Contains("localEulerAngles = new Vector3(-49.556"),
                "Equipment visual controller must not hardcode right-hand revolver offset values.");
        }

        private static void ValidateApplicatorHelpers(List<string> failures)
        {
            if (!File.Exists(ApplicatorSourcePath))
            {
                failures.Add("Missing CCS_WeaponAttachmentFitProfileApplicator.");
                return;
            }

            string source = File.ReadAllText(ApplicatorSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("ResetDirectVisualChildToIdentity"),
                "Fit applicator must support resetting direct visual children to identity.");

            if (File.Exists(EquipmentVisualSourcePath))
            {
                string visualSource = File.ReadAllText(EquipmentVisualSourcePath);
                int showDiagnosticsIndex = visualSource.IndexOf("ShowDiagnosticsEquippedPreview", System.StringComparison.Ordinal);
                if (showDiagnosticsIndex >= 0)
                {
                    string methodSlice = visualSource.Substring(
                        showDiagnosticsIndex,
                        Mathf.Min(2500, visualSource.Length - showDiagnosticsIndex));
                    int applyCount = 0;
                    int searchIndex = 0;
                    const string token = "ApplyProfileToAttachmentRoot(";
                    while (true)
                    {
                        int found = methodSlice.IndexOf(token, searchIndex, System.StringComparison.Ordinal);
                        if (found < 0)
                        {
                            break;
                        }

                        applyCount++;
                        searchIndex = found + token.Length;
                    }

                    AppendIfMissing(
                        failures,
                        applyCount <= 1,
                        "Diagnostics preview must not double-apply fit profile offset.");
                }
            }
        }

        private static void ValidateEquipmentFitStudioSupport(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio must remain present.");

            AppendIfMissing(
                failures,
                File.Exists(RightHandFitEditorMenusPath),
                "Missing right-hand fit editor helper menus.");

            string previewAttachmentPath =
                "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioPreviewAttachmentUtility.cs";
            if (File.Exists(previewAttachmentPath))
            {
                string source = File.ReadAllText(previewAttachmentPath);
                AppendIfMissing(
                    failures,
                    source.Contains("RightHandRevolverAttachmentOffsetObjectName"),
                    "Equipment Fit Studio preview attachment must use CCS_RightHandRevolverAttachmentOffset.");
            }
        }

        private static void ValidateHandSocketOnPlayerPrefab(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                failures.Add("Missing networked player prefab.");
                return;
            }

            CCS_EquipmentSocketRegistry registry = prefab.GetComponentInChildren<CCS_EquipmentSocketRegistry>(true);
            AppendIfMissing(failures, registry != null, "Player prefab must contain CCS_EquipmentSocketRegistry.");
            if (registry == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out _),
                "Player prefab must expose CCS_HandSocket_Right.");

            CCS_EquipmentSocketAnchor[] anchors = prefab.GetComponentsInChildren<CCS_EquipmentSocketAnchor>(true);
            bool hasRightHandAnchor = false;
            for (int i = 0; i < anchors.Length; i++)
            {
                if (anchors[i] != null && anchors[i].SocketId == CCS_EquipmentConstants.HandSocketRightId)
                {
                    hasRightHandAnchor = true;
                    AppendIfMissing(
                        failures,
                        anchors[i].name != CCS_EquipmentConstants.RightHandIkTargetObjectName,
                        "Right-hand equipment socket must not use IK target object name.");
                }
            }

            AppendIfMissing(failures, hasRightHandAnchor, "Player prefab must contain right-hand equipment socket anchor.");
        }

        private static void ValidateMissingScripts(List<string> failures)
        {
            CCS_SurvivalValidationResult missingScriptResult =
                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts();
            if (!missingScriptResult.IsSuccess)
            {
                failures.Add(missingScriptResult.Message);
            }
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            AppendIfPresent(failures, Directory.Exists(CharacterControllerTestsRoot), "CharacterController/Tests must not return.");
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            AppendIfPresent(failures, Directory.Exists(AnimationFitStudioRoot), "Animation Fit Studio must remain absent.");
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(EquipmentFitStudioWindowPath), "Equipment Fit Studio must remain present.");
        }

        private static void CollectDeferredWarnings(List<string> warnings)
        {
            warnings.Add("Offset may still require manual tuning in Equipment Fit Studio.");
            warnings.Add("Current fit may still be visually imperfect.");
            warnings.Add("Preview exists only in validation scene.");
        }

        private static void AppendIfMissing(List<string> target, bool condition, string message)
        {
            if (!condition)
            {
                target.Add(message);
            }
        }

        private static void AppendIfPresent(List<string> target, bool condition, string message)
        {
            if (condition)
            {
                target.Add(message);
            }
        }
    }
}
