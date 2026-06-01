using CCS.Modules.Hotbar;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ActiveItemToolRoutingBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Updates active item profile for milestone 1.2.3 tool routing foundation.
// PLACEMENT: Batch entry for 1.2.3 primitive tool use routing milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_ActiveItemToolRoutingBootstrapSetup
    {
        private const string ProfileFolder = "Assets/CCS/Survival/Profiles/Hotbar";
        private const string ProfilePath = ProfileFolder + "/CCS_DefaultActiveItemProfile.asset";
        private const string LogPrefix = "[CCS_ActiveItemToolRoutingBootstrapSetup]";

        public static void ExecuteBatch()
        {
            CCS_ActiveItemProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ActiveItemProfile>(ProfilePath);
            if (profile == null)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Missing profile at {ProfilePath}. Run active item foundation bootstrap first.");
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Active item selection and primitive tool use routing for milestone 1.2.3.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.2.3";
            serializedProfile.FindProperty("enableGatheringRouting").boolValue = true;
            serializedProfile.FindProperty("enableResourceHarvestRouting").boolValue = true;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log($"{LogPrefix} Updated {ProfilePath} for milestone 1.2.3.");
        }
    }
}
