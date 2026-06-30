using System.Collections.Generic;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationRebuildArchitectureReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.4 animation rebuild architecture report to Logs.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimationRebuildArchitectureReportBuilder
    {
        public const string ReportRelativePath =
            "Logs/CharacterController/AnimationRebuild/CCS_AnimationRebuildArchitecture_v0.7.4.md";

        public static string WriteReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Animation Rebuild Architecture Report (v0.7.4)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine("Baseline tag: v0.7.3");
            builder.AppendLine();

            AppendCurrentAnimatorState(builder);
            AppendFutureLayerLayout(builder);
            AppendActiveParameterContract(builder);
            AppendFuturePlannedParameters(builder);
            AppendCurrentAnimatorWriters(builder);
            AppendGameplayNotConnected(builder);
            AppendNextPhase(builder);

            string reportPath = ResolveReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? string.Empty);
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[Animation Rebuild Architecture] Wrote report to " + reportPath);
            return reportPath;
        }

        private static void AppendCurrentAnimatorState(StringBuilder builder)
        {
            builder.AppendLine("## Current active Animator state");
            builder.AppendLine();
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                builder.AppendLine("- Controller asset missing.");
                builder.AppendLine();
                return;
            }

            builder.AppendLine("- Controller: `" + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath + "`");
            builder.AppendLine("- Layer count: " + controller.layers.Length);
            for (int i = 0; i < controller.layers.Length; i++)
            {
                builder.AppendLine("  - Layer " + i + ": `" + controller.layers[i].name + "`");
            }

            builder.AppendLine("- Parameter count: " + controller.parameters.Length);
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = controller.parameters[i];
                builder.AppendLine("  - `" + parameter.name + "` (" + parameter.type + ")");
            }

            if (controller.layers.Length > 0 && controller.layers[0].stateMachine != null)
            {
                ChildAnimatorState[] states = controller.layers[0].stateMachine.states;
                builder.AppendLine("- Base Layer states (" + states.Length + "):");
                for (int i = 0; i < states.Length; i++)
                {
                    if (states[i].state != null)
                    {
                        builder.AppendLine("  - `" + states[i].state.name + "`");
                    }
                }
            }

            builder.AppendLine();
        }

        private static void AppendFutureLayerLayout(StringBuilder builder)
        {
            builder.AppendLine("## Intended future layer layout");
            builder.AppendLine();
            builder.AppendLine("1. **Base Locomotion Layer** — active (idle, walk, sprint, jump, in-air)");
            builder.AppendLine("2. **Upper Body Weapon Layer** — planned (`None`, `SingleRevolver`, `DualRevolver`)");
            builder.AppendLine("3. **Interaction Layer** — planned (pickup, door, workbench one-shots)");
            builder.AppendLine("4. **Additive Aim/Pose Layer** — planned (cosmetic pitch/offset only)");
            builder.AppendLine();
        }

        private static void AppendActiveParameterContract(StringBuilder builder)
        {
            builder.AppendLine("## Active parameter contract");
            builder.AppendLine();
            builder.AppendLine("| Parameter | Hash constant |");
            builder.AppendLine("|-----------|---------------|");
            builder.AppendLine("| SpeedNormalized | `CCS_CharacterAnimationParameterIds.Active.SpeedNormalizedHash` |");
            builder.AppendLine("| IsGrounded | `CCS_CharacterAnimationParameterIds.Active.IsGroundedHash` |");
            builder.AppendLine("| IsSprinting | `CCS_CharacterAnimationParameterIds.Active.IsSprintingHash` |");
            builder.AppendLine("| JumpTrigger | `CCS_CharacterAnimationParameterIds.Active.JumpTriggerHash` |");
            builder.AppendLine();
        }

        private static void AppendFuturePlannedParameters(StringBuilder builder)
        {
            builder.AppendLine("## Future planned parameters (design only)");
            builder.AppendLine();
            builder.AppendLine("Defined in `CCS_CharacterAnimationParameterIds.FutureDesignOnly` — not on Animator Controller in v0.7.4:");
            builder.AppendLine();
            string[] futureNames =
            {
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.WeaponMode,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.AimPitch,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.AimYaw,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.FireTrigger,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.ReloadTrigger,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.EquipTrigger,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.UnequipTrigger,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.InteractionTrigger,
                CCS_CharacterAnimationParameterIds.FutureDesignOnly.InteractionType,
            };

            for (int i = 0; i < futureNames.Length; i++)
            {
                builder.AppendLine("- `" + futureNames[i] + "`");
            }

            builder.AppendLine();
        }

        private static void AppendCurrentAnimatorWriters(StringBuilder builder)
        {
            builder.AppendLine("## Current scripts that write Animator parameters");
            builder.AppendLine();
            builder.AppendLine("| Script | Parameters |");
            builder.AppendLine("|--------|------------|");
            builder.AppendLine("| `CCS_PlayerLocomotionAnimator` | SpeedNormalized, IsGrounded, IsSprinting, JumpTrigger |");
            builder.AppendLine("| `CCS_AIAnimatorDriver` | AI locomotion only (must not write removed revolver/interaction params) |");
            builder.AppendLine();
        }

        private static void AppendGameplayNotConnected(StringBuilder builder)
        {
            builder.AppendLine("## Gameplay systems intentionally not connected to animation yet");
            builder.AppendLine();
            builder.AppendLine("- `CCS_RevolverController` — aim/fire/reload gameplay (no upper-body Animator bridge)");
            builder.AppendLine("- `CCS_AIWeaponController` — AI fire cadence and damage");
            builder.AppendLine("- `CCS_PlayerInteractionAnimator` — interaction lock/busy without Animator triggers");
            builder.AppendLine("- `CCS_CharacterAimLocomotionController` — aim movement gameplay");
            builder.AppendLine("- `CCS_MuzzleDrivenReticleController` / `CCS_RevolverArmReticleIK` — reticle/IK gameplay");
            builder.AppendLine("- `CCS_ICharacterAnimationPresenter` — interface only; no implementation wired");
            builder.AppendLine();
        }

        private static void AppendNextPhase(StringBuilder builder)
        {
            builder.AppendLine("## Next recommended implementation phase");
            builder.AppendLine();
            builder.AppendLine("**Phase 3D (after user sign-off):** optional locomotion clip refresh / import isolation while keeping Base Layer only.");
            builder.AppendLine("**Phase 3E:** implement `CCS_ICharacterAnimationPresenter` + single revolver upper-body layer.");
            builder.AppendLine("**Phase 3F:** interaction one-shot presentation layer.");
            builder.AppendLine("**Phase 3G:** dual revolver mode + additive aim.");
            builder.AppendLine("**Phase 3H:** CC4 retargeting pass.");
            builder.AppendLine();
            builder.AppendLine("Do not start import or Animator rebuild until explicitly approved.");
            builder.AppendLine();
        }

        private static string ResolveReportPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, ReportRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
