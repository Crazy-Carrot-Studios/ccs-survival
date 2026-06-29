using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerDisplayProfileApplicator
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Applies test player display profile layout to spawned player instances.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Shared by solo bootstrap, network spawn, and editor prefab builder.
// =============================================================================

namespace CCS.Modules.CharacterController.Local {
    public static class CCS_PlayerDisplayProfileApplicator
    {
        #region Public Methods

        public static void ApplyVisualLayout(GameObject playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            if (playerRoot == null || displayProfile == null)
            {
                return;
            }

            ApplyNameplateLayout(playerRoot.transform, displayProfile);
            ApplyCapsuleVisualLayout(playerRoot.transform, displayProfile);
            ApplyGlassesLayout(playerRoot.transform, displayProfile);
            ApplyCameraFollowLayout(playerRoot.transform, displayProfile);
        }

        public static void ApplyGameplayProfiles(GameObject playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            if (playerRoot == null || displayProfile == null || displayProfile.MovementProfile == null)
            {
                return;
            }

            CCS_CharacterMotor motor = playerRoot.GetComponent<CCS_CharacterMotor>();
            if (motor != null)
            {
                motor.SetMovementProfile(displayProfile.MovementProfile);
            }
        }

        #endregion

        #region Private Methods

        private static void ApplyNameplateLayout(Transform playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            Transform nameplateRoot = playerRoot.Find(CCS_PlayerPrefabConstants.NameplateRootObjectName);
            if (nameplateRoot != null)
            {
                nameplateRoot.localPosition = displayProfile.NameplateLocalPosition;
            }
        }

        private static void ApplyCapsuleVisualLayout(Transform playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            Transform capsuleVisual = playerRoot.Find(CCS_PlayerPrefabConstants.CapsuleVisualName);
            if (capsuleVisual == null)
            {
                return;
            }

            capsuleVisual.localPosition = displayProfile.CapsuleVisualLocalPosition;
            capsuleVisual.localScale = displayProfile.CapsuleVisualLocalScale;
        }

        private static void ApplyGlassesLayout(Transform playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            Transform glassesRoot = playerRoot.Find(CCS_PlayerPrefabConstants.GlassesVisualName);
            if (glassesRoot == null)
            {
                return;
            }

            glassesRoot.localPosition = displayProfile.GlassesLocalPosition;
            glassesRoot.localRotation = Quaternion.Euler(displayProfile.GlassesLocalEuler);
            glassesRoot.localScale = displayProfile.GlassesLocalScale;
        }

        private static void ApplyCameraFollowLayout(Transform playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            CCS_CharacterCameraFollowAnchor followAnchor =
                playerRoot.GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            if (followAnchor == null)
            {
                return;
            }

            followAnchor.ResolveReferences();
            Transform lookTarget = followAnchor.LookTarget;
            followAnchor.Configure(playerRoot, lookTarget, displayProfile.CameraProfile);
        }

        #endregion
    }
}
