using System;
using System.Collections.Generic;
using CCS.Modules.Attributes;
using CCS.Modules.Attributes.Tests;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Modules.Interaction;
using CCS.Modules.Weapons;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerPrefabComponentClassification
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Classifies player prefab MonoBehaviours for production architecture validation.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.8.0 player production prefab architecture.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public enum CCS_PlayerPrefabComponentCategory
    {
        ProductionRequired = 0,
        ProductionAllowed = 1,
        PresentationAllowed = 2,
        LocalOwnerUiOnly = 3,
        TestOnly = 4,
        Deprecated = 5,
        Unknown = 6,
    }

    public enum CCS_PlayerPrefabArchitectureLayer
    {
        Root = 0,
        RuntimeSystems = 1,
        Presentation = 2,
        LocalUi = 3,
        Any = 4,
    }

    public readonly struct CCS_PlayerPrefabComponentRule
    {
        public CCS_PlayerPrefabComponentRule(
            Type componentType,
            CCS_PlayerPrefabComponentCategory category,
            CCS_PlayerPrefabArchitectureLayer preferredLayer,
            bool requiredOnProduction = false)
        {
            ComponentType = componentType;
            Category = category;
            PreferredLayer = preferredLayer;
            RequiredOnProduction = requiredOnProduction;
        }

        public Type ComponentType { get; }

        public CCS_PlayerPrefabComponentCategory Category { get; }

        public CCS_PlayerPrefabArchitectureLayer PreferredLayer { get; }

        public bool RequiredOnProduction { get; }
    }

    public static class CCS_PlayerPrefabComponentClassification
    {
        private static readonly IReadOnlyList<CCS_PlayerPrefabComponentRule> Rules =
            BuildRules();

        public static IReadOnlyList<CCS_PlayerPrefabComponentRule> AllRules => Rules;

        public static bool TryClassify(MonoBehaviour behaviour, out CCS_PlayerPrefabComponentRule rule)
        {
            rule = default;
            if (behaviour == null)
            {
                return false;
            }

            Type behaviourType = behaviour.GetType();
            for (int ruleIndex = 0; ruleIndex < Rules.Count; ruleIndex++)
            {
                CCS_PlayerPrefabComponentRule candidate = Rules[ruleIndex];
                if (candidate.ComponentType.IsAssignableFrom(behaviourType))
                {
                    rule = candidate;
                    return true;
                }
            }

            return false;
        }

        public static CCS_PlayerPrefabComponentCategory ClassifyOrUnknown(MonoBehaviour behaviour)
        {
            return TryClassify(behaviour, out CCS_PlayerPrefabComponentRule rule)
                ? rule.Category
                : CCS_PlayerPrefabComponentCategory.Unknown;
        }

        public static CCS_PlayerPrefabArchitectureLayer ResolveLayer(Transform transform, Transform prefabRoot)
        {
            if (transform == null || prefabRoot == null)
            {
                return CCS_PlayerPrefabArchitectureLayer.Any;
            }

            if (transform == prefabRoot)
            {
                return CCS_PlayerPrefabArchitectureLayer.Root;
            }

            Transform current = transform;
            while (current != null && current != prefabRoot)
            {
                string objectName = current.name;
                if (objectName == CCS_PlayerPrefabConstants.RuntimeSystemsObjectName)
                {
                    return CCS_PlayerPrefabArchitectureLayer.RuntimeSystems;
                }

                if (objectName == CCS_PlayerPrefabConstants.PresentationObjectName
                    || objectName == "VisualRoot"
                    || IsUnderNamedAncestor(current, prefabRoot, CCS_PlayerPrefabConstants.PresentationObjectName))
                {
                    return CCS_PlayerPrefabArchitectureLayer.Presentation;
                }

                if (objectName == CCS_PlayerPrefabConstants.PlayerLocalUiObjectName
                    || objectName == "AttributeHudRoot"
                    || objectName == "WeaponHudRoot"
                    || objectName == "InteractionPromptHudRoot"
                    || IsUnderNamedAncestor(current, prefabRoot, CCS_PlayerPrefabConstants.PlayerLocalUiObjectName))
                {
                    return CCS_PlayerPrefabArchitectureLayer.LocalUi;
                }

                current = current.parent;
            }

            return CCS_PlayerPrefabArchitectureLayer.Any;
        }

        private static bool IsUnderNamedAncestor(Transform transform, Transform prefabRoot, string ancestorName)
        {
            Transform current = transform.parent;
            while (current != null && current != prefabRoot)
            {
                if (current.name == ancestorName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static IReadOnlyList<CCS_PlayerPrefabComponentRule> BuildRules()
        {
            return new List<CCS_PlayerPrefabComponentRule>
            {
                new(typeof(NetworkObject), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.Root, true),
                new(typeof(CCS_ClientOwnerNetworkTransform), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.Root, true),
                new(typeof(CCS_PlayerRuntimeFacade), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.Root, true),
                new(typeof(CCS_CharacterMotor), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.Root, true),

                new(typeof(CCS_CharacterInputActionProvider), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_CharacterCameraController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_CharacterControllerService), CCS_PlayerPrefabComponentCategory.ProductionAllowed, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems),
                new(typeof(CCS_NetworkInteractionScanner), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_AttributeContainer), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_AttributeService), CCS_PlayerPrefabComponentCategory.ProductionAllowed, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems),
                new(typeof(CCS_NetworkAttributeReplicator), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_StaminaController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_HealthRegenController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_NetworkHealth), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_RevolverController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_CharacterAimLocomotionController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_PlayerWeaponLoadout), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_EquipmentSocketRegistry), CCS_PlayerPrefabComponentCategory.ProductionAllowed, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems),
                new(typeof(CCS_PlayerEquipmentVisualController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),
                new(typeof(CCS_WeaponCarryStateController), CCS_PlayerPrefabComponentCategory.ProductionRequired, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),

                new(typeof(CCS_ControllerTestNetworkPlayerBehaviour), CCS_PlayerPrefabComponentCategory.Deprecated, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems, true),

                new(typeof(CCS_PlayerLocomotionAnimator), CCS_PlayerPrefabComponentCategory.PresentationAllowed, CCS_PlayerPrefabArchitectureLayer.Presentation),
                new(typeof(CCS_PlayerInteractionAnimator), CCS_PlayerPrefabComponentCategory.PresentationAllowed, CCS_PlayerPrefabArchitectureLayer.Presentation, true),
                new(typeof(CCS_RevolverUpperBodyAnimator), CCS_PlayerPrefabComponentCategory.PresentationAllowed, CCS_PlayerPrefabArchitectureLayer.Presentation),
                new(typeof(CCS_LocalFirstPersonHeadVisibility), CCS_PlayerPrefabComponentCategory.PresentationAllowed, CCS_PlayerPrefabArchitectureLayer.Presentation),
                new(typeof(CCS_FirstPersonBodyCameraAnchor), CCS_PlayerPrefabComponentCategory.PresentationAllowed, CCS_PlayerPrefabArchitectureLayer.Presentation),
                new(typeof(CCS_NetworkPlayerNameplate), CCS_PlayerPrefabComponentCategory.PresentationAllowed, CCS_PlayerPrefabArchitectureLayer.Presentation),

                new(typeof(CCS_RevolverHudPresenter), CCS_PlayerPrefabComponentCategory.LocalOwnerUiOnly, CCS_PlayerPrefabArchitectureLayer.LocalUi),
                new(typeof(CCS_PlayerDeathScreenController), CCS_PlayerPrefabComponentCategory.LocalOwnerUiOnly, CCS_PlayerPrefabArchitectureLayer.LocalUi),
                new(typeof(CCS_PlayerLocalOwnerUiBootstrap), CCS_PlayerPrefabComponentCategory.LocalOwnerUiOnly, CCS_PlayerPrefabArchitectureLayer.LocalUi, true),

                new(typeof(CCS_TestPlayerOfflineBootstrap), CCS_PlayerPrefabComponentCategory.TestOnly, CCS_PlayerPrefabArchitectureLayer.Root),
                new(typeof(CCS_TestPlayerAttributeDebugInput), CCS_PlayerPrefabComponentCategory.TestOnly, CCS_PlayerPrefabArchitectureLayer.RuntimeSystems),
                new(typeof(CCS_PlayerAnimatorRuntimeDiagnostics), CCS_PlayerPrefabComponentCategory.TestOnly, CCS_PlayerPrefabArchitectureLayer.Presentation),
            };
        }
    }
}
