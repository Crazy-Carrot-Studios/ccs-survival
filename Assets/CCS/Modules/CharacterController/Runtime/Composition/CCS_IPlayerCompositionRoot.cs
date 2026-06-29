// =============================================================================
// SCRIPT: CCS_IPlayerCompositionRoot
// CATEGORY: Modules / CharacterController / Runtime / Composition
// PURPOSE: Future composition hub contract for player subsystem references (v0.7.5 planning).
// PLACEMENT: Implemented by a future root facade after hierarchy migration. Not wired in v0.7.5.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Interface-only. External module bridges (Weapons, Interaction UI) added in migration milestones.
// =============================================================================

using Unity.Netcode;
using UnityEngine;

namespace CCS.Modules.CharacterController
{
    /// <summary>
    /// Future read-only composition root for the networked player prefab.
    /// A concrete facade will add validated references to Attributes, Weapons, and Interaction bridges.
    /// </summary>
    public interface CCS_IPlayerCompositionRoot
    {
        UnityEngine.CharacterController CharacterController { get; }

        NetworkObject NetworkObject { get; }

        CCS_CharacterInputActionProvider InputProvider { get; }

        CCS_CharacterMotor Motor { get; }

        CCS_CharacterCameraController CameraController { get; }

        CCS_CharacterControllerService ControllerService { get; }

        CCS_CharacterAimLocomotionController AimLocomotionController { get; }

        CCS_EquipmentSocketRegistry EquipmentSocketRegistry { get; }

        Animator ModelAnimator { get; }

        Transform ModelRoot { get; }

        Transform CameraFollowAnchor { get; }

        Transform MuzzlePoint { get; }

        Transform LocalHudAnchor { get; }

        Transform LocalReticleAnchor { get; }

        Transform InteractionScanOrigin { get; }
    }
}
