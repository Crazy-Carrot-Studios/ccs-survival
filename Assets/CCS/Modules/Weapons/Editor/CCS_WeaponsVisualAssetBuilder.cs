using System.Collections.Generic;
using System.IO;
using CCS.Modules.Interaction;
using CCS.Modules.Interaction.Editor;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsVisualAssetBuilder
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Isolates Reichsrevolver M1879 vendor assets into CCS-owned content and prefabs.
// PLACEMENT: Editor utility invoked from CCS_WeaponsAssetBuilder and validation setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.5 — world pickup, holstered, equipped, bullet/shell visual prefabs.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsVisualAssetBuilder
    {
        #region Variables

        private const string SourceRootPath = "Assets/Reichsrevolver_M1879";
        private const string SourceModelPath = SourceRootPath + "/Model/ReichsrevolverM1879.FBX";
        private const string SourceShellModelPath = SourceRootPath + "/Model/ReichsrevolverM1879Shell.FBX";
        private const string SourceGunMaterialPath = SourceRootPath + "/Materials/ReichsrevolverM1879_1024_mat.mat";
        private const string SourceShellMaterialPath = SourceRootPath + "/Materials/ReichsrevolverM1879Shell_128_mat.mat";
        private const string SourceAlbedoTexturePath = SourceRootPath + "/Textures/ReichsrevolverM1879_diffuse_1024.tga";
        private const string SourceNormalTexturePath = SourceRootPath + "/Textures/ReichsrevolverM1879_normal_1024.tga";
        private const string SourceMetallicTexturePath = SourceRootPath + "/Textures/ReichsrevolverM1879_specular_1024.tga";
        private const string SourceAoTexturePath = SourceRootPath + "/Textures/ReichsrevolverM1879_ao_1024.tga";
        private const string SourceShellAlbedoTexturePath =
            SourceRootPath + "/Textures/ReichsrevolverM1879Shell_diffuse_128.tga";
        private const string SourceShellNormalTexturePath =
            SourceRootPath + "/Textures/ReichsrevolverM1879Shell_normal_128.tga";
        private const string SourceShellMetallicTexturePath =
            SourceRootPath + "/Textures/ReichsrevolverM1879Shell_specular_128.tga";

        private const string VendorGunPrefabPath =
            CCS_WeaponsConstants.VendorSourceReichsrevolverRootPath + "/Prefabs/ReichsrevolverM1879.prefab";

        private static readonly string[] HiddenVisualPartNameTokens =
        {
            "LP_Shell",
            "LP_Bullet",
            "ReichsrevolverM1879Shell",
        };

        #endregion

        #region Public Methods

        public static bool EnsureRevolverM1879VisualAssets()
        {
            EnsureContentFolders();
            bool changed = false;
            changed |= EnsureIsolatedSourceAssets();
            changed |= EnsureGunMaterialReferences();
            changed |= EnsureVisualDefinitionDefaults();
            changed |= EnsureMaterializedVisualPrefab();
            changed |= RemoveDeprecatedVisualPrefabs();
            changed |= EnsureVisualDefinitionAsset();
            changed |= EnsureWorldPickupPrefab();
            changed |= EnsureReadme();
            changed |= MoveVendorSourceFolder();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static void EnsureContentFolders()
        {
            EnsureFolder(CCS_WeaponsConstants.RevolverM1879ContentRootPath);
            EnsureFolder(CCS_WeaponsConstants.RevolverM1879ModelsPath);
            EnsureFolder(CCS_WeaponsConstants.RevolverM1879MaterialsPath);
            EnsureFolder(CCS_WeaponsConstants.RevolverM1879TexturesPath);
            EnsureFolder(CCS_WeaponsConstants.RevolverM1879PrefabsPath);
        }

        private static bool EnsureIsolatedSourceAssets()
        {
            bool changed = false;
            changed |= CopyAssetIfMissing(SourceModelPath, CCS_WeaponsConstants.RevolverM1879ModelAssetPath);
            changed |= CopyAssetIfMissing(SourceShellModelPath, CCS_WeaponsConstants.RevolverM1879ShellModelAssetPath);
            changed |= CopyAssetIfMissing(SourceShellModelPath, CCS_WeaponsConstants.RevolverM1879BulletModelAssetPath);
            changed |= CopyAssetIfMissing(SourceAlbedoTexturePath, CCS_WeaponsConstants.RevolverM1879AlbedoTexturePath);
            changed |= CopyAssetIfMissing(SourceNormalTexturePath, CCS_WeaponsConstants.RevolverM1879NormalTexturePath);
            changed |= CopyAssetIfMissing(SourceMetallicTexturePath, CCS_WeaponsConstants.RevolverM1879MetallicTexturePath);
            changed |= CopyAssetIfMissing(SourceGunMaterialPath, CCS_WeaponsConstants.RevolverM1879MaterialAssetPath);
            changed |= CopyAssetIfMissing(SourceGunMaterialPath, CCS_WeaponsConstants.RevolverM1879WoodGripMaterialAssetPath);
            changed |= CopyAssetIfMissing(SourceGunMaterialPath, CCS_WeaponsConstants.RevolverM1879MetalMaterialAssetPath);
            changed |= CopyAssetIfMissing(SourceShellMaterialPath, CCS_WeaponsConstants.RevolverM1879ShellMaterialAssetPath);
            changed |= CopyAssetIfMissing(SourceShellMaterialPath, CCS_WeaponsConstants.RevolverM1879BulletMaterialAssetPath);
            return changed;
        }

        private static bool EnsureGunMaterialReferences()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CCS_WeaponsConstants.RevolverM1879MaterialAssetPath);
            if (material == null)
            {
                return false;
            }

            Texture albedo = AssetDatabase.LoadAssetAtPath<Texture>(CCS_WeaponsConstants.RevolverM1879AlbedoTexturePath);
            Texture normal = AssetDatabase.LoadAssetAtPath<Texture>(CCS_WeaponsConstants.RevolverM1879NormalTexturePath);
            Texture metallic = AssetDatabase.LoadAssetAtPath<Texture>(CCS_WeaponsConstants.RevolverM1879MetallicTexturePath);
            Texture ao = AssetDatabase.LoadAssetAtPath<Texture>(SourceAoTexturePath);
            bool changed = RemapLitMaterialTextures(material, albedo, normal, metallic, ao);

            Material woodGrip = AssetDatabase.LoadAssetAtPath<Material>(CCS_WeaponsConstants.RevolverM1879WoodGripMaterialAssetPath);
            Material metal = AssetDatabase.LoadAssetAtPath<Material>(CCS_WeaponsConstants.RevolverM1879MetalMaterialAssetPath);
            if (woodGrip != null && woodGrip != material)
            {
                changed |= RemapLitMaterialTextures(woodGrip, albedo, normal, metallic, ao);
            }

            if (metal != null && metal != material)
            {
                changed |= RemapLitMaterialTextures(metal, albedo, normal, metallic, ao);
            }

            return changed;
        }

        private static bool EnsureShellMaterialReferences()
        {
            Material shellMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.RevolverM1879ShellMaterialAssetPath);
            Material bulletMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.RevolverM1879BulletMaterialAssetPath);
            if (shellMaterial == null)
            {
                return false;
            }

            Texture albedo = AssetDatabase.LoadAssetAtPath<Texture>(SourceShellAlbedoTexturePath);
            Texture normal = AssetDatabase.LoadAssetAtPath<Texture>(SourceShellNormalTexturePath);
            Texture metallic = AssetDatabase.LoadAssetAtPath<Texture>(SourceShellMetallicTexturePath);
            bool changed = RemapLitMaterialTextures(shellMaterial, albedo, normal, metallic, null);
            if (bulletMaterial != null && bulletMaterial != shellMaterial)
            {
                changed |= RemapLitMaterialTextures(bulletMaterial, albedo, normal, metallic, null);
            }

            return changed;
        }

        private static bool EnsureMaterializedVisualPrefab()
        {
            GameObject prefabRoot = LoadOrCreatePrefabRoot(CCS_WeaponsConstants.RevolverM1879MaterializedVisualPrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            if (prefabRoot.name != "PF_CCS_RevolverM1879_MaterializedVisual")
            {
                prefabRoot.name = "PF_CCS_RevolverM1879_MaterializedVisual";
                changed = true;
            }

            if (RebuildMaterializedVisualHierarchy(prefabRoot))
            {
                changed = true;
            }

            changed |= StripImportedRuntimeComponents(prefabRoot);
            return SavePrefabIfChanged(
                prefabRoot,
                CCS_WeaponsConstants.RevolverM1879MaterializedVisualPrefabPath,
                changed);
        }

        private static bool EnsureWorldPickupPrefab()
        {
            CCS_InteractionLayerUtility.EnsureInteractableLayer();
            GameObject prefabRoot = LoadOrCreatePrefabRoot(CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            if (prefabRoot.name != "PF_CCS_RevolverM1879_WorldPickup")
            {
                prefabRoot.name = "PF_CCS_RevolverM1879_WorldPickup";
                changed = true;
            }

            changed |= RemoveLegacyVisualChildren(prefabRoot.transform);
            changed |= EnsureWorldPickupVisual(prefabRoot);
            changed |= RemoveDuplicateRootVisualBranches(prefabRoot.transform);

            BoxCollider boxCollider = prefabRoot.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = prefabRoot.AddComponent<BoxCollider>();
                changed = true;
            }

            Bounds visualBounds = CalculateVisualBounds(prefabRoot.transform);
            Vector3 center = visualBounds.center - prefabRoot.transform.position;
            if (boxCollider.center != center || boxCollider.size != visualBounds.size)
            {
                boxCollider.center = center;
                boxCollider.size = visualBounds.size;
                changed = true;
            }

            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            if (interactableLayer >= 0 && prefabRoot.layer != interactableLayer)
            {
                prefabRoot.layer = interactableLayer;
                changed = true;
            }

            if (!prefabRoot.CompareTag(CCS_InteractionConstants.InteractableTagName))
            {
                prefabRoot.tag = CCS_InteractionConstants.InteractableTagName;
                changed = true;
            }

            CCS_InteractableLabelTarget labelTarget = EnsureComponent<CCS_InteractableLabelTarget>(prefabRoot);
            labelTarget.ConfigureForKind(CCS_InteractionKind.Pickup, "Revolver");

            CCS_WeaponPickupInteractable pickup = EnsureComponent<CCS_WeaponPickupInteractable>(prefabRoot);
            CCS_RevolverVisualDefinition visualDefinition = AssetDatabase.LoadAssetAtPath<CCS_RevolverVisualDefinition>(
                CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
            SerializedObject serializedPickup = new SerializedObject(pickup);
            if (SetObjectReference(serializedPickup, "visualDefinition", visualDefinition))
            {
                serializedPickup.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            changed |= StripVendorScripts(prefabRoot);
            return SavePrefabIfChanged(prefabRoot, CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath, changed);
        }

        private static bool EnsureVisualDefinitionDefaults()
        {
            CCS_RevolverVisualDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RevolverVisualDefinition>(
                CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
            bool created = false;
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_RevolverVisualDefinition>();
                definition.name = "CCS_RevolverM1879VisualDefinition";
                AssetDatabase.CreateAsset(definition, CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
                created = true;
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            bool changed = created;
            changed |= SetString(serializedDefinition, "weaponId", CCS_WeaponsConstants.RevolverM1879WeaponId);
            changed |= SetVector3(
                serializedDefinition,
                "holsteredLocalPosition",
                CCS_WeaponsConstants.DefaultHolsteredLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "holsteredLocalEulerAngles",
                CCS_WeaponsConstants.DefaultHolsteredLocalEuler);
            changed |= SetVector3(
                serializedDefinition,
                "holsteredLocalScale",
                CCS_WeaponsConstants.DefaultHolsteredLocalScale);
            changed |= SetVector3(
                serializedDefinition,
                "equippedLocalPosition",
                CCS_WeaponsConstants.DefaultEquippedLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "equippedLocalEulerAngles",
                CCS_WeaponsConstants.DefaultEquippedLocalEuler);
            changed |= SetVector3(
                serializedDefinition,
                "equippedLocalScale",
                CCS_WeaponsConstants.DefaultEquippedLocalScale);
            changed |= SetVector3(
                serializedDefinition,
                "muzzleLocalPosition",
                CCS_WeaponsConstants.DefaultMuzzleLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "shellEjectLocalPosition",
                CCS_WeaponsConstants.DefaultShellEjectLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "bulletVisualSpawnLocalPosition",
                CCS_WeaponsConstants.DefaultBulletVisualSpawnLocalPosition);

            if (changed)
            {
                serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            return changed;
        }

        private static bool EnsureVisualDefinitionAsset()
        {
            CCS_RevolverVisualDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RevolverVisualDefinition>(
                CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
            bool created = false;
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_RevolverVisualDefinition>();
                definition.name = "CCS_RevolverM1879VisualDefinition";
                AssetDatabase.CreateAsset(definition, CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
                created = true;
            }

            GameObject worldPickup = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);

            SerializedObject serializedDefinition = new SerializedObject(definition);
            bool changed = created;
            changed |= SetString(serializedDefinition, "weaponId", CCS_WeaponsConstants.RevolverM1879WeaponId);
            changed |= SetObjectReference(serializedDefinition, "worldPickupPrefab", worldPickup);
            changed |= SetObjectReference(serializedDefinition, "holsteredPrefab", null);
            changed |= SetObjectReference(serializedDefinition, "equippedPrefab", null);
            changed |= SetObjectReference(serializedDefinition, "bulletVisualPrefab", null);
            changed |= SetObjectReference(serializedDefinition, "shellVisualPrefab", null);
            changed |= SetString(
                serializedDefinition,
                "holsterSocketName",
                CCS_WeaponsConstants.RevolverHolsterSocketName);
            changed |= SetString(serializedDefinition, "handSocketName", CCS_WeaponsConstants.RevolverHandSocketName);
            changed |= SetString(serializedDefinition, "muzzlePointName", CCS_WeaponsConstants.MuzzlePointObjectName);
            changed |= SetString(
                serializedDefinition,
                "shellEjectPointName",
                CCS_WeaponsConstants.ShellEjectPointObjectName);
            changed |= SetVector3(
                serializedDefinition,
                "holsteredLocalPosition",
                CCS_WeaponsConstants.DefaultHolsteredLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "holsteredLocalEulerAngles",
                CCS_WeaponsConstants.DefaultHolsteredLocalEuler);
            changed |= SetVector3(
                serializedDefinition,
                "holsteredLocalScale",
                CCS_WeaponsConstants.DefaultHolsteredLocalScale);
            changed |= SetVector3(
                serializedDefinition,
                "equippedLocalPosition",
                CCS_WeaponsConstants.DefaultEquippedLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "equippedLocalEulerAngles",
                CCS_WeaponsConstants.DefaultEquippedLocalEuler);
            changed |= SetVector3(
                serializedDefinition,
                "equippedLocalScale",
                CCS_WeaponsConstants.DefaultEquippedLocalScale);
            changed |= SetVector3(
                serializedDefinition,
                "muzzleLocalPosition",
                CCS_WeaponsConstants.DefaultMuzzleLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "shellEjectLocalPosition",
                CCS_WeaponsConstants.DefaultShellEjectLocalPosition);
            changed |= SetVector3(
                serializedDefinition,
                "bulletVisualSpawnLocalPosition",
                CCS_WeaponsConstants.DefaultBulletVisualSpawnLocalPosition);
            changed |= SetBool(serializedDefinition, "equipOnAim", false);
            changed |= SetBool(serializedDefinition, "holsterWhenAimReleased", false);
            changed |= SetBool(serializedDefinition, "enableShellVisual", false);

            if (changed)
            {
                serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            return changed;
        }

        private static bool RemoveDeprecatedVisualPrefabs()
        {
            bool changed = false;
            string[] deprecatedPaths =
            {
                CCS_WeaponsConstants.RevolverM1879HolsteredPrefabPath,
                CCS_WeaponsConstants.RevolverM1879EquippedPrefabPath,
                CCS_WeaponsConstants.RevolverM1879BulletVisualPrefabPath,
                CCS_WeaponsConstants.RevolverM1879ShellVisualPrefabPath,
            };

            for (int i = 0; i < deprecatedPaths.Length; i++)
            {
                if (File.Exists(deprecatedPaths[i]) && AssetDatabase.DeleteAsset(deprecatedPaths[i]))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureReadme()
        {
            string readmePath = CCS_WeaponsConstants.RevolverM1879ContentRootPath + "/README.md";
            const string readmeBody = @"# CCS Revolver M1879 Content

Reichsrevolver M1879 vendor source assets are isolated into CCS-owned runtime assets under this folder.

## v0.6.5 scope

World pickup only. Holstered/equipped visual attachment is intentionally deferred.

## Prefabs

| Prefab | Purpose |
|--------|---------|
| `PF_CCS_RevolverM1879_MaterializedVisual` | CCS-owned materialized gun source (builder only) |
| `PF_CCS_RevolverM1879_WorldPickup` | Scene pickup — grants weapon ownership |

## Runtime wiring

- Pickup grants ownership via `CCS_PlayerWeaponLoadout`.
- Existing revolver gameplay, aim strafe, and upper-body animations remain active.
- Hitscan remains gameplay authority on `CCS_RevolverController`.
- Vendor scripts/controllers are not part of CCS runtime.

Rebuild via **CCS → Weapons → Validate Weapons Module** or Master Test batch setup.
";
            if (File.Exists(readmePath))
            {
                string existing = File.ReadAllText(readmePath);
                if (existing == readmeBody)
                {
                    return false;
                }
            }

            File.WriteAllText(readmePath, readmeBody);
            AssetDatabase.ImportAsset(readmePath);
            return true;
        }

        private static bool MoveVendorSourceFolder()
        {
            if (!AssetDatabase.IsValidFolder(SourceRootPath))
            {
                return false;
            }

            if (AssetDatabase.IsValidFolder(CCS_WeaponsConstants.VendorSourceReichsrevolverRootPath))
            {
                return AssetDatabase.DeleteAsset(SourceRootPath);
            }

            EnsureFolder("Assets/VendorSource");
            string moveResult = AssetDatabase.MoveAsset(SourceRootPath, CCS_WeaponsConstants.VendorSourceReichsrevolverRootPath);
            if (!string.IsNullOrEmpty(moveResult))
            {
                Debug.LogWarning("[Weapons Visual Builder] Vendor move skipped: " + moveResult);
                return false;
            }

            return true;
        }

        private static bool EnsureWorldPickupVisual(GameObject prefabRoot)
        {
            CCS_RevolverVisualDefinition visualDefinition = LoadVisualDefinitionForBuild();
            if (visualDefinition == null)
            {
                return false;
            }

            bool changed = EnsureModelRootVisual(
                prefabRoot.transform,
                localPosition: Vector3.zero,
                localEuler: Vector3.zero,
                localScale: Vector3.one);
            return changed;
        }

        private static bool EnsureWeaponVisualPrefabRoot(
            GameObject prefabRoot,
            CCS_RevolverVisualDefinition visualDefinition,
            bool isEquipped)
        {
            Vector3 localPosition = isEquipped
                ? visualDefinition.EquippedLocalPosition
                : visualDefinition.HolsteredLocalPosition;
            Vector3 localEuler = isEquipped
                ? visualDefinition.EquippedLocalEulerAngles
                : visualDefinition.HolsteredLocalEulerAngles;
            Vector3 localScale = isEquipped
                ? visualDefinition.EquippedLocalScale
                : visualDefinition.HolsteredLocalScale;

            RemoveLegacyVisualChildren(prefabRoot.transform);
            bool changed = EnsureModelRootVisual(prefabRoot.transform, localPosition, localEuler, localScale);
            changed |= DisableCollidersRecursive(prefabRoot.transform);
            return changed;
        }

        private static bool EnsureModelRootVisual(
            Transform prefabRoot,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            bool changed = false;
            Transform modelRoot = prefabRoot.Find(CCS_WeaponsConstants.RevolverModelRootObjectName);
            if (modelRoot == null)
            {
                GameObject modelRootObject = new GameObject(CCS_WeaponsConstants.RevolverModelRootObjectName);
                modelRoot = modelRootObject.transform;
                modelRoot.SetParent(prefabRoot, false);
                changed = true;
            }

            if (modelRoot.parent != prefabRoot)
            {
                modelRoot.SetParent(prefabRoot, false);
                changed = true;
            }

            if (modelRoot.localPosition != localPosition)
            {
                modelRoot.localPosition = localPosition;
                changed = true;
            }

            Quaternion expectedRotation = Quaternion.Euler(localEuler);
            if (modelRoot.localRotation != expectedRotation)
            {
                modelRoot.localRotation = expectedRotation;
                changed = true;
            }

            if (modelRoot.localScale != localScale)
            {
                modelRoot.localScale = localScale;
                changed = true;
            }

            changed |= RebuildModelRootVisualInstance(modelRoot);
            return changed;
        }

        private static bool RebuildModelRootVisualInstance(Transform modelRoot)
        {
            bool changed = false;
            for (int i = modelRoot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(modelRoot.GetChild(i).gameObject);
                changed = true;
            }

            GameObject sourceVisual = InstantiateMaterializedSourceVisual();
            if (sourceVisual == null)
            {
                return changed;
            }

            HideNonGunVisualParts(sourceVisual.transform);
            RemapAllRenderersToCcsMaterials(sourceVisual.transform);
            StripImportedRuntimeComponents(sourceVisual);

            sourceVisual.name = CCS_WeaponsConstants.RevolverMaterializedVisualChildName;
            Transform visualTransform = sourceVisual.transform;
            visualTransform.SetParent(modelRoot, false);

            if (visualTransform.localPosition != Vector3.zero)
            {
                visualTransform.localPosition = Vector3.zero;
                changed = true;
            }

            if (visualTransform.localRotation != Quaternion.identity)
            {
                visualTransform.localRotation = Quaternion.identity;
                changed = true;
            }

            if (visualTransform.localScale != Vector3.one)
            {
                visualTransform.localScale = Vector3.one;
                changed = true;
            }

            changed |= DisableCollidersRecursive(modelRoot);
            return changed;
        }

        private static bool RebuildMaterializedVisualHierarchy(GameObject prefabRoot)
        {
            ClearChildren(prefabRoot.transform);
            GameObject sourceVisual = InstantiateMaterializedSourceVisual();
            if (sourceVisual == null)
            {
                return false;
            }

            sourceVisual.name = CCS_WeaponsConstants.RevolverMaterializedVisualChildName;
            sourceVisual.transform.SetParent(prefabRoot.transform, false);
            HideNonGunVisualParts(sourceVisual.transform);
            RemapAllRenderersToCcsMaterials(sourceVisual.transform);
            DisableCollidersRecursive(sourceVisual.transform);
            return true;
        }

        private static GameObject InstantiateMaterializedSourceVisual()
        {
            GameObject vendorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VendorGunPrefabPath);
            if (vendorPrefab != null)
            {
                GameObject vendorInstance = Object.Instantiate(vendorPrefab);
                StripImportedRuntimeComponents(vendorInstance);
                return vendorInstance;
            }

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879ModelAssetPath);
            if (modelAsset == null)
            {
                Debug.LogError("[Weapons Visual Builder] Missing revolver model and vendor prefab.");
                return null;
            }

            GameObject modelInstance = Object.Instantiate(modelAsset);
            StripImportedRuntimeComponents(modelInstance);
            return modelInstance;
        }

        private static bool RemoveLegacyVisualChildren(Transform prefabRoot)
        {
            bool changed = false;
            Transform legacyMesh = prefabRoot.Find("RevolverMesh");
            if (legacyMesh != null)
            {
                Object.DestroyImmediate(legacyMesh.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveDuplicateRootVisualBranches(Transform prefabRoot)
        {
            bool changed = false;
            string modelRootName = CCS_WeaponsConstants.RevolverModelRootObjectName;
            for (int i = prefabRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = prefabRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name == modelRootName)
                {
                    continue;
                }

                if (child.name == "RevolverMesh"
                    || child.name == "Body"
                    || child.name == "Revolver_Mesh"
                    || child.Find("Revolver_Mesh") != null)
                {
                    Object.DestroyImmediate(child.gameObject, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.GetChild(i).gameObject);
            }
        }

        private static void HideNonGunVisualParts(Transform root)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child == root)
                {
                    continue;
                }

                if (ShouldHideVisualPart(child.name))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static bool ShouldHideVisualPart(string objectName)
        {
            for (int i = 0; i < HiddenVisualPartNameTokens.Length; i++)
            {
                if (objectName.Contains(HiddenVisualPartNameTokens[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static CCS_RevolverVisualDefinition LoadVisualDefinitionForBuild()
        {
            CCS_RevolverVisualDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RevolverVisualDefinition>(
                CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
            if (definition == null)
            {
                Debug.LogError(
                    "[Weapons Visual Builder] Missing visual definition at "
                    + CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath);
            }

            return definition;
        }

        private static bool EnsureEquippedAnchorPoints(
            Transform prefabRoot,
            CCS_RevolverVisualDefinition visualDefinition)
        {
            bool changed = false;
            changed |= EnsureChildTransform(
                prefabRoot,
                CCS_WeaponsConstants.MuzzlePointObjectName,
                visualDefinition.MuzzleLocalPosition,
                Vector3.zero);
            changed |= EnsureChildTransform(
                prefabRoot,
                CCS_WeaponsConstants.ShellEjectPointObjectName,
                visualDefinition.ShellEjectLocalPosition,
                new Vector3(0f, 0f, 45f));
            changed |= EnsureChildTransform(
                prefabRoot,
                CCS_WeaponsConstants.BulletVisualSpawnPointObjectName,
                visualDefinition.BulletVisualSpawnLocalPosition,
                Vector3.zero);
            changed |= EnsureChildTransform(
                prefabRoot,
                CCS_WeaponsConstants.CylinderPointObjectName,
                CCS_WeaponsConstants.DefaultCylinderLocalPosition,
                Vector3.zero);
            return changed;
        }

        private static bool EnsureShellMeshVisual(
            Transform prefabRoot,
            string modelAssetPath,
            string materialAssetPath,
            string childName,
            Vector3 localScale)
        {
            Transform meshRoot = prefabRoot.Find(childName);
            bool changed = false;
            if (meshRoot == null)
            {
                GameObject meshObject = InstantiateShellModelVisual(modelAssetPath, materialAssetPath);
                if (meshObject == null)
                {
                    return false;
                }

                meshObject.name = childName;
                meshRoot = meshObject.transform;
                meshRoot.SetParent(prefabRoot, false);
                changed = true;
            }

            if (meshRoot.localScale != localScale)
            {
                meshRoot.localScale = localScale;
                changed = true;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            changed |= RemapRenderersToMaterial(meshRoot, material);
            changed |= DisableCollidersRecursive(meshRoot);
            return changed;
        }

        private static GameObject InstantiateShellModelVisual(string modelAssetPath, string materialAssetPath)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelAssetPath);
            if (modelAsset == null)
            {
                Debug.LogError("[Weapons Visual Builder] Missing shell model at " + modelAssetPath);
                return null;
            }

            GameObject instance = Object.Instantiate(modelAsset);
            StripNonVisualComponents(instance);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            RemapRenderersToMaterial(instance.transform, material);
            return instance;
        }

        private static bool EnsureChildTransform(
            Transform parent,
            string childName,
            Vector3 localPosition,
            Vector3 localEuler)
        {
            Transform child = parent.Find(childName);
            bool changed = false;
            if (child == null)
            {
                GameObject childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(parent, false);
                changed = true;
            }

            if (child.localPosition != localPosition)
            {
                child.localPosition = localPosition;
                changed = true;
            }

            Quaternion expectedRotation = Quaternion.Euler(localEuler);
            if (child.localRotation != expectedRotation)
            {
                child.localRotation = expectedRotation;
                changed = true;
            }

            return changed;
        }

        private static bool StripImportedRuntimeComponents(GameObject root)
        {
            bool changed = StripVendorScripts(root);
            changed |= RemoveComponentsOfType<CCS_RevolverController>(root);
            changed |= RemoveComponentsOfType<CCS_WeaponPickupInteractable>(root, allowOnWorldPickup: false);
            changed |= RemoveComponentsOfType<CCS_InteractableLabelTarget>(root);
            return changed;
        }

        private static bool RemoveInteractionComponents(GameObject root)
        {
            bool changed = false;
            CCS_WeaponPickupInteractable[] pickups = root.GetComponentsInChildren<CCS_WeaponPickupInteractable>(true);
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] != null)
                {
                    Object.DestroyImmediate(pickups[i], true);
                    changed = true;
                }
            }

            CCS_InteractableLabelTarget[] labels = root.GetComponentsInChildren<CCS_InteractableLabelTarget>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null)
                {
                    Object.DestroyImmediate(labels[i], true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool StripGameplayComponents(GameObject root)
        {
            return StripImportedRuntimeComponents(root);
        }

        private static bool StripVendorScripts(GameObject root)
        {
            bool changed = false;
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(behaviour));
                if (string.IsNullOrEmpty(scriptPath))
                {
                    continue;
                }

                scriptPath = scriptPath.Replace('\\', '/');
                if (scriptPath.Contains("Reichsrevolver_M1879")
                    || scriptPath.EndsWith("RevolverController.cs"))
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Object.DestroyImmediate(animators[i], true);
                changed = true;
            }

            Animation[] animations = root.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < animations.Length; i++)
            {
                Object.DestroyImmediate(animations[i], true);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveComponentsOfType<T>(GameObject root, bool allowOnWorldPickup = true)
            where T : Component
        {
            bool changed = false;
            T[] components = root.GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component == null)
                {
                    continue;
                }

                if (!allowOnWorldPickup && component is CCS_WeaponPickupInteractable)
                {
                    continue;
                }

                Object.DestroyImmediate(component, true);
                changed = true;
            }

            return changed;
        }

        private static void StripNonVisualComponents(GameObject root)
        {
            StripImportedRuntimeComponents(root);

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i], true);
            }
        }

        private static bool RemapAllRenderersToCcsMaterials(Transform root)
        {
            Material primaryMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.RevolverM1879MaterialAssetPath);
            Material metalMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.RevolverM1879MetalMaterialAssetPath);
            Material woodMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.RevolverM1879WoodGripMaterialAssetPath);
            if (primaryMaterial == null)
            {
                return false;
            }

            bool changed = false;
            SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = skinnedRenderers[i];
                if (renderer == null || ShouldHideVisualPart(renderer.gameObject.name))
                {
                    continue;
                }

                Material targetMaterial = ResolveGunMaterialForRenderer(renderer.gameObject.name, primaryMaterial, metalMaterial, woodMaterial);
                if (ApplyMaterialToRenderer(renderer, targetMaterial))
                {
                    changed = true;
                }
            }

            MeshRenderer[] meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshRenderer renderer = meshRenderers[i];
                if (renderer == null || ShouldHideVisualPart(renderer.gameObject.name))
                {
                    continue;
                }

                Material targetMaterial = ResolveGunMaterialForRenderer(renderer.gameObject.name, primaryMaterial, metalMaterial, woodMaterial);
                if (ApplyMaterialToRenderer(renderer, targetMaterial))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static Material ResolveGunMaterialForRenderer(
            string objectName,
            Material primaryMaterial,
            Material metalMaterial,
            Material woodMaterial)
        {
            if (objectName.Contains("Grip") && woodMaterial != null)
            {
                return woodMaterial;
            }

            if ((objectName.Contains("Body")
                    || objectName.Contains("Cylinder")
                    || objectName.Contains("Hammer")
                    || objectName.Contains("Trigger")
                    || objectName.Contains("Ring")
                    || objectName.Contains("Reloader"))
                && metalMaterial != null)
            {
                return metalMaterial;
            }

            return primaryMaterial;
        }

        private static bool ApplyMaterialToRenderer(Renderer renderer, Material material)
        {
            if (renderer == null || material == null)
            {
                return false;
            }

            Material[] materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != material)
                {
                    materials[i] = material;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
            }

            return changed;
        }

        private static bool RemapRenderersToMaterial(Transform root, Material material)
        {
            if (material == null)
            {
                return false;
            }

            bool changed = false;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (ApplyMaterialToRenderer(renderers[i], material))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool DisableCollidersRecursive(Transform root)
        {
            bool changed = false;
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && collider.enabled)
                {
                    collider.enabled = false;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool RemapLitMaterialTextures(
            Material material,
            Texture albedo,
            Texture normal,
            Texture metallic,
            Texture occlusion)
        {
            if (material == null)
            {
                return false;
            }

            bool changed = false;
            if (albedo != null && material.GetTexture("_BaseMap") != albedo)
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTexture("_MainTex", albedo);
                changed = true;
            }

            if (normal != null && material.GetTexture("_BumpMap") != normal)
            {
                material.SetTexture("_BumpMap", normal);
                changed = true;
            }

            if (metallic != null && material.GetTexture("_MetallicGlossMap") != metallic)
            {
                material.SetTexture("_MetallicGlossMap", metallic);
                changed = true;
            }

            if (occlusion != null && material.GetTexture("_OcclusionMap") != occlusion)
            {
                material.SetTexture("_OcclusionMap", occlusion);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(material);
            }

            return changed;
        }

        private static Bounds CalculateVisualBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.position, Vector3.one * 0.35f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static GameObject LoadOrCreatePrefabRoot(string prefabPath)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return PrefabUtility.LoadPrefabContents(prefabPath);
            }

            string directory = Path.GetDirectoryName(prefabPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            GameObject root = new GameObject(prefabName);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return PrefabUtility.LoadPrefabContents(prefabPath);
        }

        private static bool SavePrefabIfChanged(GameObject prefabRoot, string prefabPath, bool changed)
        {
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool CopyAssetIfMissing(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.Replace('\\', '/');
            destinationPath = destinationPath.Replace('\\', '/');
            if (!File.Exists(sourcePath))
            {
                string vendorSource = sourcePath.Replace(
                    SourceRootPath,
                    CCS_WeaponsConstants.VendorSourceReichsrevolverRootPath);
                if (File.Exists(vendorSource))
                {
                    sourcePath = vendorSource;
                }
                else
                {
                    return false;
                }
            }

            if (File.Exists(destinationPath))
            {
                return false;
            }

            EnsureFolder(Path.GetDirectoryName(destinationPath)?.Replace('\\', '/'));
            AssetDatabase.CopyAsset(sourcePath, destinationPath);
            return true;
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            assetFolderPath = assetFolderPath?.Replace('\\', '/');
            if (string.IsNullOrEmpty(assetFolderPath) || AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent) && parent.StartsWith("Assets/"))
            {
                EnsureFolder(parent);
            }

            string folderName = Path.GetFileName(assetFolderPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static T EnsureComponent<T>(GameObject targetObject) where T : Component
        {
            T component = targetObject.GetComponent<T>();
            if (component == null)
            {
                component = targetObject.AddComponent<T>();
            }

            return component;
        }

        private static bool SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
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

        private static bool SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.vector3Value == value)
            {
                return false;
            }

            property.vector3Value = value;
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

        #endregion
    }
}
