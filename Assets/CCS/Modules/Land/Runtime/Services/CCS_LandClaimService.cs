using System;
using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;
using Object = UnityEngine.Object;

// =============================================================================
// SCRIPT: CCS_LandClaimService
// CATEGORY: Modules / Land / Runtime / Services
// PURPOSE: Owns land claims, deed placement, structure association, and save/restore.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 — no taxes, banks, or multiplayer authority yet.
// =============================================================================

namespace CCS.Modules.Land
{
    public sealed class CCS_LandClaimService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_LandClaimService]";

        public readonly struct StructureRegistration
        {
            public StructureRegistration(string structureInstanceId, Vector3 worldPosition, string ownerId, string structureKind)
            {
                StructureInstanceId = structureInstanceId ?? string.Empty;
                WorldPosition = worldPosition;
                OwnerId = ownerId ?? string.Empty;
                StructureKind = structureKind ?? string.Empty;
            }

            public string StructureInstanceId { get; }

            public Vector3 WorldPosition { get; }

            public string OwnerId { get; }

            public string StructureKind { get; }
        }

        private readonly Dictionary<string, CCS_LandClaimInstance> claimsById =
            new Dictionary<string, CCS_LandClaimInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_LandClaimDefinition> claimDefinitionLookup =
            new Dictionary<string, CCS_LandClaimDefinition>(StringComparer.OrdinalIgnoreCase);

        private CCS_LandClaimProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private Func<Vector3, string> resolveRegionAtPosition;
        private Func<List<StructureRegistration>> structureScanProvider;
        private GameObject placementPreviewRoot;
        private GameObject placementPreviewRing;
        private GameObject placementPreviewLabel;
        private CCS_LandClaimDefinition pendingClaimDefinition;
        private Vector3 pendingPreviewPosition;
        private float pendingPreviewRadius;
        private bool pendingPreviewValid;
        private bool isPlacementModeActive;
        private bool isInitialized;

        public event Action<CCS_LandClaimInstance> LandClaimPlaced;
        public event Action<CCS_LandClaimInstance, string> StructureAssociated;

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => isPlacementModeActive;

        public CCS_LandClaimProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_LandClaimProfile profile)
        {
            activeProfile = profile;
            claimDefinitionLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_LandClaimValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_LandClaimDefinition[] definitions = profile.ClaimDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_LandClaimDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ClaimDefinitionId))
                {
                    continue;
                }

                claimDefinitionLookup[definition.ClaimDefinitionId] = definition;
            }

            isInitialized = validation.IsSuccess || claimDefinitionLookup.Count > 0;
        }

        public void BindInventoryService(CCS_PlayerInventoryService inventory)
        {
            inventoryService = inventory;
        }

        public void BindRegionResolver(Func<Vector3, string> resolver)
        {
            resolveRegionAtPosition = resolver;
        }

        public void BindStructureScanProvider(Func<List<StructureRegistration>> provider)
        {
            structureScanProvider = provider;
        }

        public bool TryResolveClaimDefinitionForDeedItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_LandClaimDefinition claimDefinition)
        {
            claimDefinition = null;
            if (!EnsureReady() || itemDefinition == null || activeProfile == null)
            {
                return false;
            }

            return activeProfile.TryGetClaimByDeedItemId(itemDefinition.ItemId, out claimDefinition);
        }

        public CCS_LandClaimPlacementResult HandlePlacementRequest(CCS_LandClaimPlacementRequest request)
        {
            if (!EnsureReady() || request == null)
            {
                return CCS_LandClaimPlacementResult.Failure("Land claim service is unavailable.");
            }

            if (request.ConfirmPlacement)
            {
                return TryConfirmPlacement();
            }

            return UpdatePlacementPreview(request);
        }

        public void CancelPlacementMode()
        {
            isPlacementModeActive = false;
            pendingClaimDefinition = null;
            pendingPreviewValid = false;
            EnsurePlacementPreviewHidden();
        }

        public bool TryAssociateStructure(
            string structureInstanceId,
            Vector3 worldPosition,
            string ownerId,
            string structureKind)
        {
            if (string.IsNullOrWhiteSpace(structureInstanceId)
                || string.IsNullOrWhiteSpace(structureKind))
            {
                return false;
            }

            CCS_LandClaimInstance claim = FindClaimContainingPosition(worldPosition, ownerId, structureKind);
            if (claim == null)
            {
                return false;
            }

            if (!claim.TryAddAssociatedStructure(structureInstanceId))
            {
                return false;
            }

            StructureAssociated?.Invoke(claim, structureInstanceId);
            return true;
        }

        public bool TryGetClaimContainingPosition(
            Vector3 position,
            out CCS_LandClaimInstance claim)
        {
            claim = FindClaimContainingPosition(position, null, null);
            return claim != null;
        }

        public string TryResolveClaimIdContainingPosition(Vector3 position)
        {
            return FindClaimContainingPosition(position, null, null)?.InstanceId ?? string.Empty;
        }

        public bool HasAssociatedStructure(string structureInstanceId)
        {
            if (string.IsNullOrWhiteSpace(structureInstanceId))
            {
                return false;
            }

            foreach (KeyValuePair<string, CCS_LandClaimInstance> entry in claimsById)
            {
                if (entry.Value != null && entry.Value.HasAssociatedStructure(structureInstanceId))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetClaimCount()
        {
            return claimsById.Count;
        }

        public int GetAssociatedStructureCount(string claimInstanceId)
        {
            if (string.IsNullOrWhiteSpace(claimInstanceId)
                || !claimsById.TryGetValue(claimInstanceId, out CCS_LandClaimInstance claim)
                || claim == null)
            {
                return 0;
            }

            return claim.AssociatedStructureIds.Count;
        }

        public void RescanStructureAssociations()
        {
            if (structureScanProvider == null)
            {
                return;
            }

            List<StructureRegistration> registrations = structureScanProvider.Invoke();
            if (registrations == null || registrations.Count == 0)
            {
                return;
            }

            for (int index = 0; index < registrations.Count; index++)
            {
                StructureRegistration registration = registrations[index];
                TryAssociateStructure(
                    registration.StructureInstanceId,
                    registration.WorldPosition,
                    registration.OwnerId,
                    registration.StructureKind);
            }
        }

        public CCS_LandClaimSnapshot[] CaptureClaimState()
        {
            if (claimsById.Count == 0)
            {
                return Array.Empty<CCS_LandClaimSnapshot>();
            }

            CCS_LandClaimSnapshot[] snapshots = new CCS_LandClaimSnapshot[claimsById.Count];
            int index = 0;
            foreach (KeyValuePair<string, CCS_LandClaimInstance> entry in claimsById)
            {
                snapshots[index++] = entry.Value.CaptureSnapshot();
            }

            return snapshots;
        }

        public void RestoreState(CCS_LandClaimSnapshot[] claimSnapshots)
        {
            ClearAllClaims();
            if (claimSnapshots == null || claimSnapshots.Length == 0 || activeProfile == null)
            {
                return;
            }

            for (int index = 0; index < claimSnapshots.Length; index++)
            {
                CCS_LandClaimSnapshot snapshot = claimSnapshots[index];
                if (snapshot == null
                    || string.IsNullOrWhiteSpace(snapshot.instanceId)
                    || !activeProfile.TryGetClaimById(snapshot.claimDefinitionId, out CCS_LandClaimDefinition definition))
                {
                    continue;
                }

                Vector3 position = new Vector3(snapshot.positionX, snapshot.positionY, snapshot.positionZ);
                CCS_LandClaimState state = Enum.IsDefined(typeof(CCS_LandClaimState), snapshot.claimState)
                    ? (CCS_LandClaimState)snapshot.claimState
                    : CCS_LandClaimState.Claimed;
                CCS_LandClaimInstance claim = new CCS_LandClaimInstance(
                    snapshot.instanceId,
                    definition,
                    position,
                    snapshot.claimRadius > 0f ? snapshot.claimRadius : definition.ClaimRadius,
                    snapshot.ownerId,
                    snapshot.regionId,
                    state);
                claim.ApplySnapshotAssociations(snapshot.associatedStructureIds);
                SpawnClaimWorldObject(claim);
                claimsById[claim.InstanceId] = claim;
            }

            RescanStructureAssociations();
        }

        public bool TryGetClaim(string instanceId, out CCS_LandClaimInstance claim)
        {
            claim = null;
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return false;
            }

            return claimsById.TryGetValue(instanceId, out claim) && claim != null;
        }

        private CCS_LandClaimPlacementResult UpdatePlacementPreview(CCS_LandClaimPlacementRequest request)
        {
            if (!activeProfile.TryGetClaimById(request.ClaimDefinitionId, out CCS_LandClaimDefinition definition))
            {
                return CCS_LandClaimPlacementResult.Failure("Unknown land claim definition.");
            }

            pendingClaimDefinition = definition;
            isPlacementModeActive = true;

            if (!TryResolvePlacementPose(
                    definition,
                    request.UseOrigin,
                    request.UseDirection,
                    out Vector3 position,
                    out bool isValid,
                    out string validationMessage))
            {
                return CCS_LandClaimPlacementResult.Preview(false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRadius = definition.ClaimRadius;
            pendingPreviewValid = isValid;
            EnsurePlacementPreview(definition, position, definition.ClaimRadius, isValid);
            return CCS_LandClaimPlacementResult.Preview(isValid, validationMessage);
        }

        private CCS_LandClaimPlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || !pendingPreviewValid || pendingClaimDefinition == null)
            {
                return CCS_LandClaimPlacementResult.Failure("Land claim placement mode is not active.");
            }

            if (inventoryService == null
                || pendingClaimDefinition.ClaimDeedItem == null
                || inventoryService.RemoveItem(pendingClaimDefinition.ClaimDeedItem, 1) <= 0)
            {
                return CCS_LandClaimPlacementResult.Failure("Homestead claim deed is not available in inventory.");
            }

            string regionId = resolveRegionAtPosition != null
                ? resolveRegionAtPosition.Invoke(pendingPreviewPosition) ?? string.Empty
                : string.Empty;

            CCS_LandClaimInstance instance = SpawnClaimInstance(
                pendingClaimDefinition,
                pendingPreviewPosition,
                pendingPreviewRadius,
                GenerateInstanceId(pendingClaimDefinition.ClaimDefinitionId),
                CCS_LandContentIds.PlayerOwnerId,
                regionId,
                CCS_LandClaimState.Claimed);

            CancelPlacementMode();
            LandClaimPlaced?.Invoke(instance);
            return CCS_LandClaimPlacementResult.Placed($"{pendingClaimDefinition.DisplayName} placed.");
        }

        private CCS_LandClaimInstance SpawnClaimInstance(
            CCS_LandClaimDefinition definition,
            Vector3 position,
            float radius,
            string instanceId,
            string ownerId,
            string regionId,
            CCS_LandClaimState claimState)
        {
            CCS_LandClaimInstance instance = new CCS_LandClaimInstance(
                instanceId,
                definition,
                position,
                radius,
                ownerId,
                regionId,
                claimState);
            SpawnClaimWorldObject(instance);
            claimsById[instanceId] = instance;
            return instance;
        }

        private void SpawnClaimWorldObject(CCS_LandClaimInstance claim)
        {
            if (claim?.Definition == null)
            {
                return;
            }

            if (claim.WorldObject != null)
            {
                Object.Destroy(claim.WorldObject);
            }

            GameObject root = new GameObject($"CCS_LandClaim_{claim.InstanceId}");
            root.transform.position = claim.WorldPosition;

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ClaimRadius";
            ring.transform.SetParent(root.transform, false);
            float diameter = claim.ClaimRadius * 2f;
            ring.transform.localScale = new Vector3(diameter, 0.05f, diameter);
            Collider ringCollider = ring.GetComponent<Collider>();
            if (ringCollider != null)
            {
                Object.Destroy(ringCollider);
            }

            MeshRenderer ringRenderer = ring.GetComponent<MeshRenderer>();
            if (ringRenderer != null)
            {
                ringRenderer.sharedMaterial.color = claim.Definition.PlacedColor;
            }

            GameObject labelObject = new GameObject("ClaimLabel");
            labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = claim.Definition.DisplayName;
            label.characterSize = 0.12f;
            label.fontSize = 48;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(0.95f, 0.95f, 0.85f, 0.95f);

            claim.WorldObject = root;
        }

        private bool TryResolvePlacementPose(
            CCS_LandClaimDefinition definition,
            Vector3 useOrigin,
            Vector3 useDirection,
            out Vector3 position,
            out bool isValid,
            out string validationMessage)
        {
            position = useOrigin + useDirection * definition.PlacementForwardDistance;
            isValid = false;
            validationMessage = "Invalid placement.";

            if (!Physics.Raycast(
                    useOrigin,
                    Vector3.down,
                    out RaycastHit _,
                    definition.PlacementMaxGroundRayDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)
                || !Physics.Raycast(
                    position + Vector3.up * 2f,
                    Vector3.down,
                    out RaycastHit targetHit,
                    definition.PlacementMaxGroundRayDistance + 2f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                validationMessage = "No ground found for land claim placement.";
                return false;
            }

            position = targetHit.point;
            float slopeAngle = Vector3.Angle(targetHit.normal, Vector3.up);
            if (slopeAngle > definition.PlacementMaxSlopeAngle)
            {
                validationMessage = $"Ground too steep ({slopeAngle:0.#}°).";
                return true;
            }

            if (!ValidateRegionRequirement(definition, position, out validationMessage))
            {
                return true;
            }

            if (!ValidateClaimSeparation(definition, position, out validationMessage))
            {
                return true;
            }

            isValid = true;
            validationMessage = "Land claim placement valid.";
            return true;
        }

        private bool ValidateRegionRequirement(
            CCS_LandClaimDefinition definition,
            Vector3 position,
            out string validationMessage)
        {
            validationMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(definition.OptionalRegionId))
            {
                return true;
            }

            string regionId = resolveRegionAtPosition != null
                ? resolveRegionAtPosition.Invoke(position) ?? string.Empty
                : string.Empty;

            if (string.Equals(regionId, definition.OptionalRegionId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            validationMessage = $"Claim requires region '{definition.OptionalRegionId}'.";
            return false;
        }

        private bool ValidateClaimSeparation(
            CCS_LandClaimDefinition definition,
            Vector3 position,
            out string validationMessage)
        {
            validationMessage = string.Empty;
            float minimumSeparation = definition.MinimumClaimSeparation;
            foreach (KeyValuePair<string, CCS_LandClaimInstance> entry in claimsById)
            {
                CCS_LandClaimInstance existing = entry.Value;
                if (existing == null || existing.ClaimState != CCS_LandClaimState.Claimed)
                {
                    continue;
                }

                float distance = Vector3.Distance(existing.WorldPosition, position);
                float required = existing.ClaimRadius + definition.ClaimRadius + minimumSeparation;
                if (distance < required)
                {
                    validationMessage = "Too close to an existing land claim.";
                    return false;
                }
            }

            return true;
        }

        private CCS_LandClaimInstance FindClaimContainingPosition(
            Vector3 position,
            string ownerId,
            string structureKind)
        {
            CCS_LandClaimInstance bestClaim = null;
            float bestDistance = float.MaxValue;

            foreach (KeyValuePair<string, CCS_LandClaimInstance> entry in claimsById)
            {
                CCS_LandClaimInstance claim = entry.Value;
                if (claim == null || claim.ClaimState != CCS_LandClaimState.Claimed)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(ownerId)
                    && !string.Equals(claim.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(structureKind)
                    && claim.Definition != null
                    && !claim.Definition.AllowsStructureKind(structureKind))
                {
                    continue;
                }

                if (!claim.ContainsPosition(position))
                {
                    continue;
                }

                float distance = Vector3.Distance(claim.WorldPosition, position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestClaim = claim;
                }
            }

            return bestClaim;
        }

        private void EnsurePlacementPreview(
            CCS_LandClaimDefinition definition,
            Vector3 position,
            float radius,
            bool isValid)
        {
            if (placementPreviewRoot == null)
            {
                placementPreviewRoot = new GameObject("CCS_LandClaimPlacementPreview");
            }

            if (placementPreviewRing == null)
            {
                placementPreviewRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                placementPreviewRing.name = "PreviewRing";
                placementPreviewRing.transform.SetParent(placementPreviewRoot.transform, false);
                Collider collider = placementPreviewRing.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.Destroy(collider);
                }
            }

            if (placementPreviewLabel == null)
            {
                placementPreviewLabel = new GameObject("PreviewLabel");
                placementPreviewLabel.transform.SetParent(placementPreviewRoot.transform, false);
                TextMesh label = placementPreviewLabel.AddComponent<TextMesh>();
                label.characterSize = 0.1f;
                label.fontSize = 48;
                label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center;
            }

            placementPreviewRoot.transform.position = position;
            float diameter = radius * 2f;
            placementPreviewRing.transform.localScale = new Vector3(diameter, 0.05f, diameter);

            MeshRenderer renderer = placementPreviewRing.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color baseColor = definition.PreviewColor;
                renderer.sharedMaterial.color = isValid
                    ? baseColor
                    : new Color(0.9f, 0.25f, 0.25f, 0.45f);
            }

            TextMesh previewLabel = placementPreviewLabel.GetComponent<TextMesh>();
            if (previewLabel != null)
            {
                previewLabel.text = $"{definition.DisplayName} ({radius:0.#}m)";
                previewLabel.color = isValid
                    ? new Color(0.9f, 0.95f, 1f, 0.95f)
                    : new Color(1f, 0.7f, 0.7f, 0.95f);
            }

            placementPreviewLabel.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            placementPreviewRoot.SetActive(true);
        }

        private void EnsurePlacementPreviewHidden()
        {
            if (placementPreviewRoot != null)
            {
                placementPreviewRoot.SetActive(false);
            }
        }

        private void ClearAllClaims()
        {
            foreach (KeyValuePair<string, CCS_LandClaimInstance> entry in claimsById)
            {
                if (entry.Value?.WorldObject != null)
                {
                    Object.Destroy(entry.Value.WorldObject);
                }
            }

            claimsById.Clear();
            EnsurePlacementPreviewHidden();
        }

        private static string GenerateInstanceId(string definitionId)
        {
            return $"{definitionId}.{Guid.NewGuid():N}";
        }

        private bool EnsureReady()
        {
            return isInitialized && activeProfile != null;
        }
    }
}
