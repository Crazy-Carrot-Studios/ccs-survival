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
// PURPOSE: Builds the simplified AAA-style hosting menu UI in SCN_CCS_MultiplayerHosting.
// PLACEMENT: Editor layout utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses top-anchored layout children so Layout Groups receive non-zero heights.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_MultiplayerHostingSceneLayoutEditor
    {
        private const float ReferenceWidth = CCS_NetcodeTestConstants.NetworkingPanelWidth;
        private const float ModeSelectCardWidth = CCS_NetcodeTestConstants.ModeSelectCardWidth;
        private const float HeaderHeight = 72f;
        private const float NameSectionHeight = 108f;
        private const float HostJoinRowHeight = 280f;
        private const float AdvancedToggleHeight = 32f;
        private const float AdvancedPanelHeight = 124f;
        private const float FooterHeight = 44f;
        private const float MenuButtonHeight = 48f;
        private const float SecondaryButtonHeight = 40f;
        private const float ModeSelectCardHeight = 420f;
        private const float ServerListHeight = 112f;

        private static readonly Color BackgroundColor = new Color(0.03f, 0.05f, 0.09f, 1f);
        private static readonly Color PanelColor = new Color(0.08f, 0.12f, 0.18f, 0.92f);
        private static readonly Color CardColor = new Color(0.1f, 0.14f, 0.2f, 0.96f);
        private static readonly Color PrimaryButtonColor = new Color(0.18f, 0.42f, 0.72f, 1f);
        private static readonly Color SecondaryButtonColor = new Color(0.14f, 0.22f, 0.32f, 1f);
        private static readonly Color TextPrimary = new Color(0.93f, 0.95f, 0.98f, 1f);
        private static readonly Color TextMuted = new Color(0.72f, 0.78f, 0.86f, 1f);

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
            Debug.Log("[Hosting Layout] Rebuilt SCN_CCS_MultiplayerHosting UI with Mode Select.");
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
            VerticalLayoutGroup panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(24, 24, 24, 24);
            panelLayout.spacing = 0f;
            panelLayout.childAlignment = TextAnchor.MiddleCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = true;

            RectTransform card = CreateLayoutSection(
                CCS_NetcodeTestConstants.ModeSelectCardObjectName,
                panel,
                ModeSelectCardWidth,
                ModeSelectCardHeight,
                PanelColor);
            LayoutElement cardLayout = card.GetComponent<LayoutElement>();
            cardLayout.preferredWidth = ModeSelectCardWidth;
            cardLayout.minWidth = ModeSelectCardWidth;
            cardLayout.preferredHeight = ModeSelectCardHeight;
            cardLayout.minHeight = ModeSelectCardHeight;
            cardLayout.flexibleWidth = 0f;
            cardLayout.flexibleHeight = 0f;

            AddVerticalLayout(card, 14f, new RectOffset(28, 28, 28, 28));
            CreateText(card, "TitleText", "Crazy Carrot Studios", 30, FontStyle.Bold, TextPrimary, TextAnchor.MiddleCenter, 36f);
            CreateText(
                card,
                "SubtitleText",
                "Character Controller Test",
                18,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.MiddleCenter,
                24f);
            CreateFlexibleSpacer(card, 8f);
            singlePlayerButton = CreatePrimaryButton(card, CCS_NetcodeTestConstants.SinglePlayerButtonObjectName, "Single Player", MenuButtonHeight);
            SetButtonExpandWidth(singlePlayerButton, false);
            CreateText(
                card,
                "SinglePlayerDescription",
                "Jump straight into the Master Test scene offline.",
                14,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.MiddleCenter,
                22f);
            CreateFlexibleSpacer(card, 6f);
            multiplayerButton = CreatePrimaryButton(card, CCS_NetcodeTestConstants.MultiplayerButtonObjectName, "Multiplayer", MenuButtonHeight);
            SetButtonExpandWidth(multiplayerButton, false);
            CreateText(
                card,
                "MultiplayerDescription",
                "Host or join a local test session.",
                14,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.MiddleCenter,
                22f);

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
                Color.clear,
                raycastTarget: false);
            VerticalLayoutGroup networkingLayout = networkingPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            networkingLayout.padding = new RectOffset(0, 0, 36, 24);
            networkingLayout.spacing = 14f;
            networkingLayout.childAlignment = TextAnchor.UpperCenter;
            networkingLayout.childControlWidth = true;
            networkingLayout.childControlHeight = true;
            networkingLayout.childForceExpandWidth = true;
            networkingLayout.childForceExpandHeight = false;

            RectTransform content = CreateLayoutSection(
                CCS_NetcodeTestConstants.NetworkingContentObjectName,
                networkingPanel,
                ReferenceWidth,
                0f,
                Color.clear);
            LayoutElement contentLayout = content.GetComponent<LayoutElement>();
            float contentHeight = ComputeNetworkingContentPreferredHeight();
            contentLayout.preferredWidth = ReferenceWidth;
            contentLayout.minWidth = ReferenceWidth;
            contentLayout.preferredHeight = contentHeight;
            contentLayout.minHeight = contentHeight;

            VerticalLayoutGroup contentGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentGroup.spacing = 14f;
            contentGroup.childAlignment = TextAnchor.UpperCenter;
            contentGroup.childControlWidth = true;
            contentGroup.childControlHeight = true;
            contentGroup.childForceExpandWidth = true;
            contentGroup.childForceExpandHeight = false;

            BuildHeader(content);
            BuildNameSection(content);
            BuildHostJoinRow(content);
            BuildAdvancedPanel(
                content,
                out GameObject advancedPanel,
                out Button advancedToggle,
                out InputField manualAddress,
                out InputField manualPort,
                out Button joinManual);
            BuildDiagnosticsPanel(content, out GameObject diagnosticsPanel, out Text diagnosticsText);
            BuildFooter(content, out Text playersText, out Button backButtonOut, out Button exitButton);
            backButton = backButtonOut;

            Canvas canvas = root.GetComponentInParent<Canvas>();
            menu = canvas.GetComponent<CCS_MultiplayerHostingMenu>();
            if (menu == null)
            {
                menu = canvas.gameObject.AddComponent<CCS_MultiplayerHostingMenu>();
            }

            SerializedObject serializedMenu = new SerializedObject(menu);
            SetReference(serializedMenu, "networkManager", networkManager);
            SetReference(serializedMenu, "transport", transport);
            SetReference(serializedMenu, "playerNameInput", content.Find("NamePanel/PlayerNameInput")?.GetComponent<InputField>());
            SetReference(serializedMenu, "hostAndStartButton", content.Find("HostJoinContainer/HostCard/HostAndStartButton")?.GetComponent<Button>());
            SetReference(
                serializedMenu,
                "serverListContainer",
                content.Find("HostJoinContainer/JoinCard/ServerListScroll/Viewport/ServerListContainer") as RectTransform);
            SetReference(serializedMenu, "emptyServerListText", content.Find("HostJoinContainer/JoinCard/EmptyServerListText")?.GetComponent<Text>());
            SetReference(serializedMenu, "refreshServersButton", content.Find("HostJoinContainer/JoinCard/JoinButtons/RefreshServersButton")?.GetComponent<Button>());
            SetReference(serializedMenu, "joinSelectedButton", content.Find("HostJoinContainer/JoinCard/JoinButtons/JoinSelectedButton")?.GetComponent<Button>());
            SetReference(serializedMenu, "advancedManualJoinPanel", advancedPanel);
            SetReference(serializedMenu, "advancedManualJoinToggleButton", advancedToggle);
            SetReference(serializedMenu, "manualAddressInput", manualAddress);
            SetReference(serializedMenu, "manualPortInput", manualPort);
            SetReference(serializedMenu, "joinManualButton", joinManual);
            SetReference(serializedMenu, "diagnosticsPanel", diagnosticsPanel);
            SetReference(serializedMenu, "diagnosticsText", diagnosticsText);
            SetReference(serializedMenu, "connectedPlayersText", playersText);
            SetReference(serializedMenu, "exitButton", exitButton);
            serializedMenu.ApplyModifiedPropertiesWithoutUndo();

            return networkingPanel;
        }

        private static void BuildHeader(RectTransform parent)
        {
            RectTransform header = CreateLayoutSection("HeaderPanel", parent, ReferenceWidth, HeaderHeight, Color.clear);
            CreateText(header, "TitleText", "Multiplayer Test", 28, FontStyle.Bold, TextPrimary, TextAnchor.MiddleCenter, 34f);
            CreateText(
                header,
                "SubtitleText",
                "Host or join a local character controller test.",
                16,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.MiddleCenter,
                22f);
        }

        private static void BuildNameSection(RectTransform parent)
        {
            RectTransform namePanel = CreateLayoutSection("NamePanel", parent, ReferenceWidth, NameSectionHeight, PanelColor);
            AddVerticalLayout(namePanel, 6f, new RectOffset(18, 18, 12, 12));
            CreateText(namePanel, "NameLabel", "Player Name", 18, FontStyle.Bold, TextPrimary, TextAnchor.MiddleLeft, 24f);
            CreateInputField(namePanel, "PlayerNameInput", "Player name", 42f);
            CreateText(
                namePanel,
                "NameHelperText",
                "This name appears above your character for other players.",
                13,
                FontStyle.Italic,
                TextMuted,
                TextAnchor.MiddleLeft,
                18f);
        }

        private static void BuildHostJoinRow(RectTransform parent)
        {
            RectTransform row = CreateLayoutSection("HostJoinContainer", parent, ReferenceWidth, HostJoinRowHeight, Color.clear);
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 18f;
            rowLayout.childAlignment = TextAnchor.UpperCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;

            RectTransform hostCard = CreateCard("HostCard", row, HostJoinRowHeight);
            AddVerticalLayout(hostCard, 8f, new RectOffset(16, 16, 16, 16));
            CreateText(hostCard, "HostCardTitle", "Host Game", 20, FontStyle.Bold, TextPrimary, TextAnchor.MiddleLeft, 26f);
            CreateText(
                hostCard,
                "HostCardDescription",
                "Create a local test session and jump into the character test.",
                14,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.UpperLeft,
                44f);
            Button hostButton = CreatePrimaryButton(hostCard, "HostAndStartButton", "Host & Start", MenuButtonHeight);
            SetButtonExpandWidth(hostButton, false);

            RectTransform joinCard = CreateCard("JoinCard", row, HostJoinRowHeight);
            AddVerticalLayout(joinCard, 8f, new RectOffset(16, 16, 16, 16));
            CreateText(joinCard, "JoinCardTitle", "Join Game", 20, FontStyle.Bold, TextPrimary, TextAnchor.MiddleLeft, 26f);
            CreateText(
                joinCard,
                "JoinCardDescription",
                "Find a local host and join the character test.",
                14,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.UpperLeft,
                36f);
            CreateServerList(joinCard, ServerListHeight);
            CreateText(
                joinCard,
                "EmptyServerListText",
                "No local hosts found. Ask a player to host, then refresh.",
                14,
                FontStyle.Italic,
                TextMuted,
                TextAnchor.MiddleLeft,
                22f);

            RectTransform buttonRow = CreateLayoutSection("JoinButtons", joinCard, 0f, MenuButtonHeight, Color.clear);
            HorizontalLayoutGroup buttonLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = false;
            Button refreshButton = CreateSecondaryButton(buttonRow, "RefreshServersButton", "Refresh", MenuButtonHeight);
            Button joinButton = CreatePrimaryButton(buttonRow, "JoinSelectedButton", "Join Selected", MenuButtonHeight);
            SetButtonExpandWidth(refreshButton, true);
            SetButtonExpandWidth(joinButton, true);
        }

        private static void BuildAdvancedPanel(
            RectTransform parent,
            out GameObject advancedPanel,
            out Button advancedToggle,
            out InputField manualAddress,
            out InputField manualPort,
            out Button joinManual)
        {
            advancedToggle = CreateSecondaryButton(parent, "AdvancedManualJoinToggleButton", "Advanced Manual Join", AdvancedToggleHeight);
            advancedPanel = CreateLayoutSection("AdvancedManualJoinPanel", parent, ReferenceWidth, AdvancedPanelHeight, PanelColor).gameObject;
            advancedPanel.SetActive(false);
            RectTransform panelRect = advancedPanel.GetComponent<RectTransform>();
            AddVerticalLayout(panelRect, 8f, new RectOffset(18, 18, 12, 12));
            CreateText(panelRect, "AdvancedLabel", "Manual address and port", 15, FontStyle.Bold, TextPrimary, TextAnchor.MiddleLeft, 22f);
            manualAddress = CreateInputField(panelRect, "ManualAddressInput", "127.0.0.1", 40f);
            manualPort = CreateInputField(panelRect, "ManualPortInput", CCS_NetcodeTestConstants.DefaultServerPort.ToString(), 40f);
            joinManual = CreateSecondaryButton(panelRect, "JoinManualButton", "Join Manual", SecondaryButtonHeight);
        }

        private static void BuildDiagnosticsPanel(RectTransform parent, out GameObject diagnosticsPanel, out Text diagnosticsText)
        {
            diagnosticsPanel = CreateLayoutSection("DiagnosticsPanel", parent, ReferenceWidth, 36f, new Color(0.08f, 0.12f, 0.18f, 0.72f)).gameObject;
            diagnosticsPanel.SetActive(false);
            diagnosticsText = CreateText(
                diagnosticsPanel.GetComponent<RectTransform>(),
                "DiagnosticsText",
                string.Empty,
                14,
                FontStyle.Italic,
                TextMuted,
                TextAnchor.MiddleLeft,
                36f);
        }

        private static void BuildFooter(
            RectTransform parent,
            out Text playersText,
            out Button backButton,
            out Button exitButton)
        {
            RectTransform footer = CreateLayoutSection("FooterPanel", parent, ReferenceWidth, FooterHeight, Color.clear);
            HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 10f;
            footerLayout.childAlignment = TextAnchor.MiddleCenter;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = true;
            footerLayout.childForceExpandHeight = false;

            backButton = CreateSecondaryButton(footer, CCS_NetcodeTestConstants.BackButtonObjectName, "Back", SecondaryButtonHeight);
            SetButtonExpandWidth(backButton, false);
            LayoutElement backLayout = backButton.GetComponent<LayoutElement>();
            backLayout.preferredWidth = 110f;
            backLayout.minWidth = 96f;

            playersText = CreateText(
                footer,
                "ConnectedPlayersText",
                "Players: 0 / 3",
                15,
                FontStyle.Normal,
                TextMuted,
                TextAnchor.MiddleLeft,
                SecondaryButtonHeight);
            LayoutElement playersLayout = playersText.GetComponent<LayoutElement>();
            playersLayout.flexibleWidth = 1f;

            exitButton = CreateSecondaryButton(footer, "ExitButton", "Exit", SecondaryButtonHeight);
            SetButtonExpandWidth(exitButton, false);
            LayoutElement exitLayout = exitButton.GetComponent<LayoutElement>();
            exitLayout.flexibleWidth = 0f;
            exitLayout.preferredWidth = 96f;
            exitLayout.minWidth = 88f;
        }

        private static void CreateServerList(RectTransform joinCard, float height)
        {
            GameObject scrollObject = new GameObject(
                "ServerListScroll",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(ScrollRect),
                typeof(LayoutElement));
            scrollObject.transform.SetParent(joinCard, false);
            ConfigureLayoutChild(scrollObject.GetComponent<RectTransform>(), 0f, height);
            LayoutElement scrollLayout = scrollObject.GetComponent<LayoutElement>();
            scrollLayout.preferredHeight = height;
            scrollLayout.minHeight = height;
            scrollObject.GetComponent<Image>().color = new Color(0.07f, 0.1f, 0.15f, 1f);

            GameObject viewport = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(RectMask2D));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            StretchRect(viewportRect, Vector2.zero, Vector2.zero);
            viewport.GetComponent<Image>().color = new Color(0.07f, 0.1f, 0.15f, 1f);

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

        private static float ComputeNetworkingContentPreferredHeight()
        {
            const int sectionCount = 6;
            return HeaderHeight
                + NameSectionHeight
                + HostJoinRowHeight
                + AdvancedToggleHeight
                + FooterHeight
                + 14f * sectionCount;
        }

        private static void SetButtonExpandWidth(Button button, bool expandWidth)
        {
            if (button == null)
            {
                return;
            }

            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout == null)
            {
                return;
            }

            layout.flexibleWidth = expandWidth ? 1f : 0f;
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

        private static RectTransform CreateLayoutSection(string name, Transform parent, float width, float height, Color color)
        {
            GameObject sectionObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            sectionObject.transform.SetParent(parent, false);
            RectTransform rect = sectionObject.GetComponent<RectTransform>();
            ConfigureLayoutChild(rect, width, height);

            Image image = sectionObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            LayoutElement layout = sectionObject.GetComponent<LayoutElement>();
            if (height > 0f)
            {
                layout.preferredHeight = height;
                layout.minHeight = height;
            }

            if (width > 0f)
            {
                layout.preferredWidth = width;
            }

            return rect;
        }

        private static RectTransform CreateCard(string name, Transform parent, float height)
        {
            RectTransform card = CreateLayoutSection(name, parent, 0f, height, CardColor);
            LayoutElement cardLayout = card.GetComponent<LayoutElement>();
            cardLayout.flexibleWidth = 1f;
            cardLayout.preferredHeight = height;
            cardLayout.minHeight = height;
            return card;
        }

        private static void ConfigureLayoutChild(RectTransform rect, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width > 0f ? width : 0f, height > 0f ? height : 0f);
        }

        private static void AddVerticalLayout(RectTransform rect, float spacing, RectOffset padding)
        {
            VerticalLayoutGroup layout = rect.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static void CreateFlexibleSpacer(RectTransform parent, float minHeight)
        {
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(parent, false);
            LayoutElement layout = spacer.GetComponent<LayoutElement>();
            layout.minHeight = minHeight;
            layout.preferredHeight = minHeight;
            layout.flexibleHeight = 1f;
        }

        private static Text CreateText(
            RectTransform parent,
            string name,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment,
            float height)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(LayoutElement));
            textObject.transform.SetParent(parent, false);
            ConfigureLayoutChild(textObject.GetComponent<RectTransform>(), 0f, height);
            LayoutElement layout = textObject.GetComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static InputField CreateInputField(RectTransform parent, string name, string placeholder, float height)
        {
            GameObject inputRoot = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(InputField),
                typeof(LayoutElement));
            inputRoot.transform.SetParent(parent, false);
            LayoutElement layout = inputRoot.GetComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;
            inputRoot.GetComponent<Image>().color = new Color(0.07f, 0.1f, 0.15f, 1f);

            GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            placeholderObject.transform.SetParent(inputRoot.transform, false);
            RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
            StretchRect(placeholderRect, new Vector2(12f, 6f), new Vector2(-12f, -6f));
            Text placeholderText = placeholderObject.GetComponent<Text>();
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.fontSize = 17;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.color = TextMuted;
            placeholderText.text = placeholder;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(inputRoot.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            StretchRect(textRect, new Vector2(12f, 6f), new Vector2(-12f, -6f));
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = TextPrimary;
            text.supportRichText = false;

            InputField inputField = inputRoot.GetComponent<InputField>();
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;
            inputField.lineType = InputField.LineType.SingleLine;
            return inputField;
        }

        private static Button CreatePrimaryButton(RectTransform parent, string name, string label, float height)
        {
            return CreateButton(parent, name, label, height, PrimaryButtonColor, true);
        }

        private static Button CreateSecondaryButton(RectTransform parent, string name, string label, float height)
        {
            return CreateButton(parent, name, label, height, SecondaryButtonColor, false);
        }

        private static Button CreateButton(
            RectTransform parent,
            string name,
            string label,
            float height,
            Color backgroundColor,
            bool expandWidth)
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);
            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;
            if (expandWidth)
            {
                layout.flexibleWidth = 1f;
            }

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            StretchRect(labelRect, Vector2.zero, Vector2.zero);
            Text text = labelObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TextPrimary;
            text.text = label;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = PrimaryButtonColor * 1.1f;
            colors.pressedColor = PrimaryButtonColor * 0.85f;
            colors.selectedColor = PrimaryButtonColor;
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
