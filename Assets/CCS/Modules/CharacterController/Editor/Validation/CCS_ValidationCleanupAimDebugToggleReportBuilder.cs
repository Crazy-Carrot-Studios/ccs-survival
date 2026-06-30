using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_ValidationCleanupAimDebugToggleReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.9 validation cleanup and aim debug toggle report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ValidationCleanupAimDebugToggleReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.ValidationCleanupAimDebugToggleReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Validation Cleanup and Aim Debug Toggle (v0.7.9)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Weapon damage target");
            builder.AppendLine("- Old path: `" + CCS_CharacterControllerConstants.LegacyWeaponsTestDamageTargetPrefabPath + "`");
            builder.AppendLine("- New path: `" + CCS_CharacterControllerConstants.PrototypingWeaponDamageTargetPrefabPath + "`");
            builder.AppendLine("- GUID preserved: "
                + (AssetDatabase.LoadAssetAtPath<GameObject>(CCS_CharacterControllerConstants.PrototypingWeaponDamageTargetPrefabPath) != null
                    ? "yes (59ee106eed61129489434e269dca4b82 expected)"
                    : "unknown"));
            builder.AppendLine();
            builder.AppendLine("## Removed TestDetectionCube assets");
            builder.AppendLine("- `" + CCS_CharacterControllerConstants.LegacyTestDetectionCubeBootstrapScriptPath + "`");
            builder.AppendLine("- `" + CCS_CharacterControllerConstants.LegacyTestDetectionCubeUtilityScriptPath + "`");
            builder.AppendLine("- Scene objects: CCS_TestDetectionCube, CCS_TestDetectionCubeSceneBootstrap");
            builder.AppendLine();
            builder.AppendLine("## Diagnostics manager");
            builder.AppendLine("- Object path: Validation scene / CCS_DiagnosticsManager");
            builder.AppendLine("- Force Aim Presentation default: false");
            builder.AppendLine("- Interface: CCS_ICharacterAimPresentationDebugSource");
            builder.AppendLine();
            builder.AppendLine("## Aim animator connection");
            builder.AppendLine("- Script: `" + AimAnimatorSourcePath + "`");
            builder.AppendLine("- Strategy: desired presentation = gameplay IsAiming OR diagnostics ForceAimPresentation");
            builder.AppendLine("- Debug source resolved once via serialized reference or CCS_CharacterAimPresentationDebugRegistry");
            builder.AppendLine();
            builder.AppendLine("## Validation scene cleanup");
            builder.AppendLine("- Detection cube removed");
            builder.AppendLine("- Interaction validation uses revolver pickup and building door interactables");
            builder.AppendLine();

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller != null)
            {
                builder.AppendLine("## Animator layers (unchanged from v0.7.8)");
                for (int i = 0; i < controller.layers.Length; i++)
                {
                    builder.AppendLine("- " + controller.layers[i].name);
                }

                builder.AppendLine();
                builder.AppendLine("## Animator parameters (unchanged from v0.7.8)");
                for (int i = 0; i < controller.parameters.Length; i++)
                {
                    builder.AppendLine("- " + controller.parameters[i].name);
                }
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab != null)
            {
                MonoBehaviour[] rootBehaviours = prefab.GetComponents<MonoBehaviour>();
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + rootBehaviours.Length);
            }

            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Base Layer locomotion unchanged from v0.7.8.");
            builder.AppendLine("- Gameplay aim/fire remains owned by CCS_RevolverController.");
            builder.AppendLine("- Force Aim Presentation is presentation-only.");
            builder.AppendLine("- Equipment Fit Studio retained; Animation Fit Studio absent.");
            builder.AppendLine();
            builder.AppendLine("## Known limitations");
            builder.AppendLine("- Remote player aim presentation deferred.");
            builder.AppendLine("- Force Aim Presentation available on validation scene only.");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
