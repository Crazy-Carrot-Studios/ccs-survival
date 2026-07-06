using System;
using System.IO;
using System.Text;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PistolAimReferenceBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Shooter
// PURPOSE: Batch-mode entry for v0.7.14 CCS pistol aim reference planning.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PistolAimReferenceBatchEntry
    {
        public static void RunFromBatchMode()
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"[{DateTime.UtcNow:O}] CCS Pistol Aim Reference {CCS_PistolAimReferencePaths.AuditVersion} started.");

            try
            {
                string projectRoot = CCS_PistolAimReferencePaths.GetProjectRoot();
                bool externalFound = CCS_PistolAimReferenceValidationUtility.TryFindExternalReferenceProject(
                    out string externalProjectPath,
                    out string poseFbxPath);

                logBuilder.AppendLine("External reference project found: " + externalFound);
                if (externalFound)
                {
                    logBuilder.AppendLine("External project: " + externalProjectPath);
                    logBuilder.AppendLine("Reference pose FBX: " + poseFbxPath);
                }

                string testControllerPath = CCS_PistolAimTestControllerBuilder.BuildOrUpdateTestController();
                string reportPath = CCS_PistolAimReferenceReportBuilder.WriteReport(
                    externalFound,
                    externalProjectPath,
                    poseFbxPath);

                CCS_SurvivalValidationResult validation =
                    CCS_PistolAimReferenceValidationUtility.ValidatePistolAimReferencePlanning();

                logBuilder.AppendLine("Test controller: " + testControllerPath);
                logBuilder.AppendLine("Report: " + reportPath);

                if (!validation.IsSuccess)
                {
                    logBuilder.AppendLine("FAILED validation: " + validation.Message);
                    WriteBatchLog(projectRoot, logBuilder.ToString());
                    Debug.LogError("[Pistol Aim Reference Batch] Validation failed: " + validation.Message);
                    EditorApplication.Exit(1);
                    return;
                }

                logBuilder.AppendLine("Validation passed.");
                logBuilder.AppendLine(validation.Message);
                WriteBatchLog(projectRoot, logBuilder.ToString());
                Debug.Log("[Pistol Aim Reference Batch] Validation passed. " + validation.Message);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                logBuilder.AppendLine("EXCEPTION: " + exception);
                WriteBatchLog(CCS_PistolAimReferencePaths.GetProjectRoot(), logBuilder.ToString());
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void WriteBatchLog(string projectRoot, string content)
        {
            string logPath = Path.Combine(projectRoot, CCS_PistolAimReferencePaths.BatchLogRelative);
            string logDirectory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            File.WriteAllText(logPath, content, Encoding.UTF8);
        }
    }
}
