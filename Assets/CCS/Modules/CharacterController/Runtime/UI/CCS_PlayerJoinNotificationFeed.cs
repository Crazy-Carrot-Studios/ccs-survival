using System.Collections;
using System.Collections.Generic;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.CharacterController.Validation;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_PlayerJoinNotificationFeed
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Top-right join notifications for master test network sessions only.
// PLACEMENT: MasterTestUiCanvas/CCS_PlayerJoinNotificationFeed in SCN_CCS_CharacterController_Validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Hidden in solo play. Panel stays visible for panelVisibleSeconds after each join event.
// =============================================================================

namespace CCS.Modules.CharacterController {
    public sealed class CCS_PlayerJoinNotificationFeed : MonoBehaviour
    {
        #region Variables

        [Header("Feed Settings")]
        [SerializeField] private int maxEntries = CCS_ValidationUiConstants.JoinNotificationMaxEntries;

        [SerializeField] private float entryLifetimeSeconds =
            CCS_ValidationUiConstants.JoinNotificationEntryLifetimeSeconds;

        [SerializeField] private float entryFadeOutSeconds = 0.35f;

        [SerializeField] private float panelVisibleSeconds =
            CCS_ValidationUiConstants.JoinNotificationPanelVisibleSeconds;

        [Header("UI References")]
        [SerializeField] private RectTransform feedRoot;

        [SerializeField] private GameObject panelRoot;

        [SerializeField] private RectTransform entriesContainer;

        [SerializeField] private TMP_Text entryTemplate;

        private readonly List<ActiveEntry> activeEntries = new List<ActiveEntry>();
        private NetworkManager subscribedNetworkManager;
        private bool registeredWithRegistry;
        private float panelHideAtUnscaledTime;
        private Coroutine panelHideRoutine;

        #endregion

        #region Properties

        public int MaxEntries => maxEntries;

        public float EntryLifetimeSeconds => entryLifetimeSeconds;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (feedRoot == null)
            {
                feedRoot = transform as RectTransform;
            }

            ResolvePanelReference();
            ApplyTopRightAnchoring();
            TryRegisterForActiveNetworkSession();
            UpdatePanelVisibility();
        }

        private void Start()
        {
            StartCoroutine(InitializeNetworkVisibility());
        }

        private void OnDisable()
        {
            UnsubscribeNetworkLifecycle();
            UnregisterFromRegistry();
            StopPanelHideRoutine();
            StopAllCoroutines();
            panelHideRoutine = null;
            Clear();
        }

        private void OnDestroy()
        {
            UnsubscribeNetworkLifecycle();
        }

        #endregion

        #region Public Methods

        public void ShowPlayerJoined(string playerName)
        {
            if (!CCS_NetworkSessionUtility.IsNetworkSessionActive() || entriesContainer == null)
            {
                return;
            }

            string sanitizedName = string.IsNullOrWhiteSpace(playerName)
                ? CCS_ValidationUiConstants.DefaultJoinPlayerDisplayName
                : playerName.Trim();

            EnsureEntryCapacity();
            ActiveEntry entry = CreateEntry($"{sanitizedName} joined");
            activeEntries.Add(entry);
            entry.ExpireRoutine = StartCoroutine(ExpireEntryRoutine(entry));
            ExtendPanelVisibility();
        }

        public void Clear()
        {
            StopPanelHideRoutine();
            panelHideAtUnscaledTime = 0f;

            for (int i = activeEntries.Count - 1; i >= 0; i--)
            {
                DestroyEntryVisual(activeEntries[i]);
            }

            activeEntries.Clear();
            UpdatePanelVisibility();
        }

        #endregion

        #region Private Methods

        private IEnumerator InitializeNetworkVisibility()
        {
            yield return null;
            TrySubscribeNetworkLifecycle();
            ApplyNetworkSessionVisibility();
        }

        private void ResolvePanelReference()
        {
            if (panelRoot != null)
            {
                return;
            }

            Transform panelTransform = transform.Find(CCS_ValidationUiConstants.JoinNotificationPanelObjectName);
            panelRoot = panelTransform != null ? panelTransform.gameObject : null;
        }

        private void ApplyTopRightAnchoring()
        {
            if (feedRoot != null)
            {
                feedRoot.anchorMin = new Vector2(1f, 1f);
                feedRoot.anchorMax = new Vector2(1f, 1f);
                feedRoot.pivot = new Vector2(1f, 1f);
                feedRoot.anchoredPosition = Vector2.zero;
                feedRoot.sizeDelta = Vector2.zero;
            }

            if (panelRoot == null)
            {
                return;
            }

            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                return;
            }

            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(
                -CCS_ValidationUiConstants.JoinNotificationPanelMargin,
                -CCS_ValidationUiConstants.JoinNotificationPanelMargin);
            panelRect.sizeDelta = new Vector2(
                CCS_ValidationUiConstants.JoinNotificationPanelWidth,
                CCS_ValidationUiConstants.JoinNotificationPanelMinHeight);
        }

        private void TrySubscribeNetworkLifecycle()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || subscribedNetworkManager == networkManager)
            {
                return;
            }

            UnsubscribeNetworkLifecycle();
            subscribedNetworkManager = networkManager;
            subscribedNetworkManager.OnClientStopped += HandleClientStopped;
        }

        private void UnsubscribeNetworkLifecycle()
        {
            if (subscribedNetworkManager == null)
            {
                return;
            }

            subscribedNetworkManager.OnClientStopped -= HandleClientStopped;
            subscribedNetworkManager = null;
        }

        private void HandleClientStopped(bool _)
        {
            ApplyNetworkSessionVisibility();
        }

        private void ApplyNetworkSessionVisibility()
        {
            if (!CCS_NetworkSessionUtility.IsNetworkSessionActive())
            {
                UnregisterFromRegistry();
                Clear();
                return;
            }

            RegisterWithRegistry();
            UpdatePanelVisibility();
        }

        private void ExtendPanelVisibility()
        {
            if (!CCS_NetworkSessionUtility.IsNetworkSessionActive() || panelRoot == null)
            {
                return;
            }

            panelHideAtUnscaledTime = Time.unscaledTime + panelVisibleSeconds;
            panelRoot.SetActive(true);
            RestartPanelHideRoutine();
        }

        private void RestartPanelHideRoutine()
        {
            StopPanelHideRoutine();
            panelHideRoutine = StartCoroutine(PanelHideRoutine());
        }

        private void StopPanelHideRoutine()
        {
            if (panelHideRoutine == null)
            {
                return;
            }

            StopCoroutine(panelHideRoutine);
            panelHideRoutine = null;
        }

        private IEnumerator PanelHideRoutine()
        {
            float waitSeconds = panelHideAtUnscaledTime - Time.unscaledTime;
            if (waitSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(waitSeconds);
            }

            UpdatePanelVisibility();
            panelHideRoutine = null;
        }

        private void UpdatePanelVisibility()
        {
            if (panelRoot == null)
            {
                return;
            }

            bool showPanel = CCS_NetworkSessionUtility.IsNetworkSessionActive()
                && (activeEntries.Count > 0 || Time.unscaledTime < panelHideAtUnscaledTime);
            panelRoot.SetActive(showPanel);
        }

        private void TryRegisterForActiveNetworkSession()
        {
            if (CCS_NetworkSessionUtility.CanProcessJoinNotifications())
            {
                RegisterWithRegistry();
            }
        }

        private void RegisterWithRegistry()
        {
            if (registeredWithRegistry)
            {
                return;
            }

            CCS_PlayerJoinNotificationFeedRegistry.Register(this);
            registeredWithRegistry = true;
        }

        private void UnregisterFromRegistry()
        {
            if (!registeredWithRegistry)
            {
                return;
            }

            CCS_PlayerJoinNotificationFeedRegistry.Unregister(this);
            registeredWithRegistry = false;
        }

        private void EnsureEntryCapacity()
        {
            while (activeEntries.Count >= maxEntries)
            {
                RemoveEntryAt(0);
            }
        }

        private ActiveEntry CreateEntry(string message)
        {
            TMP_Text entryText = CreateEntryText(message);
            CanvasGroup canvasGroup = entryText.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = entryText.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            entryText.transform.SetAsLastSibling();

            return new ActiveEntry
            {
                Text = entryText,
                CanvasGroup = canvasGroup
            };
        }

        private TMP_Text CreateEntryText(string message)
        {
            if (entryTemplate != null)
            {
                TMP_Text instance = Instantiate(entryTemplate, entriesContainer);
                instance.gameObject.SetActive(true);
                instance.text = message;
                return instance;
            }

            GameObject entryObject = new GameObject(
                "JoinEntry",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(CanvasGroup),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            entryObject.transform.SetParent(entriesContainer, false);

            LayoutElement layoutElement = entryObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 28f;
            layoutElement.minHeight = 28f;

            TMP_Text text = entryObject.GetComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 16f;
            text.color = CCS_ValidationUiConstants.JoinEntryTextColor;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            return text;
        }

        private IEnumerator ExpireEntryRoutine(ActiveEntry entry)
        {
            yield return new WaitForSeconds(entryLifetimeSeconds);

            if (entry == null || entry.Text == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < entryFadeOutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                if (entry.CanvasGroup != null)
                {
                    entry.CanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / entryFadeOutSeconds);
                }

                yield return null;
            }

            int index = activeEntries.IndexOf(entry);
            if (index >= 0)
            {
                RemoveEntryAt(index);
            }
        }

        private void RemoveEntryAt(int index)
        {
            if (index < 0 || index >= activeEntries.Count)
            {
                return;
            }

            ActiveEntry entry = activeEntries[index];
            if (entry.ExpireRoutine != null)
            {
                StopCoroutine(entry.ExpireRoutine);
            }

            DestroyEntryVisual(entry);
            activeEntries.RemoveAt(index);
            UpdatePanelVisibility();
        }

        private static void DestroyEntryVisual(ActiveEntry entry)
        {
            if (entry?.Text != null)
            {
                Destroy(entry.Text.gameObject);
            }
        }

        #endregion

        #region Nested Types

        private sealed class ActiveEntry
        {
            public TMP_Text Text;
            public CanvasGroup CanvasGroup;
            public Coroutine ExpireRoutine;
        }

        #endregion
    }
}
