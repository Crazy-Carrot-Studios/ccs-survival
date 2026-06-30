using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponAttachmentFitProfileApplicator
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Fitting
// PURPOSE: Applies saved fit profile values to runtime attachment roots under socket definitions.
// PLACEMENT: Shared by runtime visual controller and editor readback/test helpers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Profile stores absolute socket locals from Fit Studio capture. Socket stays at definition;
//        attachment root receives the relative offset so the visual matches tuned placement.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_WeaponAttachmentFitProfileApplicator
    {
        #region Public Methods

        public static void ApplyProfileToAttachmentRoot(
            Transform socketTransform,
            Transform attachmentRoot,
            CCS_WeaponAttachmentFitProfile profile,
            Vector3 socketDefinitionLocalPosition,
            Vector3 socketDefinitionLocalEuler,
            Vector3 socketDefinitionLocalScale)
        {
            if (socketTransform == null || attachmentRoot == null || profile == null)
            {
                return;
            }

            socketTransform.localPosition = socketDefinitionLocalPosition;
            socketTransform.localRotation = Quaternion.Euler(socketDefinitionLocalEuler);
            socketTransform.localScale = socketDefinitionLocalScale;

            ComputeAttachmentRootLocal(
                socketDefinitionLocalPosition,
                socketDefinitionLocalEuler,
                socketDefinitionLocalScale,
                profile.SocketLocalPosition,
                profile.SocketLocalEulerAngles,
                profile.SocketLocalScale,
                out Vector3 attachmentLocalPosition,
                out Vector3 attachmentLocalEuler,
                out Vector3 attachmentLocalScale);

            attachmentRoot.localPosition = attachmentLocalPosition;
            attachmentRoot.localRotation = Quaternion.Euler(attachmentLocalEuler);
            attachmentRoot.localScale = attachmentLocalScale;
        }

        public static void ComputeAttachmentRootLocal(
            Vector3 socketDefinitionLocalPosition,
            Vector3 socketDefinitionLocalEuler,
            Vector3 socketDefinitionLocalScale,
            Vector3 profileSocketLocalPosition,
            Vector3 profileSocketLocalEuler,
            Vector3 profileSocketLocalScale,
            out Vector3 attachmentLocalPosition,
            out Vector3 attachmentLocalEuler,
            out Vector3 attachmentLocalScale)
        {
            Matrix4x4 definitionMatrix = Matrix4x4.TRS(
                socketDefinitionLocalPosition,
                Quaternion.Euler(socketDefinitionLocalEuler),
                socketDefinitionLocalScale);
            Matrix4x4 profileMatrix = Matrix4x4.TRS(
                profileSocketLocalPosition,
                Quaternion.Euler(profileSocketLocalEuler),
                profileSocketLocalScale);
            Matrix4x4 relativeMatrix = definitionMatrix.inverse * profileMatrix;

            attachmentLocalPosition = relativeMatrix.GetColumn(3);
            attachmentLocalEuler = relativeMatrix.rotation.eulerAngles;
            attachmentLocalScale = ExtractScale(relativeMatrix);
        }

        public static void ComputeProfileSocketLocalFromAttachmentRoot(
            Vector3 socketDefinitionLocalPosition,
            Vector3 socketDefinitionLocalEuler,
            Vector3 socketDefinitionLocalScale,
            Vector3 attachmentLocalPosition,
            Vector3 attachmentLocalEuler,
            Vector3 attachmentLocalScale,
            out Vector3 profileSocketLocalPosition,
            out Vector3 profileSocketLocalEuler,
            out Vector3 profileSocketLocalScale)
        {
            Matrix4x4 definitionMatrix = Matrix4x4.TRS(
                socketDefinitionLocalPosition,
                Quaternion.Euler(socketDefinitionLocalEuler),
                socketDefinitionLocalScale);
            Matrix4x4 attachmentMatrix = Matrix4x4.TRS(
                attachmentLocalPosition,
                Quaternion.Euler(attachmentLocalEuler),
                attachmentLocalScale);
            Matrix4x4 profileMatrix = definitionMatrix * attachmentMatrix;

            profileSocketLocalPosition = profileMatrix.GetColumn(3);
            profileSocketLocalEuler = profileMatrix.rotation.eulerAngles;
            profileSocketLocalScale = ExtractScale(profileMatrix);
        }

        public static bool AttachmentRootMatchesProfile(
            Transform attachmentRoot,
            CCS_WeaponAttachmentFitProfile profile,
            Vector3 socketDefinitionLocalPosition,
            Vector3 socketDefinitionLocalEuler,
            Vector3 socketDefinitionLocalScale)
        {
            if (attachmentRoot == null || profile == null)
            {
                return false;
            }

            ComputeAttachmentRootLocal(
                socketDefinitionLocalPosition,
                socketDefinitionLocalEuler,
                socketDefinitionLocalScale,
                profile.SocketLocalPosition,
                profile.SocketLocalEulerAngles,
                profile.SocketLocalScale,
                out Vector3 expectedPosition,
                out Vector3 expectedEuler,
                out Vector3 expectedScale);

            return VectorsApproximatelyEqual(attachmentRoot.localPosition, expectedPosition)
                && VectorsApproximatelyEqual(attachmentRoot.localEulerAngles, expectedEuler)
                && VectorsApproximatelyEqual(attachmentRoot.localScale, expectedScale);
        }

        public static void ResetAttachmentRoot(Transform attachmentRoot)
        {
            if (attachmentRoot == null)
            {
                return;
            }

            attachmentRoot.localPosition = Vector3.zero;
            attachmentRoot.localRotation = Quaternion.identity;
            attachmentRoot.localScale = Vector3.one;
        }

        public static void ResetDirectVisualChildToIdentity(Transform attachmentRoot)
        {
            if (attachmentRoot == null || attachmentRoot.childCount == 0)
            {
                return;
            }

            for (int i = 0; i < attachmentRoot.childCount; i++)
            {
                Transform child = attachmentRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }
        }

        public static bool HasDuplicateProfileApplicationInSource(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            int applyCount = 0;
            int searchIndex = 0;
            const string token = "ApplyProfileToAttachmentRoot(";
            while (true)
            {
                int foundIndex = source.IndexOf(token, searchIndex, System.StringComparison.Ordinal);
                if (foundIndex < 0)
                {
                    break;
                }

                applyCount++;
                if (applyCount > 1)
                {
                    return true;
                }

                searchIndex = foundIndex + token.Length;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private static Vector3 ExtractScale(Matrix4x4 matrix)
        {
            return new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude);
        }

        private static bool VectorsApproximatelyEqual(Vector3 left, Vector3 right)
        {
            return Vector3.SqrMagnitude(left - right) <= 0.0001f;
        }

        #endregion
    }
}
