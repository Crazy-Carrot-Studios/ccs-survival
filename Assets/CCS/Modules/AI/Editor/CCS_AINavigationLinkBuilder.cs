using CCS.Modules.CharacterController.Editor;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// =============================================================================
// SCRIPT: CCS_AINavigationLinkBuilder
// CATEGORY: Modules / AI / Editor
// PURPOSE: Ensures NavMeshLink connections for stairs, ramps, and door transitions.
// PLACEMENT: Editor utility invoked from AI navigation builder.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Links connect disconnected walkable islands in Master Test building layout.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AINavigationLinkBuilder
    {
        private const float LinkWidth = 0.6f;
        private const int HumanoidAgentTypeId = 0;

        public static bool EnsureMasterTestNavigationLinks(GameObject navigationRoot)
        {
            if (navigationRoot == null)
            {
                return false;
            }

            bool changed = false;
            Transform linksRoot = navigationRoot.transform.Find(CCS_AIConstants.NavigationLinksObjectName);
            if (linksRoot == null)
            {
                GameObject linksObject = new GameObject(CCS_AIConstants.NavigationLinksObjectName);
                linksObject.transform.SetParent(navigationRoot.transform, false);
                linksRoot = linksObject.transform;
                changed = true;
            }

            changed |= EnsureLink(
                linksRoot,
                "Link_StairsBottomToTop",
                ResolveTraversalPosition("TP_StairsBottom"),
                ResolveTraversalPosition("TP_StairsTop"));

            changed |= EnsureLink(
                linksRoot,
                "Link_RampBottomToTop",
                ResolveTraversalPosition("TP_RampBottom"),
                ResolveTraversalPosition("TP_RampTop"));

            changed |= EnsureLink(
                linksRoot,
                "Link_DoorOutsideToInside",
                ResolveTraversalPosition("TP_DoorOutside"),
                ResolveTraversalPosition("TP_DoorInside"));

            changed |= EnsureLink(
                linksRoot,
                "Link_DoorOutsideToCoverInside",
                ResolveTraversalPosition("TP_DoorOutside"),
                ResolveTraversalPosition("TP_CoverInside"));

            GameObject hostSpawn = GameObject.Find("TP_Spawn_Host");
            if (hostSpawn != null)
            {
                changed |= EnsureLink(
                    linksRoot,
                    "Link_SpawnToInsideBuilding",
                    ResolveSpawnPosition(hostSpawn.transform.position),
                    ResolveTraversalPosition("TP_CoverInside"));
            }

            return changed;
        }

        private static Vector3 ResolveSpawnPosition(Vector3 spawnOrigin)
        {
            if (NavMesh.SamplePosition(spawnOrigin, out NavMeshHit hit, 12f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return spawnOrigin;
        }

        public static bool EnsureHumanoidNavMeshAgentSettings()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset");
            if (assets == null || assets.Length == 0)
            {
                return false;
            }

            SerializedObject serializedSettings = new SerializedObject(assets[0]);
            SerializedProperty settingsArray = serializedSettings.FindProperty("m_Settings");
            if (settingsArray == null || !settingsArray.isArray || settingsArray.arraySize <= 0)
            {
                return false;
            }

            SerializedProperty humanoidSettings = settingsArray.GetArrayElementAtIndex(0);
            bool changed = false;
            changed |= SetFloatProperty(humanoidSettings, "agentRadius", 0.35f);
            changed |= SetFloatProperty(humanoidSettings, "agentHeight", 1.8f);
            changed |= SetFloatProperty(humanoidSettings, "agentClimb", 0.45f);
            changed |= SetFloatProperty(humanoidSettings, "agentSlope", 50f);
            if (changed)
            {
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        private static bool EnsureLink(Transform linksRoot, string linkName, Vector3 startWorld, Vector3 endWorld)
        {
            Transform existing = linksRoot.Find(linkName);
            GameObject linkObject;
            if (existing == null)
            {
                linkObject = new GameObject(linkName);
                linkObject.transform.SetParent(linksRoot, false);
            }
            else
            {
                linkObject = existing.gameObject;
            }

            linkObject.transform.position = startWorld;
            NavMeshLink navMeshLink = linkObject.GetComponent<NavMeshLink>();
            if (navMeshLink == null)
            {
                navMeshLink = linkObject.AddComponent<NavMeshLink>();
            }

            bool changed = existing == null || navMeshLink == null;
            Vector3 localEnd = linkObject.transform.InverseTransformPoint(endWorld);
            if (!Mathf.Approximately(navMeshLink.startPoint.x, 0f)
                || !Mathf.Approximately(navMeshLink.startPoint.y, 0f)
                || !Mathf.Approximately(navMeshLink.startPoint.z, 0f))
            {
                navMeshLink.startPoint = Vector3.zero;
                changed = true;
            }

            if ((navMeshLink.endPoint - localEnd).sqrMagnitude > 0.0001f)
            {
                navMeshLink.endPoint = localEnd;
                changed = true;
            }

            if (!Mathf.Approximately(navMeshLink.width, LinkWidth))
            {
                navMeshLink.width = LinkWidth;
                changed = true;
            }

            if (!navMeshLink.bidirectional)
            {
                navMeshLink.bidirectional = true;
                changed = true;
            }

            if (navMeshLink.agentTypeID != HumanoidAgentTypeId)
            {
                navMeshLink.agentTypeID = HumanoidAgentTypeId;
                changed = true;
            }

            navMeshLink.enabled = true;
            return changed;
        }

        private static Vector3 ResolveTraversalPosition(string traversalPointName)
        {
            GameObject traversalPoint = GameObject.Find(traversalPointName);
            if (traversalPoint != null)
            {
                if (NavMesh.SamplePosition(
                    traversalPoint.transform.position,
                    out NavMeshHit hit,
                    4f,
                    NavMesh.AllAreas))
                {
                    return hit.position;
                }

                return traversalPoint.transform.position;
            }

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames.Length; i++)
            {
                if (CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames[i] != traversalPointName)
                {
                    continue;
                }

                Vector3 fallback = CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointPositions[i];
                if (NavMesh.SamplePosition(fallback, out NavMeshHit fallbackHit, 4f, NavMesh.AllAreas))
                {
                    return fallbackHit.position;
                }

                return fallback;
            }

            return Vector3.zero;
        }

        private static bool SetFloatProperty(SerializedProperty parent, string propertyName, float value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }
    }
}
