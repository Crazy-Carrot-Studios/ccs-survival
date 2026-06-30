using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimLayerAssetAuditReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.8 Wild West revolver aim asset audit report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimLayerAssetAuditReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.SingleRevolverAimLayerAssetAuditReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Aim Layer Asset Audit (v0.7.8)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Avatar mask");
            builder.AppendLine("- Path: `" + CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath + "`");
            builder.AppendLine("- Exists: " + File.Exists(CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath));
            builder.AppendLine();
            builder.AppendLine("## Wild West clips");
            AppendClipAudit(builder, CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath, CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipName);
            AppendClipAudit(builder, CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath, CCS_CharacterControllerConstants.WildWestFulldrawIdleClipName);
            AppendClipAudit(builder, CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath, CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipName);
            builder.AppendLine("## Notes");
            builder.AppendLine("- Clips are read-only FBX sub-assets under YashMakesGames import.");
            builder.AppendLine("- Humanoid retargeting enabled on import; binding warnings may remain on Kevin until rebuild milestone.");
            builder.AppendLine("- EnemyAI does not consume these clips in v0.7.8.");

            File.WriteAllText(reportPath, builder.ToString());
            Debug.Log("[Revolver Aim Asset Audit] Wrote report to " + reportPath);
            return reportPath;
        }

        private static void AppendClipAudit(StringBuilder builder, string assetPath, string clipName)
        {
            builder.AppendLine("### " + clipName);
            builder.AppendLine("- Path: `" + assetPath + "`");
            builder.AppendLine("- Exists: " + File.Exists(assetPath));

            AnimationClip clip = LoadClip(assetPath, clipName);
            if (clip == null)
            {
                builder.AppendLine("- Loaded clip: false");
                builder.AppendLine();
                return;
            }

            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            builder.AppendLine("- Loaded clip: true");
            builder.AppendLine("- Clip length: " + clip.length.ToString("0.###") + "s");
            builder.AppendLine("- Loop time: " + (clip.isLooping ? "yes" : "no"));
            builder.AppendLine("- Import animation type: " + (importer != null ? importer.animationType.ToString() : "unknown"));
            builder.AppendLine("- Humanoid-compatible import: " + (importer != null && importer.animationType == ModelImporterAnimationType.Human));
            builder.AppendLine("- Read-only imported asset: yes");
            builder.AppendLine();
        }

        private static AnimationClip LoadClip(string assetPath, string clipName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && clip.name == clipName)
                {
                    return clip;
                }
            }

            return null;
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
