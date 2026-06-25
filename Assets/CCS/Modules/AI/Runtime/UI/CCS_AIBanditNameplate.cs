using CCS.Modules.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AIBanditNameplate
// CATEGORY: Modules / AI / Runtime / UI
// PURPOSE: World-space billboard nameplate with health slider for AI bandits.
// PLACEMENT: AI bandit root.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Creates simple runtime UI when prefab children are missing.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(180)]
    public sealed class CCS_AIBanditNameplate : MonoBehaviour
    {
        [SerializeField] private CCS_NetworkHealth networkHealth;
        [SerializeField] private Transform nameplateRoot;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 2.15f, 0f);
        [SerializeField] private bool flipFacing = true;

        private Camera cachedCamera;

        private void Awake()
        {
            ResolveReferences();
            EnsureRuntimeNameplate();
            UpdateNameplate();
        }

        private void LateUpdate()
        {
            ResolveReferences();
            UpdateNameplate();
            BillboardToLocalCamera();
        }

        private void ResolveReferences()
        {
            if (networkHealth == null)
            {
                networkHealth = GetComponent<CCS_NetworkHealth>();
            }
        }

        private void EnsureRuntimeNameplate()
        {
            if (nameplateRoot == null)
            {
                Transform existingRoot = transform.Find("AIBanditNameplateRoot");
                if (existingRoot == null)
                {
                    GameObject rootObject = new GameObject("AIBanditNameplateRoot");
                    nameplateRoot = rootObject.transform;
                    nameplateRoot.SetParent(transform, false);
                }
                else
                {
                    nameplateRoot = existingRoot;
                }
            }

            nameplateRoot.localPosition = localOffset;
            nameplateRoot.localRotation = Quaternion.identity;

            Canvas canvas = nameplateRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = nameplateRoot.gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = null;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(180f, 46f);
            canvasRect.localScale = Vector3.one * 0.005f;

            if (nameText == null)
            {
                Transform textTransform = nameplateRoot.Find("BanditName");
                if (textTransform == null)
                {
                    GameObject textObject = new GameObject(
                        "BanditName",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(TextMeshProUGUI));
                    textObject.transform.SetParent(nameplateRoot, false);
                    textTransform = textObject.transform;
                }

                nameText = textTransform.GetComponent<TMP_Text>();
            }

            if (nameText != null)
            {
                RectTransform nameRect = nameText.rectTransform;
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.fontSize = 22f;
                nameText.color = Color.white;
                nameText.text = CCS_AIConstants.AIBanditLabel;
            }

            if (healthSlider == null)
            {
                Transform sliderTransform = nameplateRoot.Find("BanditHealth");
                if (sliderTransform == null)
                {
                    GameObject sliderObject = new GameObject("BanditHealth", typeof(RectTransform), typeof(Slider));
                    sliderObject.transform.SetParent(nameplateRoot, false);
                    sliderTransform = sliderObject.transform;
                }

                healthSlider = sliderTransform.GetComponent<Slider>();
            }

            if (healthSlider != null)
            {
                RectTransform sliderRect = healthSlider.GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0.1f, 0f);
                sliderRect.anchorMax = new Vector2(0.9f, 0.45f);
                sliderRect.offsetMin = Vector2.zero;
                sliderRect.offsetMax = Vector2.zero;

                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.wholeNumbers = false;
                healthSlider.interactable = false;
            }
        }

        private void UpdateNameplate()
        {
            if (networkHealth == null || healthSlider == null)
            {
                return;
            }

            float maxHealth = Mathf.Max(1f, networkHealth.MaxHealth);
            healthSlider.value = Mathf.Clamp01(networkHealth.CurrentHealth / maxHealth);
            if (nameText != null)
            {
                nameText.text = networkHealth.IsDead
                    ? CCS_AIConstants.AIBanditLabel + " [DEAD]"
                    : CCS_AIConstants.AIBanditLabel;
            }
        }

        private void BillboardToLocalCamera()
        {
            if (nameplateRoot == null)
            {
                return;
            }

            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled)
            {
                cachedCamera = Camera.main;
            }

            if (cachedCamera == null)
            {
                return;
            }

            Vector3 lookDirection = nameplateRoot.position - cachedCamera.transform.position;
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            nameplateRoot.rotation = Quaternion.LookRotation(
                flipFacing ? lookDirection.normalized : -lookDirection.normalized,
                Vector3.up);
        }
    }
}
