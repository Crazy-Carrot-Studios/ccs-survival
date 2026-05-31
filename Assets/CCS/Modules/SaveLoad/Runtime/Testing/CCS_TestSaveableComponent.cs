using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestSaveableComponent
// CATEGORY: Modules / SaveLoad / Runtime / Testing
// PURPOSE: Development-only saveable that stores string, integer, and timestamp fields.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Proves registry capture/restore without persisting gameplay modules yet.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [DefaultExecutionOrder(270)]
    public sealed class CCS_TestSaveableComponent : MonoBehaviour, CCS_ISaveable
    {
        private const string DefaultSaveableId = "ccs.survival.saveable.test.development";

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, this component registers with CCS_SaveLoadService on Start.")]
        [SerializeField] private bool enableTestSaveable = true;

        [Tooltip("Stable reverse-DNS saveable id used in save module data.")]
        [SerializeField] private string saveableId = DefaultSaveableId;

        [Tooltip("Sample string value persisted by the test saveable.")]
        [SerializeField] private string testString = "bootstrap-test";

        [Tooltip("Sample integer value persisted by the test saveable.")]
        [SerializeField] private int testInteger = 42;

        [Tooltip("UTC timestamp captured during the last save capture pass.")]
        [SerializeField] private string lastSavedTimestampUtc = string.Empty;

        private CCS_SaveLoadService registeredService;

        #endregion

        #region Properties

        public string SaveableId => string.IsNullOrWhiteSpace(saveableId) ? DefaultSaveableId : saveableId;

        public string TestString => testString;

        public int TestInteger => testInteger;

        public string LastSavedTimestampUtc => lastSavedTimestampUtc;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (!enableTestSaveable)
            {
                return;
            }

            if (!CCS_SaveLoadRuntimeBridge.TryGetSaveLoadService(out registeredService)
                || registeredService == null)
            {
                Debug.LogWarning("[CCS_TestSaveableComponent] Save/load service is unavailable.");
                return;
            }

            registeredService.RegisterSaveable(this);
        }

        private void OnDestroy()
        {
            if (registeredService != null)
            {
                registeredService.UnregisterSaveable(this);
                registeredService = null;
            }
        }

        #endregion

        #region Public Methods

        public string CaptureState()
        {
            lastSavedTimestampUtc = System.DateTime.UtcNow.ToString("o");

            CCS_TestSaveableState state = new CCS_TestSaveableState
            {
                testString = testString,
                testInteger = testInteger,
                timestampUtc = lastSavedTimestampUtc
            };

            return JsonUtility.ToJson(state);
        }

        public void RestoreState(string stateJson)
        {
            if (string.IsNullOrWhiteSpace(stateJson))
            {
                return;
            }

            CCS_TestSaveableState state = JsonUtility.FromJson<CCS_TestSaveableState>(stateJson);
            if (state == null)
            {
                return;
            }

            testString = state.testString ?? string.Empty;
            testInteger = state.testInteger;
            lastSavedTimestampUtc = state.timestampUtc ?? string.Empty;
        }

        #endregion
    }
}
