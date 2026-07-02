using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimTargetResolverReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.12 revolver aim target resolver report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimTargetResolverReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.RevolverAimTargetResolverReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_RevolverAimTargetProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverAimTargetProfile>(
                CCS_CharacterControllerConstants.RevolverAimTargetProfilePath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            CCS_RevolverAimTargetResolver resolver = prefab != null
                ? prefab.GetComponentInChildren<CCS_RevolverAimTargetResolver>(true)
                : null;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Aim Target Resolver (v0.7.12)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Profile");
            builder.AppendLine("- Path: " + CCS_CharacterControllerConstants.RevolverAimTargetProfilePath);
            if (profile != null)
            {
                builder.AppendLine("- cameraRayDistance: " + profile.CameraRayDistance.ToString("0.###"));
                builder.AppendLine("- fallbackDistance: " + profile.FallbackDistance.ToString("0.###"));
                builder.AppendLine("- targetSmoothingTime: " + profile.TargetSmoothingTime.ToString("0.###"));
                builder.AppendLine("- maxTargetSnapDistance: " + profile.MaxTargetSnapDistance.ToString("0.###"));
                builder.AppendLine("- lastValidTargetHoldSeconds: " + profile.LastValidTargetHoldSeconds.ToString("0.###"));
                builder.AppendLine("- minimumValidDistance: " + profile.MinimumValidDistance.ToString("0.###"));
                builder.AppendLine("- nearCameraRejectDistance: " + profile.NearCameraRejectDistance.ToString("0.###"));
                builder.AppendLine("- holdLastValidTargetWhenInvalid: " + profile.HoldLastValidTargetWhenInvalid);
                builder.AppendLine("- smoothTarget: " + profile.SmoothTarget);
                builder.AppendLine("- drawDebugRayWhenDiagnosticsEnabled: " + profile.DrawDebugRayWhenDiagnosticsEnabled);
            }

            builder.AppendLine();
            builder.AppendLine("## Resolver");
            builder.AppendLine("- Script: Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverAimTargetResolver.cs");
            builder.AppendLine("- Prefab: " + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (resolver != null)
            {
                builder.AppendLine("- GameObject path: Model/" + resolver.gameObject.name);
            }

            builder.AppendLine();
            builder.AppendLine("## Camera resolution");
            builder.AppendLine("- Serialized aimCamera when available, else CCS_CharacterMovementCameraContext.ActiveCamera, else Camera.main.");
            builder.AppendLine();
            builder.AppendLine("## Local-owner gating");
            builder.AppendLine("- NetworkObject.IsOwner when spawned; offline/solo treats local player as owner.");
            builder.AppendLine();
            builder.AppendLine("## Target behavior");
            builder.AppendLine("- Hit: raycast aim point from viewport center.");
            builder.AppendLine("- No hit: camera forward * fallbackDistance.");
            builder.AppendLine("- Invalid projection: hold last valid target or fallback.");
            builder.AppendLine("- Smoothing: SmoothDamp + maxTargetSnapDistance clamp when enabled.");
            builder.AppendLine("- Obstruction: diagnostic ray on obstructionLayerMask only.");
            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- No body/arm IK implementation.");
            builder.AppendLine("- No muzzle LOS implementation.");
            builder.AppendLine("- No reticle convergence implementation.");
            builder.AppendLine("- No gameplay fire/damage changes.");
            builder.AppendLine("- No Animator/clip changes.");

            if (prefab != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
            }

            builder.AppendLine();
            builder.AppendLine("## Known limitations");
            builder.AppendLine("- Resolver is diagnostics/future-system only.");
            builder.AppendLine("- Remote players do not replicate aim target yet.");
            builder.AppendLine("- Reticle and IK still use pre-v0.7.12 behavior.");
            builder.AppendLine();
            builder.AppendLine("## Next milestone");
            builder.AppendLine("- v0.7.13 Body Aim Presenter prototype.");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
