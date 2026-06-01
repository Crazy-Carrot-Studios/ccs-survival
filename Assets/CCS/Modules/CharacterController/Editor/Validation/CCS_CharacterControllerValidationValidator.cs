using System.IO;
using CCS.Modules.CharacterController;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationValidator
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates module folders, asmdefs, profile asset, and tuning rules.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require Rigidbody on character prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public sealed class CCS_CharacterControllerValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/CharacterController";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/CharacterController/CCS_DefaultCharacterControllerProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_CharacterController_Module.md";
        private const string InputActionsPath = SurvivalRoot + "/Input/CCS_Survival_InputActions.inputactions";
        private const string PlayerPrefabPath = SurvivalRoot + "/Prefabs/Player/PF_CCS_Player.prefab";
        private const string PlayerControllerScriptPath = SurvivalRoot + "/Runtime/Player/CCS_PlayerGameplayController.cs";
        private const string InputProviderScriptPath = RuntimeRoot + "/Input/CCS_CharacterInputActionProvider.cs";
        private const string MovementBridgeScriptPath = RuntimeRoot + "/Services/CCS_CharacterMovementRuntimeBridge.cs";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.charactercontroller";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/CharacterController", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Input", RuntimeRoot + "/Input");
            ValidateRequiredFolder(report, "Runtime/Movement", RuntimeRoot + "/Movement");
            ValidateRequiredFolder(report, "Runtime/Camera", RuntimeRoot + "/Camera");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.CharacterController.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.CharacterController.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_CharacterMovementService", RuntimeRoot + "/Movement/CCS_CharacterMovementService.cs");
            ValidateRequiredScript(report, "CCS_CharacterControllerMotor", RuntimeRoot + "/Movement/CCS_CharacterControllerMotor.cs");
            ValidateRequiredScript(report, "CCS_CharacterControllerProfile", RuntimeRoot + "/Profiles/CCS_CharacterControllerProfile.cs");
            ValidateRequiredScript(report, "CCS_ICharacterInputProvider", RuntimeRoot + "/Input/CCS_ICharacterInputProvider.cs");
            ValidateRequiredScript(report, "CCS_CharacterInputActionProvider", InputProviderScriptPath);
            ValidateRequiredScript(report, "CCS_CharacterMovementRuntimeBridge", MovementBridgeScriptPath);
            ValidateRequiredScript(report, "CCS_PlayerGameplayController", PlayerControllerScriptPath);

            ValidateRequiredFile(report, "Survival Input Actions", InputActionsPath);
            ValidateConsumeInputBinding(report);
            ValidatePlayerPrefabRules(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateRuntimeScriptsAvoidUnityEditor(report, SurvivalRoot + "/Runtime/Player");
            ValidateBootstrapSceneGameplayCamera(report);
            CCS_BootstrapSceneValidationUtility.ValidatePlayableGround(report);

            ValidateDocumentationAsset(report, "Character Controller Module Doc", ModuleDocPath);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Character Controller Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_CharacterControllerProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_CharacterControllerProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_CharacterControllerValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Character Controller Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Character Controller Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Character controller validator completed (CharacterController-based movement; no Rigidbody required).");
        }

        #endregion

        #region Private Methods

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string context,
            string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"File present: {filePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Documentation present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Warning,
                context,
                $"Documentation missing: {assetPath}");
        }

        private static void ValidateConsumeInputBinding(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(InputActionsPath))
            {
                return;
            }

            string inputActionsSource = File.ReadAllText(InputActionsPath);
            report.AddIssue(
                inputActionsSource.Contains("\"Consume\"")
                    && inputActionsSource.Contains("<Keyboard>/f")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Consume Input Binding",
                "Gameplay Consume action is bound to keyboard F (temporary developer binding).");

            if (File.Exists(InputProviderScriptPath))
            {
                string providerSource = File.ReadAllText(InputProviderScriptPath);
                report.AddIssue(
                    providerSource.Contains("ConsumePressedThisFrame")
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Consume Input Provider",
                    "CCS_CharacterInputActionProvider exposes ConsumePressedThisFrame.");
            }
        }

        private static void ValidatePlayerPrefabRules(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Prefab",
                    $"Missing required prefab: {PlayerPrefabPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Player Prefab",
                $"Asset present: {PlayerPrefabPath}");

            string prefabText = File.ReadAllText(PlayerPrefabPath);
            if (prefabText.Contains("CCS.Survival.Player.Runtime::CCS.Survival.Player.CCS_PlayerGameplayController"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Prefab",
                    "Player prefab includes CCS_PlayerGameplayController.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Prefab",
                    "Player prefab is missing CCS_PlayerGameplayController.");
            }

            if (prefabText.Contains("CCS.Modules.CharacterController.Runtime::CCS.Modules.CharacterController.CCS_CharacterInputActionProvider"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Prefab",
                    "Player prefab includes CCS_CharacterInputActionProvider.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Prefab",
                    "Player prefab is missing CCS_CharacterInputActionProvider.");
            }

            ValidateThirdPersonCameraPrefabRules(report, prefabText);
            ValidatePlayerGameplayRaySources(report, prefabText);
        }

        private static void ValidatePlayerGameplayRaySources(
            CCS_SurvivalValidationReport report,
            string prefabText)
        {
            if (prefabText.Contains("interactionCamera: {fileID: 0}"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Gameplay Rays",
                    "Interaction camera is not assigned on PF_CCS_Player.");
            }
            else if (prefabText.Contains("interactionCamera:"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Gameplay Rays",
                    "Interaction camera is assigned on PF_CCS_Player.");
            }

            if (prefabText.Contains("combatCamera: {fileID: 0}"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Gameplay Rays",
                    "Combat camera is not assigned on PF_CCS_Player.");
            }
            else if (prefabText.Contains("combatCamera:"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Gameplay Rays",
                    "Combat camera is assigned on PF_CCS_Player.");
            }

            if (prefabText.Contains("placementCamera: {fileID: 0}"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Gameplay Rays",
                    "Building placement camera is not assigned on PF_CCS_Player.");
            }
            else if (prefabText.Contains("placementCamera:"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Gameplay Rays",
                    "Building placement camera is assigned on PF_CCS_Player.");
            }

            if (prefabText.Contains("activeCameraMode: 0"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Camera Mode",
                    "Default profile uses ThirdPersonSurvival camera mode.");
            }
        }

        private static void ValidateThirdPersonCameraPrefabRules(
            CCS_SurvivalValidationReport report,
            string prefabText)
        {
            if (prefabText.Contains("m_Name: CameraLookTarget"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Camera Targets",
                    "Player prefab includes CameraLookTarget for Cinemachine follow.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Camera Targets",
                    "Player prefab is missing CameraLookTarget. Run CCS_PlayerThirdPersonCameraBootstrapSetup.ExecuteBatch.");
            }

            if (prefabText.Contains("CM_GameplayCamera")
                && prefabText.Contains("Unity.Cinemachine.CinemachineCamera"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Cinemachine Camera",
                    "Player prefab includes CM_GameplayCamera with CinemachineCamera.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Cinemachine Camera",
                    "Player prefab is missing CM_GameplayCamera Cinemachine setup.");
            }

            if (prefabText.Contains("Unity.Cinemachine.CinemachineThirdPersonFollow"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Cinemachine Camera",
                    "Player prefab includes CinemachineThirdPersonFollow.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Cinemachine Camera",
                    "Player prefab is missing CinemachineThirdPersonFollow.");
            }

            if (prefabText.Contains("Unity.Cinemachine.CinemachineBrain"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Cinemachine Camera",
                    "Main Camera includes CinemachineBrain.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Cinemachine Camera",
                    "Main Camera is missing CinemachineBrain.");
            }

            if (prefabText.Contains("CCS.Survival.Player.Runtime::CCS.Survival.Player.CCS_PlayerCinemachineCameraDriver"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Cinemachine Camera",
                    "Player prefab includes CCS_PlayerCinemachineCameraDriver.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Cinemachine Camera",
                    "Player prefab is missing CCS_PlayerCinemachineCameraDriver.");
            }
        }

        private static void ValidateBootstrapSceneGameplayCamera(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Gameplay Camera",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (sceneText.Contains("PF_CCS_Player") && sceneText.Contains("m_TagString: MainCamera"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Gameplay Camera",
                    "Bootstrap scene includes player camera setup.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Camera",
                    "Bootstrap scene is missing player MainCamera setup.");
            }
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeFolderPath)
        {
            if (!Directory.Exists(runtimeFolderPath))
            {
                return;
            }

            bool foundEditorReference = false;
            string[] scriptPaths = Directory.GetFiles(runtimeFolderPath, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < scriptPaths.Length; index++)
            {
                if (ScriptContainsUnityEditorReference(File.ReadAllText(scriptPaths[index])))
                {
                    foundEditorReference = true;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Script Purity",
                        $"Runtime script references UnityEditor: {scriptPaths[index]}");
                }
            }

            if (!foundEditorReference)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Runtime Script Purity",
                    $"Runtime scripts under {runtimeFolderPath} avoid UnityEditor.");
            }
        }

        private static bool ScriptContainsUnityEditorReference(string contents)
        {
            string[] lines = contents.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string trimmedLine = lines[lineIndex].Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                int commentIndex = trimmedLine.IndexOf("//", System.StringComparison.Ordinal);
                if (commentIndex >= 0)
                {
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
                }

                if (trimmedLine.StartsWith("using UnityEditor", System.StringComparison.Ordinal) ||
                    trimmedLine.Contains("UnityEditor.", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
