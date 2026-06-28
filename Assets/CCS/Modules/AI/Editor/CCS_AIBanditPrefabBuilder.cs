using System.IO;
using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AIBanditPrefabBuilder
// CATEGORY: Modules / AI / Editor
// PURPOSE: Builds PF_CCS_AI_Bandit_Networked from canonical network test player prefab.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Strips player-only control scripts and adds AI orchestration components.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditPrefabBuilder
    {
        public static bool EnsureAIBanditPrefab()
        {
            EnsureTargetFolder();
            EnsureDefaultProfileAsset();

            if (!File.Exists(CCS_AIConstants.AIBanditPrefabPath))
            {
                if (!AssetDatabase.CopyAsset(CCS_AIConstants.SourceNetworkedPlayerPrefabPath, CCS_AIConstants.AIBanditPrefabPath))
                {
                    Debug.LogError("[AI Prefab Builder] Failed to copy source player prefab.");
                    return false;
                }
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CCS_AIConstants.AIBanditPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[AI Prefab Builder] Failed to load AI prefab contents.");
                return false;
            }

            bool changed = false;
            prefabRoot.name = CCS_AIConstants.AIBanditPrefabName;
            prefabRoot.tag = "Untagged";
            changed |= StripPlayerOnlyComponents(prefabRoot);
            changed |= EnsureBanditNameplateHierarchy(prefabRoot);
            changed |= EnsureCombatAndAiComponents(prefabRoot);
            changed |= EnsureDamageHitbox(prefabRoot);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CCS_AIConstants.AIBanditPrefabPath);
                AssetDatabase.SaveAssets();
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static void EnsureTargetFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules", "AI");
            }

            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI/Content"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules/AI", "Content");
            }

            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI/Content/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules/AI/Content", "Prefabs");
            }
        }

        private static bool StripPlayerOnlyComponents(GameObject prefabRoot)
        {
            bool changed = false;
            changed |= DestroyIfPresent<CCS_CharacterInputActionProvider>(prefabRoot);
            changed |= DestroyIfPresent<CCS_RevolverHudPresenter>(prefabRoot);
            changed |= DestroyByTypeName(prefabRoot, "CCS_ControllerTestNetworkPlayerBehaviour");
            changed |= DestroyByTypeName(prefabRoot, "CCS_NetworkPlayerNameplate");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerNameplateBillboard");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerAttributeBarsHud");
            changed |= DestroyByTypeName(prefabRoot, "CCS_TestPlayerAttributeDebugInput");
            changed |= DestroyByTypeName(prefabRoot, "CCS_TestPlayerOfflineBootstrap");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerLocomotionAnimator");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerInteractionAnimator");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerWeaponLoadout");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerEquipmentVisualController");

            changed |= DestroyChildByName(prefabRoot.transform, "NameplateRoot");
            changed |= DestroyChildByName(prefabRoot.transform, "AIBanditNameplateRoot");
            changed |= DestroyChildByName(prefabRoot.transform, "AttributeHudRoot");

            Canvas[] canvases = prefabRoot.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null)
                {
                    continue;
                }

                if (canvas.name == CCS_WeaponsConstants.WeaponHudRootName
                    || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Object.DestroyImmediate(canvas.gameObject, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool DestroyChildByName(Transform root, string childName)
        {
            Transform child = root.Find(childName);
            if (child == null)
            {
                return false;
            }

            Object.DestroyImmediate(child.gameObject, true);
            return true;
        }

        private static bool DestroyByTypeName(GameObject root, string typeName)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            bool changed = false;
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour.GetType().Name == typeName)
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureCombatAndAiComponents(GameObject prefabRoot)
        {
            bool changed = false;

            changed |= EnsureComponent<CCS_NetworkHealth>(prefabRoot, out CCS_NetworkHealth networkHealth);
            changed |= EnsureComponent<CCS_AITargetSensor>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AILineOfSightSensor>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AIMotorController>(prefabRoot, out CCS_AIMotorController motorController);
            changed |= EnsureComponent<CCS_AIWeaponController>(prefabRoot, out CCS_AIWeaponController weaponController);
            changed |= EnsureComponent<CCS_AIBanditBrain>(prefabRoot, out CCS_AIBanditBrain brain);
            changed |= EnsureComponent<CCS_AIBanditController>(prefabRoot, out CCS_AIBanditController banditController);
            changed |= EnsureComponent<CCS_AIBanditDeathHandler>(prefabRoot, out CCS_AIBanditDeathHandler deathHandler);
            changed |= EnsureComponent<CCS_RagdollController>(prefabRoot, out CCS_RagdollController ragdollController);
            changed |= EnsureComponent<CCS_AIAnimatorDriver>(prefabRoot, out CCS_AIAnimatorDriver animatorDriver);
            changed |= EnsureComponent<CCS_AIBanditNameplate>(prefabRoot, out CCS_AIBanditNameplate nameplate);
            changed |= EnsureNavMeshAgent(prefabRoot);

            CCS_RevolverController revolver = prefabRoot.GetComponent<CCS_RevolverController>();
            if (revolver != null)
            {
                revolver.SetWeaponOwnershipActive(true);
            }

            CCS_AttributeDefinition healthDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(
                    CCS_AttributesConstants.HealthDefinitionPath);
            CCS_AIBanditProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath);

            if (networkHealth != null)
            {
                SerializedObject serializedHealth = new SerializedObject(networkHealth);
                bool healthChanged = SetObjectReference(
                    serializedHealth,
                    "attributeContainer",
                    prefabRoot.GetComponent<CCS_AttributeContainer>());
                healthChanged |= SetObjectReference(serializedHealth, "healthDefinition", healthDefinition);
                if (healthChanged)
                {
                    serializedHealth.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (banditController != null)
            {
                SerializedObject serializedController = new SerializedObject(banditController);
                bool controllerChanged = SetObjectReference(serializedController, "networkHealth", networkHealth);
                controllerChanged |= SetObjectReference(serializedController, "brain", brain);
                controllerChanged |= SetObjectReference(serializedController, "profile", profile);
                if (controllerChanged)
                {
                    serializedController.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (brain != null)
            {
                SerializedObject serializedBrain = new SerializedObject(brain);
                bool brainChanged = SetObjectReference(serializedBrain, "profile", profile);
                brainChanged |= SetObjectReference(serializedBrain, "networkHealth", networkHealth);
                brainChanged |= SetObjectReference(serializedBrain, "targetSensor", prefabRoot.GetComponent<CCS_AITargetSensor>());
                brainChanged |= SetObjectReference(serializedBrain, "lineOfSightSensor", prefabRoot.GetComponent<CCS_AILineOfSightSensor>());
                brainChanged |= SetObjectReference(serializedBrain, "motorController", motorController);
                brainChanged |= SetObjectReference(serializedBrain, "weaponController", weaponController);
                if (brainChanged)
                {
                    serializedBrain.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (deathHandler != null)
            {
                SerializedObject serializedDeath = new SerializedObject(deathHandler);
                bool deathChanged = SetObjectReference(serializedDeath, "networkHealth", networkHealth);
                deathChanged |= SetObjectReference(serializedDeath, "brain", brain);
                deathChanged |= SetObjectReference(serializedDeath, "motorController", motorController);
                deathChanged |= SetObjectReference(serializedDeath, "animatorDriver", animatorDriver);
                deathChanged |= SetObjectReference(serializedDeath, "weaponController", weaponController);
                deathChanged |= SetObjectReference(serializedDeath, "ragdollController", ragdollController);
                deathChanged |= SetObjectReference(serializedDeath, "navMeshAgent", prefabRoot.GetComponent<NavMeshAgent>());
                deathChanged |= SetObjectReference(
                    serializedDeath,
                    "characterController",
                    prefabRoot.GetComponent<UnityEngine.CharacterController>());
                if (deathChanged)
                {
                    serializedDeath.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (ragdollController != null)
            {
                SerializedObject serializedRagdoll = new SerializedObject(ragdollController);
                Animator visualAnimator = prefabRoot.GetComponentInChildren<Animator>(true);
                bool ragdollChanged = SetObjectReference(serializedRagdoll, "animator", visualAnimator);
                Transform visualRoot = prefabRoot.transform.Find("VisualRoot");
                ragdollChanged |= SetObjectReference(serializedRagdoll, "ragdollRoot", visualRoot);
                if (ragdollChanged)
                {
                    serializedRagdoll.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (animatorDriver != null)
            {
                SerializedObject serializedDriver = new SerializedObject(animatorDriver);
                bool driverChanged = SetObjectReference(
                    serializedDriver,
                    "animator",
                    prefabRoot.GetComponentInChildren<Animator>(true));
                driverChanged |= SetObjectReference(serializedDriver, "navMeshAgent", prefabRoot.GetComponent<NavMeshAgent>());
                driverChanged |= SetObjectReference(serializedDriver, "brain", brain);
                driverChanged |= SetObjectReference(serializedDriver, "profile", profile);
                driverChanged |= SetObjectReference(serializedDriver, "networkHealth", networkHealth);
                if (driverChanged)
                {
                    serializedDriver.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (weaponController != null)
            {
                SerializedObject serializedWeapon = new SerializedObject(weaponController);
                bool weaponChanged = SetObjectReference(serializedWeapon, "revolverController", revolver);
                weaponChanged |= SetObjectReference(
                    serializedWeapon,
                    "upperBodyAnimator",
                    prefabRoot.GetComponentInChildren<CCS_RevolverUpperBodyAnimator>(true));
                if (weaponChanged)
                {
                    serializedWeapon.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (motorController != null)
            {
                SerializedObject serializedMotor = new SerializedObject(motorController);
                bool motorChanged = SetObjectReference(
                    serializedMotor,
                    "characterController",
                    prefabRoot.GetComponent<UnityEngine.CharacterController>());
                motorChanged |= SetObjectReference(
                    serializedMotor,
                    "navMeshAgent",
                    prefabRoot.GetComponent<NavMeshAgent>());
                if (motorChanged)
                {
                    serializedMotor.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (nameplate != null)
            {
                SerializedObject serializedNameplate = new SerializedObject(nameplate);
                bool nameplateChanged = SetObjectReference(serializedNameplate, "networkHealth", networkHealth);
                if (nameplateChanged)
                {
                    serializedNameplate.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureDamageHitbox(GameObject prefabRoot)
        {
            bool changed = false;
            CCS_NetworkHealth networkHealth = prefabRoot.GetComponent<CCS_NetworkHealth>();
            Transform hitboxTransform = prefabRoot.transform.Find(CCS_AIBanditDamageHitbox.HitboxObjectName);
            GameObject hitboxObject;
            if (hitboxTransform == null)
            {
                hitboxObject = new GameObject(CCS_AIBanditDamageHitbox.HitboxObjectName);
                hitboxObject.transform.SetParent(prefabRoot.transform, false);
                hitboxObject.transform.localPosition = Vector3.zero;
                hitboxObject.transform.localRotation = Quaternion.identity;
                hitboxObject.transform.localScale = Vector3.one;
                changed = true;
            }
            else
            {
                hitboxObject = hitboxTransform.gameObject;
            }

            CapsuleCollider capsuleCollider = hitboxObject.GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = hitboxObject.AddComponent<CapsuleCollider>();
                changed = true;
            }

            CCS_AIBanditDamageHitbox damageHitbox = hitboxObject.GetComponent<CCS_AIBanditDamageHitbox>();
            if (damageHitbox == null)
            {
                damageHitbox = hitboxObject.AddComponent<CCS_AIBanditDamageHitbox>();
                changed = true;
            }

            damageHitbox.Configure(capsuleCollider, networkHealth);
            return changed;
        }

        private static bool EnsureNavMeshAgent(GameObject prefabRoot)
        {
            NavMeshAgent agent = prefabRoot.GetComponent<NavMeshAgent>();
            bool added = false;
            if (agent == null)
            {
                agent = prefabRoot.AddComponent<NavMeshAgent>();
                added = true;
            }

            bool changed = added;
            if (!Mathf.Approximately(agent.radius, 0.35f))
            {
                agent.radius = 0.35f;
                changed = true;
            }

            if (!Mathf.Approximately(agent.height, 1.8f))
            {
                agent.height = 1.8f;
                changed = true;
            }

            if (!Mathf.Approximately(agent.speed, 2.8f))
            {
                agent.speed = 2.8f;
                changed = true;
            }

            if (!Mathf.Approximately(agent.angularSpeed, 720f))
            {
                agent.angularSpeed = 720f;
                changed = true;
            }

            if (!Mathf.Approximately(agent.acceleration, 8f))
            {
                agent.acceleration = 8f;
                changed = true;
            }

            if (agent.autoBraking)
            {
                agent.autoBraking = false;
                changed = true;
            }

            if (!agent.autoRepath)
            {
                agent.autoRepath = true;
                changed = true;
            }

            if (!Mathf.Approximately(agent.stoppingDistance, 1.5f))
            {
                agent.stoppingDistance = 1.5f;
                changed = true;
            }

            if (agent.updateRotation)
            {
                agent.updateRotation = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureBanditNameplateHierarchy(GameObject prefabRoot)
        {
            bool changed = false;
            changed |= RemoveLegacyNameplateObjects(prefabRoot.transform);

            Transform nameplateAnchor = prefabRoot.transform.Find(CCS_AIConstants.NameplateAnchorObjectName);
            if (nameplateAnchor == null)
            {
                GameObject anchorObject = new GameObject(CCS_AIConstants.NameplateAnchorObjectName);
                nameplateAnchor = anchorObject.transform;
                nameplateAnchor.SetParent(prefabRoot.transform, false);
                changed = true;
            }

            nameplateAnchor.localPosition = new Vector3(0f, CCS_AIConstants.NameplateWorldHeight, 0f);
            nameplateAnchor.localRotation = Quaternion.identity;
            nameplateAnchor.localScale = Vector3.one;

            Transform nameplateRoot = CCS_AIBanditNameplateUiFactory.EnsureNameplateRoot(nameplateAnchor);
            Transform canvasRoot = CCS_AIBanditNameplateUiFactory.EnsureCanvasRoot(nameplateRoot);
            CCS_AIBanditNameplateUiFactory.EnsureHealthBar(canvasRoot, out RectTransform healthFillRect);
            Image healthFillImage = healthFillRect != null ? healthFillRect.GetComponent<Image>() : null;
            TMP_Text nameText = CCS_AIBanditNameplateUiFactory.EnsureNameText(canvasRoot);
            changed |= canvasRoot != null && healthFillRect != null && nameText != null;

            CCS_AIBanditNameplate nameplate = prefabRoot.GetComponent<CCS_AIBanditNameplate>();
            if (nameplate != null)
            {
                SerializedObject serializedNameplate = new SerializedObject(nameplate);
                bool nameplateChanged = SetObjectReference(serializedNameplate, "nameplateAnchor", nameplateAnchor);
                nameplateChanged |= SetObjectReference(serializedNameplate, "nameplateRoot", nameplateRoot);
                nameplateChanged |= SetObjectReference(serializedNameplate, "healthFillRect", healthFillRect);
                nameplateChanged |= SetObjectReference(serializedNameplate, "healthFillImage", healthFillImage);
                nameplateChanged |= SetObjectReference(serializedNameplate, "nameText", nameText);
                nameplateChanged |= SetObjectReference(
                    serializedNameplate,
                    "networkHealth",
                    prefabRoot.GetComponent<CCS_NetworkHealth>());
                if (nameplateChanged)
                {
                    serializedNameplate.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            return changed;
        }

        private static bool RemoveLegacyNameplateObjects(Transform prefabRoot)
        {
            bool changed = false;
            Transform[] allTransforms = prefabRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform current = allTransforms[i];
                if (current == null || current == prefabRoot)
                {
                    continue;
                }

                if (current.name != CCS_AIConstants.NameplateRootObjectName
                    && current.name != CCS_AIConstants.NameplateCanvasObjectName
                    && current.name != CCS_AIConstants.NameplateAnchorObjectName)
                {
                    continue;
                }

                if (current.name == CCS_AIConstants.NameplateAnchorObjectName && current.parent == prefabRoot)
                {
                    continue;
                }

                if (current.name == CCS_AIConstants.NameplateRootObjectName
                    && current.parent != null
                    && current.parent.name == CCS_AIConstants.NameplateAnchorObjectName)
                {
                    continue;
                }

                if (current.name == CCS_AIConstants.NameplateCanvasObjectName
                    && current.parent != null
                    && current.parent.name == CCS_AIConstants.NameplateRootObjectName
                    && current.parent.parent != null
                    && current.parent.parent.name == CCS_AIConstants.NameplateAnchorObjectName)
                {
                    continue;
                }

                Object.DestroyImmediate(current.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static void EnsureDefaultProfileAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI/Content/Profiles"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules/AI/Content", "Profiles");
            }

            CCS_AIBanditProfile existingProfile =
                AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath);
            if (existingProfile == null)
            {
                existingProfile = ScriptableObject.CreateInstance<CCS_AIBanditProfile>();
                AssetDatabase.CreateAsset(existingProfile, CCS_AIConstants.AIBanditProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(existingProfile);
            bool changed = false;
            changed |= SetFloatIfDifferent(serializedProfile, "detectionRange", 45f);
            changed |= SetFloatIfDifferent(serializedProfile, "attackRange", 14f);
            changed |= SetFloatIfDifferent(serializedProfile, "minimumPreferredRange", 6f);
            changed |= SetFloatIfDifferent(serializedProfile, "moveSpeed", 2.8f);
            changed |= SetFloatIfDifferent(serializedProfile, "spawnDistanceFromPlayer", 24f);
            changed |= SetFloatIfDifferent(serializedProfile, "spawnSideOffset", CCS_AIConstants.DefaultSpawnSideOffset);
            changed |= SetFloatIfDifferent(serializedProfile, "movementStopDistance", 1.5f);
            changed |= SetFloatIfDifferent(serializedProfile, "repathIntervalSeconds", 0.15f);
            changed |= SetFloatIfDifferent(serializedProfile, "destinationUpdateThreshold", 0.35f);
            changed |= SetFloatIfDifferent(serializedProfile, "targetSampleRadius", 4f);
            changed |= SetBoolIfDifferent(serializedProfile, "pathRefreshWhenStale", true);
            changed |= SetFloatIfDifferent(serializedProfile, "loseSightRepathGraceSeconds", 0.15f);
            if (changed)
            {
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }
        }

        private static bool SetFloatIfDifferent(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetBoolIfDifferent(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static bool EnsureComponent<T>(GameObject root, out T component) where T : Component
        {
            component = root.GetComponent<T>();
            if (component != null)
            {
                return false;
            }

            component = root.AddComponent<T>();
            return true;
        }

        private static bool DestroyIfPresent<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null)
            {
                return false;
            }

            Object.DestroyImmediate(component, true);
            return true;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }
    }
}
