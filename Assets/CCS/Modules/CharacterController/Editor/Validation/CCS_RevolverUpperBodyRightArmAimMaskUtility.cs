using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverUpperBodyRightArmAimMaskUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Shared Avatar Mask configuration for revolver upper-body/right-arm aim layer.
// PLACEMENT: Used by aim simplification and animation isolation builders.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Body + right arm/fingers active; legs, root, head, and left arm excluded.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverUpperBodyRightArmAimMaskUtility
    {
        public static bool EnsureMaskAsset()
        {
            string maskPath = CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath;
            string legacyMaskPath = CCS_CharacterControllerConstants.RevolverAimRightArmMaskLegacyPath;
            bool changed = false;

            if (!System.IO.File.Exists(maskPath) && System.IO.File.Exists(legacyMaskPath))
            {
                string moveError = AssetDatabase.MoveAsset(legacyMaskPath, maskPath);
                if (!string.IsNullOrEmpty(moveError))
                {
                    Debug.LogError(
                        "[Revolver Aim Mask] Failed to rename legacy aim mask: "
                        + moveError);
                }
                else
                {
                    changed = true;
                }
            }

            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
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
                AvatarMaskBodyPart.RightArm,
                AvatarMaskBodyPart.RightFingers,
            };

            AvatarMaskBodyPart[] inactiveParts =
            {
                AvatarMaskBodyPart.Root,
                AvatarMaskBodyPart.Head,
                AvatarMaskBodyPart.LeftArm,
                AvatarMaskBodyPart.LeftFingers,
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
                && mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm)
                && mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Root)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Head)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg)
                && !mask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg);
        }
    }
}
