using CCS.Modules.CharacterController;

using TMPro;

using UnityEditor;

using UnityEngine;

using UnityEngine.UI;



// =============================================================================

// SCRIPT: CCS_WeaponsTestPlayerPrefabBuilder

// CATEGORY: Modules / Weapons / Editor

// PURPOSE: Wires revolver controller, muzzle point, ownership loadout, and test HUD on the test player prefab.

// PLACEMENT: Editor utility invoked from Weapons validation and master test setup.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: v0.6.5 world-pickup-only scope. No holster/equipped sockets or gun visuals.

// =============================================================================



namespace CCS.Modules.Weapons.Editor

{

    public static class CCS_WeaponsTestPlayerPrefabBuilder

    {

        #region Public Methods



        public static bool EnsureTestPlayerWeaponWiring()

        {

            bool changed = EnsureTestPlayerWeaponWiring(CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath);

            if (changed)

            {

                AssetDatabase.SaveAssets();

                AssetDatabase.Refresh();

            }



            return changed;

        }



        public static bool EnsureTestPlayerWeaponWiring(string prefabPath)

        {

            CCS_WeaponsAssetBuilder.EnsureRevolverDefinitionAsset();



            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabRoot == null)

            {

                Debug.LogError("[Weapons Prefab Builder] Missing prefab: " + prefabPath);

                return false;

            }



            CCS_RevolverDefinition revolverDefinition = AssetDatabase.LoadAssetAtPath<CCS_RevolverDefinition>(

                CCS_WeaponsConstants.RevolverDefinitionProfilePath);

            if (revolverDefinition == null)

            {

                Debug.LogError(

                    "[Weapons Prefab Builder] Missing revolver definition: "

                    + CCS_WeaponsConstants.RevolverDefinitionProfilePath);

                PrefabUtility.UnloadPrefabContents(prefabRoot);

                return false;

            }



            bool changed = false;

            changed |= RemovePlayerWeaponVisualArtifacts(prefabRoot);

            changed |= EnsureRevolverController(prefabRoot, revolverDefinition);

            changed |= EnsureMuzzlePoint(prefabRoot);

            changed |= EnsurePlayerWeaponLoadout(prefabRoot);

            changed |= EnsurePlayerEquipmentVisualController(prefabRoot);

            changed |= EnsureWeaponHud(prefabRoot);

            changed |= EnsureMasterTestAimVisualComponents(prefabRoot);

            changed |= EnsureFireFeedback(prefabRoot);

            changed |= EnsureAimLocomotionWeaponGate(prefabRoot);



            if (changed)

            {

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

            }



            PrefabUtility.UnloadPrefabContents(prefabRoot);

            return changed;

        }



        #endregion



        #region Private Methods



        public static bool RemovePlayerWeaponVisualArtifacts(GameObject prefabRoot)

        {

            bool changed = false;

            changed |= RemoveMissingScriptsRecursive(prefabRoot);

            MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == "CCS_RevolverWeaponVisualFeedback")
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            CCS_FirstPersonAimCameraOverrideController fovOverride =
                prefabRoot.GetComponent<CCS_FirstPersonAimCameraOverrideController>();
            if (fovOverride != null)
            {
                Object.DestroyImmediate(fovOverride, true);
                changed = true;
            }



            changed |= DestroyDeepChildIfExists(prefabRoot.transform, "PF_CCS_RevolverM1879_Holstered_Instance");
            changed |= DestroyDeepChildIfExists(prefabRoot.transform, "PF_CCS_RevolverM1879_Equipped_Instance");
            changed |= DestroyDeepChildIfExists(prefabRoot.transform, "PF_CCS_RevolverM1879_Holstered");
            changed |= DestroyDeepChildIfExists(prefabRoot.transform, "PF_CCS_RevolverM1879_Equipped");

            changed |= DestroyDeepChildIfExists(
                prefabRoot.transform,
                CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName);
            changed |= DestroyDeepChildIfExists(
                prefabRoot.transform,
                CCS_EquipmentConstants.RuntimeHolsteredVisualObjectName);
            changed |= DestroyDeepChildIfExists(
                prefabRoot.transform,
                CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName);
            changed |= DestroyDeepChildIfExists(
                prefabRoot.transform,
                CCS_EquipmentConstants.RuntimeEquippedVisualObjectName);

            Transform holsterSocket = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.RevolverHolsterSocketName);

            if (holsterSocket != null)

            {

                Object.DestroyImmediate(holsterSocket.gameObject, true);

                changed = true;

            }



            Transform handSocket = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.RevolverHandSocketName);

            if (handSocket != null)

            {

                Object.DestroyImmediate(handSocket.gameObject, true);

                changed = true;

            }



            return changed;

        }



        private static bool RemoveMissingScriptsRecursive(GameObject root)
        {
            bool changed = false;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transforms[i].gameObject) > 0)
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool DestroyDeepChildIfExists(Transform root, string childName)

        {

            Transform match = FindDeepChild(root, childName);

            if (match == null)

            {

                return false;

            }



            Object.DestroyImmediate(match.gameObject, true);

            return true;

        }



        private static bool EnsureRevolverController(GameObject prefabRoot, CCS_RevolverDefinition revolverDefinition)

        {

            CCS_RevolverController controller = prefabRoot.GetComponent<CCS_RevolverController>();

            if (controller == null)

            {

                controller = prefabRoot.AddComponent<CCS_RevolverController>();

            }



            CCS_CharacterInputActionProvider inputProvider = prefabRoot.GetComponent<CCS_CharacterInputActionProvider>();

            Transform muzzlePoint = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);



            SerializedObject serializedController = new SerializedObject(controller);

            bool changed = SetObjectReference(serializedController, "revolverDefinition", revolverDefinition);

            changed |= SetObjectReference(serializedController, "inputProvider", inputProvider);

            changed |= SetObjectReference(serializedController, "muzzlePoint", muzzlePoint);



            CCS_CharacterAimLocomotionController aimLocomotion =

                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>();

            changed |= SetObjectReference(serializedController, "aimLocomotionController", aimLocomotion);



            if (changed)

            {

                serializedController.ApplyModifiedPropertiesWithoutUndo();

            }



            if (!controller.enabled)

            {

                controller.enabled = true;

                changed = true;

            }



            return changed;

        }



        private static bool EnsurePlayerWeaponLoadout(GameObject prefabRoot)

        {

            CCS_PlayerWeaponLoadout loadout = prefabRoot.GetComponent<CCS_PlayerWeaponLoadout>();

            if (loadout == null)

            {

                loadout = prefabRoot.AddComponent<CCS_PlayerWeaponLoadout>();

            }



            CCS_RevolverVisualDefinition visualDefinition = AssetDatabase.LoadAssetAtPath<CCS_RevolverVisualDefinition>(

                CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);

            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();



            SerializedObject serializedLoadout = new SerializedObject(loadout);

            bool changed = SetObjectReference(serializedLoadout, "revolverVisualDefinition", visualDefinition);

            changed |= SetObjectReference(serializedLoadout, "revolverController", revolverController);

            if (changed)

            {

                serializedLoadout.ApplyModifiedPropertiesWithoutUndo();

            }



            if (!loadout.enabled)

            {

                loadout.enabled = true;

                changed = true;

            }



            return changed;

        }



        private static bool EnsurePlayerEquipmentVisualController(GameObject prefabRoot)

        {

            CCS_PlayerEquipmentVisualController controller =

                prefabRoot.GetComponent<CCS_PlayerEquipmentVisualController>();

            if (controller == null)

            {

                controller = prefabRoot.AddComponent<CCS_PlayerEquipmentVisualController>();

            }



            CCS_EquipmentSocketRegistry socketRegistry = prefabRoot.GetComponent<CCS_EquipmentSocketRegistry>();

            CCS_PlayerWeaponLoadout loadout = prefabRoot.GetComponent<CCS_PlayerWeaponLoadout>();

            CCS_CharacterAimLocomotionController aimLocomotion =

                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>();

            CCS_WeaponAttachmentFitProfile hipFit = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(

                CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath);

            CCS_WeaponAttachmentFitProfile handFit = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(

                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);

            GameObject visualOnlyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(

                CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath);



            SerializedObject serializedController = new SerializedObject(controller);

            bool changed = SetObjectReference(serializedController, "equipmentSocketRegistry", socketRegistry);

            changed |= SetObjectReference(serializedController, "playerWeaponLoadout", loadout);

            changed |= SetObjectReference(serializedController, "aimLocomotionController", aimLocomotion);

            changed |= SetObjectReference(serializedController, "rightHipHolsterFitProfile", hipFit);

            changed |= SetObjectReference(serializedController, "rightHandEquippedFitProfile", handFit);

            changed |= SetObjectReference(serializedController, "revolverVisualOnlyPrefab", visualOnlyPrefab);

            if (changed)

            {

                serializedController.ApplyModifiedPropertiesWithoutUndo();

            }



            if (!controller.enabled)

            {

                controller.enabled = true;

                changed = true;

            }



            return changed;

        }



        private static bool EnsureAimLocomotionWeaponGate(GameObject prefabRoot)

        {

            CCS_CharacterAimLocomotionController aimLocomotion =

                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>();

            CCS_WeaponCarryStateController carryController =

                prefabRoot.GetComponent<CCS_WeaponCarryStateController>();

            if (aimLocomotion == null || carryController == null)

            {

                return false;

            }



            SerializedObject serializedAimLocomotion = new SerializedObject(aimLocomotion);

            bool changed = SetObjectReference(serializedAimLocomotion, "weaponAimGateComponent", carryController);

            if (changed)

            {

                serializedAimLocomotion.ApplyModifiedPropertiesWithoutUndo();

            }



            return changed;

        }



        private static bool EnsureMuzzlePoint(GameObject prefabRoot)

        {

            bool changed = false;

            Transform existing = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);

            Transform muzzleParent = prefabRoot.transform;



            if (existing == null)

            {

                GameObject muzzleObject = new GameObject(CCS_WeaponsConstants.MuzzlePointObjectName);

                existing = muzzleObject.transform;

                existing.SetParent(muzzleParent, false);

                changed = true;

            }

            else if (existing.parent != muzzleParent)

            {

                existing.SetParent(muzzleParent, false);

                changed = true;

            }



            if (existing.localPosition != CCS_WeaponsConstants.MuzzlePointLocalPosition)

            {

                existing.localPosition = CCS_WeaponsConstants.MuzzlePointLocalPosition;

                changed = true;

            }



            if (existing.localRotation != Quaternion.identity)

            {

                existing.localRotation = Quaternion.identity;

                changed = true;

            }



            return changed;

        }



        private static bool EnsureWeaponHud(GameObject prefabRoot)

        {

            bool changed = false;

            Transform hudRootTransform = prefabRoot.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);

            GameObject hudRootObject;

            if (hudRootTransform == null)

            {

                hudRootObject = new GameObject(CCS_WeaponsConstants.WeaponHudRootName, typeof(RectTransform));

                hudRootObject.transform.SetParent(prefabRoot.transform, false);

                changed = true;

            }

            else

            {

                hudRootObject = hudRootTransform.gameObject;

            }



            if (!hudRootObject.activeSelf)

            {

                hudRootObject.SetActive(true);

                changed = true;

            }



            RectTransform hudRectTransform = hudRootObject.GetComponent<RectTransform>();

            if (hudRectTransform != null && hudRectTransform.localScale != Vector3.one)

            {

                hudRectTransform.localScale = Vector3.one;

                changed = true;

            }



            changed |= EnsureHudCanvas(hudRootObject);



            CCS_RevolverHudPresenter presenter = hudRootObject.GetComponent<CCS_RevolverHudPresenter>();

            if (presenter == null)

            {

                presenter = hudRootObject.AddComponent<CCS_RevolverHudPresenter>();

                changed = true;

            }



            TextMeshProUGUI hudText = EnsureHudText(hudRootObject.transform, ref changed);

            Image reticleImage = EnsureReticle(hudRootObject.transform, ref changed);

            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();



            SerializedObject serializedPresenter = new SerializedObject(presenter);

            bool presenterChanged = SetObjectReference(serializedPresenter, "revolverController", revolverController);

            presenterChanged |= SetObjectReference(serializedPresenter, "hudText", hudText);

            presenterChanged |= SetObjectReference(serializedPresenter, "reticleImage", reticleImage);



            if (presenterChanged)

            {

                serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

                changed = true;

            }



            return changed;

        }

        private static bool EnsureMasterTestAimVisualComponents(GameObject prefabRoot)
        {
            bool changed = false;
            Transform hudRoot = prefabRoot.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);
            Transform visualRoot = FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();
            CCS_RevolverHudPresenter hudPresenter = hudRoot != null
                ? hudRoot.GetComponent<CCS_RevolverHudPresenter>()
                : null;
            CCS_PlayerEquipmentVisualController equipmentVisual =
                prefabRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            CCS_CharacterAimLocomotionController aimLocomotion =
                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            Animator animator = visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
            Image reticleImage = hudRoot != null
                ? hudRoot.Find(CCS_WeaponsConstants.WeaponReticleObjectName)?.GetComponent<Image>()
                : null;

            if (hudRoot != null)
            {
                CCS_MuzzleDrivenReticleController muzzleReticle =
                    hudRoot.GetComponent<CCS_MuzzleDrivenReticleController>();
                if (muzzleReticle == null)
                {
                    muzzleReticle = hudRoot.gameObject.AddComponent<CCS_MuzzleDrivenReticleController>();
                    changed = true;
                }

                SerializedObject serializedReticle = new SerializedObject(muzzleReticle);
                bool reticleChanged = false;
                reticleChanged |= SetEnum(
                    serializedReticle,
                    "reticleMode",
                    (int)CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift);
                reticleChanged |= SetBool(serializedReticle, "enableReticleClamp", true);
                reticleChanged |= SetFloat(
                    serializedReticle,
                    "maxMuzzleReticleOffsetPixels",
                    CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault);
                reticleChanged |= SetFloat(
                    serializedReticle,
                    "safeScreenPaddingPixels",
                    CCS_WeaponsConstants.MasterTestReticleSafeScreenPaddingPixelsDefault);
                reticleChanged |= SetObjectReference(serializedReticle, "revolverController", revolverController);
                reticleChanged |= SetObjectReference(serializedReticle, "equipmentVisualController", equipmentVisual);
                reticleChanged |= SetObjectReference(serializedReticle, "hudPresenter", hudPresenter);
                reticleChanged |= SetObjectReference(serializedReticle, "reticleTransform", reticleImage != null ? reticleImage.rectTransform : null);
                reticleChanged |= SetObjectReference(serializedReticle, "reticleCanvas", hudRoot.GetComponent<Canvas>());
                if (reticleChanged)
                {
                    serializedReticle.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (visualRoot != null)
            {
                CCS_RevolverBodyAimFollowController bodyAimFollow =
                    visualRoot.GetComponent<CCS_RevolverBodyAimFollowController>();
                if (bodyAimFollow == null)
                {
                    bodyAimFollow = visualRoot.gameObject.AddComponent<CCS_RevolverBodyAimFollowController>();
                    changed = true;
                }

                SerializedObject serializedBodyAim = new SerializedObject(bodyAimFollow);
                bool bodyChanged = false;
                bodyChanged |= SetBool(serializedBodyAim, "enableBodyAimFollow", true);
                bodyChanged |= SetObjectReference(serializedBodyAim, "animator", animator);
                bodyChanged |= SetObjectReference(serializedBodyAim, "revolverAnimationStateComponent", revolverController);
                bodyChanged |= SetObjectReference(
                    serializedBodyAim,
                    "cameraFollowAnchor",
                    aimLocomotion != null ? aimLocomotion.CameraFollowAnchor : null);
                if (bodyChanged)
                {
                    serializedBodyAim.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (revolverController != null)
            {
                SerializedObject serializedRevolver = new SerializedObject(revolverController);
                bool revolverChanged = false;
                revolverChanged |= SetBool(serializedRevolver, "enableVisualAimConvergence", false);
                revolverChanged |= SetBool(serializedRevolver, "enableMuzzleAuthoritativeShots", false);
                if (revolverChanged)
                {
                    serializedRevolver.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureFireFeedback(GameObject prefabRoot)

        {

            Transform muzzleTransform = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);

            CCS_RevolverFireFeedback fireFeedback = null;



            if (muzzleTransform != null)

            {

                fireFeedback = muzzleTransform.GetComponent<CCS_RevolverFireFeedback>();

            }



            CCS_RevolverFireFeedback rootFeedback = prefabRoot.GetComponent<CCS_RevolverFireFeedback>();

            if (rootFeedback != null)

            {

                if (fireFeedback == null && muzzleTransform != null)

                {

                    Object.DestroyImmediate(rootFeedback, true);

                    fireFeedback = muzzleTransform.gameObject.AddComponent<CCS_RevolverFireFeedback>();

                }

                else if (fireFeedback != rootFeedback)

                {

                    Object.DestroyImmediate(rootFeedback, true);

                }

            }



            if (fireFeedback == null)

            {

                if (muzzleTransform != null)

                {

                    fireFeedback = muzzleTransform.gameObject.AddComponent<CCS_RevolverFireFeedback>();

                }

                else

                {

                    fireFeedback = prefabRoot.AddComponent<CCS_RevolverFireFeedback>();

                }

            }



            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();
            CCS_PlayerEquipmentVisualController equipmentVisualController =
                prefabRoot.GetComponent<CCS_PlayerEquipmentVisualController>();

            SerializedObject serializedFeedback = new SerializedObject(fireFeedback);
            bool changed = SetObjectReference(serializedFeedback, "revolverController", revolverController);
            changed |= SetObjectReference(serializedFeedback, "muzzlePoint", muzzleTransform);
            changed |= SetObjectReference(serializedFeedback, "equipmentVisualController", equipmentVisualController);
            changed |= SetObjectReference(
                serializedFeedback,
                "bulletTracerPrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(CCS_WeaponsConstants.RevolverM1879BulletTracerVisualPrefabPath));
            changed |= SetObjectReference(
                serializedFeedback,
                "muzzleFlashPrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(CCS_WeaponsConstants.RevolverM1879MuzzleFlashPrefabPath));
            changed |= SetObjectReference(
                serializedFeedback,
                "muzzleSmokePrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(CCS_WeaponsConstants.RevolverM1879MuzzleSmokePrefabPath));
            changed |= SetObjectReference(
                serializedFeedback,
                "spentShellPrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(CCS_WeaponsConstants.RevolverM1879SpentShellVisualPrefabPath));



            if (changed)

            {

                serializedFeedback.ApplyModifiedPropertiesWithoutUndo();

            }



            if (!fireFeedback.enabled)

            {

                fireFeedback.enabled = true;

                changed = true;

            }



            return changed;

        }



        private static bool EnsureHudCanvas(GameObject hudObject)

        {

            bool changed = false;



            Canvas canvas = hudObject.GetComponent<Canvas>();

            if (canvas == null)

            {

                canvas = hudObject.AddComponent<Canvas>();

                changed = true;

            }



            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)

            {

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                changed = true;

            }



            CanvasScaler scaler = hudObject.GetComponent<CanvasScaler>();

            if (scaler == null)

            {

                scaler = hudObject.AddComponent<CanvasScaler>();

                changed = true;

            }



            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)

            {

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                changed = true;

            }



            if (scaler.referenceResolution != new Vector2(1920f, 1080f))

            {

                scaler.referenceResolution = new Vector2(1920f, 1080f);

                changed = true;

            }



            if (hudObject.GetComponent<GraphicRaycaster>() == null)

            {

                hudObject.AddComponent<GraphicRaycaster>();

                changed = true;

            }



            return changed;

        }



        private static TextMeshProUGUI EnsureHudText(Transform hudRoot, ref bool changed)

        {

            Transform existing = hudRoot.Find(CCS_WeaponsConstants.WeaponHudTextObjectName);

            TextMeshProUGUI hudText;

            if (existing != null)

            {

                hudText = existing.GetComponent<TextMeshProUGUI>();

                if (hudText == null)

                {

                    hudText = existing.gameObject.AddComponent<TextMeshProUGUI>();

                    changed = true;

                }

            }

            else

            {

                GameObject textObject = new GameObject(

                    CCS_WeaponsConstants.WeaponHudTextObjectName,

                    typeof(RectTransform),

                    typeof(CanvasRenderer),

                    typeof(TextMeshProUGUI));

                textObject.transform.SetParent(hudRoot, false);

                hudText = textObject.GetComponent<TextMeshProUGUI>();

                changed = true;

            }



            RectTransform rectTransform = hudText.rectTransform;

            rectTransform.anchorMin = new Vector2(1f, 1f);

            rectTransform.anchorMax = new Vector2(1f, 1f);

            rectTransform.pivot = new Vector2(1f, 1f);

            rectTransform.anchoredPosition = new Vector2(-24f, -24f);

            rectTransform.sizeDelta = new Vector2(320f, 120f);



            if (!Mathf.Approximately(hudText.fontSize, CCS_WeaponsConstants.WeaponHudFontSize))

            {

                hudText.fontSize = CCS_WeaponsConstants.WeaponHudFontSize;

                changed = true;

            }



            if (hudText.alignment != TextAlignmentOptions.TopRight)

            {

                hudText.alignment = TextAlignmentOptions.TopRight;

                changed = true;

            }



            if (hudText.color != CCS_WeaponsConstants.WeaponHudAmmoNormalColor)

            {

                hudText.color = CCS_WeaponsConstants.WeaponHudAmmoNormalColor;

                changed = true;

            }



            if (!Mathf.Approximately(hudText.outlineWidth, CCS_WeaponsConstants.WeaponHudOutlineWidth))

            {

                hudText.outlineWidth = CCS_WeaponsConstants.WeaponHudOutlineWidth;

                changed = true;

            }



            if (hudText.outlineColor != CCS_WeaponsConstants.WeaponHudOutlineColor)

            {

                hudText.outlineColor = CCS_WeaponsConstants.WeaponHudOutlineColor;

                changed = true;

            }



            if (hudText.fontStyle != FontStyles.Bold)

            {

                hudText.fontStyle = FontStyles.Bold;

                changed = true;

            }



            return hudText;

        }



        private static Image EnsureReticle(Transform hudRoot, ref bool changed)

        {

            Transform existing = hudRoot.Find(CCS_WeaponsConstants.WeaponReticleObjectName);

            Image reticleImage;

            if (existing != null)

            {

                reticleImage = existing.GetComponent<Image>();

                if (reticleImage == null)

                {

                    reticleImage = existing.gameObject.AddComponent<Image>();

                    changed = true;

                }

            }

            else

            {

                GameObject reticleObject = new GameObject(

                    CCS_WeaponsConstants.WeaponReticleObjectName,

                    typeof(RectTransform),

                    typeof(CanvasRenderer),

                    typeof(Image));

                reticleObject.transform.SetParent(hudRoot, false);

                reticleImage = reticleObject.GetComponent<Image>();

                changed = true;

            }



            RectTransform rectTransform = reticleImage.rectTransform;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);

            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            rectTransform.anchoredPosition = Vector2.zero;

            rectTransform.sizeDelta = new Vector2(12f, 12f);



            if (reticleImage.color != CCS_WeaponsConstants.WeaponReticleFillColor)
            {
                reticleImage.color = CCS_WeaponsConstants.WeaponReticleFillColor;
                changed = true;
            }

            Outline reticleOutline = reticleImage.GetComponent<Outline>();
            if (reticleOutline == null)
            {
                reticleOutline = reticleImage.gameObject.AddComponent<Outline>();
                changed = true;
            }

            if (reticleOutline.effectColor != CCS_WeaponsConstants.WeaponReticleOutlineColor)
            {
                reticleOutline.effectColor = CCS_WeaponsConstants.WeaponReticleOutlineColor;
                changed = true;
            }

            Vector2 reticleOutlineDistance = new Vector2(1f, -1f);
            if (reticleOutline.effectDistance != reticleOutlineDistance)
            {
                reticleOutline.effectDistance = reticleOutlineDistance;
                changed = true;
            }



            if (reticleImage.enabled)

            {

                reticleImage.enabled = false;

                changed = true;

            }



            return reticleImage;

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

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Boolean || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Float || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetEnum(SerializedObject serializedObject, string propertyName, int enumValue)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Enum || property.enumValueIndex == enumValue)
            {
                return false;
            }

            property.enumValueIndex = enumValue;
            return true;
        }



        #endregion

    }

}


