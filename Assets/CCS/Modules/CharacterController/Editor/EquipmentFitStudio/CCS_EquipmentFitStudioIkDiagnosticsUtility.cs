using CCS.Modules.CharacterController;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioIkDiagnosticsUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Read-only IK/rig diagnostics for Fit Studio; preview weight helpers.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: IK is diagnostic only in v0.6.8; production weights stay 0.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitStudioIkDiagnosticsSnapshot
    {
        public bool RigBuilderFound;

        public bool RigFound;

        public bool RightHandConstraintFound;

        public bool TargetFound;

        public bool HintFound;

        public float RigWeight;

        public float ConstraintWeight;

        public float TargetPositionWeight;

        public float TargetRotationWeight;

        public float HintWeight;

        public bool IsPreviewEnabled =>
            RigWeight > 0.001f || ConstraintWeight > 0.001f || TargetPositionWeight > 0.001f;
    }

    public static class CCS_EquipmentFitStudioIkDiagnosticsUtility
    {
        public static CCS_EquipmentFitStudioIkDiagnosticsSnapshot CaptureSnapshot(GameObject playerRoot)
        {
            CCS_EquipmentFitStudioIkDiagnosticsSnapshot snapshot = new CCS_EquipmentFitStudioIkDiagnosticsSnapshot();
            if (playerRoot == null)
            {
                return snapshot;
            }

            Transform visualRoot = FindDeepChild(playerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            if (visualRoot == null)
            {
                return snapshot;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return snapshot;
            }

            RigBuilder rigBuilder = animator.GetComponent<RigBuilder>();
            snapshot.RigBuilderFound = rigBuilder != null;

            Transform rigTransform = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName);
            Rig rig = rigTransform != null ? rigTransform.GetComponent<Rig>() : null;
            snapshot.RigFound = rig != null;
            snapshot.RigWeight = rig != null ? rig.weight : 0f;

            TwoBoneIKConstraint[] constraints = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < constraints.Length; i++)
            {
                TwoBoneIKConstraint constraint = constraints[i];
                if (constraint == null || !constraint.name.Contains("Right"))
                {
                    continue;
                }

                snapshot.RightHandConstraintFound = true;
                snapshot.ConstraintWeight = constraint.weight;
                snapshot.TargetPositionWeight = constraint.data.targetPositionWeight;
                snapshot.TargetRotationWeight = constraint.data.targetRotationWeight;
                snapshot.HintWeight = constraint.data.hintWeight;
                snapshot.TargetFound = constraint.data.target != null;
                snapshot.HintFound = constraint.data.hint != null;
                break;
            }

            return snapshot;
        }

        public static void EnableIkPreview(GameObject playerRoot)
        {
            CCS_EquipmentFitStudioIkDiagnosticsSnapshot snapshot = CaptureSnapshot(playerRoot);
            if (!snapshot.RigFound && !snapshot.RightHandConstraintFound)
            {
                return;
            }

            Transform visualRoot = FindDeepChild(playerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            Animator animator = visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)?.GetComponent<Rig>();
            if (rig != null)
            {
                rig.weight = 1f;
            }

            TwoBoneIKConstraint[] constraints = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < constraints.Length; i++)
            {
                TwoBoneIKConstraint constraint = constraints[i];
                if (constraint == null || !constraint.name.Contains("Right"))
                {
                    continue;
                }

                constraint.weight = 1f;
                constraint.data.targetPositionWeight = 1f;
                constraint.data.targetRotationWeight = 1f;
                if (constraint.data.hint != null)
                {
                    constraint.data.hintWeight = 1f;
                }
            }
        }

        public static void ResetIkPreviewToZero(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return;
            }

            Transform visualRoot = FindDeepChild(playerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            Animator animator = visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)?.GetComponent<Rig>();
            if (rig != null)
            {
                rig.weight = 0f;
            }

            TwoBoneIKConstraint[] constraints = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < constraints.Length; i++)
            {
                TwoBoneIKConstraint constraint = constraints[i];
                if (constraint == null)
                {
                    continue;
                }

                constraint.weight = 0f;
                constraint.data.targetPositionWeight = 0f;
                constraint.data.targetRotationWeight = 0f;
                constraint.data.hintWeight = 0f;
            }

            MultiAimConstraint[] aimConstraints = animator.GetComponentsInChildren<MultiAimConstraint>(true);
            for (int i = 0; i < aimConstraints.Length; i++)
            {
                if (aimConstraints[i] != null)
                {
                    aimConstraints[i].weight = 0f;
                }
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
