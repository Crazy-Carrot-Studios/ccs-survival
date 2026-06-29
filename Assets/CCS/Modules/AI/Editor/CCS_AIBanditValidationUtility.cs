using System.Collections.Generic;
using System.IO;
using CCS.Modules.Attributes;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.Weapons;
using CCS.Project;
using TMPro;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AIBanditValidationUtility
// CATEGORY: Modules / AI / Editor
// PURPOSE: Validates AI module foundation, prefab contracts, and v0.7.1 polish.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Milestone B13 + v0.7.1 nameplate/navigation/hosting checks.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateMilestoneB13Foundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime"), "Missing AI Runtime folder.");
            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Editor"), "Missing AI Editor folder.");
            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Documentation"), "Missing AI Documentation folder.");
            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Content/Prefabs"), "Missing AI Content/Prefabs folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/CCS.Modules.AI.Runtime.asmdef"),
                "Missing CCS.Modules.AI.Runtime.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Editor/CCS.Modules.AI.Editor.asmdef"),
                "Missing CCS.Modules.AI.Editor.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Combat/CCS_AIBanditBrain.cs"),
                "Missing CCS_AIBanditBrain.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Animation/CCS_AIAnimatorDriver.cs"),
                "Missing CCS_AIAnimatorDriver.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Components/CCS_RagdollController.cs"),
                "Missing CCS_RagdollController.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Components/CCS_AIBanditController.cs"),
                "Missing CCS_AIBanditController.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Components/CCS_AIBanditDeathHandler.cs"),
                "Missing CCS_AIBanditDeathHandler.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Spawning/CCS_AIBanditSpawner.cs"),
                "Missing CCS_AIBanditSpawner.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/UI/CCS_AIBanditNameplate.cs"),
                "Missing CCS_AIBanditNameplate.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/Attributes/Runtime/Components/CCS_NetworkHealth.cs"),
                "Missing CCS_NetworkHealth shared damage implementation.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Components/CCS_AIBanditDamageHitbox.cs"),
                "Missing CCS_AIBanditDamageHitbox alive hitbox component.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/Attributes/Runtime/Utilities/CCS_DamageableLookupUtility.cs"),
                "Missing CCS_DamageableLookupUtility shared parent lookup.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.AIBanditPrefabPath),
                "Missing PF_CCS_AI_Bandit_Networked prefab.");

            string netcodeConstantsPath =
                "Assets/CCS/Modules/CharacterController/Runtime/Netcode/CCS_NetcodeConstants.cs";
            bool netcodePathRegistered = File.Exists(netcodeConstantsPath)
                && File.ReadAllText(netcodeConstantsPath).Contains(CCS_AIConstants.AIBanditPrefabPath);

            AppendIfMissing(
                failures,
                netcodePathRegistered,
                "AI bandit prefab must be registered in CCS_NetcodeConstants.RequiredNetworkPrefabPaths.");

            failures.AddRange(ValidateV071PolishPrefabAndScenes());

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("AI Milestone B13 validation passed.");
        }

        private static List<string> ValidateV071PolishPrefabAndScenes()
        {
            List<string> failures = new List<string>();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath);
            if (prefab == null)
            {
                failures.Add("AI bandit prefab asset could not be loaded for v0.7.1 validation.");
                return failures;
            }

            ValidateBanditPrefabContent(prefab, failures);
            ValidateMasterTestNavigationScene(failures);
            ValidateMasterTestHasNoAmbientAudio(failures);
            ValidateHostingSceneAmbientAudio(failures);
            ValidateDamageReadinessContract(failures);
            ValidatePlayerWeaponHitMaskIncludesAiLayer(failures);
            return failures;
        }

        private static void ValidateBanditPrefabContent(GameObject prefab, List<string> failures)
        {
            if (prefab.GetComponent<NavMeshAgent>() == null)
            {
                failures.Add("AI bandit prefab must contain NavMeshAgent for pathfinding.");
            }

            if (prefab.GetComponentInChildren<Animator>(true) == null)
            {
                failures.Add("AI bandit prefab must contain Animator for locomotion.");
            }

            if (prefab.GetComponent<CCS_AIAnimatorDriver>() == null)
            {
                failures.Add("AI bandit prefab must contain CCS_AIAnimatorDriver.");
            }

            if (prefab.GetComponent<CCS_RagdollController>() == null)
            {
                failures.Add("AI bandit prefab must contain CCS_RagdollController.");
            }

            if (prefab.GetComponent<CCS_AIBanditDeathHandler>() == null)
            {
                failures.Add("AI bandit prefab must contain CCS_AIBanditDeathHandler.");
            }

            if (prefab.GetComponent<CCS_AIBanditNameplate>() == null)
            {
                failures.Add("AI bandit prefab must contain CCS_AIBanditNameplate.");
            }

            ValidateBanditDamageHitbox(prefab, failures);
            ValidatePlayerCanDamageAiBandit(prefab, failures);
            ValidateHealthBarFillDirection(prefab, failures);
            ValidateAiDeathDespawn(prefab, failures);

            if (prefab.transform.Find("NameplateRoot") != null)
            {
                failures.Add("AI bandit prefab must not retain legacy player NameplateRoot.");
            }

            Transform nameplateAnchor = prefab.transform.Find(CCS_AIConstants.NameplateAnchorObjectName);
            if (nameplateAnchor == null)
            {
                failures.Add("AI bandit prefab must contain UIAnchor_Nameplate under AI root.");
                return;
            }

            if (IsTransformUnderNamedAncestor(nameplateAnchor, "VisualRoot")
                || IsTransformUnderNamedAncestor(nameplateAnchor, "Armature"))
            {
                failures.Add("AI bandit nameplate anchor must not be parented under VisualRoot or Armature.");
            }

            Transform nameplateRoot = nameplateAnchor.Find(CCS_AIConstants.NameplateRootObjectName);
            if (nameplateRoot == null)
            {
                failures.Add("AI bandit prefab must contain AI_Bandit_Nameplate under UIAnchor_Nameplate.");
                return;
            }

            if (Mathf.Abs(nameplateAnchor.localPosition.y - CCS_AIConstants.NameplateWorldHeight) > 0.35f)
            {
                failures.Add("AI bandit nameplate anchor height must be near 2.15m above AI root.");
            }

            Transform canvasRoot = nameplateRoot.Find(CCS_AIConstants.NameplateCanvasObjectName);
            if (canvasRoot == null)
            {
                failures.Add("AI bandit nameplate must contain Canvas_WorldSpace.");
                return;
            }

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            if (canvas == null || !canvas.enabled)
            {
                failures.Add("AI bandit nameplate Canvas_WorldSpace must contain an enabled Canvas.");
            }
            else if (canvas.renderMode != RenderMode.WorldSpace)
            {
                failures.Add("AI bandit nameplate canvas must use World Space render mode.");
            }
            else if (canvas.sortingOrder < CCS_AIConstants.NameplateCanvasSortingOrder)
            {
                failures.Add("AI bandit nameplate canvas sortingOrder must be at least 1000.");
            }

            RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
            if (canvasRect != null && canvasRect.localScale.sqrMagnitude <= 0.000001f)
            {
                failures.Add("AI bandit nameplate canvas scale must not be zero.");
            }

            Transform backgroundTransform = canvasRoot.Find(CCS_AIConstants.NameplateHealthBackgroundObjectName);
            Transform fillTransform = backgroundTransform != null
                ? backgroundTransform.Find(CCS_AIConstants.NameplateHealthFillObjectName)
                : null;
            Transform nameTransform = canvasRoot.Find(CCS_AIConstants.NameplateNameTextObjectName);
            if (backgroundTransform == null)
            {
                failures.Add("AI bandit nameplate must contain HealthBar_Background.");
            }

            if (fillTransform == null)
            {
                failures.Add("AI bandit nameplate must contain HealthBar_Fill.");
            }

            if (nameTransform == null)
            {
                failures.Add("AI bandit nameplate must contain NameText.");
            }

            if (backgroundTransform != null && nameTransform != null)
            {
                RectTransform healthRect = backgroundTransform.GetComponent<RectTransform>();
                RectTransform nameRect = nameTransform.GetComponent<RectTransform>();
                if (healthRect != null && nameRect != null && healthRect.anchoredPosition.y <= nameRect.anchoredPosition.y)
                {
                    failures.Add("AI bandit health bar must be above AI_Bandit name text.");
                }
            }

            if (fillTransform != null)
            {
                RectTransform fillRect = fillTransform.GetComponent<RectTransform>();
                if (fillRect != null && fillRect.anchorMax.x <= 0.01f)
                {
                    failures.Add("AI bandit health fill must start visible/full at spawn.");
                }
            }

            TMP_Text nameText = nameTransform != null ? nameTransform.GetComponent<TMP_Text>() : null;
            if (nameText != null && nameText.text != CCS_AIConstants.AIBanditLabel)
            {
                failures.Add("AI bandit nameplate text must read AI_Bandit.");
            }

            if (nameText != null && !nameText.gameObject.activeSelf)
            {
                failures.Add("AI bandit nameplate text must be active on prefab.");
            }

            TMP_Text[] allTexts = prefab.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < allTexts.Length; i++)
            {
                TMP_Text text = allTexts[i];
                if (text == null)
                {
                    continue;
                }

                if (text.name == "PlayerNameText" || text.text == "Player")
                {
                    failures.Add("AI bandit prefab must not inherit player nameplate text.");
                    break;
                }
            }

            CCS_AIBanditProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath);
            if (profile != null)
            {
                float spawnDistance = Mathf.Sqrt(
                    (profile.SpawnSideOffset * profile.SpawnSideOffset)
                    + (profile.SpawnDistanceFromPlayer * profile.SpawnDistanceFromPlayer));
                if (spawnDistance <= profile.AttackRange)
                {
                    failures.Add("AI default spawn distance must be greater than attackRange.");
                }
            }

            CCS_AIBanditDeathHandler deathHandler = prefab.GetComponent<CCS_AIBanditDeathHandler>();
            CCS_RagdollController ragdollController = prefab.GetComponent<CCS_RagdollController>();
            if (deathHandler != null && ragdollController == null)
            {
                failures.Add("AI bandit death handler requires CCS_RagdollController.");
            }

            if (ragdollController != null && ragdollController.IsRagdollActive)
            {
                failures.Add("AI bandit ragdoll must start disabled while alive.");
            }

            ValidateRuntimeNameplateInstantiation(prefab, failures);
            ValidateHealthBarFillUpdatesWithDamage(prefab, failures);
            ValidateAIMotorContinuousRepathContract(prefab, failures);
            ValidateRagdollKinematicVelocityContract(failures);
            CCS_SurvivalValidationResult animatorResult =
                CCS_CharacterControllerAnimationValidationUtility.ValidateAIBanditAnimatorUsesExpectedController(prefab);
            if (!animatorResult.IsSuccess)
            {
                failures.Add(animatorResult.Message);
            }
        }

        private static void ValidateBanditDamageHitbox(GameObject prefab, List<string> failures)
        {
            CCS_AIBanditDamageHitbox damageHitbox = prefab.GetComponentInChildren<CCS_AIBanditDamageHitbox>(true);
            if (damageHitbox == null)
            {
                failures.Add("AI bandit prefab must contain CCS_AIBanditDamageHitbox for player weapon hits.");
                return;
            }

            CapsuleCollider capsuleCollider = damageHitbox.CapsuleCollider;
            if (capsuleCollider == null)
            {
                capsuleCollider = damageHitbox.GetComponent<CapsuleCollider>();
            }

            if (capsuleCollider == null || !capsuleCollider.enabled)
            {
                failures.Add("AI bandit alive hitbox must contain an enabled CapsuleCollider.");
                return;
            }

            if (capsuleCollider.isTrigger)
            {
                failures.Add("AI bandit alive hitbox collider must not be trigger-only for weapon raycasts.");
            }

            CCS_NetworkHealth networkHealth = prefab.GetComponent<CCS_NetworkHealth>();
            if (networkHealth == null)
            {
                failures.Add("AI bandit prefab must contain CCS_NetworkHealth on root for hitbox damage routing.");
                return;
            }

            if (!capsuleCollider.transform.IsChildOf(prefab.transform))
            {
                failures.Add("AI bandit alive hitbox must be parented under the AI bandit prefab root.");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for hitbox damageable lookup validation.");
                return;
            }

            try
            {
                CapsuleCollider instanceCollider = instance.GetComponentInChildren<CCS_AIBanditDamageHitbox>(true)?.CapsuleCollider;
                if (instanceCollider == null)
                {
                    failures.Add("Instantiated AI bandit must contain alive hitbox collider.");
                    return;
                }

                if (!CCS_DamageableLookupUtility.TryResolveDamageable(instanceCollider, out CCS_IDamageable damageable)
                    || damageable == null
                    || !ReferenceEquals(damageable, instance.GetComponent<CCS_NetworkHealth>()))
                {
                    failures.Add("AI bandit hitbox collider must resolve to root CCS_NetworkHealth via parent lookup.");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidatePlayerCanDamageAiBandit(GameObject prefab, List<string> failures)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for player damage validation.");
                return;
            }

            try
            {
                CCS_NetworkHealth networkHealth = instance.GetComponent<CCS_NetworkHealth>();
                CCS_AIBanditNameplate nameplate = instance.GetComponent<CCS_AIBanditNameplate>();
                CCS_AIBanditDamageHitbox damageHitbox = instance.GetComponentInChildren<CCS_AIBanditDamageHitbox>(true);
                if (networkHealth == null || nameplate == null || damageHitbox == null)
                {
                    failures.Add("Player damage validation requires AI root health, nameplate, and damage hitbox.");
                    return;
                }

                CapsuleCollider hitCollider = damageHitbox.CapsuleCollider;
                if (hitCollider == null)
                {
                    failures.Add("Player damage validation could not resolve AI alive hitbox collider.");
                    return;
                }

                CCS_AttributeContainer attributeContainer = instance.GetComponent<CCS_AttributeContainer>();
                if (attributeContainer != null)
                {
                    attributeContainer.InitializeFromDefinitions();
                }

                float maxHealth = networkHealth.MaxHealth;
                if (!Mathf.Approximately(maxHealth, 100f))
                {
                    failures.Add("Player damage validation expects AI max health of 100 (was " + maxHealth + ").");
                    return;
                }

                if (!CCS_DamageableLookupUtility.TryResolveDamageable(hitCollider, out CCS_IDamageable damageable)
                    || damageable == null
                    || !ReferenceEquals(damageable, networkHealth))
                {
                    failures.Add("Player weapon hitscan parent lookup must resolve AI hitbox to root CCS_NetworkHealth.");
                    return;
                }

                Vector3 rayOrigin = hitCollider.bounds.center + (instance.transform.forward * 3f) + (Vector3.up * 0.5f);
                Vector3 rayDirection = (hitCollider.bounds.center - rayOrigin).normalized;
                if (!Physics.Raycast(rayOrigin, rayDirection, out RaycastHit rayHit, 8f, ~0, QueryTriggerInteraction.Ignore)
                    || rayHit.collider != hitCollider)
                {
                    failures.Add("Simulated player revolver ray must hit AI alive hitbox collider.");
                    return;
                }

                if (!CCS_DamageableLookupUtility.TryResolveDamageable(rayHit.collider, out CCS_IDamageable rayDamageable)
                    || rayDamageable == null
                    || !ReferenceEquals(rayDamageable, networkHealth))
                {
                    failures.Add("Raycast hit on AI collider must resolve damageable on AI root.");
                    return;
                }

                CCS_DamageInfo firstShot = new CCS_DamageInfo(
                    20f,
                    rayHit.point,
                    -rayHit.normal,
                    CCS_DamageSourceType.RevolverShot,
                    sourceObject: null,
                    sourceNetworkObjectId: 0ul);
                if (!networkHealth.ApplyDamage(firstShot))
                {
                    failures.Add("Player damage route could not reduce AI health from 100 to 80.");
                    return;
                }

                if (networkHealth.IsDead || !Mathf.Approximately(networkHealth.CurrentHealth, 80f))
                {
                    failures.Add(
                        "Player damage route must leave AI alive at 80 health (current="
                        + networkHealth.CurrentHealth.ToString("0.##")
                        + ", dead="
                        + networkHealth.IsDead
                        + ").");
                    return;
                }

                nameplate.EnsureRuntimeNameplate();
                nameplate.RefreshHealthDisplayFromNetworkHealth();
                RectTransform fillRect = ResolveHealthFillTransform(instance)?.GetComponent<RectTransform>();
                if (fillRect == null || fillRect.anchorMin.x < 0.15f || fillRect.anchorMin.x > 0.25f)
                {
                    failures.Add(
                        "AI health bar fill must decrease to about 80% after player damage (anchorMin.x="
                        + (fillRect != null ? fillRect.anchorMin.x.ToString("0.00") : "null")
                        + ").");
                    return;
                }

                CCS_DamageInfo lethalDamage = new CCS_DamageInfo(
                    80f,
                    rayHit.point,
                    -rayHit.normal,
                    CCS_DamageSourceType.RevolverShot,
                    sourceObject: null,
                    sourceNetworkObjectId: 0ul);
                if (!networkHealth.ApplyDamage(lethalDamage))
                {
                    failures.Add("Player damage route could not reduce AI health from 80 to 0.");
                    return;
                }

                if (!networkHealth.IsDead)
                {
                    failures.Add("Player damage route must kill AI when health reaches 0.");
                    return;
                }

                CCS_AIBanditDeathHandler deathHandler = instance.GetComponent<CCS_AIBanditDeathHandler>();
                if (deathHandler == null)
                {
                    failures.Add("AI bandit must contain CCS_AIBanditDeathHandler for lethal player damage validation.");
                    return;
                }

                deathHandler.ForceDeathHandling();
                if (!deathHandler.DeathStarted)
                {
                    failures.Add("AI death handler must start once when health reaches 0 from player damage.");
                    return;
                }

                nameplate.RefreshHealthDisplayFromNetworkHealth();
                fillRect = ResolveHealthFillTransform(instance)?.GetComponent<RectTransform>();
                if (fillRect == null || fillRect.anchorMin.x < 0.99f)
                {
                    failures.Add(
                        "AI health bar fill must reach 0% after lethal player damage (anchorMin.x="
                        + (fillRect != null ? fillRect.anchorMin.x.ToString("0.00") : "null")
                        + ").");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidatePlayerWeaponHitMaskIncludesAiLayer(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_AIBanditDamageHitbox damageHitbox = prefab.GetComponentInChildren<CCS_AIBanditDamageHitbox>(true);
            CapsuleCollider hitCollider = damageHitbox != null ? damageHitbox.CapsuleCollider : null;
            if (hitCollider == null)
            {
                return;
            }

            int aiLayerMask = 1 << hitCollider.gameObject.layer;
            CCS_RevolverDefinition revolverDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_RevolverDefinition>(CCS_WeaponsConstants.RevolverDefinitionProfilePath);
            if (revolverDefinition == null)
            {
                failures.Add("Missing default revolver definition for AI hit mask validation.");
                return;
            }

            int hitMaskValue = revolverDefinition.HitMask.value;
            if ((hitMaskValue & aiLayerMask) == 0)
            {
                failures.Add(
                    "Player weapon hitMask must include AI alive hitbox layer (layer="
                    + hitCollider.gameObject.layer
                    + ", mask="
                    + hitMaskValue
                    + ").");
            }
        }

        private static void ValidateHealthBarFillDirection(GameObject prefab, List<string> failures)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for health bar direction validation.");
                return;
            }

            try
            {
                CCS_AIBanditNameplate nameplate = instance.GetComponent<CCS_AIBanditNameplate>();
                if (nameplate == null)
                {
                    failures.Add("AI bandit must contain CCS_AIBanditNameplate for fill direction validation.");
                    return;
                }

                nameplate.EnsureRuntimeNameplate();
                ValidateHealthBarNoNegativeMirroring(instance, failures);
                if (failures.Count > 0)
                {
                    return;
                }

                float[] percents = { 1f, 0.6f, 0.2f, 0f };
                for (int i = 0; i < percents.Length; i++)
                {
                    float percent = percents[i];
                    nameplate.SetHealthPercent(percent);
                    if (!TryResolveHealthBarFill(instance, out RectTransform fillRect, out Image fillImage))
                    {
                        failures.Add("AI health bar fill reference must be assigned for direction validation.");
                        return;
                    }

                    if (!Mathf.Approximately(fillRect.anchorMax.x, 1f))
                    {
                        failures.Add("AI health bar fill anchorMax.x must stay at 1 for right-anchored drain.");
                        return;
                    }

                    if (!Mathf.Approximately(fillRect.pivot.x, 1f))
                    {
                        failures.Add("AI health bar fill pivot.x must stay at 1 for right-anchored drain.");
                        return;
                    }

                    float expectedAnchorMinX = 1f - percent;
                    if (Mathf.Abs(fillRect.anchorMin.x - expectedAnchorMinX) > 0.05f)
                    {
                        failures.Add(
                            "AI health bar fill anchorMin.x must match health percent "
                            + percent.ToString("0.0")
                            + " (expected "
                            + expectedAnchorMinX.ToString("0.00")
                            + ", was "
                            + fillRect.anchorMin.x.ToString("0.00")
                            + ").");
                        return;
                    }

                    if (fillImage.type != Image.Type.Filled
                        || fillImage.fillMethod != Image.FillMethod.Horizontal
                        || fillImage.fillOrigin != (int)Image.OriginHorizontal.Right)
                    {
                        failures.Add(
                            "AI health bar fill must use Image Filled Horizontal with fillOrigin Right.");
                        return;
                    }

                    if (Mathf.Abs(fillImage.fillAmount - percent) > 0.05f)
                    {
                        failures.Add(
                            "AI health bar fillAmount must match health percent "
                            + percent.ToString("0.0")
                            + " (was "
                            + fillImage.fillAmount.ToString("0.00")
                            + ").");
                        return;
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidateAiDeathDespawn(GameObject prefab, List<string> failures)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for death despawn validation.");
                return;
            }

            try
            {
                CCS_NetworkHealth networkHealth = instance.GetComponent<CCS_NetworkHealth>();
                CCS_AIBanditDeathHandler deathHandler = instance.GetComponent<CCS_AIBanditDeathHandler>();
                if (networkHealth == null || deathHandler == null)
                {
                    failures.Add("AI death validation requires CCS_NetworkHealth and CCS_AIBanditDeathHandler.");
                    return;
                }

                CCS_AttributeContainer attributeContainer = instance.GetComponent<CCS_AttributeContainer>();
                if (attributeContainer != null)
                {
                    attributeContainer.InitializeFromDefinitions();
                }

                CCS_DamageInfo lethalDamage = new CCS_DamageInfo(
                    networkHealth.MaxHealth,
                    instance.transform.position,
                    Vector3.forward,
                    CCS_DamageSourceType.RevolverShot,
                    sourceObject: null,
                    sourceNetworkObjectId: 0ul);
                if (!networkHealth.ApplyDamage(lethalDamage) || !networkHealth.IsDead)
                {
                    failures.Add("AI death validation could not reduce health to 0.");
                    return;
                }

                deathHandler.ForceDeathHandling();
                if (!deathHandler.DeathStarted)
                {
                    failures.Add("AI death handler deathStarted must become true when health reaches 0.");
                    return;
                }

                deathHandler.ForceDeathForValidation(immediateRemoval: true);
                if (instance != null)
                {
                    failures.Add("Solo AI must be removed from scene when ForceDeathForValidation immediate removal runs.");
                }
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ValidateHealthBarFillUpdatesWithDamage(GameObject prefab, List<string> failures)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for health bar damage validation.");
                return;
            }

            try
            {
                CCS_NetworkHealth networkHealth = instance.GetComponent<CCS_NetworkHealth>();
                CCS_AIBanditNameplate nameplate = instance.GetComponent<CCS_AIBanditNameplate>();
                if (networkHealth == null)
                {
                    failures.Add("AI bandit root must contain CCS_NetworkHealth for health bar validation.");
                    return;
                }

                if (nameplate == null)
                {
                    failures.Add("AI bandit root must contain CCS_AIBanditNameplate for health bar validation.");
                    return;
                }

                if (networkHealth.transform != instance.transform)
                {
                    failures.Add("AI nameplate health must bind to CCS_NetworkHealth on the AI bandit root.");
                    return;
                }

                CCS_AttributeContainer attributeContainer = instance.GetComponent<CCS_AttributeContainer>();
                if (attributeContainer != null)
                {
                    attributeContainer.InitializeFromDefinitions();
                }

                nameplate.EnsureRuntimeNameplate();
                nameplate.SetHealthPercent(1f);

                Transform fillTransform = ResolveHealthFillTransform(instance);
                RectTransform fillRect = fillTransform != null ? fillTransform.GetComponent<RectTransform>() : null;
                Image fillImage = fillTransform != null ? fillTransform.GetComponent<Image>() : null;
                if (fillRect == null || fillImage == null || fillRect.anchorMin.x > 0.01f || !Mathf.Approximately(fillRect.anchorMax.x, 1f))
                {
                    failures.Add("AI health bar fill must start at 100% before simulated damage.");
                    return;
                }

                if (!Mathf.Approximately(fillRect.pivot.x, 1f))
                {
                    failures.Add("AI health bar fill must be right-anchored before simulated damage.");
                    return;
                }

                float maxHealth = networkHealth.MaxHealth;
                CCS_DamageInfo firstDamage = new CCS_DamageInfo(
                    maxHealth - 60f,
                    instance.transform.position,
                    Vector3.forward,
                    CCS_DamageSourceType.RevolverShot,
                    sourceObject: null,
                    sourceNetworkObjectId: 0ul);
                if (!networkHealth.ApplyDamage(firstDamage))
                {
                    failures.Add("AI health bar validation could not apply simulated damage to 60.");
                    return;
                }

                float expectedPercent = Mathf.Clamp01(
                    networkHealth.CurrentHealth / Mathf.Max(1f, networkHealth.MaxHealth));
                nameplate.SetHealthPercent(expectedPercent);
                fillRect = ResolveHealthFillTransform(instance)?.GetComponent<RectTransform>();
                if (fillRect == null)
                {
                    failures.Add("AI health bar fill could not be resolved after simulated damage.");
                    return;
                }

                if (fillRect.anchorMin.x < 0.35f || fillRect.anchorMin.x > 0.45f)
                {
                    failures.Add(
                        "AI health bar fill must decrease to about 60% after damage (anchorMin.x="
                        + fillRect.anchorMin.x.ToString("0.00")
                        + ", current="
                        + networkHealth.CurrentHealth.ToString("0.##")
                        + ", max="
                        + networkHealth.MaxHealth.ToString("0.##")
                        + ", percent="
                        + expectedPercent.ToString("0.00")
                        + ").");
                    return;
                }

                nameplate.RefreshHealthDisplayFromNetworkHealth();
                fillRect = ResolveHealthFillTransform(instance)?.GetComponent<RectTransform>();
                if (fillRect == null
                    || fillRect.anchorMin.x < 0.35f
                    || fillRect.anchorMin.x > 0.45f)
                {
                    failures.Add(
                        "AI nameplate RefreshHealthDisplayFromNetworkHealth must keep fill at about 60% (anchorMin.x="
                        + (fillRect != null ? fillRect.anchorMin.x.ToString("0.00") : "null")
                        + ").");
                    return;
                }

                CCS_DamageInfo secondDamage = new CCS_DamageInfo(
                    60f,
                    instance.transform.position,
                    Vector3.forward,
                    CCS_DamageSourceType.RevolverShot,
                    sourceObject: null,
                    sourceNetworkObjectId: 0ul);
                if (!networkHealth.ApplyDamage(secondDamage))
                {
                    failures.Add("AI health bar validation could not apply simulated damage to 0.");
                    return;
                }

                nameplate.RefreshHealthDisplayFromNetworkHealth();
                fillRect = ResolveHealthFillTransform(instance)?.GetComponent<RectTransform>();
                if (fillRect == null || fillRect.anchorMin.x < 0.99f)
                {
                    failures.Add(
                        "AI health bar fill must reach 0% after lethal damage (anchorMin.x="
                        + fillRect.anchorMin.x.ToString("0.00")
                        + ").");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static Transform ResolveHealthFillTransform(GameObject instance)
        {
            Transform anchor = instance.transform.Find(CCS_AIConstants.NameplateAnchorObjectName);
            Transform nameplateRoot = anchor != null
                ? anchor.Find(CCS_AIConstants.NameplateRootObjectName)
                : null;
            Transform canvasRoot = nameplateRoot != null
                ? nameplateRoot.Find(CCS_AIConstants.NameplateCanvasObjectName)
                : null;
            Transform background = canvasRoot != null
                ? canvasRoot.Find(CCS_AIConstants.NameplateHealthBackgroundObjectName)
                : null;
            return background != null
                ? background.Find(CCS_AIConstants.NameplateHealthFillObjectName)
                : null;
        }

        private static bool TryResolveHealthBarFill(
            GameObject instance,
            out RectTransform fillRect,
            out Image fillImage)
        {
            Transform fillTransform = ResolveHealthFillTransform(instance);
            fillRect = fillTransform != null ? fillTransform.GetComponent<RectTransform>() : null;
            fillImage = fillTransform != null ? fillTransform.GetComponent<Image>() : null;
            return fillRect != null && fillImage != null;
        }

        private static void ValidateHealthBarNoNegativeMirroring(GameObject instance, List<string> failures)
        {
            Transform anchor = instance.transform.Find(CCS_AIConstants.NameplateAnchorObjectName);
            Transform nameplateRoot = anchor != null
                ? anchor.Find(CCS_AIConstants.NameplateRootObjectName)
                : null;
            Transform canvasRoot = nameplateRoot != null
                ? nameplateRoot.Find(CCS_AIConstants.NameplateCanvasObjectName)
                : null;
            Transform fillTransform = ResolveHealthFillTransform(instance);

            ValidateTransformScaleNotMirrored(anchor, "nameplate anchor", failures);
            ValidateTransformScaleNotMirrored(nameplateRoot, "nameplate root", failures);
            ValidateTransformScaleNotMirrored(canvasRoot, "nameplate canvas", failures);
            ValidateTransformScaleNotMirrored(fillTransform, "health bar fill", failures);
        }

        private static void ValidateTransformScaleNotMirrored(
            Transform transformReference,
            string label,
            List<string> failures)
        {
            if (transformReference == null)
            {
                return;
            }

            if (transformReference.localScale.x < 0f)
            {
                failures.Add("AI " + label + " must not use negative localScale.x (mirrors health bar direction).");
            }
        }

        private static void ValidateAIMotorContinuousRepathContract(GameObject prefab, List<string> failures)
        {
            CCS_AIBanditProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath);
            if (profile == null)
            {
                failures.Add("Missing AI bandit profile for repath validation.");
                return;
            }

            if (profile.RepathIntervalSeconds > 0.25f)
            {
                failures.Add("AI profile repathIntervalSeconds must be <= 0.25 for continuous stalking.");
            }

            if (profile.DestinationUpdateThreshold > 0.75f)
            {
                failures.Add("AI profile destinationUpdateThreshold must be <= 0.75 for moving target repaths.");
            }

            if (profile.MovementStopDistance >= profile.AttackRange)
            {
                failures.Add("AI movementStopDistance must be smaller than attackRange so bandits keep chasing in range.");
            }

            string brainPath = CCS_AIConstants.ModuleRootPath + "/Runtime/Combat/CCS_AIBanditBrain.cs";
            string motorPath = CCS_AIConstants.ModuleRootPath + "/Runtime/Movement/CCS_AIMotorController.cs";
            if (!File.Exists(brainPath) || !File.Exists(motorPath))
            {
                failures.Add("Missing AI brain or motor source for repath validation.");
                return;
            }

            string brainSource = File.ReadAllText(brainPath);
            string motorSource = File.ReadAllText(motorPath);
            if (!brainSource.Contains("ShouldExitCombatForRangeOrSight"))
            {
                failures.Add("CCS_AIBanditBrain must exit combat when target leaves attack range or line of sight.");
            }

            if (!brainSource.Contains("TryRefreshLivingTarget"))
            {
                failures.Add("CCS_AIBanditBrain must continuously refresh the living player target.");
            }

            if (!brainSource.Contains("ApplyStalkingPriority"))
            {
                failures.Add("CCS_AIBanditBrain must continuously stalk via ApplyStalkingPriority.");
            }

            if (!brainSource.Contains("ChaseTarget"))
            {
                failures.Add("CCS_AIBanditBrain must chase using CCS_AIMotorController.ChaseTarget.");
            }

            if (!motorSource.Contains("TryResolveReachableDestination"))
            {
                failures.Add("CCS_AIMotorController must resolve reachable NavMesh destinations near targets.");
            }

            if (!motorSource.Contains("debugStalking"))
            {
                failures.Add("CCS_AIMotorController must expose debugStalking for stalking diagnostics.");
            }

            if (!motorSource.Contains("isPathStale") || !motorSource.Contains("PathInvalid"))
            {
                failures.Add("CCS_AIMotorController must repath on stale or invalid NavMesh paths.");
            }

            if (!motorSource.Contains("destinationUpdateThreshold"))
            {
                failures.Add("CCS_AIMotorController must support destinationUpdateThreshold repathing.");
            }

            if (!brainSource.Contains("NavMesh.SamplePosition"))
            {
                failures.Add("CCS_AIBanditBrain must sample chase targets onto NavMesh before MoveTowards.");
            }

            if (!motorSource.Contains("drawPathGizmos") || !motorSource.Contains("debugAiPathfinding"))
            {
                failures.Add("CCS_AIMotorController must expose pathfinding debug toggles.");
            }

            Scene scene = EditorSceneManager.OpenScene(
                "Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity",
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add("Master Test scene could not be opened for AI motor repath validation.");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for motor repath validation.");
                return;
            }

            try
            {
                GameObject hostSpawn = GameObject.Find("TP_Spawn_Host");
                Vector3 spawnOrigin = hostSpawn != null
                    ? hostSpawn.transform.position
                    : Vector3.zero;
                if (!NavMesh.SamplePosition(spawnOrigin, out NavMeshHit spawnHit, 12f, NavMesh.AllAreas))
                {
                    failures.Add("Master Test NavMesh must include a spawn point for AI motor repath validation.");
                    return;
                }

                UnityEngine.CharacterController characterController = instance.GetComponent<UnityEngine.CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = false;
                }

                instance.transform.position = spawnHit.position;
                NavMeshAgent navMeshAgent = instance.GetComponent<NavMeshAgent>();
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = true;
                    navMeshAgent.Warp(spawnHit.position);
                }

                CCS_AIMotorController motor = instance.GetComponent<CCS_AIMotorController>();
                if (motor == null)
                {
                    failures.Add("AI bandit prefab must contain CCS_AIMotorController for repath validation.");
                    return;
                }

                Vector3 firstTarget = spawnHit.position + new Vector3(8f, 0f, 0f);
                Vector3 secondTarget = spawnHit.position + new Vector3(-8f, 0f, 6f);
                motor.ChaseTarget(firstTarget, profile);
                Vector3 firstDestination = motor.LastDestination;

                motor.ChaseTarget(secondTarget, profile);
                Vector3 secondDestination = motor.LastDestination;

                if ((secondDestination - firstDestination).sqrMagnitude < 4f)
                {
                    failures.Add(
                        "AI motor must update NavMesh destination when target moves more than 2m (first="
                        + firstDestination
                        + ", second="
                        + secondDestination
                        + ").");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidateRuntimeNameplateInstantiation(GameObject prefab, List<string> failures)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                failures.Add("AI bandit prefab could not be instantiated for runtime nameplate validation.");
                return;
            }

            try
            {
                CCS_AIBanditNameplate nameplate = instance.GetComponent<CCS_AIBanditNameplate>();
                if (nameplate == null)
                {
                    failures.Add("Instantiated AI bandit must contain CCS_AIBanditNameplate for runtime validation.");
                    return;
                }

                nameplate.EnsureRuntimeNameplate();

                Transform anchor = instance.transform.Find(CCS_AIConstants.NameplateAnchorObjectName);
                Transform nameplateRoot = anchor != null
                    ? anchor.Find(CCS_AIConstants.NameplateRootObjectName)
                    : null;
                Transform canvasRoot = nameplateRoot != null
                    ? nameplateRoot.Find(CCS_AIConstants.NameplateCanvasObjectName)
                    : null;
                Transform background = canvasRoot != null
                    ? canvasRoot.Find(CCS_AIConstants.NameplateHealthBackgroundObjectName)
                    : null;
                Transform fill = background != null
                    ? background.Find(CCS_AIConstants.NameplateHealthFillObjectName)
                    : null;
                Transform nameTextTransform = canvasRoot != null
                    ? canvasRoot.Find(CCS_AIConstants.NameplateNameTextObjectName)
                    : null;

                if (anchor == null || nameplateRoot == null || canvasRoot == null || background == null || fill == null
                    || nameTextTransform == null)
                {
                    failures.Add("EnsureRuntimeNameplate must create UIAnchor_Nameplate, canvas, health bar, and name text.");
                    return;
                }

                if (IsTransformUnderNamedAncestor(nameplateRoot, "VisualRoot")
                    || IsTransformUnderNamedAncestor(nameplateRoot, "Armature"))
                {
                    failures.Add("Runtime nameplate must not be parented under VisualRoot or Armature.");
                }

                Canvas canvas = canvasRoot.GetComponent<Canvas>();
                if (canvas == null || !canvas.enabled || canvas.renderMode != RenderMode.WorldSpace)
                {
                    failures.Add("Runtime nameplate canvas must be enabled and world-space.");
                }

                RectTransform fillRect = fill.GetComponent<RectTransform>();
                if (fillRect == null || fillRect.anchorMax.x <= 0.01f)
                {
                    failures.Add("Runtime nameplate health fill must start visible/full.");
                }

                TMP_Text nameText = nameTextTransform.GetComponent<TMP_Text>();
                if (nameText == null || nameText.text != CCS_AIConstants.AIBanditLabel)
                {
                    failures.Add("Runtime nameplate text must read AI_Bandit.");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidateRagdollKinematicVelocityContract(List<string> failures)
        {
            string ragdollSourcePath = CCS_AIConstants.ModuleRootPath + "/Runtime/Components/CCS_RagdollController.cs";
            if (!File.Exists(ragdollSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(ragdollSourcePath);
            if (source.Contains("body.isKinematic = !active")
                && source.Contains("body.linearVelocity = Vector3.zero")
                && source.IndexOf("body.isKinematic = !active", System.StringComparison.Ordinal)
                < source.IndexOf("body.linearVelocity = Vector3.zero", System.StringComparison.Ordinal))
            {
                failures.Add(
                    "CCS_RagdollController must not assign velocity while ragdoll bodies are still kinematic.");
            }
        }

        private static bool IsTransformUnderNamedAncestor(Transform transformReference, string ancestorName)
        {
            Transform current = transformReference != null ? transformReference.parent : null;
            while (current != null)
            {
                if (current.name == ancestorName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static void ValidateMasterTestNavigationScene(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity",
                OpenSceneMode.Additive);
            if (!scene.IsValid())
            {
                failures.Add("Master Test scene could not be opened for navigation validation.");
                return;
            }

            GameObject navigationRoot = GameObject.Find(CCS_AIConstants.NavigationRootObjectName);
            if (navigationRoot == null)
            {
                failures.Add("Master Test scene must contain CCS_AINavigationRoot.");
            }
            else
            {
                Transform surface = navigationRoot.transform.Find(CCS_AIConstants.NavigationSurfaceObjectName);
                NavMeshSurface navMeshSurface = surface != null ? surface.GetComponent<NavMeshSurface>() : null;
                if (surface == null || navMeshSurface == null)
                {
                    failures.Add("Master Test scene must contain NavMeshSurface_MasterTest with NavMeshSurface.");
                }
                else if (navMeshSurface.navMeshData == null)
                {
                    failures.Add("Master Test NavMeshSurface must have baked NavMesh data.");
                }
                else
                {
                    NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
                    if (triangulation.vertices == null || triangulation.vertices.Length == 0)
                    {
                        failures.Add("Master Test scene must contain walkable NavMesh polygons.");
                    }
                    else
                    {
                        GameObject hostSpawn = GameObject.Find("TP_Spawn_Host");
                        Vector3 sampleOrigin = hostSpawn != null
                            ? hostSpawn.transform.position
                            : navigationRoot.transform.position;
                        if (!NavMesh.SamplePosition(
                            sampleOrigin,
                            out NavMeshHit _,
                            12f,
                            NavMesh.AllAreas))
                        {
                            failures.Add("Master Test NavMesh must include walkable area near player/AI spawn.");
                        }
                        else
                        {
                            ValidateMasterTestNavigationPaths(failures);
                        }
                    }
                }
            }

            EditorSceneManager.CloseScene(scene, true);
        }

        private static void ValidateMasterTestNavigationPaths(List<string> failures)
        {
            if (!CCS_AINavigationProbePointBuilder.TryResolveProbePosition(
                CCS_AINavigationProbeId.OutsideSpawn,
                out Vector3 outsideSpawn))
            {
                failures.Add("Master Test navigation probes must include OutsideSpawnPoint.");
                return;
            }

            if (!CCS_AINavigationProbePointBuilder.TryResolveProbePosition(
                CCS_AINavigationProbeId.InsideBuilding,
                out Vector3 insideBuilding))
            {
                failures.Add("Master Test navigation probes must include InsideBuildingPoint.");
                return;
            }

            Vector3 aiSpawn = outsideSpawn + new Vector3(CCS_AIConstants.DefaultSpawnSideOffset, 0f, CCS_AIConstants.DefaultSpawnDistanceFromPlayer);
            if (NavMesh.SamplePosition(aiSpawn, out NavMeshHit aiSpawnHit, 12f, NavMesh.AllAreas))
            {
                aiSpawn = aiSpawnHit.position;
            }

            ValidateNavMeshPathComplete(failures, aiSpawn, insideBuilding, "AI spawn to inside building");
            ValidateNavMeshPathComplete(failures, outsideSpawn, insideBuilding, "outside spawn to inside building");

            if (CCS_AINavigationProbePointBuilder.TryResolveProbePosition(
                CCS_AINavigationProbeId.BuildingDoor,
                out Vector3 buildingDoor))
            {
                ValidateNavMeshPathComplete(failures, outsideSpawn, buildingDoor, "outside spawn to building door");
                ValidateNavMeshPathComplete(failures, buildingDoor, insideBuilding, "building door to inside building");
            }

            if (CCS_AINavigationProbePointBuilder.TryResolveProbePosition(
                CCS_AINavigationProbeId.TopOfStairs,
                out Vector3 topOfStairs))
            {
                ValidateNavMeshPathComplete(failures, outsideSpawn, topOfStairs, "outside spawn to top of stairs");
            }

            if (CCS_AINavigationProbePointBuilder.TryResolveProbePosition(
                CCS_AINavigationProbeId.RampTop,
                out Vector3 rampTop))
            {
                ValidateNavMeshPathComplete(failures, outsideSpawn, rampTop, "outside spawn to ramp top");
            }
        }

        private static void ValidateNavMeshPathComplete(
            List<string> failures,
            Vector3 start,
            Vector3 end,
            string label)
        {
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
            {
                failures.Add("NavMesh path calculation failed for " + label + ".");
                return;
            }

            if (path.status != NavMeshPathStatus.PathComplete)
            {
                failures.Add(
                    "NavMesh path must be PathComplete for "
                    + label
                    + " (status="
                    + path.status
                    + ").");
            }
        }

        private static void ValidateMasterTestHasNoAmbientAudio(List<string> failures)
        {
            string scenePath = "Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity";
            if (!File.Exists(scenePath))
            {
                return;
            }

            string sceneText = File.ReadAllText(scenePath);
            if (sceneText.Contains("m_Name: CCS_AmbientAudio")
                || sceneText.Contains("m_Name: CCS_HostingAmbientAudio"))
            {
                failures.Add("Master Test scene must not contain active CCS_AmbientAudio gameplay music.");
            }
        }

        private static void ValidateHostingSceneAmbientAudio(List<string> failures)
        {
            string scenePath = CCS_ProjectAudioConstants.MultiplayerHostingScenePath;
            if (!File.Exists(scenePath))
            {
                failures.Add("Missing multiplayer hosting scene for ambient audio validation.");
                return;
            }

            Scene hostingScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!hostingScene.IsValid())
            {
                failures.Add("Hosting scene could not be opened for ambient audio validation.");
                return;
            }

            GameObject ambientObject = GameObject.Find(CCS_ProjectAudioConstants.HostingAmbientAudioObjectName);
            if (ambientObject == null)
            {
                ambientObject = GameObject.Find("CCS_AmbientAudio");
            }

            if (ambientObject == null)
            {
                failures.Add("Hosting scene must contain CCS_HostingAmbientAudio playlist root.");
                EditorSceneManager.CloseScene(hostingScene, true);
                return;
            }

            AudioSource audioSource = ambientObject.GetComponent<AudioSource>();
            CCS_AmbientAudioPlaylist playlist = ambientObject.GetComponent<CCS_AmbientAudioPlaylist>();
            if (audioSource == null || playlist == null)
            {
                failures.Add("Hosting ambient audio object must contain AudioSource and CCS_AmbientAudioPlaylist.");
            }
            else
            {
                if (Mathf.Approximately(audioSource.volume, 0f) && Mathf.Approximately(playlist.Volume, 0f))
                {
                    failures.Add("Hosting ambient AudioSource volume must be greater than 0.");
                }

                if (audioSource.mute)
                {
                    failures.Add("Hosting ambient AudioSource mute must be false.");
                }

                SerializedObject serializedPlaylist = new SerializedObject(playlist);
                SerializedProperty playlistProperty = serializedPlaylist.FindProperty("playlist");
                SerializedProperty playOnStartProperty = serializedPlaylist.FindProperty("playOnStart");
                SerializedProperty repeatProperty = serializedPlaylist.FindProperty("repeatPlaylist");
                SerializedProperty playlistEnabledProperty = serializedPlaylist.FindProperty("playlistEnabled");
                SerializedProperty volumeProperty = serializedPlaylist.FindProperty("volume");

                if (playlistProperty == null || playlistProperty.arraySize != 2)
                {
                    failures.Add("Hosting ambient playlist must contain exactly 2 clips.");
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (playlistProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                        {
                            failures.Add("Hosting ambient playlist clip references must not be null.");
                            break;
                        }
                    }
                }

                if (playOnStartProperty != null && !playOnStartProperty.boolValue)
                {
                    failures.Add("Hosting ambient playlist playOnStart must be true.");
                }

                if (repeatProperty != null && !repeatProperty.boolValue)
                {
                    failures.Add("Hosting ambient playlist repeatPlaylist must be true.");
                }

                if (playlistEnabledProperty != null && !playlistEnabledProperty.boolValue)
                {
                    failures.Add("Hosting ambient playlist must be enabled.");
                }

                if (volumeProperty != null && volumeProperty.floatValue <= 0f)
                {
                    failures.Add("Hosting ambient playlist volume must be greater than 0.");
                }
            }

            EditorSceneManager.CloseScene(hostingScene, true);
        }

        private static void ValidateDamageReadinessContract(List<string> failures)
        {
            string interfacePath = "Assets/CCS/Modules/Attributes/Runtime/Data/CCS_IDamageable.cs";
            if (!File.Exists(interfacePath))
            {
                return;
            }

            string interfaceText = File.ReadAllText(interfacePath);
            if (!interfaceText.Contains("IsDamageReady"))
            {
                failures.Add("CCS_IDamageable must expose IsDamageReady for spawn-safe damage.");
            }

            string healthPath = "Assets/CCS/Modules/Attributes/Runtime/Components/CCS_NetworkHealth.cs";
            if (File.Exists(healthPath))
            {
                string healthText = File.ReadAllText(healthPath);
                if (healthText.Contains("replicatedHealth.Value = damageEvent.ResultingValue.Current")
                    && !healthText.Contains("ApplyDamageLocally"))
                {
                    // offline path must exist and not be the only writer before spawn
                }

                if (!healthText.Contains("offlineCurrentHealth"))
                {
                    failures.Add("CCS_NetworkHealth must use offline health fields outside NetworkVariable writes.");
                }
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }
    }
}
