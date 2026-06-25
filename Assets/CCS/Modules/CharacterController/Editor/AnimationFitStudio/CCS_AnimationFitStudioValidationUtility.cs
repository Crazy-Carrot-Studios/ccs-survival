using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioValidationUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Validates Animation Fit Studio test tool foundation and safety rules.
// PLACEMENT: Editor validator invoked from master test and weapons validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Fails if failed FP arm presentation controller remains active on test player.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAnimationFitStudioFoundation()
        {
            List<string> failures = new List<string>();
            ValidateAnimationFitStudioCoreInternal(failures);
            GameObject testPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendResult(
                failures,
                CCS_EquipmentFitStudioValidationUtility.ValidateEquipmentFitStudioFoundation());
            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateFirstPersonRevolverArmPresentationRemoved(testPlayerPrefab));
            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRuntimeRevolverVisualBehaviorFoundation(testPlayerPrefab));

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Animation Fit Studio foundation validated. Runtime revolver visual behavior restored.");
        }

        public static CCS_SurvivalValidationResult ValidateAnimationFitStudioCore()
        {
            List<string> failures = new List<string>();
            ValidateAnimationFitStudioCoreInternal(failures);
            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Animation Fit Studio core foundation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateAnimationFitStudioHumanoidControlCalibration()
        {
            List<string> failures = new List<string>();
            ValidateAnimationFitStudioHumanoidControlCalibration(failures);
            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Humanoid control calibration validated. Upper arm and wrist buttons modify correct muscles.");
        }

        private static void ValidateAnimationFitStudioCoreInternal(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath),
                "Missing Animation Fit Studio window at "
                + CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath);
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath),
                "Missing Animation Fit Studio editor folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath),
                "Missing Animation Fit Studio layout partial at "
                + CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioPreviewUtilitySourcePath),
                "Missing Animation Fit Studio preview utility.");

            ValidateAnimationFitStudioVisualPreviewFoundation(failures);
            ValidateAnimationFitStudioPoseSamplingFoundation(failures);

            ValidateAnimationFitStudioFingerFoundation(failures);
            ValidateAnimationFitStudioPoseSourceFoundation(failures);
            ValidateAnimationFitStudioHumanoidControlCalibration(failures);

            if (File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath))
            {
                string windowSource = File.ReadAllText(
                    CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath);
                AppendIfMissing(
                    failures,
                    windowSource.Contains(CCS_CharacterControllerConstants.AnimationFitStudioMenuPath),
                    "Animation Fit Studio menu path must be registered on the window.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("CCS_AnimationFitStudioBodyPartCatalog")
                        || windowSource.Contains("CCS_AnimationFitStudioEditPartCatalog"),
                    "Animation Fit Studio must use the limited body-part catalog.");
                AppendIfMissing(
                    failures,
                    !windowSource.Contains("AnimationInventory"),
                    "Animation Fit Studio must not list the full animation inventory.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("ResolveSourceClipForSelectedPoseSource")
                        && windowSource.Contains("OnEnable"),
                    "Animation Fit Studio must auto-resolve the selected pose source clip on window open.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("TryResolveClipForPoseSource")
                        || windowSource.Contains("TryResolveDefaultSourceAimClip"),
                    "Animation Fit Studio must resolve pose source clips via the catalog resolver.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("selectedPoseSource")
                        && windowSource.Contains("OnPoseSourceChanged"),
                    "Animation Fit Studio must expose Pose Source switching.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("LoadPreviewOrWeapon")
                        && windowSource.Contains("TryAutoLoadPreviewOnOpen"),
                    "Animation Fit Studio must load preview and weapon on window open.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("ResetPose"),
                    "Animation Fit Studio must expose Reset Pose.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("selectedEditPartIndex")
                        || windowSource.Contains("EditPartCatalog"),
                    "Animation Fit Studio must expose Edit Part dropdown.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("NudgeSelectedFingerCurl")
                        && windowSource.Contains("NudgeSelectedFingerSpread"),
                    "Animation Fit Studio must expose detailed finger segment controls.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("CCS_AnimationFitStudioPoseEditData")
                        || windowSource.Contains("PoseEdits"),
                    "Animation Fit Studio must store detailed pose edit state.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("MaintainPreviewPose")
                        && windowSource.Contains("GetFingerCurlDirection"),
                    "Animation Fit Studio must maintain preview pose with all user edits.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind"),
                    "Animation Fit Studio default pose source must be Runtime Aim Idle FullDraw.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("RefreshRuntimeControllerClipInfo"),
                    "Animation Fit Studio must refresh runtime controller clip status.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("SaveRuntimeFitTest"),
                    "Animation Fit Studio must expose Save Runtime FullDraw workflow.");
            }

            string aimScoreUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioAimPoseScoreUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(aimScoreUtilityPath),
                "Missing Animation Fit Studio aim pose score utility.");
            string playableSamplerPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPlayablePreviewSampler.cs";
            AppendIfMissing(
                failures,
                File.Exists(playableSamplerPath),
                "Missing Animation Fit Studio PlayableGraph preview sampler.");
            string auditionUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioClipAuditionUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(auditionUtilityPath),
                "Missing Animation Fit Studio clip audition utility.");

            string poseTargetCatalogPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPoseTargetCatalog.cs";
            AppendIfMissing(
                failures,
                File.Exists(poseTargetCatalogPath),
                "Missing Animation Fit Studio pose target catalog.");
            if (File.Exists(poseTargetCatalogPath))
            {
                string catalogSource = File.ReadAllText(poseTargetCatalogPath);
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("AllowedPoseTargetCount")
                        && catalogSource.Contains("Final Aim — FullDraw")
                        && catalogSource.Contains("Aimed Walk — RH"),
                    "Animation Fit Studio pose target catalog must list only Final Aim FullDraw and Aimed Walk RH.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("DefaultBestAimFrameNormalizedTime = 0.65f"),
                    "Animation Fit Studio Best Aim Frame must default to 65% clip length before audition.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("CCS_WW_Revolver_AimIdle_FullDraw")
                        && catalogSource.Contains("AnimationFitStudioDefaultSourceClipPath"),
                    "Animation Fit Studio pose target catalog must reference controller FullDraw clip.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("BestAimFrame"),
                    "Animation Fit Studio must expose Best Aim Frame preset.");
            }

            string clipResolverPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioClipResolver.cs";
            if (File.Exists(clipResolverPath))
            {
                string resolverSource = File.ReadAllText(clipResolverPath);
                AppendIfMissing(
                    failures,
                    resolverSource.Contains(CCS_AnimationFitStudioConstants.DefaultSourceClipName)
                        || resolverSource.Contains("CCS_AnimationFitStudioConstants.DefaultSourceClipName")
                        || resolverSource.Contains("AnimationFitStudioDefaultSourceClipPath"),
                    "Animation Fit Studio clip resolver must target "
                    + CCS_AnimationFitStudioConstants.DefaultSourceClipName
                    + ".");
                AppendIfMissing(
                    failures,
                    resolverSource.Contains("TryResolveClipForPoseTarget")
                        || resolverSource.Contains("CCS_WW_Revolver_AimIdle_FullDraw")
                        || resolverSource.Contains("AnimationFitStudioDefaultSourceClipPath"),
                    "Animation Fit Studio clip resolver must resolve pose targets including FullDraw.");
            }

            if (CCS_AnimationFitStudioClipResolver.TryResolveDefaultRuntimeFitTestClip(
                    out AnimationClip runtimeClip,
                    out string runtimePath,
                    out string runtimeClipError))
            {
                AppendIfMissing(
                    failures,
                    runtimeClip != null && runtimePath.EndsWith(".anim"),
                    "Animation Fit Studio runtime FullDraw clip must be a CCS-owned .anim asset.");
                AppendIfMissing(
                    failures,
                    runtimePath == CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                    "Animation Fit Studio runtime default must map to RevolverAimIdleFullDrawClipPath.");
            }
            else if (!string.IsNullOrEmpty(runtimeClipError))
            {
                failures.Add(runtimeClipError);
            }

            if (CCS_AnimationFitStudioClipResolver.TryResolveClipForPoseSource(
                    CCS_AnimationFitStudioBasePoseSourceKind.EquipmentFitStudioRevolverAim,
                    out AnimationClip sourceClip,
                    out string sourcePath,
                    out string clipError))
            {
                AppendIfMissing(
                    failures,
                    sourceClip != null && sourcePath.EndsWith(".anim"),
                    "Animation Fit Studio Equipment Fit Studio source aim clip must be a CCS-owned .anim asset.");
                AppendIfMissing(
                    failures,
                    !sourcePath.Replace('\\', '/').Contains("/YashMakesGames/"),
                    "Animation Fit Studio must not require raw YashMakesGames vendor clips.");
            }
            else if (!string.IsNullOrEmpty(clipError))
            {
                failures.Add(clipError);
            }

            string runtimeControllerUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioRuntimeControllerClipUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(runtimeControllerUtilityPath),
                "Missing Animation Fit Studio runtime controller clip utility.");
            if (File.Exists(runtimeControllerUtilityPath))
            {
                string runtimeUtilitySource = File.ReadAllText(runtimeControllerUtilityPath);
                AppendIfMissing(
                    failures,
                    runtimeUtilitySource.Contains("TryQueryRuntimeAimClips"),
                    "Animation Fit Studio must query runtime controller aim clips.");
                AppendIfMissing(
                    failures,
                    runtimeUtilitySource.Contains("AnimatorRevolverAimIdleFullDrawStateName"),
                    "Animation Fit Studio must inspect Revolver_AimIdle_FullDraw controller state.");
            }

            string saveUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioSaveUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(saveUtilityPath),
                "Missing Animation Fit Studio save utility.");
            if (File.Exists(saveUtilityPath))
            {
                string saveUtilitySource = File.ReadAllText(saveUtilityPath);
                AppendIfMissing(
                    failures,
                    saveUtilitySource.Contains("TryLoadExistingControllerClip"),
                    "Animation Fit Studio save utility must load the existing controller FullDraw clip.");
                AppendIfMissing(
                    failures,
                    saveUtilitySource.Contains("LogOverwriteResult"),
                    "Animation Fit Studio save utility must log overwrite results.");
                AppendIfMissing(
                    failures,
                    saveUtilitySource.Contains("CurveHashChanged"),
                    "Animation Fit Studio save utility must log curve hash changed proof.");
                AppendIfMissing(
                    failures,
                    File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                        + "/CCS_AnimationFitStudioCurveHashUtility.cs"),
                    "Missing Animation Fit Studio curve hash utility.");
                AppendIfMissing(
                    failures,
                    saveUtilitySource.Contains("HumanoidMuscleCurvesWritten"),
                    "Animation Fit Studio save utility must report Humanoid muscle curves written.");
            }

            AnimationClip fullDrawClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
            AppendIfMissing(
                failures,
                fullDrawClip != null,
                "Missing controller FullDraw clip for Humanoid curve mode validation.");
            if (fullDrawClip != null)
            {
                AppendIfMissing(
                    failures,
                    CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(fullDrawClip)
                        == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves,
                    "FullDraw clip must be detected as Humanoid Muscle Curves mode.");
            }

            if (CCS_AnimationFitStudioRuntimeControllerClipUtility.TryQueryRuntimeAimClips(
                    CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                    out CCS_AnimationFitStudioRuntimeControllerClipInfo controllerInfo))
            {
                AppendIfMissing(
                    failures,
                    controllerInfo.ControllerFound,
                    "Animation Fit Studio must locate player Animator Controller for runtime clip validation.");
                AppendIfMissing(
                    failures,
                    controllerInfo.FullDrawStateFound,
                    "Animator Controller must define Revolver_AimIdle_FullDraw on RevolverUpperBody layer.");
            }

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath),
                "Missing controller FullDraw clip at "
                + CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath
                + ". Open Animation Fit Studio and save Runtime FullDraw first.");

            string controllerClipFolder = Path.GetDirectoryName(
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath)?.Replace('\\', '/');
            AppendIfMissing(
                failures,
                !string.IsNullOrEmpty(controllerClipFolder)
                    && controllerClipFolder == CCS_AnimationFitStudioClipResolver.GetFitTestOutputFolderPath(),
                "Controller FullDraw clip must live under Combat/Aiming/Revolver/.");

            string runtimePolicyPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioRuntimePolicy.cs";
            if (File.Exists(runtimePolicyPath))
            {
                string policySource = File.ReadAllText(runtimePolicyPath);
                AppendIfMissing(
                    failures,
                    policySource.Contains("ControllerFullDrawTargetStatusLabel"),
                    "Animation Fit Studio runtime policy must name the controller FullDraw target.");
                AppendIfMissing(
                    failures,
                    policySource.Contains("SaveDoesNotWireControllerNotice"),
                    "Animation Fit Studio runtime policy must state Save does not wire Animator Controller.");
                AppendIfMissing(
                    failures,
                    policySource.Contains("Runtime Aim Idle — FullDraw"),
                    "Animation Fit Studio runtime policy must name Runtime Aim Idle — FullDraw target.");
            }

            if (File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath))
            {
                string layoutSource = File.ReadAllText(
                    CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath);
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Clip Used By Controller"),
                    "Animation Fit Studio must show Clip Used By Controller status.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Save Runtime FullDraw"),
                    "Animation Fit Studio main footer must expose Save Runtime FullDraw.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("OverwriteSaveModeLabel")
                        || layoutSource.Contains("Overwrite Controller Clip"),
                    "Animation Fit Studio must expose Overwrite Controller Clip save mode.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("RuntimeFullDrawEditNotice"),
                    "Animation Fit Studio must explain runtime FullDraw in-place overwrite.");
                AppendIfMissing(
                    failures,
                    !GetBottomActionBarSection(layoutSource).Contains("Save FitTest Pose"),
                    "Animation Fit Studio main footer must not use legacy Save FitTest Pose label.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Save Target:")
                        || layoutSource.Contains("ControllerFullDrawTargetStatusLabel"),
                    "Animation Fit Studio must show controller FullDraw save target.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Controller Wiring:")
                        || layoutSource.Contains("SaveDoesNotWireControllerNotice"),
                    "Animation Fit Studio must clarify Save Runtime FullDraw does not wire Animator Controller.");
                AppendIfMissing(
                    failures,
                    !layoutSource.Contains("Load Source Aim Clip"),
                    "Animation Fit Studio main workflow must not require Load Source Aim Clip.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Pose Source")
                        && layoutSource.Contains("PoseSourceLabels"),
                    "Animation Fit Studio header must expose the Pose Source dropdown.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("1. Pose Target")
                        || layoutSource.Contains("Runtime Aim Idle — FullDraw"),
                    "Animation Fit Studio guide must document Runtime FullDraw pose target.");
                AppendIfMissing(
                    failures,
                    !layoutSource.Contains("Equipment Fit Studio Revolver Aim pose source"),
                    "Animation Fit Studio guide must not require Equipment Fit Studio as primary pose source.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Load Preview / Weapon"),
                    "Animation Fit Studio main footer must expose Load Preview / Weapon.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Reset Pose"),
                    "Animation Fit Studio main footer must expose Reset Pose.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Save Runtime FullDraw"),
                    "Animation Fit Studio main footer must expose Save Runtime FullDraw.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Validate"),
                    "Animation Fit Studio main footer must expose Validate.");
                AppendIfMissing(
                    failures,
                    !layoutSource.Contains("Audition Aim Clips")
                        || layoutSource.Contains("Advanced / Diagnostics"),
                    "Animation Fit Studio audition must be in Advanced / Diagnostics, not main footer.");
                AppendIfMissing(
                    failures,
                    !GetBottomActionBarSection(layoutSource).Contains("Audition Aim Clips"),
                    "Animation Fit Studio main footer must not expose Audition Aim Clips.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Advanced / Diagnostics"),
                    "Animation Fit Studio must expose Advanced / Diagnostics foldout.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Finger Bones"),
                    "Animation Fit Studio UI must show finger bone discovery status.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Edit Part")
                        && layoutSource.Contains("EditPartDisplayLabels"),
                    "Animation Fit Studio UI must expose Edit Part dropdown.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Finger Segment")
                        && layoutSource.Contains("FingerSegmentLabels"),
                    "Animation Fit Studio UI must expose Finger Segment dropdown.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("DrawAxisCalibrationFoldout")
                        && layoutSource.Contains("DrawFingerAxisControls"),
                    "Animation Fit Studio must keep finger axis controls under Advanced / Diagnostics / Axis Calibration.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Quick Grip"),
                    "Animation Fit Studio UI must keep quick grip controls secondary.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Curl +")
                        && layoutSource.Contains("Spread +"),
                    "Animation Fit Studio UI must expose detailed finger curl and spread controls.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("try")
                        && layoutSource.Contains("finally")
                        && layoutSource.Contains("DrawBottomActionBar"),
                    "Animation Fit Studio layout must guard IMGUI groups with try/finally.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("QueueGuiAction"),
                    "Animation Fit Studio must defer GUI mutations to avoid IMGUI layout errors.");
            }

            string poseUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPoseUtility.cs";
            if (File.Exists(poseUtilityPath))
            {
                string poseUtilitySource = File.ReadAllText(poseUtilityPath);
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("SavesAnimatorController => false"),
                    "Animation Fit Studio save must not wire Animator Controller.");
                AppendIfMissing(
                    failures,
                    !poseUtilitySource.Contains("UnityEditor.Animations.AnimatorController"),
                    "Animation Fit Studio pose save must not modify Animator Controller assets.");
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("WriteFingerTransformCurves"),
                    "Animation Fit Studio save must write finger transform curves.");
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("CCS_AnimationFitStudioFingerManipulationUtility"),
                    "Animation Fit Studio must apply finger edits via finger manipulation utility.");
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("TrySaveRuntimeFitTestPose"),
                    "Animation Fit Studio save must support runtime FitTest in-place overwrite.");
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("TryWriteHumanoidMuscleCurvesIntoClip"),
                    "Animation Fit Studio must save Humanoid muscle curves for FullDraw clips.");
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode"),
                    "Animation Fit Studio save must detect Humanoid vs Transform clip curve mode.");
                AppendIfMissing(
                    failures,
                    poseUtilitySource.Contains("CCS_RevolverAimPreviewPoseUtility.ApplyRevolverAimPose"),
                    "Animation Fit Studio must apply Equipment Fit Studio Revolver Aim pose.");
            }

            string saveUtilityPathForValidation = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioSaveUtility.cs";
            if (File.Exists(saveUtilityPathForValidation))
            {
                string saveUtilitySourceForValidation = File.ReadAllText(saveUtilityPathForValidation);
                AppendIfMissing(
                    failures,
                    saveUtilitySourceForValidation.Contains("GUID preserved"),
                    "Animation Fit Studio save must log GUID preserved status.");
            }

            if (File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath))
            {
                string catalogPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                    + "/CCS_AnimationFitStudioBodyPartCatalog.cs";
                if (File.Exists(catalogPath))
                {
                    string catalogSource = File.ReadAllText(catalogPath);
                    AppendIfMissing(
                        failures,
                        catalogSource.Contains(
                            "AllowedBodyPartCount = "
                            + CCS_AnimationFitStudioBodyPartCatalog.AllowedBodyPartCount),
                        "Animation Fit Studio body-part catalog count must match allowed list.");
                }
            }

            GameObject testPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (testPlayerPrefab != null)
            {
                MonoBehaviour[] behaviours = testPlayerPrefab.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour != null
                        && behaviour.GetType().Name == "CCS_FirstPersonRevolverArmPresentationController")
                    {
                        failures.Add(
                            "Test player must not contain active CCS_FirstPersonRevolverArmPresentationController.");
                        break;
                    }
                }
            }

            AppendIfMissing(
                failures,
                CCS_AnimationFitStudioBodyPartCatalog.AllDefinitions.Count
                    == CCS_AnimationFitStudioBodyPartCatalog.AllowedBodyPartCount,
                "Animation Fit Studio must expose only the allowed limited body-part list.");
        }

        private static void ValidateAnimationFitStudioVisualPreviewFoundation(List<string> failures)
        {
            string layoutPath = CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath;
            if (File.Exists(layoutPath))
            {
                string layoutSource = File.ReadAllText(layoutPath);
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("CCS_EquipmentFitStudioStyles"),
                    "Animation Fit Studio must reuse Equipment Fit Studio editor styles.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("DrawCenterPreviewViewport"),
                    "Animation Fit Studio must provide a large center preview viewport.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("DrawBottomActionBar"),
                    "Animation Fit Studio must provide a bottom action bar.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("MissingWeaponVisualWarning"),
                    "Animation Fit Studio must warn when grip editing lacks revolver preview.");
            }

            string previewPath = CCS_CharacterControllerConstants.AnimationFitStudioPreviewUtilitySourcePath;
            if (File.Exists(previewPath))
            {
                string previewSource = File.ReadAllText(previewPath);
                AppendIfMissing(
                    failures,
                    previewSource.Contains("WeaponAttachmentRootObjectName"),
                    "Animation Fit Studio preview must use a dedicated weapon attachment root.");
                AppendIfMissing(
                    failures,
                    previewSource.Contains("PreviewWeaponObjectName"),
                    "Animation Fit Studio preview must use a dedicated preview weapon object.");
                AppendIfMissing(
                    failures,
                    previewSource.Contains("LoadRevolverAttachmentFitProfile")
                        || previewSource.Contains("EquippedFitProfile"),
                    "Animation Fit Studio preview must resolve equipped fit profile path.");
                AppendIfMissing(
                    failures,
                    previewSource.Contains("ApplyProfileToAttachmentRoot"),
                    "Animation Fit Studio preview must apply equipped fit profile to attachment root.");
                AppendIfMissing(
                    failures,
                    previewSource.Contains("PreviewWeaponObjectName")
                        && previewSource.Contains("localPosition = Vector3.zero"),
                    "Animation Fit Studio preview weapon visual child must remain zeroed.");
            }

            string cleanupPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioCleanupUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(cleanupPath),
                "Missing Animation Fit Studio cleanup utility.");
            if (File.Exists(cleanupPath))
            {
                string cleanupSource = File.ReadAllText(cleanupPath);
                AppendIfMissing(
                    failures,
                    cleanupSource.Contains("PreviewPlayerObjectName"),
                    "Animation Fit Studio cleanup must remove preview player objects.");
                AppendIfMissing(
                    failures,
                    cleanupSource.Contains("PreviewCameraObjectName"),
                    "Animation Fit Studio cleanup must remove preview camera objects.");
            }
        }

        private static void ValidateAnimationFitStudioPoseSamplingFoundation(List<string> failures)
        {
            string poseUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPoseUtility.cs";
            if (File.Exists(poseUtilityPath))
            {
                string poseSource = File.ReadAllText(poseUtilityPath);
                AppendIfMissing(
                    failures,
                    poseSource.Contains("TryApplyPreviewPose"),
                    "Animation Fit Studio must apply preview poses explicitly.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("CCS_AnimationFitStudioPlayablePreviewSampler")
                        || poseSource.Contains("PlayableGraph"),
                    "Animation Fit Studio must sample clips via PlayableGraph preview path.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("AppliedAimPose")
                        && poseSource.Contains("AppliedIdleLike"),
                    "Animation Fit Studio must not report Applied — Aim Pose for idle-like samples.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("CanEditPoseParts")
                        || poseSource.Contains("SaveBlockedInvalidAimWarning"),
                    "Animation Fit Studio must block save/edit when preview is idle-like.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("SampleClipOnAnimator"),
                    "Animation Fit Studio must sample clips on the Animator GameObject.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("CanSaveFitTestPose"),
                    "Animation Fit Studio must block FitTest save when preview pose is invalid.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("TryApplySeedPose")
                        || poseSource.Contains("ApplySeedPoseInternal"),
                    "Animation Fit Studio must provide a seed pose fallback when clip sampling fails.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("ClipSampledNoBoneChangeWarning"),
                    "Animation Fit Studio must warn when clip sampling does not move preview bones.");
            }

            string windowPath = CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath;
            if (File.Exists(windowPath))
            {
                string windowSource = File.ReadAllText(windowPath);
                AppendIfMissing(
                    failures,
                    windowSource.Contains("ApplyPreviewPose"),
                    "Animation Fit Studio window must auto-apply preview pose after load.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("CanSaveFitTestPose")
                        && windowSource.Contains("SaveRuntimeFitTest"),
                    "Animation Fit Studio must disable Save Runtime FullDraw when preview is invalid.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("OnEditorUpdate"),
                    "Animation Fit Studio must maintain preview pose during editor updates.");
            }

            string layoutPath = CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath;
            if (File.Exists(layoutPath))
            {
                string layoutSource = File.ReadAllText(layoutPath);
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Pose Preview"),
                    "Animation Fit Studio must show Pose Preview status chip.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Clip Diagnostics")
                        && layoutSource.Contains("Advanced / Diagnostics"),
                    "Animation Fit Studio clip diagnostics must live in Advanced / Diagnostics.");
            }

            string previewPath = CCS_CharacterControllerConstants.AnimationFitStudioPreviewUtilitySourcePath;
            if (File.Exists(previewPath))
            {
                string previewSource = File.ReadAllText(previewPath);
                AppendIfMissing(
                    failures,
                    previewSource.Contains("RefreshWeaponAttachmentAfterPoseSample"),
                    "Animation Fit Studio must refresh weapon attachment after pose sampling.");
            }

            string diagnosticsPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioClipDiagnostics.cs";
            AppendIfMissing(
                failures,
                File.Exists(diagnosticsPath),
                "Missing Animation Fit Studio clip diagnostics utility.");
        }

        #endregion

        #region Private Methods

        private static void ValidateAnimationFitStudioFingerFoundation(List<string> failures)
        {
            string fingerDiscoveryPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioFingerDiscoveryUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(fingerDiscoveryPath),
                "Missing Animation Fit Studio finger discovery utility.");
            if (File.Exists(fingerDiscoveryPath))
            {
                string discoverySource = File.ReadAllText(fingerDiscoveryPath);
                AppendIfMissing(
                    failures,
                    discoverySource.Contains("HumanBodyBones.RightThumbProximal")
                        && discoverySource.Contains("HumanBodyBones.RightLittleDistal"),
                    "Animation Fit Studio finger discovery must support humanoid finger bones.");
                AppendIfMissing(
                    failures,
                    discoverySource.Contains("MissingFingerBonesWarning"),
                    "Animation Fit Studio must warn when finger bones are missing.");
            }

            string fingerManipulationPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioFingerManipulationUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(fingerManipulationPath),
                "Missing Animation Fit Studio finger manipulation utility.");
            string poseEditDataPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPoseEditData.cs";
            AppendIfMissing(
                failures,
                File.Exists(poseEditDataPath),
                "Missing Animation Fit Studio pose edit data.");
            string editPartCatalogPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioEditPartCatalog.cs";
            AppendIfMissing(
                failures,
                File.Exists(editPartCatalogPath),
                "Missing Animation Fit Studio edit part catalog.");
            if (File.Exists(fingerManipulationPath))
            {
                string manipulationSource = File.ReadAllText(fingerManipulationPath);
                AppendIfMissing(
                    failures,
                    manipulationSource.Contains("ApplyFingerEdits")
                        && manipulationSource.Contains("LocalAxisUtility"),
                    "Animation Fit Studio finger manipulation must support axis-corrected finger edits.");
            }
            if (File.Exists(poseEditDataPath))
            {
                string poseEditSource = File.ReadAllText(poseEditDataPath);
                AppendIfMissing(
                    failures,
                    poseEditSource.Contains("CCS_AnimationFitStudioLocalAxisKind")
                        && poseEditSource.Contains("FingerSegmentEditState"),
                    "Animation Fit Studio pose edit data must define finger axis and segment edit types.");
            }
        }

        private static void ValidateAnimationFitStudioPoseSourceFoundation(List<string> failures)
        {
            string revolverAimUtilityPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/Common/CCS_RevolverAimPreviewPoseUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(revolverAimUtilityPath),
                "Missing shared Revolver Aim preview pose utility.");
            if (File.Exists(revolverAimUtilityPath))
            {
                string utilitySource = File.ReadAllText(revolverAimUtilityPath);
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("ApplyRevolverAimPose"),
                    "Revolver Aim preview pose utility must expose ApplyRevolverAimPose.");
            }

            string poseSourceCatalogPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPoseSourceCatalog.cs";
            AppendIfMissing(
                failures,
                File.Exists(poseSourceCatalogPath),
                "Missing Animation Fit Studio pose source catalog.");
            if (File.Exists(poseSourceCatalogPath))
            {
                string catalogSource = File.ReadAllText(poseSourceCatalogPath);
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("RuntimeAimIdleFullDraw")
                        && catalogSource.Contains("EquipmentFitStudioRevolverAim"),
                    "Animation Fit Studio pose source catalog must list Runtime FullDraw and EFS Revolver Aim.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("DefaultPoseSourceKind")
                        && catalogSource.Contains("RuntimeAimIdleFullDraw"),
                    "Animation Fit Studio default pose source must be Runtime Aim Idle FullDraw.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("Runtime Aim Idle — FullDraw"),
                    "Animation Fit Studio pose source catalog must include Runtime Aim Idle — FullDraw target.");
            }

            if (File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath))
            {
                string layoutSource = File.ReadAllText(
                    CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath);
                int loadPreviewCount = CountOccurrences(
                    GetBottomActionBarSection(layoutSource),
                    "Load Preview / Weapon");
                AppendIfMissing(
                    failures,
                    loadPreviewCount == 1,
                    "Animation Fit Studio main footer must expose only one Load Preview / Weapon button.");
            }
        }

        private static void ValidateAnimationFitStudioHumanoidControlCalibration(List<string> failures)
        {
            string humanoidControlUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioHumanoidControlUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(humanoidControlUtilityPath),
                "Missing Animation Fit Studio Humanoid control utility.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                    + "/CCS_AnimationFitStudioHumanoidControlState.cs"),
                "Missing Animation Fit Studio Humanoid control state.");

            if (File.Exists(humanoidControlUtilityPath))
            {
                string utilitySource = File.ReadAllText(humanoidControlUtilityPath);
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("NudgeDegreesToMuscleDelta"),
                    "Humanoid control utility must map nudge degrees to muscle deltas.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("ShoulderPartScale"),
                    "Humanoid control utility must scale shoulder nudges separately from upper arm.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("BuildReadoutText"),
                    "Humanoid control utility must build muscle readout text.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("ValidateReloadedClipMatchesState"),
                    "Humanoid control utility must validate reloaded clip values match UI state.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("ApplyTestAimOffset"),
                    "Humanoid control utility must expose Apply Test Aim Offset.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("Edit reached Humanoid muscle limit"),
                    "Humanoid control utility must report when edits hit muscle clamp limits.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("CanNudgePartAxis"),
                    "Humanoid control utility must expose axis availability checks.");
                AppendIfMissing(
                    failures,
                    utilitySource.Contains("\"spine\"")
                        && utilitySource.Contains("Spine Front-Back"),
                    "Humanoid control utility must map Spine Pitch/Yaw/Roll to torso muscles.");
            }

            string maskUtilityPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/Validation/CCS_RevolverUpperBodyRightArmAimMaskUtility.cs";
            AppendIfMissing(
                failures,
                File.Exists(maskUtilityPath),
                "Missing shared revolver upper-body/right-arm aim mask utility.");
            if (File.Exists(maskUtilityPath))
            {
                string maskUtilitySource = File.ReadAllText(maskUtilityPath);
                AppendIfMissing(
                    failures,
                    maskUtilitySource.Contains("AvatarMaskBodyPart.Body")
                        && maskUtilitySource.Contains("AvatarMaskBodyPart.RightArm"),
                    "Aim mask utility must include upper body and right arm.");
                AppendIfMissing(
                    failures,
                    maskUtilitySource.Contains("AvatarMaskBodyPart.LeftArm")
                        && maskUtilitySource.Contains("SetHumanoidBodyPartActive(inactiveParts[i], false)"),
                    "Aim mask utility must exclude left arm from aim mask.");
            }

            string editPartCatalogPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioEditPartCatalog.cs";
            if (File.Exists(editPartCatalogPath))
            {
                string catalogSource = File.ReadAllText(editPartCatalogPath);
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("Shoulder / Clavicle"),
                    "Edit Part catalog must label shoulder as Shoulder / Clavicle.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("Wrist / Hand"),
                    "Edit Part catalog must label wrist as Wrist / Hand.");
                AppendIfMissing(
                    failures,
                    catalogSource.Contains("\"spine\"")
                        && catalogSource.Contains("Upper Chest"),
                    "Edit Part catalog must include Spine and Upper Chest.");
            }

            if (File.Exists(CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath))
            {
                string layoutSource = File.ReadAllText(
                    CCS_CharacterControllerConstants.AnimationFitStudioWindowLayoutSourcePath);
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Humanoid Muscle Values"),
                    "Animation Fit Studio UI must expose Humanoid Muscle Values readout.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("DrawHumanoidMuscleReadoutPanel"),
                    "Animation Fit Studio layout must draw Humanoid muscle readout panel.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("DrawUnifiedPitchYawRollControls"),
                    "Animation Fit Studio must expose unified Pitch/Yaw/Roll controls for all edit parts.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Axis Calibration"),
                    "Animation Fit Studio must move axis calibration controls to Advanced / Diagnostics.");
                AppendIfMissing(
                    failures,
                    !layoutSource.Contains("DrawHumanoidInvertToggles"),
                    "Animation Fit Studio main panel must not expose Humanoid axis invert toggles.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Invert Shoulder Pitch")
                        && layoutSource.Contains("DrawHumanoidAxisCalibrationControls"),
                    "Humanoid axis invert controls must remain available under Advanced / Diagnostics / Axis Calibration.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("DrawPartAxisMappingHints"),
                    "Animation Fit Studio must report unmapped Humanoid axis controls.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Apply Test Aim Offset"),
                    "Animation Fit Studio Advanced / Diagnostics must expose Apply Test Aim Offset.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("Delta from loaded pose"),
                    "Humanoid muscle readout must show delta from loaded pose.");
                AppendIfMissing(
                    failures,
                    layoutSource.Contains("near Humanoid clamp limit"),
                    "Humanoid muscle readout must warn when muscles are near clamp limits.");
            }

            string windowSourcePath = CCS_CharacterControllerConstants.AnimationFitStudioWindowSourcePath;
            if (File.Exists(windowSourcePath))
            {
                string windowSource = File.ReadAllText(windowSourcePath);
                AppendIfMissing(
                    failures,
                    windowSource.Contains("ApplyTestAimOffset"),
                    "Animation Fit Studio window must wire Apply Test Aim Offset.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("TryHumanoidNudge"),
                    "Animation Fit Studio window must route part nudges through Humanoid muscle controls.");
                AppendIfMissing(
                    failures,
                    windowSource.Contains("IsPartAxisEnabled"),
                    "Animation Fit Studio window must disable unmapped Humanoid axis buttons.");
            }

            string poseUtilityPath = CCS_CharacterControllerConstants.AnimationFitStudioEditorFolderPath
                + "/CCS_AnimationFitStudioPoseUtility.cs";
            if (File.Exists(poseUtilityPath))
            {
                string poseSource = File.ReadAllText(poseUtilityPath);
                AppendIfMissing(
                    failures,
                    poseSource.Contains("HumanoidControl.CurrentValues")
                        || poseSource.Contains("WriteCurrentValuesToClip"),
                    "Animation Fit Studio save must write HumanoidControl current values to clip.");
                AppendIfMissing(
                    failures,
                    poseSource.Contains("ValidateReloadedClipMatchesState"),
                    "Animation Fit Studio save must confirm reloaded clip matches UI Humanoid values.");
            }

            AppendResult(
                failures,
                CCS_AnimationFitStudioHumanoidControlUtility.ValidateHumanoidControlCalibration());
        }

        private static string GetRightPosePanelSection(string layoutSource)
        {
            if (string.IsNullOrEmpty(layoutSource))
            {
                return string.Empty;
            }

            const string marker = "private void DrawRightPosePanel";
            int startIndex = layoutSource.IndexOf(marker, System.StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            int nextMethodIndex = layoutSource.IndexOf("private void DrawBottomActionBar", startIndex, System.StringComparison.Ordinal);
            return nextMethodIndex > startIndex
                ? layoutSource.Substring(startIndex, nextMethodIndex - startIndex)
                : layoutSource.Substring(startIndex);
        }

        private static string GetBottomActionBarSection(string layoutSource)
        {
            if (string.IsNullOrEmpty(layoutSource))
            {
                return string.Empty;
            }

            const string marker = "private void DrawBottomActionBar";
            int startIndex = layoutSource.IndexOf(marker, System.StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            int nextMethodIndex = layoutSource.IndexOf("private void", startIndex + marker.Length, System.StringComparison.Ordinal);
            return nextMethodIndex > startIndex
                ? layoutSource.Substring(startIndex, nextMethodIndex - startIndex)
                : layoutSource.Substring(startIndex);
        }

        private static int CountOccurrences(string source, string token)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(token))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(token, index, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                failures.Add(result.Message);
            }
        }

        #endregion
    }
}
