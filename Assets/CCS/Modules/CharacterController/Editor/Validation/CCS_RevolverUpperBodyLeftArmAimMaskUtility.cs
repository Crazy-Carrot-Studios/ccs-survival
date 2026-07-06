using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverUpperBodyLeftArmAimMaskUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Avatar Mask configuration for experimental left-arm revolver aim layer.
// PLACEMENT: Used by dual-revolver preview builders and stripped baseline validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Body + left arm/fingers active; legs, root, head, and right arm excluded.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverUpperBodyLeftArmAimMaskUtility
    {
        public static bool EnsureMaskAsset()
        {
            string maskPath = CCS_CharacterControllerConstants.RevolverAimLeftArmMaskPath;
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            bool changed = false;
            if (mask == null)
            {
                mask = new AvatarMask { name = System.IO.Path.GetFileNameWithoutExtension(maskPath) };
                AssetDatabase.CreateAsset(mask, maskPath);
                changed = true;
            }

            if (ConfigureMask(mask))
            {
                EditorUtility.SetDirty(mask);
                changed = true;
            }

            return changed;
        }

        public static bool ConfigureMask(AvatarMask mask)
        {
            if (mask == null)
            {
                return false;
            }

            bool changed = false;
            AvatarMaskBodyPart[] activeParts =
            {
                AvatarMaskBodyPart.Body,
                AvatarMaskBodyPart.LeftArm,
                AvatarMaskBodyPart.LeftFingers,
            };

            AvatarMaskBodyPart[] inactiveParts =
            {
                AvatarMaskBodyPart.Root,
                AvatarMaskBodyPart.Head,
                AvatarMaskBodyPart.RightArm,
                AvatarMaskBodyPart.RightFingers,
                AvatarMaskBodyPart.LeftLeg,
                AvatarMaskBodyPart.RightLeg,
            };

            for (int i = 0; i < activeParts.Length; i++)
            {
                if (!mask.GetHumanoidBodyPartActive(activeParts[i]))
                {
                    mask.SetHumanoidBodyPartActive(activeParts[i], true);
                    changed = true;
                }
            }

            for (int i = 0; i < inactiveParts.Length; i++)
            {
                if (mask.GetHumanoidBodyPartActive(inactiveParts[i]))
                {
                    mask.SetHumanoidBodyPartActive(inactiveParts[i], false);
                    changed = true;
                }
            }

            return changed;
        }

        public static bool ValidateMaskConfiguration(AvatarMask mask)
        {
            if (mask == null)
            {
                return false;
            }

            return mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Body)
                && mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm)
                && mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Root)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Head)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg);
        }
    }
}
