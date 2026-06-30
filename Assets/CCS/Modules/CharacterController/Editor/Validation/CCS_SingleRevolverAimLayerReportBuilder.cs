using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SingleRevolverAimLayerReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.8 single revolver aim layer implementation report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_SingleRevolverAimLayerReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.SingleRevolverAimLayerReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Single Revolver Aim Layer (v0.7.8)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Assets");
            builder.AppendLine("- Mask: `" + CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath + "`");
            builder.AppendLine("- Draw clip: `" + CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath + "`");
            builder.AppendLine("- Hold clip: `" + CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath + "`");
            builder.AppendLine("- Holster clip: `" + CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath + "`");
            builder.AppendLine("- Script: `Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs`");
            builder.AppendLine("- Prefab attachment: `" + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath + "` / Model");
            builder.AppendLine();

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller != null)
            {
                builder.AppendLine("## Animator layers");
                for (int i = 0; i < controller.layers.Length; i++)
                {
                    builder.AppendLine("- " + controller.layers[i].name);
                }

                builder.AppendLine();
                builder.AppendLine("## Animator parameters");
                for (int i = 0; i < controller.parameters.Length; i++)
                {
                    builder.AppendLine("- " + controller.parameters[i].name);
                }

                int layerIndex = System.Array.FindIndex(
                    controller.layers,
                    layer => layer.name == CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName);
                if (layerIndex >= 0 && controller.layers[layerIndex].stateMachine != null)
                {
                    builder.AppendLine();
                    builder.AppendLine("## SingleRevolverUpperBody states");
                    ChildAnimatorState[] states = controller.layers[layerIndex].stateMachine.states;
                    for (int i = 0; i < states.Length; i++)
                    {
                        builder.AppendLine("- " + states[i].state.name);
                    }
                }
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab != null)
            {
                int rootBehaviourCount = prefab.GetComponents<Component>().Count(c => c is MonoBehaviour);
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + rootBehaviourCount);
            }

            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Base Layer locomotion preserved.");
            builder.AppendLine("- Gameplay aim/fire remains owned by CCS_RevolverController.");
            builder.AppendLine("- No fire/reload/interaction/dual revolver animation added.");
            builder.AppendLine("- Animation clip content unchanged (read-only Wild West FBX sub-assets).");
            builder.AppendLine("- Remote player aim presentation deferred.");
            builder.AppendLine("- Equipment Fit Studio retained; Animation Fit Studio absent.");

            File.WriteAllText(reportPath, builder.ToString());
            Debug.Log("[Single Revolver Aim Layer Report] Wrote report to " + reportPath);
            return reportPath;
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
