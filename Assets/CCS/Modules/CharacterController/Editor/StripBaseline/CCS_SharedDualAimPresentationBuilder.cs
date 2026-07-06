using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SharedDualAimPresentationBuilder
// CATEGORY: Modules / CharacterController / Editor / StripBaseline
// PURPOSE: Wires shared dual-aim point and screen reticle on player prefab.
// PLACEMENT: Editor builder invoked from stripped baseline strip pass.
// AUTHOR: James Schilz
// CREATED: 2026-07-02
// NOTES: Camera ray -> SharedDualAimPoint under passive visual aim reference -> reticle projection.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_SharedDualAimPresentationBuilder
    {
        public static bool EnsureSharedDualAimPresentation(
            Transform modelRoot,
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            if (modelRoot == null || prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= EnsureSharedDualAimPointPresenter(modelRoot, prefabRoot, result);
            changed |= EnsureSharedDualAimReticle(prefabRoot, result);
            return changed;
        }

        private static bool EnsureSharedDualAimPointPresenter(
            Transform modelRoot,
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
            Transform visualReferenceRoot = aimingRoot != null
                ? aimingRoot.Find(CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName)
                : null;
            Transform sharedAimPoint = visualReferenceRoot != null
                ? visualReferenceRoot.Find(CCS_CharacterControllerConstants.SharedDualAimPointObjectName)
                : null;

            CCS_SharedDualAimPointPresenter presenter = modelRoot.GetComponent<CCS_SharedDualAimPointPresenter>();
            bool changed = false;
            if (presenter == null)
            {
                presenter = modelRoot.gameObject.AddComponent<CCS_SharedDualAimPointPresenter>();
                changed = true;
                result.Notes.Add("Added CCS_SharedDualAimPointPresenter on Model.");
            }

            CCS_RevolverAimPresentationGate aimGate = prefabRoot.GetComponent<CCS_RevolverAimPresentationGate>();
            SerializedObject serializedPresenter = new SerializedObject(presenter);
            changed |= SetObjectReference(serializedPresenter, "aimPresentationInputComponent", aimGate);
            changed |= SetObjectReference(serializedPresenter, "sharedDualAimPoint", sharedAimPoint);
            changed |= SetFloatValue(
                serializedPresenter,
                "convergenceDistance",
                CCS_CharacterControllerConstants.SharedDualAimPointDefaultDistance);
            changed |= SetFloatValue(
                serializedPresenter,
                "targetSmoothTime",
                CCS_CharacterControllerConstants.SharedDualAimPointDefaultSmoothTime);
            if (changed)
            {
                serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureSharedDualAimReticle(
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            Transform hudRoot = prefabRoot.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);
            if (hudRoot == null)
            {
                return false;
            }

            bool changed = false;
            Transform reticleTransform = hudRoot.Find(CCS_CharacterControllerConstants.SharedDualAimReticleObjectName);
            if (reticleTransform == null)
            {
                GameObject reticleObject = new GameObject(
                    CCS_CharacterControllerConstants.SharedDualAimReticleObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                reticleTransform = reticleObject.transform;
                reticleTransform.SetParent(hudRoot, false);
                changed = true;
                result.Notes.Add("Created CCS_SharedDualAimReticle under WeaponHudRoot.");
            }

            RectTransform rectTransform = reticleTransform.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (rectTransform.sizeDelta != new Vector2(18f, 18f))
                {
                    rectTransform.sizeDelta = new Vector2(18f, 18f);
                    changed = true;
                }

                if (rectTransform.anchorMin != new Vector2(0.5f, 0.5f) || rectTransform.anchorMax != new Vector2(0.5f, 0.5f))
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    changed = true;
                }
            }

            Image reticleImage = reticleTransform.GetComponent<Image>();
            if (reticleImage != null)
            {
                Color fill = new Color(1f, 1f, 1f, 0.85f);
                if (reticleImage.color != fill)
                {
                    reticleImage.color = fill;
                    changed = true;
                }

                if (reticleImage.raycastTarget)
                {
                    reticleImage.raycastTarget = false;
                    changed = true;
                }
            }

            CCS_SharedDualAimReticlePresenter presenter = hudRoot.GetComponent<CCS_SharedDualAimReticlePresenter>();
            if (presenter == null)
            {
                presenter = hudRoot.gameObject.AddComponent<CCS_SharedDualAimReticlePresenter>();
                changed = true;
                result.Notes.Add("Added CCS_SharedDualAimReticlePresenter on WeaponHudRoot.");
            }

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            changed |= SetObjectReference(serializedPresenter, "reticleRectTransform", reticleTransform);
            changed |= SetObjectReference(serializedPresenter, "reticleImage", reticleImage);
            if (changed)
            {
                serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
            }

            if (reticleTransform.gameObject.activeSelf)
            {
                reticleTransform.gameObject.SetActive(false);
                changed = true;
            }

            return changed;
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

        private static bool SetFloatValue(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }
    }
}
