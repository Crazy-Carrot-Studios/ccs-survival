using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeCarcassSpawnUtility
// CATEGORY: Modules / Wildlife / Runtime / Health
// PURPOSE: Spawns harvestable carcass placeholders when living wildlife dies.
// PLACEMENT: Called by CCS_WildlifeDamageable after combat kills.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Primitive placeholders only. No prefab art or pooling in 0.9.8 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public static class CCS_WildlifeCarcassSpawnUtility
    {
        #region Public Methods

        public static GameObject SpawnCarcass(
            Vector3 worldPosition,
            Quaternion worldRotation,
            CCS_WildlifeDefinition carcassDefinition,
            CCS_WildlifeProfile wildlifeProfile,
            PrimitiveType primitiveType,
            Vector3 localScale,
            string objectName)
        {
            if (carcassDefinition == null)
            {
                return null;
            }

            GameObject carcassObject = GameObject.CreatePrimitive(primitiveType);
            carcassObject.name = string.IsNullOrWhiteSpace(objectName)
                ? carcassDefinition.DisplayName
                : objectName;
            carcassObject.transform.SetPositionAndRotation(worldPosition, worldRotation);
            carcassObject.transform.localScale = localScale;

            Collider collider = carcassObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            CCS_HarvestableWildlife harvestable = carcassObject.GetComponent<CCS_HarvestableWildlife>();
            if (harvestable == null)
            {
                harvestable = carcassObject.AddComponent<CCS_HarvestableWildlife>();
            }

            harvestable.ConfigureCarcass(carcassDefinition, wildlifeProfile);
            return carcassObject;
        }

        #endregion
    }
}
