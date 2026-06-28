using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationInventoryReporter
// CATEGORY: Modules / CharacterController / Editor / AnimationInventory
// PURPOSE: Scans vendor animation packs and writes markdown/CSV inventory reports.
// PLACEMENT: Editor utility invoked by Master Test batch inventory pass. No user-facing menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-23
// NOTES: Source-only inventory pass. Writes generated reports under Logs/ (not committed).
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimationInventoryReporter
    {
        public const string WildWestPackRoot = "Assets/YashMakesGames/Wild West Animation Pack";

        public const string ReportDirectory = "Logs/CharacterController/AnimationInventory";

        public const string MarkdownReportPath = ReportDirectory + "/CCS_WildWestAnimationInventory.md";

        public const string CsvReportPath = ReportDirectory + "/CCS_WildWestAnimationInventory.csv";

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static readonly string[] MovementKeywords = { "walk", "run", "crouch", "dodge", "vault" };
        private static readonly string[] WeaponKeywords =
        {
            "revolver", "gun", "pistol", "aimed", "aim", "hip", "fire", "shoot", "shot", "reload", "draw", "holster"
        };
        private static readonly string[] DirectionKeywords = { "forward", "back", "left", "right", "strafe" };
        private static readonly string[] CombatStateKeywords = { "aimed", "hip aimed", "cover", "full cover", "half cover" };
        private static readonly string[] BodySideKeywords = { "left", "right", "dual" };

        public static CCS_SurvivalValidationResult GenerateWildWestInventory()
        {
            if (!AssetDatabase.IsValidFolder(WildWestPackRoot))
            {
                CCS_SurvivalValidationResult ownedClipsResult = ValidateCcsOwnedWildWestClips();
                if (!ownedClipsResult.IsSuccess)
                {
                    return ownedClipsResult;
                }

                WriteVendorAbsentReports(ownedClipsResult.Message);
                return CCS_SurvivalValidationResult.Pass(
                    "Vendor Wild West pack absent; validated CCS-owned isolated Wild West clips. "
                    + ownedClipsResult.Message);
            }

            EnsureReportDirectory();
            List<CCS_AnimationInventoryEntry> entries = CollectEntries(WildWestPackRoot);
            if (entries.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "No animation clips found under " + WildWestPackRoot + ".");
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.ClipName, b.ClipName));
            WriteMarkdownReport(entries);
            WriteCsvReport(entries);

            return CCS_SurvivalValidationResult.Pass(
                "Generated Wild West animation inventory with " + entries.Count + " clips.");
        }

        public static CCS_SurvivalValidationResult ValidateReportsExist()
        {
            List<string> failures = new List<string>();
            string markdownPath = ResolveReportPath(MarkdownReportPath);
            string csvPath = ResolveReportPath(CsvReportPath);
            if (!File.Exists(markdownPath))
            {
                failures.Add("Missing " + markdownPath);
            }

            if (!File.Exists(csvPath))
            {
                failures.Add("Missing " + csvPath);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Wild West animation inventory reports present.");
        }

        public static CCS_SurvivalValidationResult ValidateCcsOwnedWildWestClips()
        {
            List<string> failures = new List<string>();
            AppendClipExists(
                failures,
                CCS_CharacterControllerConstants.WildWestRevolverAimIdleRhClipPath);
            AppendClipExists(
                failures,
                CCS_CharacterControllerConstants.WildWestRevolverFireFanningRhClipPath);
            AppendClipExists(
                failures,
                CCS_CharacterControllerConstants.WildWestRevolverAimIdleFullDrawClipPath);
            AppendClipExists(
                failures,
                CCS_CharacterControllerConstants.WildWestRevolverFireFanningClipPath);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("CCS-owned Wild West revolver clips present.");
        }

        private static void AppendClipExists(List<string> failures, string clipPath)
        {
            if (!File.Exists(clipPath))
            {
                failures.Add("Missing CCS-owned Wild West clip: " + clipPath);
            }
        }

        private static void EnsureReportDirectory()
        {
            Directory.CreateDirectory(ResolveReportPath(ReportDirectory));
        }

        private static void WriteVendorAbsentReports(string ownedClipSummary)
        {
            EnsureReportDirectory();
            string markdownPath = ResolveReportPath(MarkdownReportPath);
            string csvPath = ResolveReportPath(CsvReportPath);
            StringBuilder markdown = new StringBuilder();
            markdown.AppendLine("# CCS Wild West Animation Inventory");
            markdown.AppendLine();
            markdown.AppendLine("**Generated:** " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC");
            markdown.AppendLine();
            markdown.AppendLine("Vendor pack `" + WildWestPackRoot + "` is not present in this workspace.");
            markdown.AppendLine();
            markdown.AppendLine("Validated CCS-owned Wild West revolver clips only.");
            markdown.AppendLine();
            markdown.AppendLine(ownedClipSummary);
            File.WriteAllText(markdownPath, markdown.ToString(), Encoding.UTF8);

            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Status,Detail");
            csv.AppendLine(string.Join(",", Csv("Vendor pack absent"), Csv(WildWestPackRoot)));
            csv.AppendLine(string.Join(",", Csv("Owned clip validation"), Csv(ownedClipSummary)));
            File.WriteAllText(csvPath, csv.ToString(), Encoding.UTF8);
        }

        private static List<CCS_AnimationInventoryEntry> CollectEntries(string rootFolder)
        {
            HashSet<string> seenClipKeys = new HashSet<string>();
            List<CCS_AnimationInventoryEntry> entries = new List<CCS_AnimationInventoryEntry>();

            string[] assetGuids = AssetDatabase.FindAssets(string.Empty, new[] { rootFolder });
            for (int i = 0; i < assetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                Type mainType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (mainType == typeof(AnimationClip))
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                    if (clip != null)
                    {
                        TryAddClip(entries, seenClipKeys, clip, assetPath, assetPath);
                    }

                    continue;
                }

                if (!assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                    && !assetPath.EndsWith(".FBX", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                for (int j = 0; j < subAssets.Length; j++)
                {
                    if (subAssets[j] is AnimationClip clip && !clip.name.StartsWith("__preview", StringComparison.Ordinal))
                    {
                        TryAddClip(entries, seenClipKeys, clip, assetPath + " :: " + clip.name, assetPath);
                    }
                }
            }

            return entries;
        }

        private static void TryAddClip(
            List<CCS_AnimationInventoryEntry> entries,
            HashSet<string> seenClipKeys,
            AnimationClip clip,
            string reportPath,
            string sourceAssetPath)
        {
            string key = reportPath + "|" + clip.name;
            if (!seenClipKeys.Add(key))
            {
                return;
            }

            entries.Add(BuildEntry(clip, reportPath, sourceAssetPath));
        }

        private static CCS_AnimationInventoryEntry BuildEntry(
            AnimationClip clip,
            string reportPath,
            string sourceAssetPath)
        {
            string folderCategory = ResolveFolderCategory(sourceAssetPath);
            string lowerName = clip.name.ToLowerInvariant();
            List<string> keywords = DetectKeywords(lowerName, folderCategory);
            bool mirrored = DetectMirrored(sourceAssetPath, clip.name);
            string rigType = ResolveRigType(sourceAssetPath);
            bool? rootMotion = ResolveRootMotion(sourceAssetPath, clip);
            string inPlace = ResolveInPlaceLikely(lowerName, rootMotion);
            string suggestedUse = SuggestUse(lowerName, keywords, folderCategory);
            string notes = BuildNotes(lowerName, keywords, mirrored, rigType);

            return new CCS_AnimationInventoryEntry
            {
                ClipName = clip.name,
                AssetPath = reportPath,
                FolderCategory = folderCategory,
                Duration = clip.length,
                FrameRate = clip.frameRate,
                LoopTime = clip.isLooping,
                RigType = rigType,
                RootMotionEnabled = rootMotion,
                InPlaceLikely = inPlace,
                Mirrored = mirrored ? "Yes" : "No",
                KeywordsDetected = string.Join(", ", keywords),
                SuggestedCcsUse = suggestedUse,
                Notes = notes
            };
        }

        private static string ResolveFolderCategory(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/');
            const string prefix = WildWestPackRoot + "/";
            if (!normalized.StartsWith(prefix, StringComparison.Ordinal))
            {
                return "Unknown";
            }

            string relative = normalized.Substring(prefix.Length);
            int slash = relative.IndexOf('/');
            return slash >= 0 ? relative.Substring(0, slash) : "Root";
        }

        private static List<string> DetectKeywords(string lowerName, string folderCategory)
        {
            List<string> keywords = new List<string>();
            AppendKeywordMatches(keywords, "Movement", lowerName, MovementKeywords);
            AppendKeywordMatches(keywords, "Weapon", lowerName, WeaponKeywords);
            AppendKeywordMatches(keywords, "Direction", lowerName, DirectionKeywords);
            AppendKeywordMatches(keywords, "CombatState", lowerName, CombatStateKeywords);
            AppendKeywordMatches(keywords, "BodySide", lowerName, BodySideKeywords);

            string lowerFolder = folderCategory.ToLowerInvariant();
            if (lowerFolder.Contains("cover"))
            {
                keywords.Add("CombatState:cover-folder");
            }

            if (lowerFolder.Contains("reload"))
            {
                keywords.Add("Weapon:reload-folder");
            }

            return keywords.Distinct().ToList();
        }

        private static void AppendKeywordMatches(
            List<string> keywords,
            string category,
            string lowerName,
            string[] terms)
        {
            for (int i = 0; i < terms.Length; i++)
            {
                if (lowerName.Contains(terms[i]))
                {
                    keywords.Add(category + ":" + terms[i]);
                }
            }
        }

        private static bool DetectMirrored(string assetPath, string clipName)
        {
            string metaText = File.Exists(assetPath + ".meta") ? File.ReadAllText(assetPath + ".meta") : string.Empty;
            if (metaText.Contains("mirror: 1") && metaText.Contains("name: " + clipName))
            {
                return true;
            }

            return clipName.IndexOf("mirror", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ResolveRigType(string assetPath)
        {
            string metaPath = assetPath + ".meta";
            if (!File.Exists(metaPath))
            {
                return "Unknown";
            }

            string metaText = File.ReadAllText(metaPath);
            if (metaText.Contains("animationType: 3"))
            {
                return "Generic";
            }

            if (metaText.Contains("animationType: 2"))
            {
                return "Humanoid";
            }

            if (metaText.Contains("animationType: 1"))
            {
                return "Legacy";
            }

            return "Unknown";
        }

        private static bool? ResolveRootMotion(string assetPath, AnimationClip clip)
        {
            if (clip.hasRootCurves)
            {
                return true;
            }

            string metaPath = assetPath + ".meta";
            if (!File.Exists(metaPath))
            {
                return null;
            }

            string metaText = File.ReadAllText(metaPath);
            if (metaText.Contains("keepOriginalPositionXZ: 0") || metaText.Contains("keepOriginalPositionY: 0"))
            {
                return true;
            }

            if (metaText.Contains("keepOriginalPositionXZ: 1") && metaText.Contains("keepOriginalPositionY: 1"))
            {
                return false;
            }

            return null;
        }

        private static string ResolveInPlaceLikely(string lowerName, bool? rootMotion)
        {
            if (rootMotion == false)
            {
                return "Yes";
            }

            if (rootMotion == true)
            {
                return "No";
            }

            if (lowerName.Contains("idle") || lowerName.Contains("aim") || lowerName.Contains("reload")
                || lowerName.Contains("fire") || lowerName.Contains("draw") || lowerName.Contains("holster"))
            {
                return "Yes";
            }

            if (lowerName.Contains("walk") || lowerName.Contains("run") || lowerName.Contains("move"))
            {
                return "Unknown";
            }

            return "Unknown";
        }

        private static string SuggestUse(string lowerName, List<string> keywords, string folderCategory)
        {
            if (lowerName.Contains("reload"))
            {
                return "Revolver reload candidate";
            }

            if (lowerName.Contains("draw") || lowerName.Contains("holster"))
            {
                return "Draw/holster transition candidate";
            }

            if (lowerName.Contains("fire") || lowerName.Contains("shoot") || lowerName.Contains("shot")
                || lowerName.Contains("fanning"))
            {
                return "Fire/recoil candidate";
            }

            if (lowerName.Contains("aim") || lowerName.Contains("aimed") || lowerName.Contains("hipdraw")
                || lowerName.Contains("fulldraw"))
            {
                return "Aim idle/pose candidate";
            }

            if (lowerName.Contains("walk") || lowerName.Contains("run") || lowerName.Contains("crouch"))
            {
                return "Combat locomotion candidate";
            }

            if (folderCategory.StartsWith("Cover", StringComparison.OrdinalIgnoreCase)
                || folderCategory.StartsWith("HalfCover", StringComparison.OrdinalIgnoreCase))
            {
                return "Cover combat candidate (future)";
            }

            if (keywords.Count == 0)
            {
                return "Review manually";
            }

            return "General gameplay review";
        }

        private static string BuildNotes(string lowerName, List<string> keywords, bool mirrored, string rigType)
        {
            List<string> notes = new List<string>();
            if (mirrored)
            {
                notes.Add("Importer mirror flag or mirrored naming detected.");
            }

            if (rigType == "Humanoid")
            {
                notes.Add("Humanoid retarget candidate.");
            }

            if (lowerName.Contains("_l") || lowerName.EndsWith(" l") || lowerName.Contains("left"))
            {
                notes.Add("Explicit left-side naming.");
            }

            if (lowerName.Contains("_r") || lowerName.EndsWith(" r") || lowerName.Contains("right"))
            {
                notes.Add("Explicit right-side naming.");
            }

            if (lowerName.Contains("dual"))
            {
                notes.Add("Explicit dual-weapon naming.");
            }

            return notes.Count > 0 ? string.Join(" ", notes) : string.Empty;
        }

        private static void WriteMarkdownReport(List<CCS_AnimationInventoryEntry> entries)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Wild West Animation Inventory");
            builder.AppendLine();
            builder.AppendLine("**Version:** 0.6.9");
            builder.AppendLine("**Source root:** `" + WildWestPackRoot + "`");
            builder.AppendLine("**Generated:** " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC");
            builder.AppendLine();
            builder.AppendLine("Vendor assets remain **source-only**. No production animator wiring in this pass.");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- Total clips: **" + entries.Count + "**");
            builder.AppendLine("- Categories: **" + entries.Select(entry => entry.FolderCategory).Distinct().Count() + "**");
            builder.AppendLine();
            builder.AppendLine("## All Clips");
            builder.AppendLine();
            builder.AppendLine("| Clip Name | Folder | Duration | FPS | Loop | Rig | Root Motion | In-Place | Mirrored | Suggested CCS Use |");
            builder.AppendLine("|-----------|--------|----------|-----|------|-----|-------------|----------|----------|-------------------|");
            for (int i = 0; i < entries.Count; i++)
            {
                CCS_AnimationInventoryEntry entry = entries[i];
                builder.AppendLine(
                    "| "
                    + EscapeMarkdown(entry.ClipName)
                    + " | "
                    + EscapeMarkdown(entry.FolderCategory)
                    + " | "
                    + entry.Duration.ToString("0.00", CultureInfo.InvariantCulture)
                    + " | "
                    + entry.FrameRate.ToString("0.##", CultureInfo.InvariantCulture)
                    + " | "
                    + (entry.LoopTime ? "Yes" : "No")
                    + " | "
                    + entry.RigType
                    + " | "
                    + FormatNullableBool(entry.RootMotionEnabled)
                    + " | "
                    + entry.InPlaceLikely
                    + " | "
                    + entry.Mirrored
                    + " | "
                    + EscapeMarkdown(entry.SuggestedCcsUse)
                    + " |");
            }

            AppendCandidateSection(builder, entries);
            AppendPipelineSection(builder, entries);
            File.WriteAllText(ResolveReportPath(MarkdownReportPath), builder.ToString(), Encoding.UTF8);
        }

        private static void AppendCandidateSection(StringBuilder builder, List<CCS_AnimationInventoryEntry> entries)
        {
            builder.AppendLine();
            builder.AppendLine("## Recommended CCS Candidate Clips");
            builder.AppendLine();
            AppendCandidateGroup(builder, "Right-Hand Revolver — aim idle", entries, entry =>
                entry.ClipName.IndexOf("revolver", StringComparison.OrdinalIgnoreCase) >= 0
                && (entry.ClipName.IndexOf("aim", StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.ClipName.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0));
            AppendCandidateGroup(builder, "Right-Hand Revolver — fire", entries, entry =>
                entry.ClipName.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0
                || entry.ClipName.IndexOf("shoot", StringComparison.OrdinalIgnoreCase) >= 0
                || entry.ClipName.IndexOf("fanning", StringComparison.OrdinalIgnoreCase) >= 0);
            AppendCandidateGroup(builder, "Right-Hand Revolver — reload", entries, entry =>
                entry.ClipName.IndexOf("reload", StringComparison.OrdinalIgnoreCase) >= 0);
            AppendCandidateGroup(builder, "Right-Hand Revolver — draw/holster", entries, entry =>
                entry.ClipName.IndexOf("draw", StringComparison.OrdinalIgnoreCase) >= 0
                || entry.ClipName.IndexOf("holster", StringComparison.OrdinalIgnoreCase) >= 0);
            AppendCandidateGroup(builder, "Locomotion / Strafe — walk/run aimed", entries, entry =>
                (entry.ClipName.IndexOf("walk", StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.ClipName.IndexOf("run", StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.ClipName.IndexOf("crouch", StringComparison.OrdinalIgnoreCase) >= 0)
                && entry.ClipName.IndexOf("aim", StringComparison.OrdinalIgnoreCase) >= 0);
            AppendCandidateGroup(builder, "Transitions", entries, entry =>
                entry.ClipName.IndexOf("_to_", StringComparison.OrdinalIgnoreCase) >= 0
                || entry.ClipName.IndexOf("to_", StringComparison.OrdinalIgnoreCase) >= 0);

            List<CCS_AnimationInventoryEntry> leftCandidates = entries
                .Where(entry => entry.ClipName.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.ClipName.IndexOf("_l", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            builder.AppendLine("### Left-Hand Revolver Candidates");
            builder.AppendLine();
            if (leftCandidates.Count == 0)
            {
                builder.AppendLine("No explicit left-hand / dual-revolver clip found. Recommend mirrored left-arm pipeline.");
            }
            else
            {
                for (int i = 0; i < leftCandidates.Count; i++)
                {
                    builder.AppendLine("- `" + leftCandidates[i].ClipName + "` — " + leftCandidates[i].AssetPath);
                }
            }

            List<CCS_AnimationInventoryEntry> dualCandidates = entries
                .Where(entry => entry.ClipName.IndexOf("dual", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            builder.AppendLine();
            builder.AppendLine("### Dual Revolver Candidates");
            builder.AppendLine();
            if (dualCandidates.Count == 0)
            {
                builder.AppendLine("No explicit dual-revolver clip found. Recommend layered/mirrored dual setup.");
            }
            else
            {
                for (int i = 0; i < dualCandidates.Count; i++)
                {
                    builder.AppendLine("- `" + dualCandidates[i].ClipName + "` — " + dualCandidates[i].AssetPath);
                }
            }
        }

        private static void AppendCandidateGroup(
            StringBuilder builder,
            string title,
            List<CCS_AnimationInventoryEntry> entries,
            Func<CCS_AnimationInventoryEntry, bool> predicate)
        {
            List<CCS_AnimationInventoryEntry> matches = entries.Where(predicate).ToList();
            builder.AppendLine("### " + title);
            builder.AppendLine();
            if (matches.Count == 0)
            {
                builder.AppendLine("- None matched by keyword scan.");
            }
            else
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    builder.AppendLine("- `" + matches[i].ClipName + "` — " + matches[i].AssetPath);
                }
            }

            builder.AppendLine();
        }

        private static void AppendPipelineSection(StringBuilder builder, List<CCS_AnimationInventoryEntry> entries)
        {
            builder.AppendLine("## Recommended Next Animation Pipeline");
            builder.AppendLine();
            builder.AppendLine("1. Pick right-hand revolver clips.");
            builder.AppendLine("2. Extract/isolate CCS-owned `.anim` copies under `Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/`.");
            builder.AppendLine("3. Build RightArm revolver upper-body mask.");
            builder.AppendLine("4. Build LeftArm mirrored mask.");
            builder.AppendLine("5. Create mirrored left-hand variants where needed.");
            builder.AppendLine("6. Build DualRevolver upper-body mask.");
            builder.AppendLine("7. Keep locomotion separate.");
            builder.AppendLine("8. Use IK/profile system for final hand/gun alignment.");
            builder.AppendLine();
            builder.AppendLine("Total inventoried clips: **" + entries.Count + "**.");
        }

        private static void WriteCsvReport(List<CCS_AnimationInventoryEntry> entries)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "Clip Name,Asset Path,Folder Category,Duration,Frame Rate,Loop Time,Rig Type,Root Motion Enabled,In-Place Likely,Mirrored,Keywords Detected,Suggested CCS Use,Notes");
            for (int i = 0; i < entries.Count; i++)
            {
                CCS_AnimationInventoryEntry entry = entries[i];
                builder.AppendLine(string.Join(",",
                    Csv(entry.ClipName),
                    Csv(entry.AssetPath),
                    Csv(entry.FolderCategory),
                    entry.Duration.ToString("0.###", CultureInfo.InvariantCulture),
                    entry.FrameRate.ToString("0.##", CultureInfo.InvariantCulture),
                    entry.LoopTime ? "Yes" : "No",
                    Csv(entry.RigType),
                    FormatNullableBool(entry.RootMotionEnabled),
                    Csv(entry.InPlaceLikely),
                    Csv(entry.Mirrored),
                    Csv(entry.KeywordsDetected),
                    Csv(entry.SuggestedCcsUse),
                    Csv(entry.Notes)));
            }

            File.WriteAllText(ResolveReportPath(CsvReportPath), builder.ToString(), Encoding.UTF8);
        }

        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        private static string EscapeMarkdown(string value)
        {
            return value.Replace("|", "\\|");
        }

        private static string FormatNullableBool(bool? value)
        {
            if (!value.HasValue)
            {
                return "Unknown";
            }

            return value.Value ? "Yes" : "No";
        }

        private sealed class CCS_AnimationInventoryEntry
        {
            public string ClipName;
            public string AssetPath;
            public string FolderCategory;
            public float Duration;
            public float FrameRate;
            public bool LoopTime;
            public string RigType;
            public bool? RootMotionEnabled;
            public string InPlaceLikely;
            public string Mirrored;
            public string KeywordsDetected;
            public string SuggestedCcsUse;
            public string Notes;
        }
    }
}
