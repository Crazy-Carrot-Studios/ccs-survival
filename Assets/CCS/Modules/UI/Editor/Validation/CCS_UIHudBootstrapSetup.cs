using System.IO;
using CCS.Modules.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_UIHudBootstrapSetup
// CATEGORY: Modules / UI / Editor / Validation
// PURPOSE: Creates default HUD profile, HUD prefab, and bootstrap scene integration.
// PLACEMENT: Batch entry for 0.4.2 UI/HUD foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Permanent bootstrap setup used by milestone batch verification.
// =============================================================================

namespace CCS.Modules.UI.Editor
{
    public static class CCS_UIHudBootstrapSetup
    {
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/UI/CCS_DefaultHudProfile.asset";
        private const string HudPrefabPath = "Assets/CCS/Modules/UI/Prefabs/PF_CCS_HUD_Root.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_UIHudBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureProfileFolders();
            CCS_HudProfile profile = EnsureDefaultProfile();
            EnsureHudPrefab(profile);
            EnsureBootstrapSceneHudInstance();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} UI HUD bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureProfileFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder("Assets/CCS/Survival/Profiles/UI");
        }

        private static CCS_HudProfile EnsureDefaultProfile()
        {
            CCS_HudProfile existing = AssetDatabase.LoadAssetAtPath<CCS_HudProfile>(DefaultProfilePath);
            if (existing != null)
            {
                return existing;
            }

            CCS_HudProfile profile = ScriptableObject.CreateInstance<CCS_HudProfile>();
            AssetDatabase.CreateAsset(profile, DefaultProfilePath);

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default HUD";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.ui.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default HUD presentation profile for 0.4.2 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.4.2";
            serializedProfile.FindProperty("showSurvivalBars").boolValue = true;
            serializedProfile.FindProperty("showInteractionPrompt").boolValue = true;
            serializedProfile.FindProperty("showInventorySummary").boolValue = true;
            serializedProfile.FindProperty("showEquipmentSummary").boolValue = true;
            serializedProfile.FindProperty("showNotifications").boolValue = true;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);

            return profile;
        }

        private static void EnsureHudPrefab(CCS_HudProfile profile)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (existingPrefab != null)
            {
                SerializedObject rootSerialized = new SerializedObject(existingPrefab.GetComponent<CCS_HudRootPresenter>());
                rootSerialized.FindProperty("hudProfile").objectReferenceValue = profile;
                rootSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(existingPrefab);
                return;
            }

            GameObject root = new GameObject("PF_CCS_HUD_Root");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            root.AddComponent<GraphicRaycaster>();
            CCS_HudRootPresenter rootPresenter = root.AddComponent<CCS_HudRootPresenter>();

            GameObject survivalArea = CreatePanel(root.transform, "SurvivalBarArea", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(320f, 180f));
            CCS_SurvivalBarPresenter survivalPresenter = survivalArea.AddComponent<CCS_SurvivalBarPresenter>();
            CreateSurvivalBars(survivalArea.transform, survivalPresenter);

            GameObject promptArea = CreatePanel(root.transform, "InteractionPromptArea", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(420f, 40f));
            CCS_InteractionPromptPresenter promptPresenter = promptArea.AddComponent<CCS_InteractionPromptPresenter>();
            Text promptText = CreateText(promptArea.transform, "PromptText", string.Empty, 18, TextAnchor.MiddleCenter);
            promptText.color = Color.white;
            SetPresenterField(promptPresenter, "promptText", promptText);

            GameObject inventoryArea = CreatePanel(root.transform, "InventorySummaryArea", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(320f, 36f));
            CCS_InventorySummaryPresenter inventoryPresenter = inventoryArea.AddComponent<CCS_InventorySummaryPresenter>();
            Text inventoryText = CreateText(inventoryArea.transform, "SummaryText", "Inventory: --", 14, TextAnchor.MiddleRight);
            inventoryText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            SetPresenterField(inventoryPresenter, "summaryText", inventoryText);

            GameObject equipmentArea = CreatePanel(root.transform, "EquipmentSummaryArea", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -60f), new Vector2(420f, 36f));
            CCS_EquipmentSummaryPresenter equipmentPresenter = equipmentArea.AddComponent<CCS_EquipmentSummaryPresenter>();
            Text equipmentText = CreateText(equipmentArea.transform, "SummaryText", "Equipment: --", 14, TextAnchor.MiddleRight);
            equipmentText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            SetPresenterField(equipmentPresenter, "summaryText", equipmentText);

            GameObject notificationArea = CreatePanel(root.transform, "NotificationArea", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -110f), new Vector2(360f, 180f));
            CCS_NotificationQueue notificationQueue = notificationArea.AddComponent<CCS_NotificationQueue>();
            RectTransform notificationContainer = notificationArea.GetComponent<RectTransform>();
            GameObject notificationTemplateObject = CreatePanel(notificationArea.transform, "NotificationTemplate", new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(340f, 32f));
            notificationTemplateObject.SetActive(false);
            CCS_NotificationPresenter notificationTemplate = notificationTemplateObject.AddComponent<CCS_NotificationPresenter>();
            Text notificationText = CreateText(notificationTemplateObject.transform, "MessageText", "Notification", 13, TextAnchor.MiddleLeft);
            notificationText.color = Color.white;
            SetPresenterField(notificationTemplate, "messageText", notificationText);
            Image notificationBackground = notificationTemplateObject.GetComponent<Image>();
            SetPresenterField(notificationTemplate, "backgroundImage", notificationBackground);
            SetPresenterField(notificationQueue, "notificationContainer", notificationContainer);
            SetPresenterField(notificationQueue, "notificationTemplate", notificationTemplate);

            SerializedObject rootSerializedObject = new SerializedObject(rootPresenter);
            rootSerializedObject.FindProperty("hudProfile").objectReferenceValue = profile;
            rootSerializedObject.FindProperty("survivalBarPresenter").objectReferenceValue = survivalPresenter;
            rootSerializedObject.FindProperty("interactionPromptPresenter").objectReferenceValue = promptPresenter;
            rootSerializedObject.FindProperty("inventorySummaryPresenter").objectReferenceValue = inventoryPresenter;
            rootSerializedObject.FindProperty("equipmentSummaryPresenter").objectReferenceValue = equipmentPresenter;
            rootSerializedObject.FindProperty("notificationQueue").objectReferenceValue = notificationQueue;
            rootSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder("Assets/CCS/Modules/UI/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void EnsureBootstrapSceneHudInstance()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"{LogPrefix} Missing HUD prefab: {HudPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_HudRootPresenter[] existingPresenters =
                CCS.Survival.CCS_SurvivalSceneQueryUtility.FindAllObjectsByType<CCS_HudRootPresenter>();
            for (int index = 1; index < existingPresenters.Length; index++)
            {
                Object.DestroyImmediate(existingPresenters[index].gameObject);
            }

            if (existingPresenters.Length >= 1)
            {
                EditorSceneManager.SaveScene(scene);
                return;
            }

            PrefabUtility.InstantiatePrefab(prefab, scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateSurvivalBars(Transform parent, CCS_SurvivalBarPresenter presenter)
        {
            CreateBarRow(parent, "HealthRow", "Health", 140f, out Text healthLabel, out Image healthFill);
            CreateBarRow(parent, "StaminaRow", "Stamina", 110f, out Text staminaLabel, out Image staminaFill);
            CreateBarRow(parent, "HungerRow", "Hunger", 80f, out Text hungerLabel, out Image hungerFill);
            CreateBarRow(parent, "ThirstRow", "Thirst", 50f, out Text thirstLabel, out Image thirstFill);

            SetPresenterField(presenter, "healthLabel", healthLabel);
            SetPresenterField(presenter, "healthFill", healthFill);
            SetPresenterField(presenter, "staminaLabel", staminaLabel);
            SetPresenterField(presenter, "staminaFill", staminaFill);
            SetPresenterField(presenter, "hungerLabel", hungerLabel);
            SetPresenterField(presenter, "hungerFill", hungerFill);
            SetPresenterField(presenter, "thirstLabel", thirstLabel);
            SetPresenterField(presenter, "thirstFill", thirstFill);
        }

        private static void CreateBarRow(
            Transform parent,
            string rowName,
            string labelPrefix,
            float anchoredY,
            out Text label,
            out Image fill)
        {
            GameObject row = CreatePanel(parent, rowName, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, anchoredY), new Vector2(300f, 28f));
            label = CreateText(row.transform, "Label", $"{labelPrefix}: --", 13, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(0.45f, 1f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.color = Color.white;

            GameObject fillBackground = CreatePanel(row.transform, "FillBackground", new Vector2(0.45f, 0.1f), new Vector2(1f, 0.9f), Vector2.zero, Vector2.zero);
            fillBackground.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            GameObject fillObject = CreatePanel(fillBackground.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.2f, 0.75f, 0.35f, 0.95f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
        }

        private static GameObject CreatePanel(
            Transform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
            Image image = panelObject.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.25f);
            return panelObject;
        }

        private static Text CreateText(Transform parent, string objectName, string text, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(8f, 2f);
            rectTransform.offsetMax = new Vector2(-8f, -2f);
            Text textComponent = textObject.GetComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            return textComponent;
        }

        private static void SetPresenterField(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        #endregion
    }
}
