using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StrippedBaselineRemovalAuditBuilder
// CATEGORY: Modules / CharacterController / Editor / StripBaseline
// PURPOSE: Scans project for v0.7.13 strip keywords and writes removal audit report.
// PLACEMENT: Editor strip utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public enum StripBaselineRemovalClassification
    {
        Keep,
        RemoveFromPrefab,
        RemoveFromScene,
        Delete,
        Prototyping,
        DocOnly
    }

    public sealed class StripBaselineRemovalAuditEntry
    {
        public string Keyword;
        public StripBaselineRemovalClassification Classification;
        public string FilePath;
        public int LineNumber;
        public string LinePreview;
    }

    public static class CCS_StrippedBaselineRemovalAuditBuilder
    {
        private static readonly string[] ScanRoots =
        {
            "Assets/CCS",
            "README.md",
        };

        private static readonly string[] ScanExtensions =
        {
            ".cs",
            ".unity",
            ".prefab",
            ".asset",
            ".controller",
            ".mask",
            ".anim",
            ".md",
            ".asmdef",
        };

        private static readonly (string Keyword, StripBaselineRemovalClassification Classification)[] KeywordRules =
        {
            ("CCS_RevolverController", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_PlayerWeaponLoadout", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_WeaponCarryStateController", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_PlayerEquipmentVisualController", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_NetworkInteractionScanner", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_SingleRevolverAimAnimator", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_RevolverReticleAnimationEventReceiver", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_RevolverArmReticleIK", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_RevolverBodyAimFollowController", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_PlayerInteractionAnimator", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_MuzzleDrivenReticleController", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_RevolverAimTargetResolver", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("WeaponReticle", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("Rig_RevolverArmReticleIK", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("RightHandReticleIKTarget", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("ReticleAimWorldTarget", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_WeaponIKTargets", StripBaselineRemovalClassification.RemoveFromPrefab),
            ("CCS_OnRevolverAimHoldStarted", StripBaselineRemovalClassification.Delete),
            ("PF_CCS_TestInteractable_PickupItem", StripBaselineRemovalClassification.RemoveFromScene),
            ("CCS_TestPickupItemSpawner", StripBaselineRemovalClassification.RemoveFromScene),
            ("PF_CCS_TestWeaponDamageTarget", StripBaselineRemovalClassification.RemoveFromScene),
            ("CCS_AIBanditSpawner", StripBaselineRemovalClassification.RemoveFromScene),
            ("CCS_RevolverAimPresentationGate", StripBaselineRemovalClassification.Keep),
            ("CCS_RevolverAimLayerAnimator", StripBaselineRemovalClassification.Keep),
            ("CCS_PlayerHolsteredRevolverVisualPresenter", StripBaselineRemovalClassification.Keep),
            ("CCS_DiagnosticsEnemyBanditController", StripBaselineRemovalClassification.Keep),
            ("CCS_DualRevolverAimConvergenceRigPresenter", StripBaselineRemovalClassification.Keep),
            ("CCS_DualRevolverAimConvergenceProfile", StripBaselineRemovalClassification.Keep),
            ("CCS_DualRevolverArmAimBiasPresenter", StripBaselineRemovalClassification.Keep),
            ("CCS_DualRevolverArmAimBiasProfile", StripBaselineRemovalClassification.Keep),
            ("CCS_RevolverM1879_LeftHandEquipped_Fit", StripBaselineRemovalClassification.Keep),
            ("CCS_DiagnosticsEnemyBanditRegistry", StripBaselineRemovalClassification.Keep),
            ("SingleRevolverUpperBody", StripBaselineRemovalClassification.Keep),
            ("CCS_EquipmentSocketRegistry", StripBaselineRemovalClassification.Keep),
            ("CCS_RevolverHudPresenter", StripBaselineRemovalClassification.Prototyping),
            ("CCS_MouseDriven_RevolverAim", StripBaselineRemovalClassification.DocOnly),
            ("v0.7.13", StripBaselineRemovalClassification.DocOnly),
            ("StrippedCharacterControllerBaseline", StripBaselineRemovalClassification.Keep),
            ("StripBaseline", StripBaselineRemovalClassification.Keep),
        };

        public static string WriteRemovalAuditReport()
        {
            List<StripBaselineRemovalAuditEntry> hits = ScanProject();
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.StripBaselineRemovalAuditReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Strip Baseline Removal Audit (v0.7.13)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("Total hits: " + hits.Count);
            builder.AppendLine();
            builder.AppendLine("| Classification | Keyword | File | Line | Preview |");
            builder.AppendLine("| -------------- | ------- | ---- | ---- | ------- |");

            for (int i = 0; i < hits.Count; i++)
            {
                StripBaselineRemovalAuditEntry hit = hits[i];
                builder.AppendLine(
                    "| "
                    + hit.Classification
                    + " | "
                    + hit.Keyword
                    + " | `"
                    + hit.FilePath
                    + "` | "
                    + hit.LineNumber
                    + " | "
                    + EscapeTableCell(hit.LinePreview)
                    + " |");
            }

            builder.AppendLine();
            builder.AppendLine("## Classification legend");
            builder.AppendLine("- **Keep** — required for stripped baseline runtime/editor infrastructure.");
            builder.AppendLine("- **RemoveFromPrefab** — remove from PF_CCS_CharacterController_Player_Networked or child hierarchy.");
            builder.AppendLine("- **RemoveFromScene** — remove from SCN_CCS_CharacterController_Validation only.");
            builder.AppendLine("- **Delete** — delete asset or source when safe.");
            builder.AppendLine("- **Prototyping** — keep under Prototyping paths only.");
            builder.AppendLine("- **DocOnly** — documentation references; no runtime removal.");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[Strip Baseline Removal Audit] Wrote report to " + reportPath);
            return reportPath;
        }

        public static List<StripBaselineRemovalAuditEntry> ScanProject()
        {
            List<StripBaselineRemovalAuditEntry> hits = new List<StripBaselineRemovalAuditEntry>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            for (int rootIndex = 0; rootIndex < ScanRoots.Length; rootIndex++)
            {
                string scanRoot = ScanRoots[rootIndex];
                if (!File.Exists(scanRoot) && !Directory.Exists(scanRoot))
                {
                    continue;
                }

                IEnumerable<string> files = File.Exists(scanRoot)
                    ? new[] { scanRoot }
                    : Directory.GetFiles(scanRoot, "*.*", SearchOption.AllDirectories)
                        .Where(path => ScanExtensions.Any(path.EndsWith));

                foreach (string filePath in files)
                {
                    string normalizedPath = filePath.Replace('\\', '/');
                    if (ShouldSkipScanPath(normalizedPath))
                    {
                        continue;
                    }

                    string[] lines;
                    try
                    {
                        lines = File.ReadAllLines(normalizedPath);
                    }
                    catch (IOException)
                    {
                        continue;
                    }

                    for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                    {
                        string line = lines[lineIndex];
                        for (int ruleIndex = 0; ruleIndex < KeywordRules.Length; ruleIndex++)
                        {
                            string keyword = KeywordRules[ruleIndex].Keyword;
                            if (line.IndexOf(keyword, StringComparison.Ordinal) < 0)
                            {
                                continue;
                            }

                            string dedupeKey = keyword + "|" + normalizedPath + "|" + lineIndex;
                            if (!seen.Add(dedupeKey))
                            {
                                continue;
                            }

                            hits.Add(new StripBaselineRemovalAuditEntry
                            {
                                Keyword = keyword,
                                Classification = KeywordRules[ruleIndex].Classification,
                                FilePath = normalizedPath,
                                LineNumber = lineIndex + 1,
                                LinePreview = TrimPreview(line),
                            });
                        }
                    }
                }
            }

            return hits;
        }

        private static bool ShouldSkipScanPath(string normalizedPath)
        {
            return normalizedPath.Contains("/Library/")
                || normalizedPath.Contains("/Temp/")
                || normalizedPath.Contains("/obj/")
                || normalizedPath.Contains("/Logs/")
                || normalizedPath.EndsWith(".meta");
        }

        private static string TrimPreview(string line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length <= 120)
            {
                return trimmed;
            }

            return trimmed.Substring(0, 117) + "...";
        }

        private static string EscapeTableCell(string value)
        {
            return value.Replace("|", "\\|");
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
