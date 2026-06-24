using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitProfilePersistenceUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Save/reload/verify revolver fit profiles with disk persistence logging.
// PLACEMENT: Editor utility used by Fit Studio save flow and readback UI.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Profile asset on disk is the source of truth after save.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitProfileSaveResult
    {
        public bool Success;

        public string ProfilePath = string.Empty;

        public string ProfileName = string.Empty;

        public Vector3 Position;

        public Vector3 Rotation;

        public Vector3 Scale;

        public string Message = string.Empty;
    }

    public static class CCS_EquipmentFitProfilePersistenceUtility
    {
        public static readonly Vector3 RightHipHolsterSeedPosition = new Vector3(0.11f, -0.04f, 0.05f);

        public static readonly Vector3 RightHipHolsterSeedEuler = new Vector3(68f, 98f, -10f);

        public static CCS_WeaponAttachmentFitProfile LoadProfileFromDisk(string profilePath)
        {
            if (string.IsNullOrEmpty(profilePath))
            {
                return null;
            }

            AssetDatabase.ImportAsset(profilePath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(profilePath);
        }

        public static CCS_WeaponAttachmentFitProfile LoadHolsterProfileFromDisk()
        {
            return LoadProfileFromDisk(CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath);
        }

        public static CCS_WeaponAttachmentFitProfile LoadEquippedProfileFromDisk()
        {
            return LoadProfileFromDisk(CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
        }

        public static bool TrySavePendingCaptureDetailed(
            CCS_EquipmentFitStudioPendingChange pending,
            string socketId,
            out CCS_EquipmentFitProfileSaveResult result)
        {
            result = new CCS_EquipmentFitProfileSaveResult();

            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                result.Message = "Cannot save fit profile while entering/exiting Play Mode.";
                Debug.LogWarning("[Equipment Fit Studio] " + result.Message);
                return false;
            }

            if (!CCS_EquipmentFitStudioCaptureUtility.TrySavePendingSocketCapture(
                    pending,
                    socketId,
                    out string successMessage,
                    out string errorMessage))
            {
                result.Message = errorMessage;
                Debug.LogError("[Equipment Fit Studio] Save failed: " + errorMessage);
                return false;
            }

            string profilePath = CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(socketId);
            CCS_WeaponAttachmentFitProfile reloaded = LoadProfileFromDisk(profilePath);
            if (reloaded == null)
            {
                result.Message = "Save reported success but profile could not be reloaded from disk.";
                Debug.LogError("[Equipment Fit Studio] " + result.Message);
                return false;
            }

            result.Success = true;
            result.ProfilePath = profilePath;
            result.ProfileName = reloaded.name;
            result.Position = reloaded.SocketLocalPosition;
            result.Rotation = reloaded.SocketLocalEulerAngles;
            result.Scale = reloaded.SocketLocalScale;
            result.Message = BuildSavedAndReloadedMessage(reloaded, profilePath);

            Debug.Log(
                "[Equipment Fit Studio] Saved and verified profile "
                + reloaded.name
                + " Position="
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(result.Position)
                + " Rotation="
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(result.Rotation)
                + " Scale="
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(result.Scale)
                + " Path="
                + profilePath);

            return true;
        }

        public static bool ProfileMatchesSeedDefaults(CCS_WeaponAttachmentFitProfile profile)
        {
            if (profile == null)
            {
                return false;
            }

            return CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(
                    profile.SocketLocalPosition,
                    RightHipHolsterSeedPosition)
                && CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(
                    profile.SocketLocalEulerAngles,
                    RightHipHolsterSeedEuler);
        }

        public static bool ResetHolsterProfileToSeedDefaults()
        {
            return ResetProfileToSeed(
                CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath,
                "CCS_RevolverM1879_RightHipHolster_Fit",
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                RightHipHolsterSeedPosition,
                RightHipHolsterSeedEuler,
                Vector3.one);
        }

        public static bool ResetEquippedProfileToSeedDefaults()
        {
            return ResetProfileToSeed(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath,
                "CCS_RevolverM1879_RightHandEquipped_Fit",
                CCS_EquipmentConstants.HandSocketRightId,
                new Vector3(0.03f, 0.015f, 0.05f),
                new Vector3(-12f, 92f, 8f),
                Vector3.one);
        }

        private static bool ResetProfileToSeed(
            string profilePath,
            string profileId,
            string socketId,
            Vector3 position,
            Vector3 euler,
            Vector3 scale)
        {
            if (!File.Exists(profilePath))
            {
                return false;
            }

            CCS_WeaponAttachmentFitProfile profile = LoadProfileFromDisk(profilePath);
            if (profile == null)
            {
                return false;
            }

            profile.SetIdentity(
                profileId,
                CCS_EquipmentConstants.RevolverM1879WeaponId,
                CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                socketId);
            profile.ApplySocketTransform(profileId, position, euler, scale);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Equipment Fit Studio] Reset revolver fit profile to seed defaults: " + profile.name);
            return true;
        }

        private static string BuildSavedAndReloadedMessage(
            CCS_WeaponAttachmentFitProfile profile,
            string profilePath)
        {
            return "Saved and reloaded profile:\n"
                + "Position: "
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(profile.SocketLocalPosition)
                + "\nRotation: "
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(profile.SocketLocalEulerAngles)
                + "\nScale: "
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(profile.SocketLocalScale)
                + "\nAsset Path: "
                + profilePath;
        }
    }
}
