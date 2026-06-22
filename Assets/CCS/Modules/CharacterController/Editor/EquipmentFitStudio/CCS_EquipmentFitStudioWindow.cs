using System.Collections.Generic;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only socket and IK tuning tool with live preview viewport.
// PLACEMENT: Open via CCS/Character Controller/Equipment/Equipment Fit Studio.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Preview objects are temporary and must never be saved to assets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitStudioWindow : EditorWindow
    {
        #region Variables

        private CCS_EquipmentFitStudioSettings settings;

        private CCS_EquipmentFitStudioSelectionState state = new CCS_EquipmentFitStudioSelectionState();

        private CCS_EquipmentFitStudioPreviewCamera previewCamera = new CCS_EquipmentFitStudioPreviewCamera();

        private CCS_EquipmentFitStudioPreviewItem previewItem = new CCS_EquipmentFitStudioPreviewItem();

        private Vector2 scrollPosition;

        private string statusMessage = "Ready.";

        private MessageType statusType = MessageType.Info;

        private float previewRigWeight;

        private float previewRightHandIkWeight;

        private float previewLeftHandIkWeight;

        private float previewAimWeight;

        private readonly string[] ikTargetNames =
        {
            CCS_EquipmentConstants.RightHandIkTargetObjectName,
            CCS_EquipmentConstants.RightElbowHintObjectName,
            CCS_EquipmentConstants.LeftHandIkTargetObjectName,
            CCS_EquipmentConstants.LeftElbowHintObjectName,
            CCS_EquipmentConstants.WeaponAimTargetObjectName,
        };

        #endregion

        #region Unity Callbacks

        [MenuItem(CCS_EquipmentConstants.EquipmentFitStudioMenuPath)]
        public static void OpenWindow()
        {
            CCS_EquipmentFitStudioWindow window = GetWindow<CCS_EquipmentFitStudioWindow>("Equipment Fit Studio");
            window.minSize = new Vector2(980f, 640f);
            window.Show();
        }

        private void OnEnable()
        {
            CCS_EquipmentFitStudioProfileBuilder.EnsureEquipmentFitStudioAssets();
            settings = AssetDatabase.LoadAssetAtPath<CCS_EquipmentFitStudioSettings>(
                CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
            previewCamera.EnsureCamera(settings);
            SceneView.duringSceneGui += OnSceneGui;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            TryAssignDefaultPlayer();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            CleanupTemporaryObjects(resetIkPreviewWeights: true);
        }

        private void OnGUI()
        {
            CCS_EquipmentFitStudioStyles.EnsureInitialized();
            DrawTopBar();
            DrawModeTabs();
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawPreviewPanel();
            EditorGUILayout.EndHorizontal();
            DrawStatusBar();
        }

        #endregion

        #region Private Methods

        private void DrawTopBar()
        {
            EditorGUILayout.LabelField("CCS Equipment Fit Studio", CCS_EquipmentFitStudioStyles.TitleLabel);
            EditorGUILayout.BeginHorizontal();
            state.PlayerRoot = (GameObject)EditorGUILayout.ObjectField(
                "Player",
                state.PlayerRoot,
                typeof(GameObject),
                true);
            if (GUILayout.Button("Find Player", GUILayout.Width(90f)))
            {
                TryAssignDefaultPlayer();
            }

            int socketIndex = GetSelectedSocketIndex();
            socketIndex = EditorGUILayout.Popup("Socket", socketIndex, CCS_EquipmentConstants.RequiredSocketIds);
            state.SelectedSocketId = CCS_EquipmentConstants.RequiredSocketIds[socketIndex];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn Preview Item"))
            {
                SpawnPreviewItem();
            }

            if (GUILayout.Button("Clear Preview"))
            {
                previewItem.DestroyPreview();
                state.PreviewItemSpawned = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawModeTabs()
        {
            state.Mode = (CCS_EquipmentFitStudioMode)GUILayout.Toolbar(
                (int)state.Mode,
                new[]
                {
                    "Socket Tuner",
                    "IK Target Tuner",
                    "Preview View",
                    "Hand/Finger Pose",
                    "Save / Validate",
                });
        }

        private void DrawLeftPanel()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(360f));
            switch (state.Mode)
            {
                case CCS_EquipmentFitStudioMode.SocketTuner:
                    DrawSocketTunerPanel();
                    break;
                case CCS_EquipmentFitStudioMode.IkTargetTuner:
                    DrawIkTargetTunerPanel();
                    break;
                case CCS_EquipmentFitStudioMode.PreviewView:
                    DrawPreviewControlsPanel();
                    break;
                case CCS_EquipmentFitStudioMode.HandPoseFoundation:
                    DrawHandPoseFoundationPanel();
                    break;
                case CCS_EquipmentFitStudioMode.SaveValidate:
                    DrawSaveValidatePanel();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            Rect previewRect = GUILayoutUtility.GetRect(10f, 10000f, 10f, 10000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            previewCamera.EnsureRenderTexture(Mathf.Max(320, (int)previewRect.width), Mathf.Max(180, (int)previewRect.height));
            previewCamera.HandleInput(previewRect, Event.current);
            previewCamera.RenderNow();
            if (previewCamera.RenderTexture != null)
            {
                GUI.DrawTexture(previewRect, previewCamera.RenderTexture, ScaleMode.StretchToFill, false);
            }

            DrawPreviewItemStatus();
            DrawPreviewPresetButtons();
            EditorGUILayout.EndVertical();
        }

        private void DrawSocketTunerPanel()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            CCS_EquipmentSocketAnchor anchor = socketTransform != null
                ? socketTransform.GetComponent<CCS_EquipmentSocketAnchor>()
                : null;
            DrawHint("Move the socket until the item sits where it should attach.");
            DrawHint("Keep the preview item zeroed. Only the socket should move.");

            if (socketTransform == null)
            {
                EditorGUILayout.HelpBox("Select a player with equipment sockets.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Socket ID", anchor != null ? anchor.SocketId : state.SelectedSocketId);
            EditorGUILayout.LabelField("Parent Bone", anchor != null ? anchor.ParentBone.ToString() : "Unknown");
            if (anchor != null && anchor.AllowedItemTypes != null)
            {
                EditorGUILayout.LabelField("Allowed Types", string.Join(", ", anchor.AllowedItemTypes));
            }

            Vector3 position = socketTransform.localPosition;
            Vector3 euler = socketTransform.localEulerAngles;
            Vector3 scale = socketTransform.localScale;
            position = EditorGUILayout.Vector3Field("Local Position", position);
            euler = EditorGUILayout.Vector3Field("Local Rotation", euler);
            scale = EditorGUILayout.Vector3Field("Local Scale", scale);
            socketTransform.localPosition = position;
            socketTransform.localRotation = Quaternion.Euler(euler);
            socketTransform.localScale = scale;
            previewItem.EnforceZeroedTransform();

            DrawNudgeControls(settings, ref position, ref euler, ref scale, socketTransform);
            DrawPendingDiff(state.SocketPendingChange, socketTransform);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture Live Socket"))
            {
                state.SocketPendingChange.Capture(
                    state.SelectedSocketId,
                    socketTransform.localPosition,
                    socketTransform.localEulerAngles,
                    socketTransform.localScale);
            }

            if (GUILayout.Button("Reset To Profile"))
            {
                ResetSocketToProfile(socketTransform);
            }

            if (GUILayout.Button("Mirror Right ↔ Left"))
            {
                MirrorSelectedSocket(socketTransform);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawIkTargetTunerPanel()
        {
            Transform ikTarget = GetSelectedIkTargetTransform();
            DrawHint("Use the elbow hint to make the arm bend naturally.");
            if (!HasActiveIkConstraints())
            {
                EditorGUILayout.HelpBox(
                    "IK constraints are not active on this rig, but target positions can still be authored.",
                    MessageType.Info);
            }

            int ikIndex = System.Array.IndexOf(ikTargetNames, state.SelectedIkTargetName);
            if (ikIndex < 0)
            {
                ikIndex = 0;
            }

            ikIndex = EditorGUILayout.Popup("IK Target", ikIndex, ikTargetNames);
            state.SelectedIkTargetName = ikTargetNames[ikIndex];
            ikTarget = GetSelectedIkTargetTransform();
            if (ikTarget == null)
            {
                EditorGUILayout.HelpBox("Select a player with CCS_WeaponIKTargets.", MessageType.Warning);
                return;
            }

            Vector3 position = ikTarget.localPosition;
            Vector3 euler = ikTarget.localEulerAngles;
            position = EditorGUILayout.Vector3Field("Local Position", position);
            euler = EditorGUILayout.Vector3Field("Local Rotation", euler);
            ikTarget.localPosition = position;
            ikTarget.localRotation = Quaternion.Euler(euler);
            DrawNudgeControls(settings, ref position, ref euler, ref position, ikTarget, allowScale: false);

            EditorGUILayout.Space();
            previewRigWeight = EditorGUILayout.Slider("Preview Rig Weight", previewRigWeight, 0f, 1f);
            previewRightHandIkWeight = EditorGUILayout.Slider("Preview Right Hand IK", previewRightHandIkWeight, 0f, 1f);
            previewLeftHandIkWeight = EditorGUILayout.Slider("Preview Left Hand IK", previewLeftHandIkWeight, 0f, 1f);
            previewAimWeight = EditorGUILayout.Slider("Preview Aim Weight", previewAimWeight, 0f, 1f);
            ApplyIkPreviewWeights();
            if (GUILayout.Button("Reset Weights To Zero"))
            {
                previewRigWeight = 0f;
                previewRightHandIkWeight = 0f;
                previewLeftHandIkWeight = 0f;
                previewAimWeight = 0f;
                ApplyIkPreviewWeights();
            }

            if (GUILayout.Button("Capture Live Target"))
            {
                state.IkPendingChange.Capture(
                    state.SelectedIkTargetName,
                    ikTarget.localPosition,
                    ikTarget.localEulerAngles,
                    Vector3.one);
            }
        }

        private void DrawPreviewControlsPanel()
        {
            DrawHint("Left drag = orbit. Middle drag or Shift+drag = pan. Mouse wheel = zoom.");
            DrawPreviewPresetButtons();
        }

        private void DrawHandPoseFoundationPanel()
        {
            DrawHint("Finger pose controls are foundation-only in v0.6.7.");
            Animator animator = GetPlayerAnimator();
            DrawFingerBoneDetection("Right Hand", animator, true);
            DrawFingerBoneDetection("Left Hand", animator, false);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Slider("Thumb Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Index Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Middle Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Ring Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Little Curl (future)", 0f, 0f, 1f);
            EditorGUI.EndDisabledGroup();

            state.SelectedHandPoseDefinition = (CCS_HandPoseDefinition)EditorGUILayout.ObjectField(
                "Hand Pose Asset",
                state.SelectedHandPoseDefinition,
                typeof(CCS_HandPoseDefinition),
                false);
            if (GUILayout.Button("Create Hand Pose Asset"))
            {
                CreateHandPoseAsset();
            }
        }

        private void DrawSaveValidatePanel()
        {
            DrawPendingDiff(state.SocketPendingChange, GetSelectedSocketTransform());
            DrawPendingDiff(state.IkPendingChange, GetSelectedIkTargetTransform());
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save To Socket Definition"))
            {
                SaveSocketToDefinition(showDialog: true);
            }

            if (GUILayout.Button("Save To Attachment Fit Profile"))
            {
                SaveAttachmentFitProfile(showDialog: true);
            }

            if (GUILayout.Button("Save To IK Pose Profile"))
            {
                SaveIkPoseProfile(showDialog: true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Profile To Player"))
            {
                ApplySocketProfileToPlayer();
            }

            if (GUILayout.Button("Rebuild / Apply"))
            {
                RebuildAndApply();
            }

            if (GUILayout.Button("Validate"))
            {
                RunValidation();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.Space();
            if (settings != null && settings.ShowHints)
            {
                DrawHint("Green means safe. Yellow means check it. Red means fix before saving.");
            }

            EditorGUILayout.HelpBox(statusMessage, statusType);
            if (state.HasUnsavedChanges)
            {
                EditorGUILayout.LabelField("Pending unsaved changes", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
            }
        }

        private void DrawPreviewItemStatus()
        {
            if (!previewItem.IsSpawned)
            {
                EditorGUILayout.LabelField("Preview item: not spawned", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
                return;
            }

            GUIStyle style = previewItem.IsZeroed
                ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                : CCS_EquipmentFitStudioStyles.StatusErrorLabel;
            EditorGUILayout.LabelField(
                "Preview item local transform — Position: 0,0,0  Rotation: identity  Scale: 1,1,1 "
                + (previewItem.IsZeroed ? "✅" : "❌"),
                style);
            if (!previewItem.IsZeroed && GUILayout.Button("Reset Preview Item To Zero"))
            {
                previewItem.ResetPreviewItemToZero();
            }
        }

        private void DrawPreviewPresetButtons()
        {
            EditorGUILayout.BeginHorizontal();
            DrawPresetButton("Frame", CCS_EquipmentFitStudioCameraPreset.Frame);
            DrawPresetButton("Full Body", CCS_EquipmentFitStudioCameraPreset.FullBody);
            DrawPresetButton("Right Hand", CCS_EquipmentFitStudioCameraPreset.RightHand);
            DrawPresetButton("Left Hand", CCS_EquipmentFitStudioCameraPreset.LeftHand);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawPresetButton("Right Hip", CCS_EquipmentFitStudioCameraPreset.RightHip);
            DrawPresetButton("Left Hip", CCS_EquipmentFitStudioCameraPreset.LeftHip);
            DrawPresetButton("Back", CCS_EquipmentFitStudioCameraPreset.Back);
            DrawPresetButton("Trigger Close-Up", CCS_EquipmentFitStudioCameraPreset.TriggerCloseUp);
            DrawPresetButton("Muzzle View", CCS_EquipmentFitStudioCameraPreset.MuzzleView);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPresetButton(string label, CCS_EquipmentFitStudioCameraPreset preset)
        {
            if (GUILayout.Button(label))
            {
                previewCamera.ApplyPreset(preset, state.PlayerRoot, state.SelectedSocketId);
            }
        }

        private static void DrawHint(string message)
        {
            EditorGUILayout.LabelField(message, CCS_EquipmentFitStudioStyles.HintLabel);
        }

        private static void DrawPendingDiff(CCS_EquipmentFitStudioPendingChange pending, Transform target)
        {
            if (target == null || !pending.HasChanges)
            {
                return;
            }

            EditorGUILayout.LabelField(
                pending.Label + " Position: " + pending.OldPosition + " -> " + pending.NewPosition);
            EditorGUILayout.LabelField(
                pending.Label + " Rotation: " + pending.OldEulerAngles + " -> " + pending.NewEulerAngles);
            EditorGUILayout.LabelField(
                pending.Label + " Scale: " + pending.OldScale + " -> " + pending.NewScale);
        }

        private static void DrawNudgeControls(
            CCS_EquipmentFitStudioSettings studioSettings,
            ref Vector3 position,
            ref Vector3 euler,
            ref Vector3 scale,
            Transform target,
            bool allowScale = true)
        {
            float posSmall = studioSettings != null ? studioSettings.NudgePositionSmall : 0.01f;
            float posLarge = studioSettings != null ? studioSettings.NudgePositionLarge : 0.05f;
            float rotSmall = studioSettings != null ? studioSettings.NudgeRotationSmall : 1f;
            float rotLarge = studioSettings != null ? studioSettings.NudgeRotationLarge : 5f;

            EditorGUILayout.LabelField("Position Nudge");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+X")) position.x += posSmall;
            if (GUILayout.Button("-X")) position.x -= posSmall;
            if (GUILayout.Button("+Y")) position.y += posSmall;
            if (GUILayout.Button("-Y")) position.y -= posSmall;
            if (GUILayout.Button("+Z")) position.z += posSmall;
            if (GUILayout.Button("-Z")) position.z -= posSmall;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Rotation Nudge");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pitch +")) euler.x += rotSmall;
            if (GUILayout.Button("Pitch -")) euler.x -= rotSmall;
            if (GUILayout.Button("Yaw +")) euler.y += rotSmall;
            if (GUILayout.Button("Yaw -")) euler.y -= rotSmall;
            if (GUILayout.Button("Roll +")) euler.z += rotSmall;
            if (GUILayout.Button("Roll -")) euler.z -= rotSmall;
            EditorGUILayout.EndHorizontal();

            if (allowScale)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Scale +")) scale += Vector3.one * 0.01f;
                if (GUILayout.Button("Scale -")) scale -= Vector3.one * 0.01f;
                EditorGUILayout.EndHorizontal();
            }

            target.localPosition = position;
            target.localRotation = Quaternion.Euler(euler);
            if (allowScale)
            {
                target.localScale = scale;
            }
        }

        private void SpawnPreviewItem()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            GameObject source = settings != null ? settings.DefaultPreviewWeaponPrefab : null;
            if (previewItem.SpawnUnderSocket(socketTransform, source))
            {
                state.PreviewItemSpawned = true;
                if (settings != null && settings.AutoFrameOnSelection)
                {
                    previewCamera.FrameTransform(socketTransform, 1.2f);
                }

                SetStatus("Preview item spawned under " + state.SelectedSocketId + ".", MessageType.Info);
            }
            else
            {
                SetStatus("Could not spawn preview item.", MessageType.Error);
            }
        }

        private void SaveSocketToDefinition(bool showDialog)
        {
            Transform socketTransform = GetSelectedSocketTransform();
            CCS_EquipmentSocketDefinition definition = FindSocketDefinition(state.SelectedSocketId);
            if (socketTransform == null || definition == null)
            {
                SetStatus("Missing socket transform or definition.", MessageType.Error);
                return;
            }

            if (showDialog && !EditorUtility.DisplayDialog(
                    "Save Socket Definition",
                    "Save live socket values to " + definition.name + "?",
                    "Save",
                    "Cancel"))
            {
                return;
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("localPosition").vector3Value = socketTransform.localPosition;
            serializedDefinition.FindProperty("localEulerAngles").vector3Value = socketTransform.localEulerAngles;
            serializedDefinition.FindProperty("localScale").vector3Value = socketTransform.localScale;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            state.SocketPendingChange.SetBaseline(
                socketTransform.localPosition,
                socketTransform.localEulerAngles,
                socketTransform.localScale);
            SetStatus("Saved socket definition for " + state.SelectedSocketId + ".", MessageType.Info);
        }

        private void SaveAttachmentFitProfile(bool showDialog)
        {
            Transform socketTransform = GetSelectedSocketTransform();
            if (socketTransform == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Attachment Fit Profile",
                "CCS_WeaponAttachmentFitProfile_" + state.SelectedSocketId,
                "asset",
                "Choose save location",
                CCS_EquipmentConstants.EquipmentFittingProfileRootPath);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WeaponAttachmentFitProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.SetIdentity(
                System.IO.Path.GetFileNameWithoutExtension(path),
                settings != null ? settings.DefaultWeaponId : CCS_EquipmentConstants.DefaultPreviewWeaponId,
                settings != null ? settings.DefaultCharacterRigId : CCS_EquipmentConstants.DefaultCharacterRigId,
                state.SelectedSocketId);
            profile.ApplySocketTransform(
                profile.ProfileId,
                socketTransform.localPosition,
                socketTransform.localEulerAngles,
                socketTransform.localScale);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            state.SelectedAttachmentFitProfile = profile;
            SetStatus("Saved attachment fit profile.", MessageType.Info);
        }

        private void SaveIkPoseProfile(bool showDialog)
        {
            Transform ikRoot = GetIkTargetsRoot();
            if (ikRoot == null)
            {
                return;
            }

            CCS_WeaponIKPoseProfile profile = state.SelectedIkPoseProfile;
            if (profile == null)
            {
                profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponIKPoseProfile>(
                    CCS_EquipmentConstants.DefaultWeaponIkPoseProfilePath);
            }

            if (profile == null)
            {
                SetStatus("Missing IK pose profile asset.", MessageType.Error);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            WriteVector3(serializedProfile, "rightHandTargetLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.RightHandIkTargetObjectName));
            WriteVector3(serializedProfile, "rightElbowHintLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.RightElbowHintObjectName));
            WriteVector3(serializedProfile, "leftHandTargetLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.LeftHandIkTargetObjectName));
            WriteVector3(serializedProfile, "leftElbowHintLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.LeftElbowHintObjectName));
            WriteVector3(serializedProfile, "weaponAimTargetLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.WeaponAimTargetObjectName));
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            SetStatus("Saved IK pose profile.", MessageType.Info);
        }

        private void ApplySocketProfileToPlayer()
        {
            if (state.PlayerRoot == null)
            {
                return;
            }

            CCS_EquipmentSocketPlayerBuilder.EnsurePlayerEquipmentSocketFoundation(state.PlayerRoot);
            SetStatus("Applied socket profile to selected player.", MessageType.Info);
        }

        private void RebuildAndApply()
        {
            CleanupTemporaryObjects(resetIkPreviewWeights: true);
            CCS_EquipmentSocketProfileBuilder.EnsureDefaultEquipmentSocketProfile();
            CCS_EquipmentFitStudioProfileBuilder.EnsureEquipmentFitStudioAssets();
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            CCS_WeaponsAssetBuilder.EnsureWeaponsAssets();
            CCS_EquipmentFitStudioCleanupUtility.CleanupAllPreviewObjects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SetStatus("Rebuild complete. Preview objects cleaned up.", MessageType.Info);
        }

        private void RunValidation()
        {
            CCS_SurvivalValidationResult result = CCS_EquipmentFitStudioValidationUtility.ValidateEquipmentFitStudioFoundation();
            statusMessage = result.Message;
            statusType = result.IsSuccess ? MessageType.Info : MessageType.Error;
        }

        private void ResetSocketToProfile(Transform socketTransform)
        {
            CCS_EquipmentSocketDefinition definition = FindSocketDefinition(state.SelectedSocketId);
            if (definition == null || socketTransform == null)
            {
                return;
            }

            socketTransform.localPosition = definition.LocalPosition;
            socketTransform.localRotation = Quaternion.Euler(definition.LocalEulerAngles);
            socketTransform.localScale = definition.LocalScale;
            previewItem.EnforceZeroedTransform();
        }

        private void MirrorSelectedSocket(Transform socketTransform)
        {
            if (socketTransform == null)
            {
                return;
            }

            Vector3 position = socketTransform.localPosition;
            Vector3 euler = socketTransform.localEulerAngles;
            position.x *= -1f;
            euler.y *= -1f;
            euler.z *= -1f;
            socketTransform.localPosition = position;
            socketTransform.localRotation = Quaternion.Euler(euler);
        }

        private void CreateHandPoseAsset()
        {
            string path = AssetDatabase.GenerateUniqueAssetPath(
                CCS_EquipmentConstants.EquipmentFittingHandPoseFolderPath + "/CCS_HandPoseDefinition_New.asset");
            CCS_HandPoseDefinition pose = ScriptableObject.CreateInstance<CCS_HandPoseDefinition>();
            AssetDatabase.CreateAsset(pose, path);
            AssetDatabase.SaveAssets();
            state.SelectedHandPoseDefinition = pose;
        }

        private void TryAssignDefaultPlayer()
        {
            if (state.PlayerRoot != null)
            {
                return;
            }

            CCS_EquipmentSocketRegistry registry = Object.FindFirstObjectByType<CCS_EquipmentSocketRegistry>();
            if (registry != null)
            {
                state.PlayerRoot = registry.gameObject;
                return;
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null
                && prefabStage.prefabContentsRoot.GetComponent<CCS_EquipmentSocketRegistry>() != null)
            {
                state.PlayerRoot = prefabStage.prefabContentsRoot;
            }
        }

        private Transform GetSelectedSocketTransform()
        {
            if (state.PlayerRoot == null)
            {
                return null;
            }

            CCS_EquipmentSocketRegistry registry = state.PlayerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry != null && registry.TryGetSocket(state.SelectedSocketId, out Transform socketTransform))
            {
                return socketTransform;
            }

            return null;
        }

        private Transform GetSelectedIkTargetTransform()
        {
            Transform ikRoot = GetIkTargetsRoot();
            return ikRoot != null ? ikRoot.Find(state.SelectedIkTargetName) : null;
        }

        private Transform GetIkTargetsRoot()
        {
            if (state.PlayerRoot == null)
            {
                return null;
            }

            Transform visualRoot = FindDeepChild(state.PlayerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            return visualRoot != null ? visualRoot.Find(CCS_EquipmentConstants.WeaponIkTargetsObjectName) : null;
        }

        private CCS_EquipmentSocketDefinition FindSocketDefinition(string socketId)
        {
            CCS_EquipmentSocketProfile profile = settings != null
                ? settings.DefaultSocketProfile
                : AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketProfile>(
                    CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
            if (profile == null)
            {
                return null;
            }

            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = profile.SocketDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].SocketId == socketId)
                {
                    return definitions[i];
                }
            }

            return null;
        }

        private Animator GetPlayerAnimator()
        {
            if (state.PlayerRoot == null)
            {
                return null;
            }

            Transform visualRoot = FindDeepChild(state.PlayerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            return visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
        }

        private bool HasActiveIkConstraints()
        {
            Animator animator = GetPlayerAnimator();
            if (animator == null)
            {
                return false;
            }

            return animator.GetComponentsInChildren<TwoBoneIKConstraint>(true).Length > 0;
        }

        private void ApplyIkPreviewWeights()
        {
            Animator animator = GetPlayerAnimator();
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)?.GetComponent<Rig>();
            if (rig != null)
            {
                rig.weight = previewRigWeight;
            }

            TwoBoneIKConstraint[] twoBone = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < twoBone.Length; i++)
            {
                TwoBoneIKConstraint constraint = twoBone[i];
                if (constraint == null)
                {
                    continue;
                }

                if (constraint.name.Contains("Right"))
                {
                    constraint.weight = previewRightHandIkWeight;
                }
                else if (constraint.name.Contains("Left"))
                {
                    constraint.weight = previewLeftHandIkWeight;
                }
            }

            MultiAimConstraint[] aimConstraints = animator.GetComponentsInChildren<MultiAimConstraint>(true);
            for (int i = 0; i < aimConstraints.Length; i++)
            {
                if (aimConstraints[i] != null)
                {
                    aimConstraints[i].weight = previewAimWeight;
                }
            }
        }

        private void ResetIkPreviewWeights()
        {
            previewRigWeight = 0f;
            previewRightHandIkWeight = 0f;
            previewLeftHandIkWeight = 0f;
            previewAimWeight = 0f;
            ApplyIkPreviewWeights();
        }

        private void DrawFingerBoneDetection(string label, Animator animator, bool rightHand)
        {
            if (animator == null || !animator.isHuman)
            {
                EditorGUILayout.LabelField(label + ": humanoid rig missing");
                return;
            }

            HumanBodyBones[] bones =
            {
                rightHand ? HumanBodyBones.RightThumbProximal : HumanBodyBones.LeftThumbProximal,
                rightHand ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal,
                rightHand ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal,
            };
            int found = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                if (animator.GetBoneTransform(bones[i]) != null)
                {
                    found++;
                }
            }

            EditorGUILayout.LabelField(label + " finger bones detected: " + found + " / " + bones.Length);
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (settings == null || !settings.ShowGizmos)
            {
                return;
            }

            Transform socket = GetSelectedSocketTransform();
            if (socket != null)
            {
                Handles.color = Color.blue;
                Handles.SphereHandleCap(0, socket.position, Quaternion.identity, 0.03f, EventType.Repaint);
                Handles.Label(socket.position, "Socket: " + state.SelectedSocketId);
            }

            Transform ikTarget = GetSelectedIkTargetTransform();
            if (ikTarget != null)
            {
                Handles.color = new Color(0.7f, 0.3f, 1f);
                Handles.SphereHandleCap(0, ikTarget.position, Quaternion.identity, 0.025f, EventType.Repaint);
                Handles.Label(ikTarget.position, "IK: " + state.SelectedIkTargetName);
            }

            if (previewItem.IsSpawned)
            {
                Handles.color = previewItem.IsZeroed ? Color.green : Color.red;
                Handles.Label(previewItem.PreviewRoot.transform.position, "Preview zeroed: " + previewItem.IsZeroed);
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (state.HasUnsavedChanges && change == PlayModeStateChange.ExitingEditMode)
            {
                Debug.LogWarning("[Equipment Fit Studio] Pending unsaved changes exist before Play Mode.");
            }

            if (change == PlayModeStateChange.EnteredEditMode || change == PlayModeStateChange.ExitingPlayMode)
            {
                CleanupTemporaryObjects(resetIkPreviewWeights: true);
            }
        }

        private void CleanupTemporaryObjects(bool resetIkPreviewWeights)
        {
            previewItem.DestroyPreview();
            previewCamera.DestroyCamera();
            state.PreviewItemSpawned = false;
            if (resetIkPreviewWeights)
            {
                ResetIkPreviewWeights();
            }

            CCS_EquipmentFitStudioCleanupUtility.CleanupPreviewObjectsInOpenScenes();
        }

        private int GetSelectedSocketIndex()
        {
            for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
            {
                if (CCS_EquipmentConstants.RequiredSocketIds[i] == state.SelectedSocketId)
                {
                    return i;
                }
            }

            return 0;
        }

        private static Vector3 GetIkLocalPosition(Transform ikRoot, string childName)
        {
            Transform child = ikRoot.Find(childName);
            return child != null ? child.localPosition : Vector3.zero;
        }

        private static void WriteVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
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

        private void SetStatus(string message, MessageType type)
        {
            statusMessage = message;
            statusType = type;
        }

        #endregion
    }
}
