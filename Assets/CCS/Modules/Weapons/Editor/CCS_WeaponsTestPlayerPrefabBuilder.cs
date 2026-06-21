using CCS.Modules.CharacterController;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_WeaponsTestPlayerPrefabBuilder
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Wires revolver controller, muzzle point, and test HUD on the test player prefab.
// PLACEMENT: Editor utility invoked from Weapons validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not modify CharacterController movement, camera, or interaction wiring.
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
            changed |= EnsureRevolverController(prefabRoot, revolverDefinition);
            changed |= EnsureMuzzlePoint(prefabRoot);
            changed |= EnsureWeaponHud(prefabRoot);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        #endregion

        #region Private Methods

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

        private static bool EnsureMuzzlePoint(GameObject prefabRoot)
        {
            Transform existing = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);
            if (existing != null)
            {
                return false;
            }

            Transform weaponRoot = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.WeaponRootObjectName);
            if (weaponRoot == null)
            {
                Transform visualRoot = prefabRoot.transform.Find("VisualRoot");
                GameObject weaponRootObject = new GameObject(CCS_WeaponsConstants.WeaponRootObjectName);
                weaponRootObject.transform.SetParent(visualRoot != null ? visualRoot : prefabRoot.transform, false);
                weaponRoot = weaponRootObject.transform;
            }

            Transform rightHand = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.PlayerRightHandSocketName);
            Transform muzzleParent = rightHand != null ? rightHand : weaponRoot;

            GameObject muzzleObject = new GameObject(CCS_WeaponsConstants.MuzzlePointObjectName);
            muzzleObject.transform.SetParent(muzzleParent, false);
            muzzleObject.transform.localPosition = new Vector3(0f, 0f, 0.12f);
            muzzleObject.transform.localRotation = Quaternion.identity;
            return true;
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

            if (hudText.color != Color.white)
            {
                hudText.color = Color.white;
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

            if (reticleImage.color != Color.white)
            {
                reticleImage.color = Color.white;
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

        #endregion
    }
}
