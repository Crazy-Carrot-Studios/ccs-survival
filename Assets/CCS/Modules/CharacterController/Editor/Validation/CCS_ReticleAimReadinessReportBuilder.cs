using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleAimReadinessReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.10d reticle aim presentation readiness gate report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleAimReadinessReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.ReticleAimReadinessGateReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Reticle Aim Readiness Gate (v0.7.10d)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Issue observed");
            builder.AppendLine("- Reticle was visible too early at Play start and during draw animation.");
            builder.AppendLine("- Reticle should appear only after upper-body aim presentation reaches Revolver_Aim_Hold / Fulldraw_Idle.");
            builder.AppendLine();
            builder.AppendLine("## Readiness contract");
            builder.AppendLine("- Path: Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_IRevolverAimPresentationReadinessSource.cs");
            builder.AppendLine("- IsAimPresentationActive: RMB aim or Force Revolver Aim Setup Pose drives upper-body aim layer.");
            builder.AppendLine("- IsAimPresentationReadyForReticle: true only in Revolver_Aim_Hold / Fulldraw_Idle.");
            builder.AppendLine();
            builder.AppendLine("## Aim animator");
            builder.AppendLine("- Script: Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs");
            builder.AppendLine("- Exposes IsAimPresentationActive and IsAimPresentationReadyForReticle.");
            builder.AppendLine("- Uses cached SingleRevolverUpperBody state hashes; no per-frame string lookups.");
            builder.AppendLine();
            builder.AppendLine("## Reticle controller gating");
            builder.AppendLine("- Script: Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs");
            builder.AppendLine("- Reticle hidden by default at startup.");
            builder.AppendLine("- Visible only when local owner, not hand-socket preview, aim/setup intent active, and readiness is true.");
            builder.AppendLine("- Draw/holster states keep reticle hidden.");
            builder.AppendLine("- Force Revolver Hand Socket Preview never shows reticle.");
            builder.AppendLine("- Force Revolver Aim Setup Pose shows reticle only after hold-state readiness.");
            builder.AppendLine();
            builder.AppendLine("## Fit profile unchanged");
            builder.AppendLine("- Path: " + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            if (profile != null)
            {
                builder.AppendLine("- Local position: " + FormatVector3(profile.SocketLocalPosition));
                builder.AppendLine("- Local Euler: " + FormatVector3(profile.SocketLocalEulerAngles));
                builder.AppendLine("- Local scale: " + FormatVector3(profile.SocketLocalScale));
            }
            builder.AppendLine("- Offset parent: CCS_HandSocket_Right/CCS_RightHandRevolverAttachmentOffset");
            builder.AppendLine("- Fit values remain ScriptableObject-driven; not hardcoded in runtime scripts.");
            builder.AppendLine();
            builder.AppendLine("## Barrel line-of-sight plan");
            builder.AppendLine("- Path: Assets/CCS/Modules/CharacterController/Documentation/CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md");
            builder.AppendLine("- Planning only; not implemented in v0.7.10d.");
            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Gameplay ownership, ammo, damage, fire, pickup remain unchanged.");
            builder.AppendLine("- Equipment Fit Studio retained; Animation Fit Studio absent.");
            builder.AppendLine("- No new Animator layers/states or animation clip edits.");

            if (prefab != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
            }

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static string FormatVector3(Vector3 value)
        {
            return "(" + value.x.ToString("0.######") + ", " + value.y.ToString("0.######") + ", " + value.z.ToString("0.######") + ")";
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
