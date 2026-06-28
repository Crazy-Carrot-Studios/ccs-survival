using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_PlayerDeathScreenController
// CATEGORY: Modules / Attributes / Runtime / UI
// PURPOSE: Freezes gameplay and shows restart UI when the local player dies.
// PLACEMENT: Networked test player root with CCS_NetworkHealth.
// AUTHOR: James Schilz
// CREATED: 2026-06-26
// NOTES: Local-owner only. Restarts active scene on button press.
// =============================================================================

namespace CCS.Modules.Attributes
{
    [DefaultExecutionOrder(500)]
    public sealed class CCS_PlayerDeathScreenController : NetworkBehaviour
    {
        private const string DeathEventSystemObjectName = "CCS_PlayerDeathEventSystem";

        [SerializeField] private CCS_NetworkHealth networkHealth;
        [SerializeField] private bool enableDeathScreenDebugLogs;

        private GameObject overlayRoot;
        private Button restartButton;
        private bool deathScreenShown;
        private bool restartInProgress;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            if (networkHealth == null)
            {
                networkHealth = GetComponent<CCS_NetworkHealth>();
            }
        }

        private void OnEnable()
        {
            if (networkHealth != null)
            {
                networkHealth.DeadStateChanged += HandleDeadStateChanged;
            }
        }

        private void OnDisable()
        {
            if (networkHealth != null)
            {
                networkHealth.DeadStateChanged -= HandleDeadStateChanged;
            }

            HideDeathScreen(restoreTimeScale: true);
        }

        private void Update()
        {
            if (deathScreenShown)
            {
                if (!restartInProgress && TryHandleRestartInput())
                {
                    RestartActiveScene();
                }

                return;
            }

            if (networkHealth == null || !ShouldMonitorLocalPlayer())
            {
                return;
            }

            if (networkHealth.IsDead)
            {
                ShowDeathScreen();
            }
        }

        private bool ShouldMonitorLocalPlayer()
        {
            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                return true;
            }

            return IsOwner;
        }

        private void HandleDeadStateChanged(bool isDead)
        {
            if (!ShouldMonitorLocalPlayer())
            {
                return;
            }

            if (isDead)
            {
                ShowDeathScreen();
            }
        }

        private void ShowDeathScreen()
        {
            if (deathScreenShown)
            {
                return;
            }

            deathScreenShown = true;
            EnsureOverlayBuilt();
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
            }

            EnsureDeathUiEventSystem();
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (restartButton != null)
            {
                restartButton.interactable = true;
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
                }
            }

            if (enableDeathScreenDebugLogs)
            {
                Debug.Log("[Player Death Screen] Local player died. Gameplay frozen.", this);
            }
        }

        private void HideDeathScreen(bool restoreTimeScale)
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }

            if (restoreTimeScale)
            {
                Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            }

            deathScreenShown = false;
        }

        private void RestartActiveScene()
        {
            if (restartInProgress)
            {
                return;
            }

            restartInProgress = true;
            HideDeathScreen(restoreTimeScale: true);
            Time.timeScale = 1f;

            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsListening)
            {
                networkManager.Shutdown();
            }

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private bool TryHandleRestartInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                {
                    return true;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureDeathUiEventSystem()
        {
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            EventSystem targetEventSystem = null;

            for (int i = 0; i < eventSystems.Length; i++)
            {
                EventSystem eventSystem = eventSystems[i];
                if (eventSystem == null)
                {
                    continue;
                }

                RemoveStandaloneInputModule(eventSystem.gameObject);
                EnsureInputSystemUiModule(eventSystem.gameObject);
                targetEventSystem ??= eventSystem;
            }

            if (targetEventSystem == null)
            {
                GameObject eventSystemObject = new GameObject(DeathEventSystemObjectName);
                targetEventSystem = eventSystemObject.AddComponent<EventSystem>();
                EnsureInputSystemUiModule(eventSystemObject);
            }

            EventSystem.current = targetEventSystem;

            if (restartButton != null)
            {
                targetEventSystem.SetSelectedGameObject(restartButton.gameObject);
            }
        }

        private static void RemoveStandaloneInputModule(GameObject eventSystemObject)
        {
            StandaloneInputModule legacyModule = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                legacyModule.enabled = false;
                Destroy(legacyModule);
            }
        }

        private static void EnsureInputSystemUiModule(GameObject eventSystemObject)
        {
            InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            inputModule.enabled = true;
        }

        private void EnsureOverlayBuilt()
        {
            if (overlayRoot != null)
            {
                return;
            }

            overlayRoot = new GameObject("PlayerDeathScreenOverlay");
            overlayRoot.transform.SetParent(null, false);

            Canvas canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            overlayRoot.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            overlayRoot.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new GameObject(
                "DeathPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            panelObject.transform.SetParent(overlayRoot.transform, false);
            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.72f);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            TMP_Text headerText = CreateText(
                panelObject.transform,
                "HeaderText",
                "You Died",
                54,
                new Vector2(0.5f, 0.62f),
                new Vector2(640f, 80f));

            TMP_Text subheaderText = CreateText(
                panelObject.transform,
                "SubheaderText",
                "Do you wish to restart?",
                28,
                new Vector2(0.5f, 0.52f),
                new Vector2(640f, 48f));

            GameObject buttonObject = new GameObject(
                "RestartButton",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(panelObject.transform, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.38f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.38f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(240f, 56f);
            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.55f, 0.24f, 1f);
            restartButton = buttonObject.GetComponent<Button>();
            restartButton.interactable = true;
            restartButton.onClick.RemoveListener(RestartActiveScene);
            restartButton.onClick.AddListener(RestartActiveScene);

            CreateText(
                buttonObject.transform,
                "RestartLabel",
                "Restart",
                24,
                new Vector2(0.5f, 0.5f),
                new Vector2(220f, 40f));

            headerText.alignment = TextAlignmentOptions.Center;
            subheaderText.alignment = TextAlignmentOptions.Center;
            overlayRoot.SetActive(false);
        }

        private static TMP_Text CreateText(
            Transform parent,
            string objectName,
            string textValue,
            float fontSize,
            Vector2 anchor,
            Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = anchor;
            textRect.anchorMax = anchor;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = sizeDelta;
            textRect.anchoredPosition = Vector2.zero;
            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.text = textValue;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }
    }
}
