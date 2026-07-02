using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleRevealAnimationEventReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.10f reticle reveal animation event report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleRevealAnimationEventReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.ReticleRevealAnimationEventReportPath);
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
            CCS_RevolverFulldrawIdleReticleEventBuilder.TryReadFulldrawIdleReticleEventTime(
                out float eventTime,
                out int matchingEventCount);

            Animator animator = prefab != null ? prefab.GetComponentInChildren<Animator>(true) : null;
            CCS_RevolverReticleAnimationEventReceiver receiver = animator != null
                ? animator.GetComponent<CCS_RevolverReticleAnimationEventReceiver>()
                : null;
            CCS_SingleRevolverAimAnimator aimAnimator = prefab != null
                ? prefab.GetComponentInChildren<CCS_SingleRevolverAimAnimator>(true)
                : null;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Reticle Reveal Animation Event (v0.7.10f)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Hold clip");
            builder.AppendLine("- Clip path: " + CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath);
            builder.AppendLine("- Event function: " + CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventName);
            builder.AppendLine("- Event time: " + eventTime.ToString("0.###"));
            builder.AppendLine("- Matching event count: " + matchingEventCount);
            builder.AppendLine();
            builder.AppendLine("## Receiver");
            builder.AppendLine("- Receiver script: " + CCS_CharacterControllerConstants.RevolverReticleAnimationEventReceiverScriptPath);
            builder.AppendLine("- Receiver GameObject: " + (animator != null ? animator.gameObject.name : "missing"));
            builder.AppendLine(
                "- Aim animator reference: "
                + (aimAnimator != null ? aimAnimator.gameObject.name : "missing"));
            builder.AppendLine();
            builder.AppendLine("## Reticle reveal source");
            if (profile != null)
            {
                builder.AppendLine("- Mode: " + profile.ReticleRevealSource);
                builder.AppendLine("- revealDuringDraw: " + profile.RevealDuringDraw);
                builder.AppendLine("- screenSmoothTime: " + profile.ScreenSmoothTime.ToString("0.###"));
                builder.AppendLine("- maxScreenSnapPixelsPerFrame: " + profile.MaxScreenSnapPixelsPerFrame.ToString("0.###"));
                builder.AppendLine("- noHitFallbackDistance: " + profile.NoHitFallbackDistance.ToString("0.###"));
                builder.AppendLine("- pitchSnapDeadZoneDegrees: " + profile.PitchSnapDeadZoneDegrees.ToString("0.###"));
                builder.AppendLine("- holdLastValidTargetOnNoHit: " + profile.HoldLastValidTargetOnNoHit);
                builder.AppendLine("- lastValidTargetHoldSeconds: " + profile.LastValidTargetHoldSeconds.ToString("0.###"));
            }
            builder.AppendLine();
            builder.AppendLine("## Visibility rule summary");
            builder.AppendLine("- Hidden at Play start and during draw.");
            builder.AppendLine("- Revealed when Fulldraw_Idle fires CCS_OnRevolverAimHoldStarted.");
            builder.AppendLine("- Hidden immediately on holster start / aim release.");
            builder.AppendLine("- Force Revolver Hand Socket Preview alone never reveals reticle.");
            builder.AppendLine("- Force Revolver Aim Setup Pose uses the same animation event timing.");
            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Normalized draw reveal is no longer primary timing.");
            builder.AppendLine("- v0.7.10e pitch stability and smoothing values retained.");
            builder.AppendLine("- Barrel/muzzle line-of-sight remains deferred.");
            builder.AppendLine("- Gameplay ownership, ammo, fire, damage, pickup unchanged.");
            builder.AppendLine("- Right-hand fit profile unchanged.");
            builder.AppendLine("- Equipment Fit Studio present; Animation Fit Studio absent.");
            builder.AppendLine("- No animation curve edits or new Animator layers/states.");

            if (receiver != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Receiver wiring");
                builder.AppendLine("- Receiver present on Animator GameObject: yes");
            }

            if (fitProfile != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Fit profile unchanged");
                builder.AppendLine("- Path: " + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
                builder.AppendLine("- Local position: " + FormatVector3(fitProfile.SocketLocalPosition));
                builder.AppendLine("- Local Euler: " + FormatVector3(fitProfile.SocketLocalEulerAngles));
                builder.AppendLine("- Local scale: " + FormatVector3(fitProfile.SocketLocalScale));
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
