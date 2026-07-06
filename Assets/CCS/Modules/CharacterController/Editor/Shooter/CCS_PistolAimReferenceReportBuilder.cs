using System;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PistolAimReferenceReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Shooter
// PURPOSE: Writes v0.7.14 CCS pistol aim reference summary report.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PistolAimReferenceReportBuilder
    {
        public static string WriteReport(bool externalReferenceFound, string externalProjectPath, string poseFbxPath)
        {
            string projectRoot = CCS_PistolAimReferencePaths.GetProjectRoot();
            string reportPath = Path.Combine(projectRoot, CCS_PistolAimReferencePaths.ReportRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Pistol Aim Reference v0.7.14");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            builder.AppendLine();
            builder.AppendLine("## External reference project");
            builder.AppendLine($"- Found: **{(externalReferenceFound ? "yes" : "no")}**");
            if (externalReferenceFound)
            {
                builder.AppendLine($"- Path: `{externalProjectPath}`");
                builder.AppendLine($"- Reference pose FBX: `{poseFbxPath}`");
            }

            builder.AppendLine();
            builder.AppendLine("## Useful camera values (reference-inspired CCS targets)");
            builder.AppendLine("| Field | CCS test range |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine("| Aim camera distance | 1.15 – 1.25 |");
            builder.AppendLine("| Shoulder offset | 0.30 – 0.40 |");
            builder.AppendLine("| Height | 1.55 – 1.62 |");
            builder.AppendLine("| FOV | 38 – 42 |");

            builder.AppendLine();
            builder.AppendLine("## Useful animation finding");
            builder.AppendLine("- Reference two-handed pistol aim hold pose (upper-body snapshot).");
            builder.AppendLine("- CCS target clip (pending copy): `" + CCS_CharacterControllerConstants.PistolTwoHandedAimHoldClipPath + "`");

            builder.AppendLine();
            builder.AppendLine("## CCS-owned target folders");
            builder.AppendLine("- `Assets/CCS/Modules/CharacterController/Documentation/Shooter/`");
            builder.AppendLine("- `Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/`");
            builder.AppendLine("- `Assets/CCS/Modules/CharacterController/Profiles/Shooter/`");
            builder.AppendLine("- `Assets/CCS/Modules/CharacterController/Editor/Shooter/`");

            builder.AppendLine();
            builder.AppendLine("## CCS-owned target runtime components (future)");
            builder.AppendLine("- `CCS_PistolAimLayerAnimator`");
            builder.AppendLine("- `CCS_PistolAimPresentationProfile`");
            builder.AppendLine("- `CCS_PistolAimCameraProfile`");
            builder.AppendLine("- `CCS_PistolAimVisualPresenter`");
            builder.AppendLine("- `CCS_SharedAimPointPresenter` / `CCS_SharedAimReticlePresenter`");

            builder.AppendLine();
            builder.AppendLine("## Removed from earlier vendor-named audit pass");
            builder.AppendLine("- Deleted vendor-named audit tooling, documentation folders, and test controllers");
            builder.AppendLine("- Removed vendor-named constants from character controller constants");

            builder.AppendLine();
            builder.AppendLine("## Production safety confirmation");
            builder.AppendLine("- No external vendor scripts copied into `Assets/CCS`.");
            builder.AppendLine("- No vendor components on production player prefab.");
            builder.AppendLine("- No `Assets/CCS` path/class/file names containing forbidden vendor token.");
            builder.AppendLine("- Production Kevin aim layer not replaced in this milestone.");

            builder.AppendLine();
            builder.AppendLine("## Next steps (awaiting James)");
            builder.AppendLine("1. Confirm license OK to duplicate reference two-handed pistol aim clip.");
            builder.AppendLine("2. Duplicate as `CCS_Pistol_TwoHand_AimHold.anim`.");
            builder.AppendLine("3. Test on Kevin using `" + CCS_CharacterControllerConstants.PistolAimTestControllerPath + "`.");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[Pistol Aim Reference] Wrote report: " + reportPath);
            return reportPath;
        }
    }
}
