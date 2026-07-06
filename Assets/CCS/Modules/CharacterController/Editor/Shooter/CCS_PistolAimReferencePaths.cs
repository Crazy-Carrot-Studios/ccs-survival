using System.IO;

// =============================================================================
// SCRIPT: CCS_PistolAimReferencePaths
// CATEGORY: Modules / CharacterController / Editor / Shooter
// PURPOSE: Path constants for v0.7.14 CCS pistol aim reference planning.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PistolAimReferencePaths
    {
        public const string AuditVersion = "v0.7.14";

        public static readonly string[] ExternalReferenceProjectCandidates =
        {
            @"C:\Users\james\OneDrive\Documents\GitHub\CCS_Assets",
            @"C:\Users\james\Documents\GitHub\CCS_Assets",
        };

        public const string ExternalReferencePoseFbxRelative =
            "Shooter/3DModels/Animations/Shooter_UpperBodyPoses.fbx";

        public const string DocumentationShooterRelative =
            "Assets/CCS/Modules/CharacterController/Documentation/Shooter";

        public const string AdaptationPlanRelative =
            DocumentationShooterRelative + "/CCS_PistolAimAdaptationPlan_v0.7.14.md";

        public const string CameraReferenceRelative =
            DocumentationShooterRelative + "/CCS_PistolAimCameraReference_v0.7.14.md";

        public const string AnimationReferenceRelative =
            DocumentationShooterRelative + "/CCS_PistolAnimationReferenceAudit_v0.7.14.md";

        public const string ReportRelative = "Logs/CharacterController/Shooter/CCS_PistolAimReference_v0.7.14.md";

        public const string BatchLogRelative = "Logs/pistol-aim-reference-v0.7.14-batch.log";

        public static string GetProjectRoot()
        {
            return Directory.GetParent(UnityEngine.Application.dataPath)?.FullName
                ?? Directory.GetCurrentDirectory();
        }
    }
}
