using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_StrippedCharacterControllerBaselineReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.13 stripped Character Controller baseline report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_StrippedCharacterControllerBaselineReportBuilder
    {
        public static string WriteReport(StrippedCharacterControllerBaselineStripResult stripResult = null)
        {
            string reportPath = ResolveReportPath(
                CCS_CharacterControllerConstants.StrippedCharacterControllerBaselineReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Stripped Character Controller Baseline (v0.7.13)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine("- Presentation-only revolver aim draw/hold/holster via SingleRevolverUpperBody layer.");
            builder.AppendLine("- Visual-only holstered revolver on right hip when Equip Weapon is enabled.");
            builder.AppendLine("- Removed gameplay weapons bridge, reticle/IK stack, interaction pickup wiring, and auto bandit spawn.");
            builder.AppendLine();

            if (stripResult != null)
            {
                builder.AppendLine("## Strip run");
                builder.AppendLine("- Animator changed: " + stripResult.AnimatorChanged);
                builder.AppendLine("- Prefab changed: " + stripResult.PrefabChanged);
                builder.AppendLine("- Scene changed: " + stripResult.SceneChanged);
                builder.AppendLine("- Player root MonoBehaviour count: " + stripResult.PlayerRootMonoBehaviourCount);
                builder.AppendLine();
                builder.AppendLine("### Notes");
                for (int i = 0; i < stripResult.Notes.Count; i++)
                {
                    builder.AppendLine("- " + stripResult.Notes[i]);
                }

                builder.AppendLine();
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller != null)
            {
                builder.AppendLine("## Animator layers");
                for (int i = 0; i < controller.layers.Length; i++)
                {
                    builder.AppendLine("- " + controller.layers[i].name);
                }

                builder.AppendLine();
                builder.AppendLine("## Animator parameters");
                for (int i = 0; i < controller.parameters.Length; i++)
                {
                    builder.AppendLine("- " + controller.parameters[i].name);
                }

                builder.AppendLine();
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab != null)
            {
                builder.AppendLine("## Player prefab");
                builder.AppendLine("- Path: `" + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath + "`");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
                builder.AppendLine("- Max allowed root MonoBehaviour count: "
                    + CCS_CharacterControllerConstants.StrippedBaselineExpectedRootMonoBehaviourCountMax);
                builder.AppendLine("- Root gate: "
                    + (prefab.GetComponent<CCS_RevolverAimPresentationGate>() != null ? "present" : "missing"));

                Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
                if (modelRoot != null)
                {
                    builder.AppendLine("- Model aim layer animator: "
                        + (modelRoot.GetComponent<CCS_RevolverAimLayerAnimator>() != null ? "present" : "missing"));
                    builder.AppendLine("- Model holstered visual presenter: "
                        + (modelRoot.GetComponent<CCS_PlayerHolsteredRevolverVisualPresenter>() != null
                            ? "present"
                            : "missing"));
                }
            }

            builder.AppendLine();
            builder.AppendLine("## Diagnostics Manager final controls");
            builder.AppendLine("- Enable Enemy (default false)");
            builder.AppendLine("- Enable Aim Pose (default false)");
            builder.AppendLine("- Equip Weapon (default true)");
            builder.AppendLine();
            builder.AppendLine("## Removed diagnostics controls");
            builder.AppendLine("- Recording / ambience / apply-on-start");
            builder.AppendLine("- Central debug toggles (verbose, camera, aim, animation, interaction, damage, visual helpers)");
            builder.AppendLine("- Debug testing manager");
            builder.AppendLine("- Reticle / aim target diagnostics");
            builder.AppendLine("- Dual holstered bool");
            builder.AppendLine("- Force revolver hand socket preview");
            builder.AppendLine();
            builder.AppendLine("## Manual smoke (validation scene)");
            builder.AppendLine("- CCS_DiagnosticsManager inspector shows only Enable Enemy, Enable Aim Pose, Equip Weapon.");
            builder.AppendLine("- No procedural arm rigging remains: no MultiAim, MultiRotation, TwoBoneIK, RigBuilder convergence layers, or rotation-bias pose handles.");
            builder.AppendLine("- Play scene with Equip Weapon true. Hold RMB: aim camera uses screenshot framing, shared passive reticle appears, authored animation pose only.");
            builder.AppendLine("- Right/left revolver visuals appear if dual preview is still acceptable.");
            builder.AppendLine("- No procedural arm rigging, hand/wrist/gun flipping, shooting, damage, or pickup.");
            builder.AppendLine("- Enable Aim Pose true: same presentation as RMB held, including camera aim zoom.");
            builder.AppendLine("- Equip Weapon false: no weapon visuals.");
            builder.AppendLine("- No legacy reticle stack, missing scripts, or Animator warnings.");
            builder.AppendLine();
            builder.AppendLine("## Aim Presentation State Audit");
            builder.AppendLine("- RMB input source: `CCS_CharacterInputActionProvider.AimHeld` on player root.");
            builder.AppendLine("- Diagnostics Enable Aim Pose source: `CCS_CharacterControllerDiagnosticsManager` via `CCS_RevolverAimSetupPoseDebugRegistry`.");
            builder.AppendLine("- Shared gate: `CCS_RevolverAimPresentationGate.IsAimPresentationActive` = RMB held OR Enable Aim Pose.");
            builder.AppendLine("- Camera aim zoom consumer: `CCS_CharacterCameraController` via `CCS_IWeaponCarryStateCameraSource.WantsAimOverShoulderCamera` and `CCS_CharacterAimLocomotionController.CanUseFirearmAimCamera`.");
            builder.AppendLine("- Aim layer consumer: `CCS_RevolverAimLayerAnimator` reads shared gate.");
            builder.AppendLine("- Shared aim point consumer: `CCS_SharedDualAimPointPresenter` moves passive `Model/Aiming/CCS_DualRevolverVisualAimReference/CCS_SharedDualAimPoint`.");
            builder.AppendLine("- Shared reticle consumer: `CCS_SharedDualAimReticlePresenter` projects passive screen reticle from shared aim point.");
            builder.AppendLine("- Weapon visual consumer: `CCS_PlayerEquippedRevolverAimVisualPresenter` reads shared gate + Equip Weapon.");
            builder.AppendLine("- All consumers use the same shared aim presentation state.");
            builder.AppendLine();
            builder.AppendLine("## Shared Aim Presentation State");
            builder.AppendLine("- Gate script: `Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_RevolverAimPresentationGate.cs`");
            builder.AppendLine("- Gate prefab path: `PF_CCS_CharacterController_Player_Networked` root");
            builder.AppendLine("- RMB input source: `CCS_CharacterInputActionProvider`");
            builder.AppendLine("- Diagnostics source: `CCS_CharacterControllerDiagnosticsManager.enableAimPose`");
            builder.AppendLine("- Camera zoom: `CCS_CharacterCameraController` + `CCS_CharacterAimLocomotionController`");
            builder.AppendLine("- Aim layers: `CCS_RevolverAimLayerAnimator`");
            builder.AppendLine("- Shared aim point: `CCS_SharedDualAimPointPresenter`");
            builder.AppendLine("- Shared reticle: `CCS_SharedDualAimReticlePresenter`");
            builder.AppendLine("- Weapon visuals: `CCS_PlayerEquippedRevolverAimVisualPresenter` + `CCS_PlayerHolsteredRevolverVisualPresenter`");
            builder.AppendLine("- No shooting, legacy reticle stack, damage, ammo, pickup, or ownership restored.");
            builder.AppendLine();
            builder.AppendLine("## Experimental Dual Revolver Aim Preview");
            builder.AppendLine("- Right-hand fit: `"
                + CCS_RevolverFitProfilePaths.RightHandEquippedFitPath
                + "` position `(0.091, 0.179, 0.005)` euler `(-56.622, 110.661, 60)` scale `(1, 1, 1)`.");
            builder.AppendLine("- Left-hand fit: `"
                + CCS_RevolverFitProfilePaths.LeftHandEquippedFitPath
                + "` mirrored seed from right (X negated, Y/Z euler negated). Manual tuning expected.");
            builder.AppendLine("- Left mask: `" + CCS_CharacterControllerConstants.RevolverAimLeftArmMaskPath + "`");
            builder.AppendLine("- Left layer: `"
                + CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyLayerName
                + "` with Animator state mirroring on Wild West source clips.");
            builder.AppendLine("- Experimental Animator params: LeftIsAiming, LeftRevolverDrawTrigger, LeftRevolverHolsterTrigger.");
            builder.AppendLine("- Equip Weapon true + Enable Aim Pose false: one right-hip holstered revolver, no left-hand visual.");
            builder.AppendLine("- Equip Weapon true + Enable Aim Pose true: right-hand + experimental left-hand equipped visuals and mirrored left aim layer.");
            builder.AppendLine("- Enable Aim Pose false: holster/return, left experimental visual clears, right holster returns when Equip Weapon true.");
            builder.AppendLine("- Equip Weapon false: all weapon visuals hidden.");
            builder.AppendLine("- WARNING: left-arm aim preview is experimental and may be reverted without affecting stripped baseline.");
            builder.AppendLine();
            builder.AppendLine("## Aim Camera Tuning");
            builder.AppendLine("- Profile: `" + CCS_CharacterControllerConstants.AimCameraProfilePath + "`");
            builder.AppendLine("- Previous distance: 1.85, shoulder offset: (0.45, 0.15, 0), camera side: 1.0, FOV: 58");
            builder.AppendLine("- Prior batch tuning: distance 1.55, shoulder offset (0.28, 0.18, 0), camera side 0.72, FOV 56");
            builder.AppendLine("- Screenshot lock: distance 1.06, shoulder offset (0.65, 0, -0.01), vertical arm 0.18, camera side 1, damping (0.06, 0.08, 0.06), FOV 56");
            builder.AppendLine("- Consumer: `CCS_CharacterCameraController` via `CCS_IWeaponCarryStateCameraSource.WantsAimOverShoulderCamera`");
            builder.AppendLine("- Secondary consumer: `CCS_CharacterAimLocomotionController.SetFirearmAimCameraActive`");
            builder.AppendLine("- RMB aim and Enable Aim Pose both trigger aim zoom through `CCS_RevolverAimPresentationGate`");
            builder.AppendLine();
            builder.AppendLine("## Procedural Arm Convergence Removed");
            builder.AppendLine("- MultiAimConstraint rejected: upper-arm evaluation raised/rolled the arm instead of cleanly angling inward; no stable Aim Axis / Up Axis combination.");
            builder.AppendLine("- MultiRotationConstraint rejected: copied pose rotations but did not solve a correct dual-gun aim pose.");
            builder.AppendLine("- TwoBoneIK rejected: hand targets twisted wrists and made revolvers point upward.");
            builder.AppendLine("- Active v0.7.13 uses authored animation only through Animator layers; no runtime procedural arm convergence remains on the production prefab.");
            builder.AppendLine("- Passive visual aim reference path: `Model/Aiming/CCS_DualRevolverVisualAimReference/CCS_SharedDualAimPoint`.");
            builder.AppendLine("- Passive reticle path: `WeaponHudRoot/CCS_SharedDualAimReticle` (screen projection only; does not drive arms, weapons, or shooting).");
            builder.AppendLine("- Kevin keeps Animator, skeleton, meshes, and aim animation layers only; no procedural convergence rig layer.");
            builder.AppendLine("- Hands, wrists, sockets, and weapon transforms are no longer procedurally manipulated.");
            builder.AppendLine("- Future v0.7.14: author a dual revolver aim pose/clip. See `Assets/CCS/Modules/CharacterController/Documentation/CCS_DualRevolverAimPose_AuthoringPlan.md`.");
            builder.AppendLine("- No shooting, damage, ammo, or pickup restored.");
            builder.AppendLine();
            builder.AppendLine("## Shared Dual Aim Presentation");
            builder.AppendLine("- Camera forward ray -> passive `CCS_SharedDualAimPoint` -> passive shared reticle projection.");
            builder.AppendLine("- Presenters: `CCS_SharedDualAimPointPresenter`, `CCS_SharedDualAimReticlePresenter`.");
            builder.AppendLine("- Reticle and aim point are visual reference only; authored animation drives the pose.");
            builder.AppendLine();
            AppendPassiveVisualAimReferenceAudit(builder);
            builder.AppendLine();
            builder.AppendLine("## Reports");
            builder.AppendLine("- Removal audit: `" + CCS_CharacterControllerConstants.StripBaselineRemovalAuditReportPath + "`");
            builder.AppendLine("- Baseline report: `" + CCS_CharacterControllerConstants.StrippedCharacterControllerBaselineReportPath + "`");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static void AppendPassiveVisualAimReferenceAudit(StringBuilder builder)
        {
            builder.AppendLine("## Passive Visual Aim Reference Audit");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                builder.AppendLine("- Player prefab missing.");
                return;
            }

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            if (modelRoot == null)
            {
                builder.AppendLine("- Model root missing.");
                return;
            }

            Animator animator = modelRoot.GetComponentInChildren<Animator>(true);
            RigBuilder rigBuilder = animator != null ? animator.GetComponent<RigBuilder>() : null;
            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
            Transform visualReferenceRoot = aimingRoot != null
                ? aimingRoot.Find(CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName)
                : null;
            Transform sharedAimPoint = visualReferenceRoot != null
                ? visualReferenceRoot.Find(CCS_CharacterControllerConstants.SharedDualAimPointObjectName)
                : null;
            Transform hudRoot = prefab.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);
            Transform sharedReticle = hudRoot != null
                ? hudRoot.Find(CCS_CharacterControllerConstants.SharedDualAimReticleObjectName)
                : null;

            builder.AppendLine("- Animator path: `" + GetTransformPath(prefab.transform, animator != null ? animator.transform : null) + "`");
            builder.AppendLine("- RigBuilder present: " + (rigBuilder != null ? "yes (FAIL)" : "no"));
            builder.AppendLine("- Visual aim reference path: `" + GetTransformPath(prefab.transform, visualReferenceRoot) + "`");
            builder.AppendLine("- Shared aim point path: `" + GetTransformPath(prefab.transform, sharedAimPoint) + "`");
            builder.AppendLine("- Shared reticle path: `" + GetTransformPath(prefab.transform, sharedReticle) + "`");
            builder.AppendLine("- Active shared aim point count: "
                + CountActiveByName(prefab.transform, CCS_CharacterControllerConstants.SharedDualAimPointObjectName));
            builder.AppendLine("- MultiAimConstraint count: " + prefab.GetComponentsInChildren<MultiAimConstraint>(true).Length);
            builder.AppendLine("- MultiRotationConstraint count: " + prefab.GetComponentsInChildren<MultiRotationConstraint>(true).Length);
            builder.AppendLine("- TwoBoneIKConstraint count: " + prefab.GetComponentsInChildren<TwoBoneIKConstraint>(true).Length);
            builder.AppendLine("- Procedural arm presenter scripts: "
                + (HasProceduralArmPresenterScript(prefab) ? "present (FAIL)" : "none detected"));
            builder.AppendLine("- Legacy manual aim targets present: "
                + (FindChildByName(prefab.transform, CCS_CharacterControllerConstants.DualRevolverManualAimTargetsRootObjectName) != null
                    ? "yes (FAIL)"
                    : "no"));
        }

        private static Transform FindChildByName(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == objectName)
                {
                    return transforms[i];
                }
            }

            return null;
        }

        private static bool HasProceduralArmPresenterScript(GameObject prefab)
        {
            string[] typeNames =
            {
                "CCS_DualRevolverAimConvergenceRigPresenter",
                "CCS_DualRevolverArmAimBiasPresenter",
                "CCS_DualRevolverArmAimConstraintPresenter",
                "CCS_ManualArmRotationBiasPresenter",
            };

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                string typeName = components[i].GetType().Name;
                for (int j = 0; j < typeNames.Length; j++)
                {
                    if (typeName == typeNames[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int CountActiveByName(Transform searchRoot, string objectName)
        {
            int count = 0;
            Transform[] transforms = searchRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName && transforms[i].gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static string GetTransformPath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return "missing";
            }

            StringBuilder pathBuilder = new StringBuilder(target.name);
            Transform current = target.parent;
            while (current != null)
            {
                pathBuilder.Insert(0, current.name + "/");
                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            return pathBuilder.ToString();
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
