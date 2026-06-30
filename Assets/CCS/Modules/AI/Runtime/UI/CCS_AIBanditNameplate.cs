using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AIBanditNameplate
// CATEGORY: Modules / AI / Runtime / UI
// PURPOSE: World-space billboard nameplate with health bar for AI bandits.
// PLACEMENT: AI bandit root.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Client-local billboard only. Name always comes from AI constants/profile.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(180)]
    public sealed class CCS_AIBanditNameplate : MonoBehaviour
    {
        [SerializeField] private CCS_NetworkHealth networkHealth;
        [SerializeField] private Transform nameplateAnchor;
        [SerializeField] private Transform nameplateRoot;
        [SerializeField] private RectTransform healthFillRect;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private bool invertBillboardForward = true;
        [SerializeField] private bool debugNameplateRuntime;

        private Transform canvasTransform;
        private Canvas worldCanvas;
        private Camera cachedCamera;
        private bool nameplateBuilt;
        private bool runtimeDiagnosticLogged;
        private bool boundHealthSourceLogged;
        private float lastDisplayedHealth = -1f;
        private float lastDisplayedMaxHealth = -1f;

        private void Awake()
        {
            ResolveReferences();
            EnsureRuntimeNameplate();
            BindHealthEvents();
            UpdateNameplate(force: true);
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (!IsNameplateHierarchyValid())
            {
                nameplateBuilt = false;
                nameplateAnchor = null;
                nameplateRoot = null;
                canvasTransform = null;
                worldCanvas = null;
                healthFillRect = null;
                healthFillImage = null;
                healthSlider = null;
                nameText = null;
            }

            EnsureRuntimeNameplate();
            BindHealthEvents();
            UpdateNameplate(force: true);
        }

        private void OnDisable()
        {
            UnbindHealthEvents();
        }

        private void LateUpdate()
        {
            ResolveReferences();
            if (!IsNameplateHierarchyValid())
            {
                nameplateBuilt = false;
                EnsureRuntimeNameplate();
            }

            UpdateNameplate(force: false);
            BillboardToLocalCamera();
        }

        public void EnsureRuntimeNameplate()
        {
            if (nameplateBuilt && IsNameplateHierarchyValid())
            {
                return;
            }

            ReparentAwayFromForbiddenParents();

            nameplateAnchor = CCS_AIBanditNameplateUiFactory.EnsureNameplateAnchor(transform);
            nameplateRoot = CCS_AIBanditNameplateUiFactory.EnsureNameplateRoot(nameplateAnchor);
            canvasTransform = CCS_AIBanditNameplateUiFactory.EnsureCanvasRoot(nameplateRoot);
            healthFillImage = CCS_AIBanditNameplateUiFactory.EnsureHealthBar(canvasTransform, out healthFillRect);
            nameText = CCS_AIBanditNameplateUiFactory.EnsureNameText(canvasTransform);
            DisableHealthSliderIfPresent(canvasTransform);
            worldCanvas = canvasTransform != null ? canvasTransform.GetComponent<Canvas>() : null;

            if (worldCanvas != null)
            {
                worldCanvas.gameObject.SetActive(true);
                worldCanvas.enabled = true;
                worldCanvas.overrideSorting = true;
                worldCanvas.sortingOrder = CCS_AIConstants.NameplateCanvasSortingOrder;
            }

            nameplateBuilt = IsNameplateHierarchyValid();
            LogRuntimeDiagnosticOnce();
        }

        public void SetNameplateVisible(bool visible)
        {
            if (IsTransformAlive(nameplateAnchor))
            {
                nameplateAnchor.gameObject.SetActive(visible);
            }
        }

        private void BindHealthEvents()
        {
            if (networkHealth == null)
            {
                return;
            }

            networkHealth.HealthChanged -= HandleHealthChanged;
            networkHealth.HealthChanged += HandleHealthChanged;
            networkHealth.DeadStateChanged -= HandleDeadStateChanged;
            networkHealth.DeadStateChanged += HandleDeadStateChanged;
        }

        private void UnbindHealthEvents()
        {
            if (networkHealth == null)
            {
                return;
            }

            networkHealth.HealthChanged -= HandleHealthChanged;
            networkHealth.DeadStateChanged -= HandleDeadStateChanged;
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateNameplate(force: true);
        }

        private void HandleDeadStateChanged(bool isDead)
        {
            UpdateNameplate(force: true);
        }

        private void ResolveReferences()
        {
            if (networkHealth == null || networkHealth.transform != transform)
            {
                networkHealth = GetComponent<CCS_NetworkHealth>();
            }

            if (debugNameplateRuntime && !boundHealthSourceLogged && networkHealth != null)
            {
                boundHealthSourceLogged = true;
                Debug.Log(
                    "[AI Bandit Nameplate] Bound health source: " + networkHealth.gameObject.name,
                    this);
            }

            if (!IsTransformAlive(nameplateAnchor))
            {
                nameplateAnchor = transform.Find(CCS_AIConstants.NameplateAnchorObjectName);
            }

            if (!IsTransformAlive(nameplateRoot))
            {
                nameplateRoot = FindNameplateRoot();
            }

            canvasTransform = ResolveCanvasTransform();
            if (canvasTransform != null)
            {
                worldCanvas = canvasTransform.GetComponent<Canvas>();
                Transform fillTransform = canvasTransform.Find(
                    CCS_AIConstants.NameplateHealthBackgroundObjectName + "/"
                    + CCS_AIConstants.NameplateHealthFillObjectName);
                if (fillTransform == null)
                {
                    Transform background = canvasTransform.Find(CCS_AIConstants.NameplateHealthBackgroundObjectName);
                    fillTransform = background != null
                        ? background.Find(CCS_AIConstants.NameplateHealthFillObjectName)
                        : null;
                }

                if (fillTransform != null)
                {
                    healthFillRect = fillTransform.GetComponent<RectTransform>();
                    healthFillImage = fillTransform.GetComponent<Image>();
                }

                if (nameText == null)
                {
                    Transform textTransform = canvasTransform.Find(CCS_AIConstants.NameplateNameTextObjectName);
                    nameText = textTransform != null ? textTransform.GetComponent<TMP_Text>() : null;
                }
            }
        }

        private Transform FindNameplateRoot()
        {
            if (IsTransformAlive(nameplateAnchor))
            {
                Transform anchoredRoot = nameplateAnchor.Find(CCS_AIConstants.NameplateRootObjectName);
                if (IsTransformAlive(anchoredRoot))
                {
                    return anchoredRoot;
                }
            }

            Transform direct = transform.Find(CCS_AIConstants.NameplateRootObjectName);
            if (IsTransformAlive(direct))
            {
                return direct;
            }

            Transform legacy = transform.Find("AIBanditNameplateRoot");
            if (IsTransformAlive(legacy))
            {
                return legacy;
            }

            return null;
        }

        private Transform ResolveCanvasTransform()
        {
            if (!IsTransformAlive(nameplateRoot))
            {
                return null;
            }

            Transform canvasRoot = nameplateRoot.Find(CCS_AIConstants.NameplateCanvasObjectName);
            if (IsTransformAlive(canvasRoot))
            {
                return canvasRoot;
            }

            if (nameplateRoot.GetComponent<Canvas>() != null)
            {
                return nameplateRoot;
            }

            return null;
        }

        private void ReparentAwayFromForbiddenParents()
        {
            Transform existingRoot = FindNameplateRoot();
            if (!IsTransformAlive(existingRoot) || !IsUnderForbiddenParent(existingRoot))
            {
                return;
            }

            Transform anchor = CCS_AIBanditNameplateUiFactory.EnsureNameplateAnchor(transform);
            existingRoot.SetParent(anchor, false);
            existingRoot.localPosition = Vector3.zero;
            existingRoot.localRotation = Quaternion.identity;
            existingRoot.localScale = Vector3.one;
        }

        private bool IsNameplateHierarchyValid()
        {
            return IsTransformAlive(nameplateAnchor)
                && IsTransformAlive(nameplateRoot)
                && IsTransformAlive(canvasTransform)
                && healthFillRect != null
                && nameText != null
                && !IsUnderForbiddenParent(nameplateRoot);
        }

        private void UpdateNameplate(bool force)
        {
            ResolveReferences();
            if (networkHealth == null || networkHealth.transform != transform || healthFillRect == null || nameText == null)
            {
                return;
            }

            float currentHealth = networkHealth.CurrentHealth;
            float maxHealth = Mathf.Max(1f, networkHealth.MaxHealth);
            if (!force
                && Mathf.Approximately(currentHealth, lastDisplayedHealth)
                && Mathf.Approximately(maxHealth, lastDisplayedMaxHealth))
            {
                return;
            }

            lastDisplayedHealth = currentHealth;
            lastDisplayedMaxHealth = maxHealth;
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
            SetHealthPercent(healthPercent);
            nameText.text = networkHealth.IsDead
                ? CCS_AIConstants.AIBanditLabel + " [DEAD]"
                : CCS_AIConstants.AIBanditLabel;

            if (debugNameplateRuntime)
            {
                Debug.Log(
                    "[AI Bandit Nameplate] Health UI update: "
                    + currentHealth.ToString("0")
                    + "/"
                    + maxHealth.ToString("0")
                    + " => "
                    + healthPercent.ToString("0.00"),
                    this);
            }
        }

        public void SetHealthPercent(float percent)
        {
            ResolveReferences();
            if (healthFillRect == null)
            {
                return;
            }

            if (healthFillImage == null)
            {
                healthFillImage = healthFillRect.GetComponent<Image>();
            }

            percent = Mathf.Clamp01(percent);

            // Right-anchored fill drains camera-left to camera-right on the billboard canvas.
            healthFillRect.anchorMin = new Vector2(1f - percent, 0f);
            healthFillRect.anchorMax = new Vector2(1f, 1f);
            healthFillRect.pivot = new Vector2(1f, 0.5f);
            healthFillRect.offsetMin = Vector2.zero;
            healthFillRect.offsetMax = Vector2.zero;
            healthFillRect.anchoredPosition = Vector2.zero;

            if (healthFillImage != null)
            {
                healthFillImage.type = Image.Type.Filled;
                healthFillImage.fillMethod = Image.FillMethod.Horizontal;
                healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
                healthFillImage.fillAmount = percent;
            }

            if (healthSlider != null)
            {
                healthSlider.enabled = false;
            }
        }

        public void RefreshHealthDisplayFromNetworkHealth()
        {
            ResolveReferences();
            BindHealthEvents();
            UpdateNameplate(force: true);
        }

        private static void DisableHealthSliderIfPresent(Transform canvasRoot)
        {
            if (canvasRoot == null)
            {
                return;
            }

            Transform sliderTransform = canvasRoot.Find(CCS_AIConstants.NameplateHealthSliderObjectName);
            if (sliderTransform != null)
            {
                Slider slider = sliderTransform.GetComponent<Slider>();
                if (slider != null)
                {
                    slider.enabled = false;
                }

                sliderTransform.gameObject.SetActive(false);
            }
        }

        private void BillboardToLocalCamera()
        {
            if (!IsTransformAlive(canvasTransform))
            {
                return;
            }

            Camera targetCamera = ResolveLocalCamera();
            if (targetCamera == null)
            {
                return;
            }

            if (worldCanvas != null)
            {
                if (worldCanvas.worldCamera != targetCamera)
                {
                    worldCanvas.worldCamera = targetCamera;
                }

                worldCanvas.enabled = true;
            }

            Vector3 lookDirection = invertBillboardForward
                ? targetCamera.transform.position - canvasTransform.position
                : canvasTransform.position - targetCamera.transform.position;
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            canvasTransform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }

        private Camera ResolveLocalCamera()
        {
            if (cachedCamera != null && cachedCamera.isActiveAndEnabled)
            {
                return cachedCamera;
            }

            if (CCS_CharacterMovementCameraContext.HasActiveCamera)
            {
                cachedCamera = CCS_CharacterMovementCameraContext.ActiveCamera;
                return cachedCamera;
            }

            CCS_CharacterCameraController[] cameraControllers =
                FindObjectsByType<CCS_CharacterCameraController>(FindObjectsSortMode.None);
            for (int i = 0; i < cameraControllers.Length; i++)
            {
                CCS_CharacterCameraController controller = cameraControllers[i];
                if (controller == null)
                {
                    continue;
                }

                Camera outputCamera = controller.GetOutputCamera();
                if (outputCamera != null && outputCamera.isActiveAndEnabled)
                {
                    cachedCamera = outputCamera;
                    return cachedCamera;
                }
            }

            cachedCamera = Camera.main;
            if (cachedCamera == null)
            {
                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                for (int i = 0; i < cameras.Length; i++)
                {
                    Camera candidate = cameras[i];
                    if (candidate != null && candidate.isActiveAndEnabled && candidate.CompareTag("MainCamera"))
                    {
                        cachedCamera = candidate;
                        break;
                    }
                }
            }

            return cachedCamera;
        }

        private void LogRuntimeDiagnosticOnce()
        {
            if (!debugNameplateRuntime || runtimeDiagnosticLogged)
            {
                return;
            }

            runtimeDiagnosticLogged = true;
            if (!IsNameplateHierarchyValid())
            {
                Debug.LogError(
                    "[AI Bandit Nameplate] Failed to create visible nameplate: hierarchy incomplete or invalid parent.",
                    this);
                return;
            }

            Camera targetCamera = ResolveLocalCamera();
            float healthPercent = networkHealth != null
                ? Mathf.Clamp01(networkHealth.CurrentHealth / Mathf.Max(1f, networkHealth.MaxHealth))
                : 1f;
            Debug.Log(
                "[AI Bandit Nameplate] Runtime UI ready:\n"
                + "Text=" + nameText.text + "\n"
                + "CanvasActive=" + worldCanvas.gameObject.activeSelf + "\n"
                + "CanvasEnabled=" + worldCanvas.enabled + "\n"
                + "HasHealthFill=" + (healthFillRect != null) + "\n"
                + "HealthPercent=" + healthPercent.ToString("0.00") + "\n"
                + "WorldPos=" + canvasTransform.position + "\n"
                + "Camera=" + (targetCamera != null ? targetCamera.name : "null") + "\n"
                + "Scale=" + canvasTransform.localScale,
                this);
        }

        private static bool IsUnderForbiddenParent(Transform transformReference)
        {
            Transform current = transformReference;
            while (current != null)
            {
                string objectName = current.name;
                if (objectName == "Model" || objectName == "VisualRoot" || objectName == "Armature")
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool IsTransformAlive(Transform transformReference)
        {
            return transformReference != null;
        }
    }
}
