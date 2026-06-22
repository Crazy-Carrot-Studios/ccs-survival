using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketValidationUtility
// CATEGORY: Modules / CharacterController / Runtime / Equipment
// PURPOSE: Validation helpers for equipment socket profile and player wiring.
// PLACEMENT: Called from editor validators and project audit.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.6 socket/IK foundation validation only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_EquipmentSocketValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAnimationRiggingPackageInstalled()
        {
            List<string> failures = new List<string>();
            string manifestPath = "Packages/manifest.json";
            AppendIfMissing(failures, File.Exists(manifestPath), "Missing Packages/manifest.json.");
            if (File.Exists(manifestPath))
            {
                string manifestText = File.ReadAllText(manifestPath);
                AppendIfMissing(
                    failures,
                    manifestText.Contains("\"" + CCS_EquipmentConstants.AnimationRiggingPackageName + "\""),
                    "Packages/manifest.json must include " + CCS_EquipmentConstants.AnimationRiggingPackageName + ".");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Animation Rigging package is installed.");
        }

        public static CCS_SurvivalValidationResult ValidateDefaultEquipmentSocketProfile()
        {
            List<string> failures = new List<string>();
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath),
                "Missing default equipment socket profile asset.");

            CCS_EquipmentSocketProfile profile = ResourcesLoadProfile();
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = profile.SocketDefinitions;
            AppendIfMissing(
                failures,
                definitions != null && definitions.Count == CCS_EquipmentConstants.RequiredSocketIds.Length,
                "Default equipment socket profile must contain exactly six socket definitions.");

            if (definitions == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            HashSet<string> uniqueIds = new HashSet<string>();
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = definitions[i];
                if (definition == null)
                {
                    failures.Add("Equipment socket profile contains a null socket definition.");
                    continue;
                }

                if (string.IsNullOrEmpty(definition.SocketId))
                {
                    failures.Add("Equipment socket definition must define socketId.");
                }
                else if (!uniqueIds.Add(definition.SocketId))
                {
                    failures.Add("Duplicate equipment socket ID in profile: " + definition.SocketId + ".");
                }

                if (definition.ParentBone == HumanBodyBones.LastBone)
                {
                    failures.Add(definition.SocketId + " must define parentBone.");
                }

                if (definition.AllowedItemTypes == null || definition.AllowedItemTypes.Count == 0)
                {
                    failures.Add(definition.SocketId + " must define allowedItemTypes.");
                }

                if (definition.LocalScale == Vector3.zero)
                {
                    failures.Add(definition.SocketId + " localScale must not be zero.");
                }
            }

            for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
            {
                AppendIfMissing(
                    failures,
                    uniqueIds.Contains(CCS_EquipmentConstants.RequiredSocketIds[i]),
                    "Missing required socket definition: " + CCS_EquipmentConstants.RequiredSocketIds[i] + ".");
            }

            ValidateBackSocketBlocking(failures, definitions);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Default equipment socket profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerEquipmentSocketFoundation(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Player prefab is missing.");
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_EquipmentSocketRegistry registry = prefabRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            AppendIfMissing(
                failures,
                registry != null,
                "Player prefab must contain CCS_EquipmentSocketRegistry.");

            CCS_EquipmentSocketAnchor[] anchors = prefabRoot.GetComponentsInChildren<CCS_EquipmentSocketAnchor>(true);
            AppendIfMissing(
                failures,
                anchors.Length == CCS_EquipmentConstants.RequiredSocketIds.Length,
                "Player prefab must contain exactly six CCS_EquipmentSocketAnchor components.");

            HashSet<string> uniqueIds = new HashSet<string>();
            for (int i = 0; i < anchors.Length; i++)
            {
                CCS_EquipmentSocketAnchor anchor = anchors[i];
                if (anchor == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(anchor.SocketId))
                {
                    failures.Add("Player equipment socket anchor is missing socketId.");
                    continue;
                }

                if (!uniqueIds.Add(anchor.SocketId))
                {
                    failures.Add("Duplicate player equipment socket ID: " + anchor.SocketId + ".");
                }

                if (anchor.transform.childCount > 0)
                {
                    failures.Add("Equipment socket " + anchor.SocketId + " must not have attached equipment children yet.");
                }
            }

            for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
            {
                AppendIfMissing(
                    failures,
                    uniqueIds.Contains(CCS_EquipmentConstants.RequiredSocketIds[i]),
                    "Missing required player socket: " + CCS_EquipmentConstants.RequiredSocketIds[i] + ".");
            }

            ValidateNoLegacyRevolverSockets(failures, prefabRoot);
            ValidateSocketParenting(failures, prefabRoot, anchors);
            ValidateWeaponIkTargets(failures, prefabRoot);
            ValidateWeaponIkRig(failures, prefabRoot);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player equipment socket foundation is valid.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerWeaponIkFoundation(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            ValidateWeaponIkTargets(failures, prefabRoot);
            ValidateWeaponIkRig(failures, prefabRoot);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player weapon IK foundation is valid.");
        }

        #endregion

        #region Private Methods

        private static void ValidateSocketParenting(
            List<string> failures,
            GameObject prefabRoot,
            CCS_EquipmentSocketAnchor[] anchors)
        {
            Transform visualRoot = FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            if (visualRoot == null)
            {
                return;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            bool hasHumanoidRig = HasUsableHumanoidRig(animator);
            Transform fallbackRoot = visualRoot.Find(CCS_EquipmentConstants.TestBoneSocketFallbacksObjectName);

            if (hasHumanoidRig && fallbackRoot != null)
            {
                failures.Add("Real humanoid player must not contain CCS_TestBoneSocketFallbacks.");
            }

            for (int i = 0; i < anchors.Length; i++)
            {
                CCS_EquipmentSocketAnchor anchor = anchors[i];
                if (anchor == null)
                {
                    continue;
                }

                Transform parent = anchor.transform.parent;
                if (parent == null)
                {
                    failures.Add("Equipment socket " + anchor.SocketId + " must have a parent transform.");
                    continue;
                }

                if (anchor.ParentMode == CCS_EquipmentSocketParentMode.RealHumanoidBone)
                {
                    if (hasHumanoidRig && animator != null)
                    {
                        Transform expectedBone = ResolveExpectedBoneTransform(anchor, animator);
                        if (expectedBone == null)
                        {
                            failures.Add(
                                anchor.SocketId
                                + " parent bone "
                                + anchor.ParentBone
                                + " is unavailable on humanoid rig.");
                        }
                        else if (parent != expectedBone)
                        {
                            failures.Add(
                                anchor.SocketId
                                + " must be parented directly to humanoid bone "
                                + expectedBone.name
                                + ".");
                        }
                    }

                    if (anchor.IsFallbackSocket)
                    {
                        failures.Add(anchor.SocketId + " reports fallback socket while using RealHumanoidBone mode.");
                    }
                }
                else if (anchor.ParentMode == CCS_EquipmentSocketParentMode.TestFallbackAnchor)
                {
                    if (!anchor.IsFallbackSocket)
                    {
                        failures.Add(anchor.SocketId + " must set isFallbackSocket when using test fallback anchors.");
                    }

                    if (fallbackRoot == null || parent.parent != fallbackRoot)
                    {
                        failures.Add(
                            anchor.SocketId
                            + " must be parented under approved test fallback anchors.");
                    }
                }
            }
        }

        private static void ValidateWeaponIkRig(List<string> failures, GameObject prefabRoot)
        {
            Transform visualRoot = FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            if (visualRoot == null)
            {
                return;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            bool hasHumanoidRig = HasUsableHumanoidRig(animator);
            AppendIfMissing(
                failures,
                ValidateAnimationRiggingPackageInstalled().IsSuccess,
                "Animation Rigging package must be installed for weapon IK foundation.");

            if (animator == null)
            {
                failures.Add("Player prefab must contain an Animator for weapon IK foundation.");
                return;
            }

            RigBuilder rigBuilder = animator.GetComponent<RigBuilder>();
            AppendIfMissing(
                failures,
                rigBuilder != null,
                "Humanoid player must contain RigBuilder when Animation Rigging is installed.");

            Transform rigTransform = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName);
            AppendIfMissing(
                failures,
                rigTransform != null,
                "Player must contain " + CCS_EquipmentConstants.WeaponIkRigObjectName + ".");

            if (rigTransform == null)
            {
                return;
            }

            Rig weaponIkRig = rigTransform.GetComponent<Rig>();
            AppendIfMissing(failures, weaponIkRig != null, "Missing Rig component on " + CCS_EquipmentConstants.WeaponIkRigObjectName + ".");
            if (weaponIkRig != null && weaponIkRig.weight != 0f)
            {
                failures.Add(CCS_EquipmentConstants.WeaponIkRigObjectName + " weight must default to 0.");
            }

            TwoBoneIKConstraint[] twoBoneConstraints = rigTransform.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            MultiAimConstraint[] aimConstraints = rigTransform.GetComponentsInChildren<MultiAimConstraint>(true);

            if (!hasHumanoidRig)
            {
                AppendIfMissing(
                    failures,
                    twoBoneConstraints.Length == 0 && aimConstraints.Length == 0,
                    "Test fallback rigs must not contain active weapon IK constraints.");
                return;
            }

            AppendIfMissing(
                failures,
                twoBoneConstraints.Length >= 2,
                "Humanoid player must contain right and left Two Bone IK constraints at weight 0.");

            for (int i = 0; i < twoBoneConstraints.Length; i++)
            {
                TwoBoneIKConstraint constraint = twoBoneConstraints[i];
                if (constraint == null)
                {
                    continue;
                }

                if (constraint.weight != 0f)
                {
                    failures.Add(constraint.name + " constraint weight must default to 0.");
                }

                if (constraint.data.targetPositionWeight != 0f
                    || constraint.data.targetRotationWeight != 0f
                    || constraint.data.hintWeight != 0f)
                {
                    failures.Add(constraint.name + " IK data weights must default to 0.");
                }
            }

            for (int i = 0; i < aimConstraints.Length; i++)
            {
                MultiAimConstraint constraint = aimConstraints[i];
                if (constraint == null)
                {
                    continue;
                }

                if (constraint.weight != 0f)
                {
                    failures.Add(constraint.name + " aim constraint weight must default to 0.");
                }
            }
        }

        private static bool HasUsableHumanoidRig(Animator animator)
        {
            if (animator == null || !animator.isHuman || animator.avatar == null || !animator.avatar.isValid)
            {
                return false;
            }

            HumanBodyBones[] requiredBones =
            {
                HumanBodyBones.Hips,
                HumanBodyBones.RightHand,
                HumanBodyBones.LeftHand,
                HumanBodyBones.Chest,
                HumanBodyBones.Spine,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
            };

            for (int i = 0; i < requiredBones.Length; i++)
            {
                if (animator.GetBoneTransform(requiredBones[i]) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static Transform ResolveExpectedBoneTransform(
            CCS_EquipmentSocketAnchor anchor,
            Animator animator)
        {
            Transform boneTransform = animator.GetBoneTransform(anchor.ParentBone);
            if (boneTransform != null)
            {
                return boneTransform;
            }

            CCS_EquipmentSocketProfile profile = ResourcesLoadProfile();
            if (profile == null)
            {
                return null;
            }

            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = profile.SocketDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = definitions[i];
                if (definition == null || definition.SocketId != anchor.SocketId)
                {
                    continue;
                }

                if (definition.HasFallbackParentBone)
                {
                    return animator.GetBoneTransform(definition.FallbackParentBone);
                }
            }

            return null;
        }

        private static CCS_EquipmentSocketProfile ResourcesLoadProfile()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketProfile>(
                CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
#else
            return null;
#endif
        }

        private static void ValidateBackSocketBlocking(
            List<string> failures,
            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions)
        {
            bool backABlocksB = false;
            bool backBBlocksA = false;
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = definitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (definition.SocketId == CCS_EquipmentConstants.BackSocketLongGunAId)
                {
                    backABlocksB = ContainsBlock(definition.BlocksOtherSockets, CCS_EquipmentConstants.BackSocketLongGunBId);
                }

                if (definition.SocketId == CCS_EquipmentConstants.BackSocketLongGunBId)
                {
                    backBBlocksA = ContainsBlock(definition.BlocksOtherSockets, CCS_EquipmentConstants.BackSocketLongGunAId);
                }
            }

            AppendIfMissing(
                failures,
                backABlocksB && backBBlocksA,
                "Back long-gun sockets must block each other by default.");
        }

        private static bool ContainsBlock(IReadOnlyList<string> blockedIds, string socketId)
        {
            if (blockedIds == null)
            {
                return false;
            }

            for (int i = 0; i < blockedIds.Count; i++)
            {
                if (blockedIds[i] == socketId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateNoLegacyRevolverSockets(List<string> failures, GameObject prefabRoot)
        {
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "CCS_RevolverHolsterSocket_RightHip") == null,
                "Player must not contain legacy CCS_RevolverHolsterSocket_RightHip.");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "CCS_RevolverHandSocket_Right") == null,
                "Player must not contain legacy CCS_RevolverHandSocket_Right.");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "PF_CCS_RevolverM1879_Holstered_Instance") == null,
                "Player must not contain holstered revolver visual instance.");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "PF_CCS_RevolverM1879_Equipped_Instance") == null,
                "Player must not contain equipped revolver visual instance.");
        }

        private static void ValidateWeaponIkTargets(List<string> failures, GameObject prefabRoot)
        {
            Transform visualRoot = FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            AppendIfMissing(failures, visualRoot != null, "Player prefab must contain VisualRoot.");
            if (visualRoot == null)
            {
                return;
            }

            Transform ikTargetsRoot = visualRoot.Find(CCS_EquipmentConstants.WeaponIkTargetsObjectName);
            AppendIfMissing(
                failures,
                ikTargetsRoot != null,
                "VisualRoot must contain " + CCS_EquipmentConstants.WeaponIkTargetsObjectName + ".");

            if (ikTargetsRoot == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                ikTargetsRoot.Find(CCS_EquipmentConstants.RightHandIkTargetObjectName) != null,
                "Missing " + CCS_EquipmentConstants.RightHandIkTargetObjectName + ".");
            AppendIfMissing(
                failures,
                ikTargetsRoot.Find(CCS_EquipmentConstants.RightElbowHintObjectName) != null,
                "Missing " + CCS_EquipmentConstants.RightElbowHintObjectName + ".");
            AppendIfMissing(
                failures,
                ikTargetsRoot.Find(CCS_EquipmentConstants.LeftHandIkTargetObjectName) != null,
                "Missing " + CCS_EquipmentConstants.LeftHandIkTargetObjectName + ".");
            AppendIfMissing(
                failures,
                ikTargetsRoot.Find(CCS_EquipmentConstants.LeftElbowHintObjectName) != null,
                "Missing " + CCS_EquipmentConstants.LeftElbowHintObjectName + ".");
            AppendIfMissing(
                failures,
                ikTargetsRoot.Find(CCS_EquipmentConstants.WeaponAimTargetObjectName) != null,
                "Missing " + CCS_EquipmentConstants.WeaponAimTargetObjectName + ".");
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

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
