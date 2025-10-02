using UnityEngine;

namespace RealisticSpears
{
    // Simple marker to avoid double-rotating the same equipped instance and to store original transform.
    internal sealed class SpearRotatedMarker : MonoBehaviour
    {
        // preserved baseline to allow idempotent apply/revert
        public Vector3 origLocalPos;
        public Quaternion origLocalRot;

        // current state
        public bool rotationApplied;
        public bool initialized;

        // whether an extra 180° throw-flip is currently applied
        public bool throwFlipped;
    }

    internal static class SpearPositioner
    {
        internal static void FixSpearRotationAndPosition(GameObject go, bool isRevert, bool isFangSpear)
        {
            if (!go) return;

            if (!isRevert)
            {
                FixSpearRotation(go); // Remove explicit qualification to avoid ambiguity
                FixSpearPositionOffset(go, isRevert, isFangSpear);
            }
            else
            {
                FixSpearPositionOffset(go, isRevert, isFangSpear);
                FixSpearRotation(go); // Remove explicit qualification to avoid ambiguity
            }
        }

        // kept for compatibility but no longer used for runtime toggling
        internal static void FixSpearRotation(GameObject go)
        {
            go.transform.Rotate(180f, 25f, 0f, Space.Self);
        }

        // Rename the method to avoid ambiguity
        internal static void FixSpearPositionOffset(GameObject go, bool isRevert, bool isFangSpear)
        {
            var direction = isRevert ? Vector3.back : Vector3.forward;
            float offset = isFangSpear ? -0.72f : -0.50f;
            go.transform.Translate(direction * offset, Space.Self);
        }

        // New idempotent applier: uses the stored original local transform on the marker
        // and sets localRotation/localPosition based on whether we want rotation applied.
        // Added 'extraFlip' to apply an additional 180° flip (used for throws).
        internal static void ApplyState(GameObject go, SpearRotatedMarker marker, bool applyRotation, bool isFangSpear, bool extraFlip = false)
        {
            if (go == null || marker == null || !marker.initialized) return;

            // choose offset along the original local forward vector
            float offset = isFangSpear ? -0.72f : -0.50f;

            // compute base rotation to apply (original baseline +/- held rotation)
            Quaternion baseRot;
            if (applyRotation)
            {
                baseRot = marker.origLocalRot * Quaternion.Euler(180f, 25f, 0f);
            }
            else
            {
                baseRot = marker.origLocalRot;
            }

            // apply optional extra 180° flip for throwing (idempotent)
            Quaternion finalRot = extraFlip ? (baseRot * Quaternion.Euler(180f, 0f, 0f)) : baseRot;

            go.transform.localRotation = finalRot;
            go.transform.localPosition = marker.origLocalPos + (finalRot * Vector3.forward * offset);

            marker.rotationApplied = applyRotation;
            marker.throwFlipped = extraFlip;
        }
    }
}
