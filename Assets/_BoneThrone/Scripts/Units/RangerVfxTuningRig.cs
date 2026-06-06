using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Scene-only tuning helper. Designers move real preview objects, then the editor tool captures their transforms.
    /// </summary>
    [ExecuteAlways]
    public sealed class RangerVfxTuningRig : MonoBehaviour
    {
        [Header("Apply Target")]
        [SerializeField] [Tooltip("Ranger prefab asset that receives the captured arrow and skill transform values.")]
        private GameObject rangerPrefabAsset;

        [Header("Scene References")]
        [SerializeField] [Tooltip("Where the Ranger stands in this tuning scene. Used only to calculate attack direction.")]
        private Transform rangerOrigin;
        [SerializeField] [Tooltip("The real enemy preview body. Move/rotate/scale this as the target reference.")]
        private Transform targetRoot;
        [SerializeField] [Tooltip("Optional body anchor. If empty, the tool uses the target Animator transform, then targetRoot.")]
        private Transform targetAnchorOverride;

        [Header("Arrow")]
        [SerializeField] [Tooltip("Move this handle to the visible arrow launch position.")]
        private Transform arrowStart;
        [SerializeField] [Tooltip("Move this handle to the exact point where the arrow should stick into the enemy.")]
        private Transform arrowImpact;
        [SerializeField] [Tooltip("The real arrow prefab instance. Move/rotate/scale this directly in Scene view.")]
        private Transform arrowPreview;

        [Header("Skill Preview Objects")]
        [SerializeField] private Transform precisionShotPreview;
        [SerializeField] private Transform quickShotPreview;
        [SerializeField] private Transform piercingArrowPreview;

        [Header("Particle Preview")]
        [SerializeField] [Tooltip("Editor preview time used by the custom Inspector to simulate real ParticleSystem objects.")]
        [Range(0f, 3f)]
        private float particlePreviewTime = 0.35f;

        public GameObject RangerPrefabAsset => rangerPrefabAsset;
        public Transform PrecisionShotPreview => precisionShotPreview;
        public Transform QuickShotPreview => quickShotPreview;
        public Transform PiercingArrowPreview => piercingArrowPreview;
        public float ParticlePreviewTime => particlePreviewTime;

        public void Initialize(
            GameObject prefabAsset,
            Transform rangerOriginTransform,
            Transform targetRootTransform,
            Transform targetAnchor,
            Transform arrowStartTransform,
            Transform arrowImpactTransform,
            Transform arrowPreviewTransform,
            Transform precisionPreviewTransform,
            Transform quickPreviewTransform,
            Transform piercingPreviewTransform)
        {
            rangerPrefabAsset = prefabAsset;
            rangerOrigin = rangerOriginTransform;
            targetRoot = targetRootTransform;
            targetAnchorOverride = targetAnchor;
            arrowStart = arrowStartTransform;
            arrowImpact = arrowImpactTransform;
            arrowPreview = arrowPreviewTransform;
            precisionShotPreview = precisionPreviewTransform;
            quickShotPreview = quickPreviewTransform;
            piercingArrowPreview = piercingPreviewTransform;
        }

        public bool TryCapture(out RangerVfxTuningSnapshot snapshot, out string error)
        {
            snapshot = default;
            error = string.Empty;

            Transform targetAnchor = ResolveTargetAnchor();
            if (rangerOrigin == null || targetAnchor == null || arrowStart == null || arrowImpact == null || arrowPreview == null)
            {
                error = "Missing required Ranger tuning references. Recreate the rig from BoneThrone/VFX/Ranger/Create Real Tuning Rig.";
                return false;
            }

            Vector3 direction = arrowImpact.position - rangerOrigin.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = targetAnchor.position - rangerOrigin.position;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                error = "Cannot resolve attack direction. Move the Ranger origin or target away from each other.";
                return false;
            }

            direction.Normalize();
            Quaternion arrowBaseRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion arrowOffsetRotation = Quaternion.Inverse(arrowBaseRotation) * arrowPreview.rotation;

            snapshot.ArrowStartDistanceFromTarget = Mathf.Max(0f, Vector3.Dot(arrowImpact.position - arrowStart.position, direction));
            snapshot.ArrowRotationOffsetEuler = arrowOffsetRotation.eulerAngles;
            snapshot.ArrowWorldScale = ClampScaleVector(arrowPreview.lossyScale);
            snapshot.EmbeddedArrowLocalOffset = targetAnchor.InverseTransformPoint(arrowImpact.position);
            snapshot.PrecisionShot = CaptureAttachment(targetAnchor, precisionShotPreview);
            snapshot.QuickShot = CaptureAttachment(targetAnchor, quickShotPreview);
            snapshot.PiercingArrow = CaptureAttachment(targetAnchor, piercingArrowPreview);
            return true;
        }

        private Transform ResolveTargetAnchor()
        {
            if (targetAnchorOverride != null)
            {
                return targetAnchorOverride;
            }

            if (targetRoot == null)
            {
                return null;
            }

            Animator animator = targetRoot.GetComponentInChildren<Animator>(true);
            return animator != null ? animator.transform : targetRoot;
        }

        private static RangerVfxAttachmentSnapshot CaptureAttachment(Transform targetAnchor, Transform preview)
        {
            if (targetAnchor == null || preview == null)
            {
                return default;
            }

            Quaternion localRotation = Quaternion.Inverse(targetAnchor.rotation) * preview.rotation;
            return new RangerVfxAttachmentSnapshot
            {
                LocalOffset = targetAnchor.InverseTransformPoint(preview.position),
                LocalEulerAngles = localRotation.eulerAngles,
                LocalScale = ClampScaleVector(preview.lossyScale)
            };
        }

        private static Vector3 ClampScaleVector(Vector3 value)
        {
            const float minimumScale = 0.01f;
            return new Vector3(
                Mathf.Max(minimumScale, Mathf.Abs(value.x)),
                Mathf.Max(minimumScale, Mathf.Abs(value.y)),
                Mathf.Max(minimumScale, Mathf.Abs(value.z)));
        }
    }

    public struct RangerVfxTuningSnapshot
    {
        public float ArrowStartDistanceFromTarget;
        public Vector3 ArrowRotationOffsetEuler;
        public Vector3 ArrowWorldScale;
        public Vector3 EmbeddedArrowLocalOffset;
        public RangerVfxAttachmentSnapshot PrecisionShot;
        public RangerVfxAttachmentSnapshot QuickShot;
        public RangerVfxAttachmentSnapshot PiercingArrow;
    }

    public struct RangerVfxAttachmentSnapshot
    {
        public Vector3 LocalOffset;
        public Vector3 LocalEulerAngles;
        public Vector3 LocalScale;
    }
}
