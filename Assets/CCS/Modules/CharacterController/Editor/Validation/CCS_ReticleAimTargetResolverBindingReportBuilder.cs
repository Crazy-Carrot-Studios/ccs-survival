using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleAimTargetResolverBindingReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.12a reticle aim target resolver binding audit and report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleAimTargetResolverBindingReportBuilder
    {
        public static string WriteAuditReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.ReticleAimTargetResolverBindingAuditReportPath);
            WriteReportContent(reportPath, isAudit: true);
            return reportPath;
        }

        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.ReticleAimTargetResolverBindingReportPath);
            WriteReportContent(reportPath, isAudit: false);
            return reportPath;
        }

        private static void WriteReportContent(string reportPath, bool isAudit)
        {
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            CCS_RevolverAimTargetResolver resolver = prefab != null
                ? prefab.GetComponentInChildren<CCS_RevolverAimTargetResolver>(true)
                : null;
            CCS_MuzzleDrivenReticleController reticleController = prefab != null
                ? prefab.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true)
                : null;

            SerializedProperty sourceProperty = reticleController != null
                ? new SerializedObject(reticleController).FindProperty("aimTargetSourceComponent")
                : null;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(isAudit
                ? "# CCS Reticle Aim Target Resolver Binding Audit (v0.7.12a)"
                : "# CCS Reticle Aim Target Resolver Binding (v0.7.12a)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Reticle controller");
            builder.AppendLine("- Script: Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs");
            builder.AppendLine("- Prefab object: WeaponHudRoot");
            builder.AppendLine();
            builder.AppendLine("## Resolver");
            builder.AppendLine("- Script: Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverAimTargetResolver.cs");
            builder.AppendLine("- Prefab object: Model/Aiming");
            builder.AppendLine();
            builder.AppendLine("## Current target path (pre-v0.7.12a legacy)");
            builder.AppendLine("- ResolveStableCameraTargetScreen performed its own viewport-center raycast.");
            builder.AppendLine("- Switched between hit point and no-hit fallback distance.");
            builder.AppendLine("- Horizon pitch could flip hit/no-hit and snap reticle screen position.");
            builder.AppendLine();
            builder.AppendLine("## v0.7.12a binding path");
            builder.AppendLine("- Primary: CCS_IRevolverAimTargetSource.AimWorldPoint from Model/Aiming resolver.");
            builder.AppendLine("- Project world target to screen with active aim camera.");
            builder.AppendLine("- Preserve v0.7.10e SmoothDamp + maxScreenSnapPixelsPerFrame clamp.");
            builder.AppendLine("- No independent reticle raycast when aim target source is present.");
            builder.AppendLine("- Legacy raycast path remains only when source is missing.");
            builder.AppendLine();
            builder.AppendLine("## Binding status");
            if (sourceProperty != null)
            {
                Object boundSource = sourceProperty.objectReferenceValue;
                builder.AppendLine("- aimTargetSourceComponent assigned: " + (boundSource != null));
                builder.AppendLine("- bound component: " + (boundSource != null ? boundSource.name : "none"));
                builder.AppendLine("- bound to resolver: " + (boundSource == resolver));
            }

            if (isAudit)
            {
                builder.AppendLine();
                builder.AppendLine("## Horizon snap cause");
                builder.AppendLine("- Reticle and resolver used separate hit/no-hit decisions near horizon.");
                builder.AppendLine("- Reticle pitch dead-zone held screen target while raycast path still flipped.");
                builder.AppendLine("- Shared resolver smoothing/clamp now drives reticle world target first.");
            }

            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Reticle reveal remains Fulldraw_Idle Animation Event driven.");
            builder.AppendLine("- No body/arm IK implementation.");
            builder.AppendLine("- No muzzle LOS implementation.");
            builder.AppendLine("- No gameplay fire/damage changes.");
            builder.AppendLine("- No Animator/clip changes.");

            if (prefab != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
            }

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
