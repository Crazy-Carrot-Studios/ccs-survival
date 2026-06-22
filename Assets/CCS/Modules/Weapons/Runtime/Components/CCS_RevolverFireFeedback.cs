using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverFireFeedback
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Lightweight test-visible hitscan tracer for manual weapon playtests.
// PLACEMENT: Child of MuzzlePoint on PF_CCS_CharacterController_TestPlayer_Networked.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Tracer always starts at muzzlePoint. Never uses player root as origin.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(130)]
    public sealed class CCS_RevolverFireFeedback : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_RevolverController revolverController;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private float tracerDurationSeconds = 0.12f;
        [SerializeField] private float tracerWidth = 0.035f;
        [SerializeField] private Color tracerColor = new Color(1f, 0.85f, 0.2f, 1f);

        private LineRenderer lineRenderer;
        private float tracerEndTime;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            EnsureLineRenderer();
        }

        private void OnEnable()
        {
            if (revolverController != null)
            {
                revolverController.FireResolved += HandleFireResolved;
            }
        }

        private void OnDisable()
        {
            if (revolverController != null)
            {
                revolverController.FireResolved -= HandleFireResolved;
            }

            HideTracer();
        }

        private void Update()
        {
            if (lineRenderer == null || !lineRenderer.enabled)
            {
                return;
            }

            if (Time.time >= tracerEndTime)
            {
                HideTracer();
            }
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (revolverController == null)
            {
                revolverController = GetComponentInParent<CCS_RevolverController>();
            }

            if (muzzlePoint == null)
            {
                muzzlePoint = transform;
            }

            if (muzzlePoint == transform.root)
            {
                Transform resolvedMuzzle = FindDeepChild(transform.root, CCS_WeaponsConstants.MuzzlePointObjectName);
                if (resolvedMuzzle != null)
                {
                    muzzlePoint = resolvedMuzzle;
                }
            }
        }

        private void EnsureLineRenderer()
        {
            Transform lineHost = muzzlePoint != null ? muzzlePoint : transform;
            lineRenderer = lineHost.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = lineHost.gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = tracerWidth;
            lineRenderer.endWidth = tracerWidth * 0.35f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = tracerColor;
            lineRenderer.endColor = tracerColor;
        }

        private void HandleFireResolved(CCS_RevolverFireResultEvent fireResultEvent)
        {
            if (fireResultEvent.WasDryFire)
            {
                return;
            }

            Transform activeMuzzle = revolverController != null
                ? revolverController.MuzzlePointTransform
                : muzzlePoint;
            if (activeMuzzle == null)
            {
                return;
            }

            Vector3 start = fireResultEvent.HitscanResult.RayOrigin;
            if (Vector3.Distance(start, activeMuzzle.position) > 0.05f)
            {
                start = activeMuzzle.position;
            }

            Vector3 end = fireResultEvent.HitscanResult.DidHit
                ? fireResultEvent.HitscanResult.HitPoint
                : start + (fireResultEvent.HitscanResult.RayDirection * 60f);

            ShowTracer(start, end);
        }

        private void ShowTracer(Vector3 start, Vector3 end)
        {
            EnsureLineRenderer();
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.enabled = true;
            tracerEndTime = Time.time + tracerDurationSeconds;
        }

        private void HideTracer()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
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

        #endregion
    }
}
