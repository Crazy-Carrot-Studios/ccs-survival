using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsTestDamageTargetPrefabBuilder
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Builds red capsule + glasses test weapon damage target prefab layout.
// PLACEMENT: Editor utility invoked from CCS_WeaponsAssetBuilder.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Mirrors capsule player visual layout; static target with no movement components.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsTestDamageTargetPrefabBuilder
    {
        #region Public Methods

        public static bool EnsureTestDamageTargetPrefab()
        {
            GameObject prefabRoot = LoadOrCreatePrefabRoot();
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= EnsureRootCollider(prefabRoot);
            changed |= EnsureCapsuleVisual(prefabRoot.transform);
            changed |= EnsureGlassesVisual(prefabRoot.transform);
            changed |= EnsureDamageTargetComponent(prefabRoot);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CCS_WeaponsConstants.TestDamageTargetPrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        #endregion

        #region Private Methods

        private static GameObject LoadOrCreatePrefabRoot()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.TestDamageTargetPrefabPath);
            if (existingPrefab != null)
            {
                return PrefabUtility.LoadPrefabContents(CCS_WeaponsConstants.TestDamageTargetPrefabPath);
            }

            string directory = System.IO.Path.GetDirectoryName(CCS_WeaponsConstants.TestDamageTargetPrefabPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            GameObject root = new GameObject("PF_CCS_TestWeaponDamageTarget");
            PrefabUtility.SaveAsPrefabAsset(root, CCS_WeaponsConstants.TestDamageTargetPrefabPath);
            Object.DestroyImmediate(root);
            return PrefabUtility.LoadPrefabContents(CCS_WeaponsConstants.TestDamageTargetPrefabPath);
        }

        private static bool EnsureRootCollider(GameObject prefabRoot)
        {
            bool changed = false;
            BoxCollider legacyBox = prefabRoot.GetComponent<BoxCollider>();
            if (legacyBox != null)
            {
                Object.DestroyImmediate(legacyBox);
                changed = true;
            }

            MeshFilter legacyMesh = prefabRoot.GetComponent<MeshFilter>();
            if (legacyMesh != null)
            {
                Object.DestroyImmediate(legacyMesh);
                changed = true;
            }

            MeshRenderer legacyRenderer = prefabRoot.GetComponent<MeshRenderer>();
            if (legacyRenderer != null)
            {
                Object.DestroyImmediate(legacyRenderer);
                changed = true;
            }

            CapsuleCollider capsuleCollider = prefabRoot.GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = prefabRoot.AddComponent<CapsuleCollider>();
                changed = true;
            }

            if (!Mathf.Approximately(capsuleCollider.height, CCS_WeaponsConstants.DamageTargetCapsuleHeight))
            {
                capsuleCollider.height = CCS_WeaponsConstants.DamageTargetCapsuleHeight;
                changed = true;
            }

            if (!Mathf.Approximately(capsuleCollider.radius, CCS_WeaponsConstants.DamageTargetCapsuleRadius))
            {
                capsuleCollider.radius = CCS_WeaponsConstants.DamageTargetCapsuleRadius;
                changed = true;
            }

            Vector3 expectedCenter = new Vector3(
                0f,
                CCS_WeaponsConstants.DamageTargetCapsuleCenterY,
                0f);
            if (capsuleCollider.center != expectedCenter)
            {
                capsuleCollider.center = expectedCenter;
                changed = true;
            }

            if (capsuleCollider.direction != 1)
            {
                capsuleCollider.direction = 1;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureCapsuleVisual(Transform prefabRoot)
        {
            Material redMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.TestPlayerRedMaterialPath);
            if (redMaterial == null)
            {
                Debug.LogError(
                    "[Weapons Builder] Missing red player material at "
                    + CCS_WeaponsConstants.TestPlayerRedMaterialPath);
                return false;
            }

            Transform bodyVisual = prefabRoot.Find(CCS_WeaponsConstants.CapsuleVisualName);
            bool changed = false;
            if (bodyVisual == null)
            {
                GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bodyObject.name = CCS_WeaponsConstants.CapsuleVisualName;
                bodyVisual = bodyObject.transform;
                bodyVisual.SetParent(prefabRoot, false);
                changed = true;
            }

            Collider bodyCollider = bodyVisual.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                Object.DestroyImmediate(bodyCollider);
                changed = true;
            }

            if (bodyVisual.localPosition != CCS_WeaponsConstants.CapsuleVisualLocalPosition)
            {
                bodyVisual.localPosition = CCS_WeaponsConstants.CapsuleVisualLocalPosition;
                changed = true;
            }

            if (bodyVisual.localScale != CCS_WeaponsConstants.CapsuleVisualLocalScale)
            {
                bodyVisual.localScale = CCS_WeaponsConstants.CapsuleVisualLocalScale;
                changed = true;
            }

            if (!bodyVisual.gameObject.activeSelf)
            {
                bodyVisual.gameObject.SetActive(true);
                changed = true;
            }

            MeshRenderer renderer = bodyVisual.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial != redMaterial)
            {
                renderer.sharedMaterial = redMaterial;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureGlassesVisual(Transform prefabRoot)
        {
            Material blackMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.TestPlayerBlackMaterialPath);
            if (blackMaterial == null)
            {
                Debug.LogError(
                    "[Weapons Builder] Missing black player material at "
                    + CCS_WeaponsConstants.TestPlayerBlackMaterialPath);
                return false;
            }

            Transform glasses = prefabRoot.Find(CCS_WeaponsConstants.GlassesVisualName);
            bool changed = false;
            if (glasses == null)
            {
                GameObject glassesObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                glassesObject.name = CCS_WeaponsConstants.GlassesVisualName;
                glasses = glassesObject.transform;
                glasses.SetParent(prefabRoot, false);
                changed = true;
            }

            Collider glassesCollider = glasses.GetComponent<Collider>();
            if (glassesCollider != null)
            {
                Object.DestroyImmediate(glassesCollider);
                changed = true;
            }

            if (glasses.localPosition != CCS_WeaponsConstants.GlassesVisualLocalPosition)
            {
                glasses.localPosition = CCS_WeaponsConstants.GlassesVisualLocalPosition;
                changed = true;
            }

            Quaternion expectedRotation = Quaternion.Euler(CCS_WeaponsConstants.GlassesVisualLocalEuler);
            if (glasses.localRotation != expectedRotation)
            {
                glasses.localRotation = expectedRotation;
                changed = true;
            }

            if (glasses.localScale != CCS_WeaponsConstants.GlassesVisualLocalScale)
            {
                glasses.localScale = CCS_WeaponsConstants.GlassesVisualLocalScale;
                changed = true;
            }

            if (!glasses.gameObject.activeSelf)
            {
                glasses.gameObject.SetActive(true);
                changed = true;
            }

            MeshRenderer renderer = glasses.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial != blackMaterial)
            {
                renderer.sharedMaterial = blackMaterial;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureDamageTargetComponent(GameObject prefabRoot)
        {
            CCS_TestDamageTarget damageTarget = prefabRoot.GetComponent<CCS_TestDamageTarget>();
            bool changed = false;
            if (damageTarget == null)
            {
                damageTarget = prefabRoot.AddComponent<CCS_TestDamageTarget>();
                changed = true;
            }

            Transform bodyVisual = prefabRoot.transform.Find(CCS_WeaponsConstants.CapsuleVisualName);
            MeshRenderer bodyRenderer = bodyVisual != null ? bodyVisual.GetComponent<MeshRenderer>() : null;

            SerializedObject serializedTarget = new SerializedObject(damageTarget);
            bool serializedChanged = false;
            serializedChanged |= SetFloat(serializedTarget, "maxHealth", 100f);
            serializedChanged |= SetBool(serializedTarget, "resetOnPlay", true);
            serializedChanged |= SetObjectReference(serializedTarget, "targetRenderer", bodyRenderer);
            serializedChanged |= SetColor(
                serializedTarget,
                "healthyColor",
                CCS_WeaponsConstants.DamageTargetHealthyColor);
            serializedChanged |= SetColor(
                serializedTarget,
                "damagedColor",
                CCS_WeaponsConstants.DamageTargetDamagedColor);
            serializedChanged |= SetColor(
                serializedTarget,
                "deadColor",
                CCS_WeaponsConstants.DamageTargetDeadColor);

            if (serializedChanged)
            {
                serializedTarget.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            return changed;
        }

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static bool SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.colorValue == value)
            {
                return false;
            }

            property.colorValue = value;
            return true;
        }

        #endregion
    }
}
