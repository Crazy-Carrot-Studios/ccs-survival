using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow (Workflow Steps partial)
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Accordion workflow step content for Equipment Fit Studio window.
// PLACEMENT: Partial class extension of CCS_EquipmentFitStudioWindow.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Buttons for each step live here; bottom bar mirrors primary actions.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed partial class CCS_EquipmentFitStudioWindow
    {
        private CCS_EquipmentFitStudioWorkflowAccordionCallbacks BuildWorkflowCallbacks()
        {
            return new CCS_EquipmentFitStudioWorkflowAccordionCallbacks
            {
                DrawSelectPlayerStep = DrawStepSelectPlayer,
                DrawSelectWeaponStep = DrawStepSelectWeapon,
                DrawSelectSocketStep = DrawStepSelectSocket,
                DrawSpawnPreviewStep = DrawStepSpawnPreview,
                DrawTuneSocketStep = DrawStepTuneSocket,
                DrawCaptureValuesStep = DrawStepCaptureValues,
                DrawSaveProfileStep = DrawStepSaveProfile,
                DrawTestSavedFitStep = DrawStepTestSavedFit,
                DrawClearValidateStep = DrawStepClearValidate,
                GetStepSummary = GetWorkflowStepSummary,
            };
        }

        private string GetWorkflowStepSummary(CCS_EquipmentFitStudioWorkflowStep step)
        {
            switch (step)
            {
                case CCS_EquipmentFitStudioWorkflowStep.SelectPlayer:
                    return state.PlayerRoot != null ? state.PlayerRoot.name : "missing";
                case CCS_EquipmentFitStudioWorkflowStep.SelectWeapon:
                    return state.SelectedWeaponId;
                case CCS_EquipmentFitStudioWorkflowStep.SelectSocket:
                    return CCS_EquipmentFitStudioRevolverFitUtility.GetSocketDisplayLabel(state.SelectedSocketId);
                case CCS_EquipmentFitStudioWorkflowStep.SpawnPreview:
                    return previewItem.IsSpawned ? "spawned" : "not spawned";
                case CCS_EquipmentFitStudioWorkflowStep.TuneSocket:
                    return state.Mode.ToString();
                case CCS_EquipmentFitStudioWorkflowStep.CaptureValues:
                    return state.HasPendingSaveCapture || state.SocketPendingChange.HasCaptured ? "captured" : "ready";
                case CCS_EquipmentFitStudioWorkflowStep.SaveProfile:
                    return state.JustSavedProfileThisSession
                        ? "saved"
                        : state.HasPendingSaveCapture
                            ? "ready to save"
                            : "waiting";
                case CCS_EquipmentFitStudioWorkflowStep.TestSavedFit:
                    return HasTestAttachments() ? "testing" : "ready";
                case CCS_EquipmentFitStudioWorkflowStep.ClearValidate:
                    return RequiresCleanup() ? "cleanup needed" : "clean";
                default:
                    return string.Empty;
            }
        }

        private void DrawStepSelectPlayer()
        {
            DrawHint("Pick the character you want to fit equipment on.");
            state.PlayerRoot = (GameObject)EditorGUILayout.ObjectField(
                "Player",
                state.PlayerRoot,
                typeof(GameObject),
                true);
            if (GUILayout.Button("Find Player"))
            {
                TryAssignDefaultPlayer();
            }

            EditorGUILayout.LabelField(
                "Status: " + (state.PlayerRoot != null ? "Found" : "Missing"),
                state.PlayerRoot != null
                    ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                    : CCS_EquipmentFitStudioStyles.StatusWarnLabel);
        }

        private void DrawStepSelectWeapon()
        {
            DrawHint("Revolver M1879 is the only weapon required for v0.6.8 tuning.");
            EditorGUILayout.LabelField("Weapon", "Revolver M1879");
            EditorGUILayout.LabelField("weaponId", state.SelectedWeaponId);
            bool sourceOk = CCS_EquipmentFitStudioVisualSourceUtility.ResolveRevolverPreviewVisualSource(
                settings != null ? settings.DefaultPreviewWeaponPrefab : null) != null;
            EditorGUILayout.LabelField(
                "Preview source: " + (sourceOk ? "ModelRoot/RevolverVisual OK" : "Missing"),
                sourceOk ? CCS_EquipmentFitStudioStyles.StatusOkLabel : CCS_EquipmentFitStudioStyles.StatusErrorLabel);
        }

        private void DrawStepSelectSocket()
        {
            DrawHint("Use the Active Fit Target selector above to choose weapon slot / fit target at any time.");
            EditorGUILayout.LabelField(
                "Active Slot",
                CCS_EquipmentFitStudioWorkflowSessionUtility.GetSlotDisplayLabel(state.SelectedSocketId));
            EditorGUILayout.LabelField("Socket ID", state.SelectedSocketId);
            EditorGUILayout.LabelField(
                "Profile",
                CCS_EquipmentFitStudioWorkflowSessionUtility.GetActiveProfileFileName(state.SelectedSocketId));

            CCS_EquipmentSocketAnchor anchor = GetSelectedSocketAnchor();
            EditorGUILayout.LabelField(
                "Validity: " + CCS_EquipmentFitStudioSocketCompatibilityUtility.GetSocketValidityLabel(
                    anchor,
                    state.SelectedSocketId));

            if (!IsSelectedSocketCompatible())
            {
                EditorGUILayout.HelpBox(
                    "Revolver is not allowed on this socket. Use Right Hip Holster or Right Hand for this weapon.",
                    MessageType.Error);
            }

            if (state.SelectedSocketId == CCS_EquipmentConstants.HolsterSocketLeftHipId && !IsSelectedSocketCompatible())
            {
                EditorGUILayout.HelpBox(
                    "Left Hip does not allow weapon.revolver. Preview is blocked unless override is enabled.",
                    MessageType.Warning);
            }

            state.AllowIncompatibleSocketOverride = EditorGUILayout.ToggleLeft(
                "Allow preview on incompatible socket",
                state.AllowIncompatibleSocketOverride);
        }

        private void DrawStepSpawnPreview()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            CCS_EquipmentSocketAnchor anchor = GetSelectedSocketAnchor();
            EditorGUILayout.LabelField("Socket", state.SelectedSocketId);
            EditorGUILayout.LabelField("Parent Bone", anchor != null ? anchor.ParentBone.ToString() : "Unknown");

            if (!string.IsNullOrEmpty(state.LastPreviewError))
            {
                EditorGUILayout.HelpBox(state.LastPreviewError, MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Spawn Preview Item",
                    CCS_EquipmentFitStudioButtonKind.SpawnPreview,
                    "Creates a temporary editor-only visual under the selected socket. It must stay zeroed.",
                    GUILayout.ExpandWidth(true)))
            {
                SpawnPreviewItem();
            }

            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Clear Preview",
                    CCS_EquipmentFitStudioButtonKind.ClearPreview,
                    "Removes preview item and resets IK preview weights.",
                    GUILayout.ExpandWidth(true)))
            {
                ClearPreviewAndWeights();
            }

            EditorGUILayout.EndHorizontal();

            if (previewItem.IsSpawned)
            {
                EditorGUILayout.LabelField(
                    previewItem.IsZeroed ? "Preview Spawned / Zeroed ✅" : "Preview Spawned / NOT Zeroed ❌",
                    previewItem.IsZeroed
                        ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                        : CCS_EquipmentFitStudioStyles.StatusErrorLabel);
            }
            else if (CanSpawnPreview(out string reason))
            {
                EditorGUILayout.LabelField("Ready to spawn preview.", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
            }
            else
            {
                EditorGUILayout.LabelField(reason, CCS_EquipmentFitStudioStyles.StatusErrorLabel);
            }
        }

        private void DrawStepTuneSocket()
        {
            DrawPoseTuningHintForActiveTarget();

            state.Mode = (CCS_EquipmentFitStudioMode)GUILayout.Toolbar(
                (int)state.Mode,
                new[] { "Socket", "IK", "Hand Pose" },
                GUILayout.ExpandWidth(true));

            if (state.Mode == CCS_EquipmentFitStudioMode.IkTargetTuner
                && CCS_EquipmentFitStudioWorkflowGuide.ShouldDeemphasizeIk(state.SelectedSocketId))
            {
                EditorGUILayout.HelpBox("IK is not needed for this passive carry socket.", MessageType.Info);
                EditorGUI.BeginDisabledGroup(true);
                DrawIkTargetTunerPanel();
                EditorGUI.EndDisabledGroup();
                return;
            }

            switch (state.Mode)
            {
                case CCS_EquipmentFitStudioMode.IkTargetTuner:
                    DrawIkTargetTunerPanel();
                    break;
                case CCS_EquipmentFitStudioMode.HandPoseFoundation:
                    DrawHandPoseFoundationPanel();
                    break;
                default:
                    DrawSocketTunerPanel();
                    break;
            }

            EditorGUILayout.Space(6f);
            DrawHint("When tuning looks good, capture live values before saving.");
            EditorGUI.BeginDisabledGroup(!CanCaptureLiveValues());
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Capture Live Values",
                    CCS_EquipmentFitStudioButtonKind.Capture,
                    CanCaptureLiveValues()
                        ? "Copies the current socket values into a pending save buffer."
                        : "Select a player and socket before capturing values.",
                    GUILayout.ExpandWidth(true)))
            {
                CaptureLiveValues();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawPoseTuningHintForActiveTarget()
        {
            if (state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                EditorGUILayout.HelpBox(
                    "Right Hand Equipped is tuned in Revolver Aim pose. Move the socket until the grip sits in the palm and the barrel points forward.",
                    MessageType.Info);
                return;
            }

            if (state.SelectedSocketId == CCS_EquipmentConstants.HolsterSocketRightHipId)
            {
                EditorGUILayout.HelpBox(
                    "Right Hip Holster is tuned in Neutral pose. IK is not needed for passive holster placement.",
                    MessageType.Info);
            }
        }

        private void DrawStepCaptureValues()
        {
            DrawHint("Capture does not move anything. It copies the current socket values so they can be saved in the next step.");
            EditorGUI.BeginDisabledGroup(!CanCaptureLiveValues());
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Capture Live Values",
                    CCS_EquipmentFitStudioButtonKind.Capture,
                    CanCaptureLiveValues()
                        ? "Copies the current socket values into a pending save buffer."
                        : "Select a player and socket before capturing values.",
                    GUILayout.ExpandWidth(true)))
            {
                CaptureLiveValues();
            }

            EditorGUI.EndDisabledGroup();

            if (state.HasPendingSaveCapture || state.SocketPendingChange.HasCaptured)
            {
                DrawPendingDiff(state.SocketPendingChange, GetSelectedSocketTransform());
                EditorGUILayout.LabelField("Step 6 complete. Step 7 Save Profile is now active.", CCS_EquipmentFitStudioStyles.StatusOkLabel);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Tune the socket in Step 5, then capture here or from the bottom action bar.",
                    MessageType.Info);
            }

            DrawPendingDiff(state.IkPendingChange, GetSelectedIkTargetTransform());
        }

        private void DrawStepSaveProfile()
        {
            DrawHint(
                "Save writes the captured values into the revolver fit profile asset. "
                + "After saving, builders and future runtime visual systems can reuse this exact placement.");
            DrawSavedFitProfileReadout();

            if (!string.IsNullOrEmpty(state.LastSaveConfirmationMessage))
            {
                EditorGUILayout.HelpBox(state.LastSaveConfirmationMessage, MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload Saved Profile Values"))
            {
                ReloadSelectedProfileFromDisk();
            }

            if (GUILayout.Button("Reload Profile From Disk"))
            {
                ReloadSelectedProfileFromDisk();
            }

            if (GUILayout.Button("Ping Profile Asset"))
            {
                PingSelectedFitProfileAsset();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Revolver Fit Profile To Seed Defaults"))
            {
                ResetRevolverFitProfilesToSeedDefaultsWithConfirmation();
            }

            EditorGUI.BeginDisabledGroup(!CanSavePendingCapture());
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Save To Revolver Fit Profile",
                    CCS_EquipmentFitStudioButtonKind.SaveProfile,
                    CanSavePendingCapture()
                        ? "Saves the captured socket values into the selected revolver fit profile asset."
                        : "Capture live values before saving.",
                    GUILayout.ExpandWidth(true)))
            {
                SaveActiveProfile();
            }

            EditorGUI.EndDisabledGroup();

            if (state.HasPendingSaveCapture)
            {
                EditorGUILayout.HelpBox(
                    "You have captured fit values that are not saved to the profile asset. Save before testing in Play Mode.",
                    MessageType.Warning);
                DrawPendingDiff(state.SocketPendingChange, GetSelectedSocketTransform());
            }

            EditorGUILayout.Space(6f);
            DrawRuntimeReadbackCheck();

            if (state.JustSavedProfileThisSession)
            {
                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Tune Another Slot"))
                {
                    TuneAnotherSlotSession();
                }
            }
        }

        private void ReloadSelectedProfileFromDisk()
        {
            string path = CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(state.SelectedSocketId);
            state.SelectedAttachmentFitProfile = CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(path);
            InitializePendingBaselineFromProfile();
            SetStatus("Reloaded profile from disk: " + path, MessageType.Info);
            Repaint();
        }

        private void ResetRevolverFitProfilesToSeedDefaultsWithConfirmation()
        {
            if (!EditorUtility.DisplayDialog(
                    "Reset Revolver Fit Profiles",
                    "Reset both revolver fit profile assets to builder seed defaults? This cannot be undone except by re-tuning.",
                    "Reset To Seed Defaults",
                    "Cancel"))
            {
                return;
            }

            if (CCS_RevolverM1879FitProfileBuilder.ResetRevolverFitProfilesToSeedDefaults())
            {
                state.LastSaveConfirmationMessage = string.Empty;
                LoadRevolverProfileSelections();
                SetStatus("Reset revolver fit profiles to seed defaults.", MessageType.Warning);
            }
            else
            {
                SetStatus("Could not reset revolver fit profiles.", MessageType.Error);
            }
        }

        private void DrawRuntimeReadbackCheck()
        {
            EditorGUILayout.LabelField("Runtime Readback Check", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            CCS_WeaponAttachmentFitProfile savedProfile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(
                    CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(state.SelectedSocketId));
            EditorGUILayout.LabelField("Saved profile values:", EditorStyles.boldLabel);
            if (savedProfile == null)
            {
                EditorGUILayout.LabelField(
                    "Missing fit profile for "
                    + CCS_EquipmentFitStudioWorkflowSessionUtility.GetSlotDisplayLabel(state.SelectedSocketId)
                    + ".",
                    CCS_EquipmentFitStudioStyles.StatusErrorLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Profile: " + savedProfile.name + ".asset");
                EditorGUILayout.LabelField(
                    "Position: "
                    + CCS_EquipmentFitStudioPendingChange.FormatVector3(savedProfile.SocketLocalPosition));
                EditorGUILayout.LabelField(
                    "Rotation: "
                    + CCS_EquipmentFitStudioPendingChange.FormatVector3(savedProfile.SocketLocalEulerAngles));
                EditorGUILayout.LabelField(
                    "Scale: "
                    + CCS_EquipmentFitStudioPendingChange.FormatVector3(savedProfile.SocketLocalScale));
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Current runtime attachment values:", EditorStyles.boldLabel);
            Transform attachmentRoot = GetActiveTargetRuntimeAttachmentRoot();
            if (attachmentRoot == null)
            {
                EditorGUILayout.LabelField("No attachment root active for this slot. Run test or enter Play Mode after pickup.");
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Position: "
                    + CCS_EquipmentFitStudioPendingChange.FormatVector3(attachmentRoot.localPosition));
                EditorGUILayout.LabelField(
                    "Rotation: "
                    + CCS_EquipmentFitStudioPendingChange.FormatVector3(attachmentRoot.localEulerAngles));
                EditorGUILayout.LabelField(
                    "Scale: "
                    + CCS_EquipmentFitStudioPendingChange.FormatVector3(attachmentRoot.localScale));
            }

            bool match = false;
            if (savedProfile != null && attachmentRoot != null && state.PlayerRoot != null)
            {
                CCS_EquipmentSocketRegistry registry = state.PlayerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
                if (registry != null
                    && TryGetSocketDefinitionForActiveTarget(registry, out Vector3 defPos, out Vector3 defEuler, out Vector3 defScale))
                {
                    match = CCS_WeaponAttachmentFitProfileApplicator.AttachmentRootMatchesProfile(
                        attachmentRoot,
                        savedProfile,
                        defPos,
                        defEuler,
                        defScale);
                }
            }

            EditorGUILayout.LabelField(
                "Match: " + (match ? "YES" : "NO"),
                match ? CCS_EquipmentFitStudioStyles.StatusOkLabel : CCS_EquipmentFitStudioStyles.StatusWarnLabel);

            EditorGUILayout.EndVertical();

            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    GetActiveTargetPrimaryTestButtonLabel(),
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Reloads saved profile from disk and applies it through the runtime attachment-root path.",
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedActiveTargetFit();
            }
        }

        private bool TryGetSocketDefinitionForActiveTarget(
            CCS_EquipmentSocketRegistry registry,
            out Vector3 position,
            out Vector3 euler,
            out Vector3 scale)
        {
            position = Vector3.zero;
            euler = Vector3.zero;
            scale = Vector3.one;

            CCS_EquipmentSocketProfile socketProfile = registry.EquipmentSocketProfile;
            if (socketProfile == null)
            {
                return false;
            }

            for (int i = 0; i < socketProfile.SocketDefinitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = socketProfile.SocketDefinitions[i];
                if (definition != null && definition.SocketId == state.SelectedSocketId)
                {
                    position = definition.LocalPosition;
                    euler = definition.LocalEulerAngles;
                    scale = definition.LocalScale;
                    return true;
                }
            }

            return false;
        }

        private void DrawSavedFitProfileReadout()
        {
            CCS_WeaponAttachmentFitProfile profile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(
                    CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(state.SelectedSocketId));
            if (profile == null)
            {
                EditorGUILayout.HelpBox("No revolver fit profile mapped for this socket.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (state.SelectedSocketId == CCS_EquipmentConstants.HolsterSocketRightHipId)
            {
                EditorGUILayout.LabelField("Saved Right Hip Profile Values:", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Saved Fit Profile Values:", EditorStyles.boldLabel);
            }

            EditorGUILayout.LabelField("Profile: " + profile.name + ".asset");
            EditorGUILayout.LabelField(
                "Saved Position: "
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(profile.SocketLocalPosition));
            EditorGUILayout.LabelField(
                "Saved Rotation: "
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(profile.SocketLocalEulerAngles));
            EditorGUILayout.LabelField(
                "Saved Scale: "
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(profile.SocketLocalScale));

            if (CCS_EquipmentFitStudioWorkflowSessionUtility.ProfileUsesSeedDefaults(profile))
            {
                EditorGUILayout.HelpBox(
                    "This slot is still using seed values. Tune and save this slot before production use.",
                    MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void PingSelectedFitProfileAsset()
        {
            string path = CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(state.SelectedSocketId);
            if (string.IsNullOrEmpty(path))
            {
                SetStatus("No revolver fit profile mapped for this socket.", MessageType.Error);
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null)
            {
                SetStatus("Missing fit profile asset at " + path, MessageType.Error);
                return;
            }

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
            SetStatus("Opened fit profile asset: " + asset.name, MessageType.Info);
        }

        private void DrawStepTestSavedFit()
        {
            DrawHint("Editor-only test attachments. Never saved to scene or prefab.");
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    GetActiveTargetPrimaryTestButtonLabel(),
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Tests the saved fit profile for the active slot.",
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedActiveTargetFit();
            }

            EditorGUILayout.BeginHorizontal();
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Test Hip",
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Temporary holster test using saved right hip profile.",
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedHolsterFit();
            }

            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Test Hand",
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Temporary equipped test using saved right hand profile.",
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedEquippedFit();
            }

            EditorGUILayout.EndHorizontal();

            if (state.JustSavedProfileThisSession && GUILayout.Button("Tune Another Slot"))
            {
                TuneAnotherSlotSession();
            }

            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Clear Test Attachments",
                    CCS_EquipmentFitStudioButtonKind.Warning,
                    "Removes editor test holster/equipped objects.",
                    GUILayout.ExpandWidth(true)))
            {
                ClearTestAttachments();
            }
        }

        private void DrawStepClearValidate()
        {
            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Clear Preview",
                    CCS_EquipmentFitStudioButtonKind.ClearPreview,
                    "Removes preview item.",
                    GUILayout.ExpandWidth(true)))
            {
                ClearPreviewAndWeights();
            }

            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Cleanup All Temp Objects",
                    CCS_EquipmentFitStudioButtonKind.Warning,
                    "Removes preview, test attachments, and preview camera.",
                    GUILayout.ExpandWidth(true)))
            {
                CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
                SetStatus("Cleanup complete.", MessageType.Info);
            }

            if (CCS_EquipmentFitStudioStyles.DrawMainActionButton(
                    "Validate",
                    CCS_EquipmentFitStudioButtonKind.Validate,
                    "Runs Fit Studio and revolver profile validation.",
                    GUILayout.ExpandWidth(true)))
            {
                RunValidation();
            }
        }

        private CCS_EquipmentSocketAnchor GetSelectedSocketAnchor()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            return socketTransform != null ? socketTransform.GetComponent<CCS_EquipmentSocketAnchor>() : null;
        }

        private bool IsSelectedSocketCompatible()
        {
            return CCS_EquipmentFitStudioSocketCompatibilityUtility.CanPreviewRevolverOnSocket(
                GetSelectedSocketAnchor(),
                state.SelectedSocketId,
                state.AllowIncompatibleSocketOverride,
                out _);
        }

        private bool CanSpawnPreview(out string blockingReason)
        {
            blockingReason = string.Empty;
            if (!CanUseEditFitTuning())
            {
                blockingReason =
                    "Spawn preview in Edit Fit Preview mode. Use Play Mode Runtime Test to verify saved profiles.";
                return false;
            }

            if (state.PlayerRoot == null)
            {
                blockingReason = "Preview failed: no player selected. Create an editor preview player first.";
                return false;
            }

            if (!CCS_EquipmentFitStudioSocketCompatibilityUtility.CanPreviewRevolverOnSocket(
                    GetSelectedSocketAnchor(),
                    state.SelectedSocketId,
                    state.AllowIncompatibleSocketOverride,
                    out blockingReason))
            {
                return false;
            }

            if (GetSelectedSocketTransform() == null)
            {
                blockingReason = "Preview failed: selected socket transform not found on player.";
                return false;
            }

            GameObject source = settings != null ? settings.DefaultPreviewWeaponPrefab : null;
            if (CCS_EquipmentFitStudioVisualSourceUtility.ResolveRevolverPreviewVisualSource(source) == null)
            {
                blockingReason = "Preview failed: ModelRoot/RevolverVisual not found on source prefab.";
                return false;
            }

            return true;
        }
    }
}
