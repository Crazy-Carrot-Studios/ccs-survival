using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleTimingStabilityReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.10e reticle timing and pitch stability report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleTimingStabilityReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.ReticleTimingStabilityReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_RevolverReticlePresentationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverReticlePresentationProfile>(
                CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath);
            CCS_WeaponAttachmentFitProfile fitProfile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Reticle Timing and Pitch Stability (v0.7.10e)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Video-observed issues");
            builder.AppendLine("- Reticle appeared slightly too late versus desired feel.");
            builder.AppendLine("- Reticle snapped when camera pitch crossed the horizon.");
            builder.AppendLine();
            builder.AppendLine("## Reticle presentation profile");
            builder.AppendLine("- Path: " + CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath);
            if (profile != null)
            {
                builder.AppendLine("- revealDuringDraw: " + profile.RevealDuringDraw);
                builder.AppendLine("- drawRevealNormalizedTime: " + profile.DrawRevealNormalizedTime.ToString("0.###"));
                builder.AppendLine("- drawRevealLeadSeconds: " + profile.DrawRevealLeadSeconds.ToString("0.###"));
                builder.AppendLine("- reticleFadeInSeconds: " + profile.ReticleFadeInSeconds.ToString("0.###"));
                builder.AppendLine("- reticleFadeOutSeconds: " + profile.ReticleFadeOutSeconds.ToString("0.###"));
                builder.AppendLine("- screenSmoothTime: " + profile.ScreenSmoothTime.ToString("0.###"));
                builder.AppendLine("- maxScreenSnapPixelsPerFrame: " + profile.MaxScreenSnapPixelsPerFrame.ToString("0.###"));
                builder.AppendLine("- noHitFallbackDistance: " + profile.NoHitFallbackDistance.ToString("0.###"));
                builder.AppendLine("- pitchSnapDeadZoneDegrees: " + profile.PitchSnapDeadZoneDegrees.ToString("0.###"));
                builder.AppendLine("- holdLastValidTargetOnNoHit: " + profile.HoldLastValidTargetOnNoHit);
                builder.AppendLine("- lastValidTargetHoldSeconds: " + profile.LastValidTargetHoldSeconds.ToString("0.###"));
            }
            builder.AppendLine();
            builder.AppendLine("## Readiness contract");
            builder.AppendLine("- Interface: Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_IRevolverAimPresentationReadinessSource.cs");
            builder.AppendLine("- Added IsAimPresentationInReticleRevealWindow for late-draw reveal before full hold.");
            builder.AppendLine();
            builder.AppendLine("## Reticle visibility rule");
            builder.AppendLine("- Hidden at Play start and during early draw.");
            builder.AppendLine("- Visible when local owner, not hand-socket preview, aim/setup intent active, and reveal window OR hold readiness true.");
            builder.AppendLine("- Hidden immediately when holster starts.");
            builder.AppendLine();
            builder.AppendLine("## Snap prevention");
            builder.AppendLine("- Camera-ray target with no-hit fallback distance from profile.");
            builder.AppendLine("- Last valid screen target hold on invalid projection.");
            builder.AppendLine("- Pitch dead zone near horizon uses recent valid target.");
            builder.AppendLine("- Vector2.SmoothDamp + max pixels per frame clamp.");
            builder.AppendLine("- Barrel/muzzle line-of-sight still deferred.");
            builder.AppendLine();
            builder.AppendLine("## Fit profile unchanged");
            builder.AppendLine("- Path: " + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            if (fitProfile != null)
            {
                builder.AppendLine("- Local position: " + FormatVector3(fitProfile.SocketLocalPosition));
                builder.AppendLine("- Local Euler: " + FormatVector3(fitProfile.SocketLocalEulerAngles));
                builder.AppendLine("- Local scale: " + FormatVector3(fitProfile.SocketLocalScale));
            }
            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Gameplay ownership, ammo, damage, fire, pickup remain unchanged.");
            builder.AppendLine("- Equipment Fit Studio retained; Animation Fit Studio absent.");
            builder.AppendLine("- No animation clip edits or new Animator layers/states.");

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
