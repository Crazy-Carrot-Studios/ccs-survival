using CCS.Modules.CharacterController.Tests.Netcode;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingSceneLayoutEditor
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Builds the AAA-style hosting menu UI in SCN_CCS_MultiplayerHosting.
// PLACEMENT: Editor layout utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.4.2 reference-guided anchor layout — centered cards, fixed menu buttons, no advanced join UI.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_MultiplayerHostingSceneLayoutEditor
    {
        private const float ModeSelectCardWidth = CCS_NetcodeTestConstants.ModeSelectCardWidth;
        private const float ModeSelectCardHeight = CCS_NetcodeTestConstants.ModeSelectCardHeight;
        private const float ModeSelectButtonWidth = CCS_NetcodeTestConstants.ModeSelectMenuButtonWidth;
        private const float ModeSelectButtonHeight = CCS_NetcodeTestConstants.ModeSelectMenuButtonHeight;
        private const float ModeSelectButtonSpacing = CCS_NetcodeTestConstants.ModeSelectButtonSpacing;
        private const float NetworkingCardWidth = CCS_NetcodeTestConstants.NetworkingCardWidth;
        private const float NetworkingCardHeight = CCS_NetcodeTestConstants.NetworkingCardHeight;

        private static readonly Color BackgroundColor = HexColor("#061018");
        private static readonly Color VignetteColor = new Color(0.01f, 0.02f, 0.05f, 0.55f);
        private static readonly Color NetworkingPanelColor = HexColor("#050A12");
        private static readonly Color MainCardColor = new Color(8f / 255f, 18f / 255f, 32f / 255f, 220f / 255f);
        private static readonly Color SubCardColor = new Color(12f / 255f, 28f / 255f, 48f / 255f, 185f / 255f);
        private static readonly Color InputBackgroundColor = HexColor("#050B14");
        private static readonly Color ListBackgroundColor = HexColor("#050B14");
        private static readonly Color MainCardBorderColor = HexColor("#34506A");
        private static readonly Color SubCardBorderColor = HexColor("#2E4A64");
        private static readonly Color InputBorderColor = HexColor("#263C52");
        private static readonly Color ListBorderColor = HexColor("#24394F");
        private static readonly Color DividerColor = HexColor("#2B4058");
        private static readonly Color AccentGoldColor = new Color(0.92f, 0.62f, 0.22f, 0.95f);
        private static readonly Color TextPrimary = HexColor("#F2F5FA");
        private static readonly Color TextSecondary = HexColor("#8FA8C8");
        private static readonly Color TextBody = HexColor("#CAD4E2");
        private static readonly Color TextHint = HexColor("#B7C3D4");
        private static readonly Color TextFooter = HexColor("#BFD0E6");
        private static readonly Color TextLabel = HexColor("#EAF0F8");
        private static readonly Color TextEmptyList = HexColor("#C9D2DF");
        private static readonly Color PrimaryButtonColor = HexColor("#0D4AAD");
        private static readonly Color PrimaryButtonHoverColor = HexColor("#1A63D8");
        private static readonly Color SecondaryButtonColor = HexColor("#102742");
        private static readonly Color SecondaryButtonHoverColor = HexColor("#1A3A5C");
        private static readonly Color MenuButtonColor = HexColor("#102742");
        private static readonly Color MenuButtonHoverColor = HexColor("#1A3A5C");

        #region Public Methods

        public static bool BuildOrRebuildLayout()
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MultiplayerHostingScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError(
                    "[Hosting Layout] Could not open "
                    + CCS_NetcodeTestConstants.MultiplayerHostingScenePath);
                return false;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                canvas = CreateCanvasRoot();
            }

            SanitizeCanvasRoot(canvas);
            ConfigureCanvasScaler(canvas);

            NetworkManager networkManager = Object.FindFirstObjectByType<NetworkManager>();
            Unity.Netcode.Transports.UTP.UnityTransport transport = networkManager != null
                ? networkManager.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>()
                : null;

            ClearCanvasChildren(canvas.transform);

            RectTransform root = CreateStretchPanel(
                "HostingUiRoot",
                canvas.transform,
                BackgroundColor,
                raycastTarget: false);
            ApplyBackgroundTreatment(root);

            BuildModeSelectPanel(
                root,
                out GameObject modeSelectPanel,
                out Button singlePlayerButton,
                out Button multiplayerButton);

            RectTransform networkingPanel = BuildNetworkingPanel(
                root,
                networkManager,
                transport,
                out CCS_MultiplayerHostingMenu menu,
                out Button backButton);

            CCS_HostingSceneModeSelectController modeController = canvas.GetComponent<CCS_HostingSceneModeSelectController>();
            if (modeController == null)
            {
                modeController = canvas.gameObject.AddComponent<CCS_HostingSceneModeSelectController>();
            }

            SerializedObject serializedModeController = new SerializedObject(modeController);
            SetReference(serializedModeController, "modeSelectPanel", modeSelectPanel);
            SetReference(serializedModeController, "networkingPanel", networkingPanel.gameObject);
            SetReference(serializedModeController, "singlePlayerButton", singlePlayerButton);
            SetReference(serializedModeController, "multiplayerButton", multiplayerButton);
            SetReference(serializedModeController, "backButton", backButton);
            SetReference(serializedModeController, "networkManager", networkManager);
            serializedModeController.ApplyModifiedPropertiesWithoutUndo();

            modeSelectPanel.SetActive(true);
            networkingPanel.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Hosting Layout] Rebuilt SCN_CCS_MultiplayerHosting UI (v0.4.2 networking polish).");
            return true;
        }

        #endregion

        #region Layout Sections

        private static void BuildModeSelectPanel(
            RectTransform root,
            out GameObject modeSelectPanel,
            out Button singlePlayerButton,
            out Button multiplayerButton)
        {
            RectTransform panel = CreateStretchPanel(
                CCS_NetcodeTestConstants.ModeSelectPanelObjectName,
                root,
                Color.clear,
                raycastTarget: false);

            RectTransform card = CreateBorderedPanel(
                CCS_NetcodeTestConstants.ModeSelectCardObjectName,
                panel,
                new Vector2(ModeSelectCardWidth, ModeSelectCardHeight),
                Vector2.zero,
                MainCardColor,
                MainCardBorderColor);

            CreateAnchoredText(
                card,
                "StudioTitleText",
                "CRAZY CARROT STUDIOS",
                22,
                FontStyle.Bold,
                TextPrimary,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -48f),
                new Vector2(520f, 30f));
            CreateAnchoredText(
                card,
                "SubtitleText",
                "CHARACTER CONTROLLER TEST",
                15,
                FontStyle.Normal,
                TextSecondary,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -88f),
                new Vector2(520f, 22f));
            CreateAccentDivider(
                card,
                CCS_NetcodeTestConstants.ModeSelectDividerObjectName,
                420f,
                new Vector2(0f, -118f));

            singlePlayerButton = CreateAnchoredButton(
                card,
                CCS_NetcodeTestConstants.SinglePlayerButtonObjectName,
                "SINGLE PLAYER",
                new Vector2(ModeSelectButtonWidth, ModeSelectButtonHeight),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -190f),
                MenuButtonColor,
                PrimaryButtonHoverColor,
                18);
            CreateAnchoredText(
                card,
                "SinglePlayerDescription",
                "Start directly in the Master Test scene.",
                12,
                FontStyle.Normal,
                TextHint,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -262f),
                new Vector2(460f, 18f));

            multiplayerButton = CreateAnchoredButton(
                card,
                CCS_NetcodeTestConstants.MultiplayerButtonObjectName,
                "MULTIPLAYER",
                new Vector2(ModeSelectButtonWidth, ModeSelectButtonHeight),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -304f),
                MenuButtonColor,
                PrimaryButtonHoverColor,
                18);
            CreateAnchoredText(
                card,
                "MultiplayerDescription",
                "Host or join a local multiplayer session.",
                12,
                FontStyle.Normal,
                TextHint,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -376f),
                new Vector2(460f, 18f));

            modeSelectPanel = panel.gameObject;
        }

        private static RectTransform BuildNetworkingPanel(
            RectTransform root,
            NetworkManager networkManager,
            Unity.Netcode.Transports.UTP.UnityTransport transport,
            out CCS_MultiplayerHostingMenu menu,
            out Button backButton)
        {
            RectTransform networkingPanel = CreateStretchPanel(
                CCS_NetcodeTestConstants.NetworkingPanelObjectName,
                root,
                NetworkingPanelColor,
                raycastTarget: false);

            RectTransform card = CreateBorderedPanel(
                CCS_NetcodeTestConstants.NetworkingCardObjectName,
                networkingPanel,
                new Vector2(NetworkingCardWidth, NetworkingCardHeight),
                Vector2.zero,
                MainCardColor,
                MainCardBorderColor);

            BuildNetworkingHeader(card);
            BuildNamePanel(card);
            BuildHostCard(card);
            BuildJoinCard(card);
            BuildNetworkingFooter(card, out Text playersText, out Button backButtonOut, out Button exitButton);
            backButton = backButtonOut;

            GameObject diagnosticsPanel = CreateHiddenDiagnosticsPanel(card, out Text diagnosticsText);

            Canvas canvas = root.GetComponentInParent<Canvas>();
            menu = canvas.GetComponent<CCS_MultiplayerHostingMenu>();
            if (menu == null)
            {
                menu = canvas.gameObject.AddComponent<CCS_MultiplayerHostingMenu>();
            }

            SerializedObject serializedMenu = new SerializedObject(menu);
            SetReference(serializedMenu, "networkManager", networkManager);
            SetReference(serializedMenu, "transport", transport);
            SetReference(serializedMenu, "playerNameInput", card.Find("NamePanel/PlayerNameInput")?.GetComponent<InputField>());
            SetReference(serializedMenu, "hostAndStartButton", card.Find("HostCard/HostAndStartButton")?.GetComponent<Button>());
            SetReference(
                serializedMenu,
                "serverListContainer",
                card.Find("JoinCard/ServerListScroll/Viewport/ServerListContainer") as RectTransform);
            SetReference(serializedMenu, "emptyServerListText", card.Find("JoinCard/EmptyServerListText")?.GetComponent<Text>());
            SetReference(serializedMenu, "refreshServersButton", card.Find("JoinCard/JoinButtons/RefreshServersButton")?.GetComponent<Button>());
            SetReference(serializedMenu, "joinSelectedButton", card.Find("JoinCard/JoinButtons/JoinSelectedButton")?.GetComponent<Button>());
            SetReference(serializedMenu, "diagnosticsPanel", diagnosticsPanel);
            SetReference(serializedMenu, "diagnosticsText", diagnosticsText);
            SetReference(serializedMenu, "connectedPlayersText", playersText);
            SetReference(serializedMenu, "exitButton", exitButton);
            serializedMenu.ApplyModifiedPropertiesWithoutUndo();

            return networkingPanel;
        }

        private static void BuildNetworkingHeader(RectTransform card)
        {
            CreateAnchoredText(
                card,
                "TitleText",
                "MULTIPLAYER TEST",
                48,
                FontStyle.Bold,
                TextPrimary,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -70f),
                new Vector2(900f, 56f));
            CreateAnchoredText(
                card,
                "SubtitleText",
                "LOCAL TEST SESSION",
                22,
                FontStyle.Normal,
                TextSecondary,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -130f),
                new Vector2(700f, 28f));
            CreateAccentDivider(card, "HeaderDivider", 420f, new Vector2(0f, -165f));
        }

        private static void BuildNamePanel(RectTransform card)
        {
            RectTransform namePanel = CreateBorderedPanel(
                "NamePanel",
                card,
                new Vector2(1220f, 150f),
                new Vector2(0f, -250f),
                SubCardColor,
                SubCardBorderColor,
                new Vector2(0.5f, 1f));

            CreateAnchoredText(
                namePanel,
                "NameLabel",
                "PLAYER NAME",
                18,
                FontStyle.Bold,
                TextLabel,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(110f, -35f),
                new Vector2(240f, 24f));
            CreateAnchoredInputField(
                namePanel,
                "PlayerNameInput",
                "Enter your name...",
                new Vector2(560f, 54f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(110f, -80f),
                InputBackgroundColor,
                InputBorderColor,
                20);
            CreateAnchoredText(
                namePanel,
                "NameHintText",
                "This name appears above your character for other players.",
                16,
                FontStyle.Normal,
                TextHint,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(110f, -125f),
                new Vector2(760f, 22f));
        }

        private static void BuildHostCard(RectTransform card)
        {
            RectTransform hostCard = CreateBorderedPanel(
                "HostCard",
                card,
                new Vector2(600f, 380f),
                new Vector2(-320f, -500f),
                SubCardColor,
                SubCardBorderColor,
                new Vector2(0.5f, 1f));

            CreateAnchoredText(
                hostCard,
                "HostCardTitle",
                "HOST GAME",
                26,
                FontStyle.Bold,
                TextPrimary,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(50f, -55f),
                new Vector2(300f, 32f));
            CreateAnchoredText(
                hostCard,
                "HostCardDescription",
                "Create a local test session and jump into the character test.",
                17,
                FontStyle.Normal,
                TextBody,
                TextAnchor.UpperLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(50f, -95f),
                new Vector2(430f, 56f));
            CreateAnchoredButton(
                hostCard,
                "HostAndStartButton",
                "HOST & START",
                new Vector2(500f, 64f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 65f),
                PrimaryButtonColor,
                PrimaryButtonHoverColor,
                22);
        }

        private static void BuildJoinCard(RectTransform card)
        {
            RectTransform joinCard = CreateBorderedPanel(
                "JoinCard",
                card,
                new Vector2(600f, 380f),
                new Vector2(320f, -500f),
                SubCardColor,
                SubCardBorderColor,
                new Vector2(0.5f, 1f));

            CreateAnchoredText(
                joinCard,
                "JoinCardTitle",
                "JOIN GAME",
                26,
                FontStyle.Bold,
                TextPrimary,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(50f, -55f),
                new Vector2(300f, 32f));
            CreateAnchoredText(
                joinCard,
                "JoinCardDescription",
                "Find a local host and join the character test.",
                17,
                FontStyle.Normal,
                TextBody,
                TextAnchor.UpperLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(50f, -95f),
                new Vector2(430f, 44f));

            CreateServerList(joinCard, new Vector2(520f, 130f), new Vector2(0f, -40f));
            CreateAnchoredText(
                joinCard,
                "EmptyServerListText",
                "No local hosts found.\nAsk a player to host, then refresh.",
                18,
                FontStyle.Normal,
                TextEmptyList,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f),
                new Vector2(480f, 60f));

            RectTransform buttonRow = CreateAnchoredPanel(
                "JoinButtons",
                joinCard,
                new Vector2(530f, 64f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 65f),
                Color.clear);
            CreateAnchoredButton(
                buttonRow,
                "RefreshServersButton",
                "REFRESH",
                new Vector2(220f, 64f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-155f, 0f),
                SecondaryButtonColor,
                SecondaryButtonHoverColor,
                20);
            CreateAnchoredButton(
                buttonRow,
                "JoinSelectedButton",
                "JOIN SELECTED",
                new Vector2(290f, 64f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(155f, 0f),
                PrimaryButtonColor,
                PrimaryButtonHoverColor,
                20);
        }

        private static void BuildNetworkingFooter(
            RectTransform card,
            out Text playersText,
            out Button backButton,
            out Button exitButton)
        {
            CreateAnchoredDivider(
                card,
                "FooterDivider",
                new Vector2(1340f, 1f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 110f),
                DividerColor);

            backButton = CreateAnchoredButton(
                card,
                CCS_NetcodeTestConstants.BackButtonObjectName,
                "BACK",
                new Vector2(230f, 58f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(100f, 55f),
                SecondaryButtonColor,
                SecondaryButtonHoverColor,
                18);

            RectTransform playersPanel = CreateBorderedPanel(
                "ConnectedPlayersPanel",
                card,
                new Vector2(260f, 58f),
                new Vector2(0f, 55f),
                new Color(0.04f, 0.08f, 0.14f, 0.85f),
                ListBorderColor,
                new Vector2(0.5f, 0f));
            playersText = CreateAnchoredText(
                playersPanel,
                "ConnectedPlayersText",
                "PLAYERS: 0 / 3",
                18,
                FontStyle.Normal,
                TextFooter,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(240f, 28f));

            exitButton = CreateAnchoredButton(
                card,
                "ExitButton",
                "EXIT",
                new Vector2(230f, 58f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-100f, 55f),
                SecondaryButtonColor,
                SecondaryButtonHoverColor,
                18);
        }

        private static GameObject CreateHiddenDiagnosticsPanel(RectTransform card, out Text diagnosticsText)
        {
            RectTransform diagnosticsPanel = CreateAnchoredPanel(
                "DiagnosticsPanel",
                card,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                Vector2.zero,
                new Color(0f, 0f, 0f, 0f));
            diagnosticsPanel.gameObject.SetActive(false);
            diagnosticsText = CreateAnchoredText(
                diagnosticsPanel,
                "DiagnosticsText",
                string.Empty,
                10,
                FontStyle.Italic,
                TextHint,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                Vector2.zero,
                new Vector2(1f, 12f));
            return diagnosticsPanel.gameObject;
        }

        private static void CreateServerList(RectTransform joinCard, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject scrollObject = new GameObject(
                "ServerListScroll",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(ScrollRect));
            scrollObject.transform.SetParent(joinCard, false);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            ConfigureAnchoredRect(
                scrollRectTransform,
                size,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition);
            Image scrollImage = scrollObject.GetComponent<Image>();
            scrollImage.color = ListBackgroundColor;
            AddOutline(scrollObject, ListBorderColor);

            GameObject viewport = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(RectMask2D));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            StretchRect(viewportRect, Vector2.zero, Vector2.zero);
            viewport.GetComponent<Image>().color = ListBackgroundColor;

            GameObject content = new GameObject(
                "ServerListContainer",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        private static void ApplyBackgroundTreatment(RectTransform root)
        {
            RectTransform vignette = CreateStretchPanel("BackgroundVignette", root, VignetteColor, raycastTarget: false);
            vignette.SetAsFirstSibling();
        }

        #endregion

        #region Factory Helpers

        private static Canvas CreateCanvasRoot()
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            ConfigureCanvasScaler(canvas);
            return canvas;
        }

        private static void SanitizeCanvasRoot(Canvas canvas)
        {
            GameObject canvasObject = canvas.gameObject;
            RemoveComponentIfPresent<VerticalLayoutGroup>(canvasObject);
            RemoveComponentIfPresent<HorizontalLayoutGroup>(canvasObject);
            RemoveComponentIfPresent<ContentSizeFitter>(canvasObject);
            RemoveComponentIfPresent<ScrollRect>(canvasObject);

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.localScale = Vector3.one;
            StretchRect(canvasRect, Vector2.zero, Vector2.zero);
        }

        private static void RemoveComponentIfPresent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component);
            }
        }

        private static void ConfigureCanvasScaler(Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void ClearCanvasChildren(Transform canvasTransform)
        {
            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvasTransform.GetChild(i).gameObject);
            }
        }

        private static RectTransform CreateStretchPanel(string name, Transform parent, Color color, bool raycastTarget)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            StretchRect(rect, Vector2.zero, Vector2.zero);
            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return rect;
        }

        private static RectTransform CreateAnchoredPanel(
            string name,
            Transform parent,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            ConfigureAnchoredRect(rect, size, anchorMin, anchorMax, anchoredPosition);
            panelObject.GetComponent<Image>().color = color;
            panelObject.GetComponent<Image>().raycastTarget = false;
            return rect;
        }

        private static RectTransform CreateBorderedPanel(
            string name,
            Transform parent,
            Vector2 size,
            Vector2 anchoredPosition,
            Color fillColor,
            Color borderColor,
            Vector2 anchor = default)
        {
            if (anchor == default)
            {
                anchor = new Vector2(0.5f, 0.5f);
            }

            RectTransform panel = CreateAnchoredPanel(name, parent, size, anchor, anchor, anchoredPosition, fillColor);
            AddOutline(panel.gameObject, borderColor);
            return panel;
        }

        private static void AddOutline(GameObject target, Color borderColor)
        {
            Outline outline = target.GetComponent<Outline>();
            if (outline == null)
            {
                outline = target.AddComponent<Outline>();
            }

            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static void ConfigureAnchoredRect(
            RectTransform rect,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(
                (anchorMin.x + anchorMax.x) * 0.5f,
                (anchorMin.y + anchorMax.y) * 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void CreateAccentDivider(RectTransform parent, string name, float width, Vector2 anchoredPosition)
        {
            RectTransform dividerRoot = CreateAnchoredPanel(
                name,
                parent,
                new Vector2(width, 2f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                anchoredPosition,
                Color.clear);

            RectTransform line = CreateAnchoredPanel(
                "Line",
                dividerRoot,
                new Vector2(width, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                DividerColor);
            CreateAnchoredPanel(
                "Accent",
                dividerRoot,
                new Vector2(8f, 8f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                AccentGoldColor);
            line.SetAsFirstSibling();
        }

        private static void CreateAnchoredDivider(
            RectTransform parent,
            string name,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Color color)
        {
            CreateAnchoredPanel(name, parent, size, anchorMin, anchorMax, anchoredPosition, color);
        }

        private static Text CreateAnchoredText(
            RectTransform parent,
            string name,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            ConfigureAnchoredRect(textObject.GetComponent<RectTransform>(), size, anchorMin, anchorMax, anchoredPosition);

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static InputField CreateAnchoredInputField(
            RectTransform parent,
            string name,
            string placeholder,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Color backgroundColor,
            Color borderColor,
            int fontSize)
        {
            GameObject inputRoot = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(InputField));
            inputRoot.transform.SetParent(parent, false);
            ConfigureAnchoredRect(inputRoot.GetComponent<RectTransform>(), size, anchorMin, anchorMax, anchoredPosition);
            inputRoot.GetComponent<Image>().color = backgroundColor;
            AddOutline(inputRoot, borderColor);

            GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            placeholderObject.transform.SetParent(inputRoot.transform, false);
            RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
            StretchRect(placeholderRect, new Vector2(14f, 8f), new Vector2(-14f, -8f));
            Text placeholderText = placeholderObject.GetComponent<Text>();
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.fontSize = fontSize;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.color = TextSecondary;
            placeholderText.text = placeholder;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(inputRoot.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            StretchRect(textRect, new Vector2(14f, 8f), new Vector2(-14f, -8f));
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = TextPrimary;
            text.supportRichText = false;

            InputField inputField = inputRoot.GetComponent<InputField>();
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;
            inputField.lineType = InputField.LineType.SingleLine;
            return inputField;
        }

        private static Button CreateAnchoredButton(
            RectTransform parent,
            string name,
            string label,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Color normalColor,
            Color highlightColor,
            int fontSize)
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            ConfigureAnchoredRect(buttonObject.GetComponent<RectTransform>(), size, anchorMin, anchorMax, anchoredPosition);

            Image image = buttonObject.GetComponent<Image>();
            image.color = normalColor;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            StretchRect(labelRect, Vector2.zero, Vector2.zero);
            Text text = labelObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TextPrimary;
            text.text = label;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = highlightColor * 0.85f;
            colors.selectedColor = highlightColor;
            button.colors = colors;
            return button;
        }

        private static void StretchRect(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static Color HexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            return Color.white;
        }

        private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        #endregion
    }
}
