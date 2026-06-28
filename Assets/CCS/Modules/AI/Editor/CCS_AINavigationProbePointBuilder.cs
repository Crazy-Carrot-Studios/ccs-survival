using CCS.Modules.CharacterController.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// =============================================================================
// SCRIPT: CCS_AINavigationProbePointBuilder
// CATEGORY: Modules / AI / Editor
// PURPOSE: Creates NavMesh probe markers used by AI navigation validation.
// PLACEMENT: Editor utility invoked from AI navigation builder.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Probes mirror Master Test traversal points on sampled NavMesh positions.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AINavigationProbePointBuilder
    {
        public static bool EnsureMasterTestNavigationProbes(GameObject navigationRoot)
        {
            if (navigationRoot == null)
            {
                return false;
            }

            bool changed = false;
            Transform probesRoot = navigationRoot.transform.Find(CCS_AIConstants.NavigationProbesObjectName);
            if (probesRoot == null)
            {
                GameObject probesObject = new GameObject(CCS_AIConstants.NavigationProbesObjectName);
                probesObject.transform.SetParent(navigationRoot.transform, false);
                probesRoot = probesObject.transform;
                changed = true;
            }

            changed |= EnsureProbe(
                probesRoot,
                CCS_AIConstants.NavigationProbeOutsideSpawn,
                CCS_AINavigationProbeId.OutsideSpawn,
                ResolveSpawnPosition());

            changed |= EnsureProbe(
                probesRoot,
                CCS_AIConstants.NavigationProbeBuildingDoor,
                CCS_AINavigationProbeId.BuildingDoor,
                ResolveTraversalPosition("TP_DoorOutside"));

            changed |= EnsureProbe(
                probesRoot,
                CCS_AIConstants.NavigationProbeInsideBuilding,
                CCS_AINavigationProbeId.InsideBuilding,
                ResolveTraversalPosition("TP_CoverInside"));

            changed |= EnsureProbe(
                probesRoot,
                CCS_AIConstants.NavigationProbeTopOfStairs,
                CCS_AINavigationProbeId.TopOfStairs,
                ResolveTraversalPosition("TP_StairsTop"));

            changed |= EnsureProbe(
                probesRoot,
                CCS_AIConstants.NavigationProbeRampTop,
                CCS_AINavigationProbeId.RampTop,
                ResolveTraversalPosition("TP_RampTop"));

            return changed;
        }

        public static bool TryResolveProbePosition(CCS_AINavigationProbeId probeId, out Vector3 position)
        {
            position = Vector3.zero;
            GameObject navigationRoot = GameObject.Find(CCS_AIConstants.NavigationRootObjectName);
            if (navigationRoot == null)
            {
                return false;
            }

            Transform probesRoot = navigationRoot.transform.Find(CCS_AIConstants.NavigationProbesObjectName);
            if (probesRoot == null)
            {
                return false;
            }

            CCS_AINavigationProbePoint[] probes = probesRoot.GetComponentsInChildren<CCS_AINavigationProbePoint>(true);
            for (int i = 0; i < probes.Length; i++)
            {
                CCS_AINavigationProbePoint probe = probes[i];
                if (probe == null || probe.ProbeId != probeId)
                {
                    continue;
                }

                position = probe.transform.position;
                return true;
            }

            return false;
        }

        private static bool EnsureProbe(
            Transform probesRoot,
            string objectName,
            CCS_AINavigationProbeId probeId,
            Vector3 worldPosition)
        {
            Transform existing = probesRoot.Find(objectName);
            GameObject probeObject;
            if (existing == null)
            {
                probeObject = new GameObject(objectName);
                probeObject.transform.SetParent(probesRoot, false);
            }
            else
            {
                probeObject = existing.gameObject;
            }

            bool changed = existing == null;
            if ((probeObject.transform.position - worldPosition).sqrMagnitude > 0.0001f)
            {
                probeObject.transform.position = worldPosition;
                changed = true;
            }

            CCS_AINavigationProbePoint probePoint = probeObject.GetComponent<CCS_AINavigationProbePoint>();
            if (probePoint == null)
            {
                probePoint = probeObject.AddComponent<CCS_AINavigationProbePoint>();
                changed = true;
            }

            if (probePoint.ProbeId != probeId)
            {
                probePoint.Configure(probeId);
                changed = true;
            }

            return changed;
        }

        private static Vector3 ResolveSpawnPosition()
        {
            GameObject hostSpawn = GameObject.Find("TP_Spawn_Host");
            Vector3 spawnOrigin = hostSpawn != null ? hostSpawn.transform.position : Vector3.zero;
            if (NavMesh.SamplePosition(spawnOrigin, out NavMeshHit hit, 12f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return spawnOrigin;
        }

        private static Vector3 ResolveTraversalPosition(string traversalPointName)
        {
            GameObject traversalPoint = GameObject.Find(traversalPointName);
            Vector3 source = traversalPoint != null
                ? traversalPoint.transform.position
                : ResolveFallbackTraversalPosition(traversalPointName);
            if (NavMesh.SamplePosition(source, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return source;
        }

        private static Vector3 ResolveFallbackTraversalPosition(string traversalPointName)
        {
            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames.Length; i++)
            {
                if (CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames[i] == traversalPointName)
                {
                    return CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointPositions[i];
                }
            }

            return Vector3.zero;
        }
    }
}
