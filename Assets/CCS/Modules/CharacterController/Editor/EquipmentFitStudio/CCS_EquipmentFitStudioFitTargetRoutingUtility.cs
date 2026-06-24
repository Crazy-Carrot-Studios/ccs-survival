using System.IO;
using CCS.Project;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioFitTargetRoutingUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Maps Fit Target to socket, profile, pose, camera, and scene labels.
// PLACEMENT: Editor utility used by revamped Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Prevents holster profile edits when Equipped Item is selected and vice versa.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitStudioFitTargetRoute
    {
        public CCS_EquipmentFitStudioFitTarget FitTarget;

        public string WeaponId = CCS_EquipmentConstants.RevolverM1879WeaponId;

        public string SocketId = string.Empty;

        public string ProfilePath = string.Empty;

        public string ProfileFileName = string.Empty;

        public CCS_EquipmentFitStudioPosePreviewMode PoseMode;

        public CCS_EquipmentFitStudioCameraPreset DefaultCameraPreset;

        public string SceneLabel = string.Empty;

        public string FocusLabel = string.Empty;
    }

    public static class CCS_EquipmentFitStudioFitTargetRoutingUtility
    {
        public static readonly string[] FitTargetLabels =
        {
            "Holstered Item",
            "Equipped Item",
        };

        public static CCS_EquipmentFitStudioFitTargetRoute Resolve(CCS_EquipmentFitStudioFitTarget fitTarget)
        {
            CCS_EquipmentFitStudioFitTargetRoute route = new CCS_EquipmentFitStudioFitTargetRoute
            {
                FitTarget = fitTarget,
                WeaponId = CCS_EquipmentConstants.RevolverM1879WeaponId,
            };

            switch (fitTarget)
            {
                case CCS_EquipmentFitStudioFitTarget.EquippedItem:
                    route.SocketId = CCS_EquipmentConstants.HandSocketRightId;
                    route.ProfilePath = CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath;
                    route.PoseMode = CCS_EquipmentFitStudioPosePreviewMode.RevolverAim;
                    route.DefaultCameraPreset = CCS_EquipmentFitStudioCameraPreset.RightHand;
                    route.SceneLabel = "Fit_Preview_Equipped";
                    route.FocusLabel = "Right Hand / Weapon";
                    break;
                default:
                    route.SocketId = CCS_EquipmentConstants.HolsterSocketRightHipId;
                    route.ProfilePath = CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath;
                    route.PoseMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;
                    route.DefaultCameraPreset = CCS_EquipmentFitStudioCameraPreset.RightHip;
                    route.SceneLabel = "Fit_Preview_Holstered";
                    route.FocusLabel = "Right Hip";
                    break;
            }

            route.ProfileFileName = string.IsNullOrEmpty(route.ProfilePath)
                ? string.Empty
                : Path.GetFileName(route.ProfilePath);
            return route;
        }

        public static bool RouteMatchesSelection(
            CCS_EquipmentFitStudioFitTarget fitTarget,
            string selectedSocketId,
            string selectedProfilePath)
        {
            CCS_EquipmentFitStudioFitTargetRoute route = Resolve(fitTarget);
            return route.SocketId == selectedSocketId
                && route.ProfilePath == selectedProfilePath;
        }

        public static CCS_SurvivalValidationResult ValidateFitTargetRoutingFoundation()
        {
            CCS_EquipmentFitStudioFitTargetRoute holster = Resolve(CCS_EquipmentFitStudioFitTarget.HolsteredItem);
            if (holster.SocketId != CCS_EquipmentConstants.HolsterSocketRightHipId)
            {
                return CCS_SurvivalValidationResult.Fail("Holstered Item must map to right hip holster socket.");
            }

            if (holster.ProfilePath != CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath)
            {
                return CCS_SurvivalValidationResult.Fail("Holstered Item must map to right hip holster profile.");
            }

            if (holster.PoseMode != CCS_EquipmentFitStudioPosePreviewMode.Neutral)
            {
                return CCS_SurvivalValidationResult.Fail("Holstered Item must load Neutral pose.");
            }

            CCS_EquipmentFitStudioFitTargetRoute equipped = Resolve(CCS_EquipmentFitStudioFitTarget.EquippedItem);
            if (equipped.SocketId != CCS_EquipmentConstants.HandSocketRightId)
            {
                return CCS_SurvivalValidationResult.Fail("Equipped Item must map to right hand socket.");
            }

            if (equipped.ProfilePath != CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath)
            {
                return CCS_SurvivalValidationResult.Fail("Equipped Item must map to right hand equipped profile.");
            }

            if (equipped.PoseMode != CCS_EquipmentFitStudioPosePreviewMode.RevolverAim)
            {
                return CCS_SurvivalValidationResult.Fail("Equipped Item must load Revolver Aim pose.");
            }

            return CCS_SurvivalValidationResult.Pass("Fit Target routing validated.");
        }
    }
}
