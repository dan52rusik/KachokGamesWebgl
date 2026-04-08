using UnityEngine;

namespace Tutorial
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ConfigurableJoint))]
    public class PunchingBag : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField] private float anchorHeight = 1.2f;
        [SerializeField] private float colliderRadius = 0.36f;
        [SerializeField] private float colliderHeight = 2.35f;
        [SerializeField] private Transform anchorTransform;

        [Header("Impact")]
        [SerializeField] private float hitForceMultiplier = 30f;
        [SerializeField] private float maxVelocityChange = 16f;
        [SerializeField] private float angularImpulse = 5f;

        [Header("Swing Limits")]
        [SerializeField] private float swingLimit = 28f;
        [SerializeField] private float twistLimit = 8f;

        [Header("Auto Setup")]
        [SerializeField] private bool autoConfigureJointOnAwake = true;

        [Header("Visuals")]
        [SerializeField] private Transform ropeVisual;
        [SerializeField] private Transform anchorVisual;

        private Rigidbody _rb;
        private ConfigurableJoint _joint;
        private CapsuleCollider _capsule;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _joint = GetComponent<ConfigurableJoint>();
            _capsule = GetComponent<CapsuleCollider>();

            if (autoConfigureJointOnAwake)
                ConfigureJoint();
        }

        private void OnValidate()
        {
            if (_joint == null)
                _joint = GetComponent<ConfigurableJoint>();

            if (_joint != null && anchorTransform == null)
            {
                _joint.autoConfigureConnectedAnchor = false;
                _joint.connectedAnchor = transform.position + Vector3.up * anchorHeight;
            }
        }

        private void LateUpdate()
        {
            UpdateRopeVisual();
        }

        public void ApplyHit(Vector3 hitPoint, Vector3 hitDirection, float forceScale)
        {
            if (_rb == null)
                return;

            Vector3 direction = hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : transform.forward;
            float force = Mathf.Clamp(hitForceMultiplier * Mathf.Max(0.1f, forceScale), 0f, hitForceMultiplier * maxVelocityChange);

            _rb.WakeUp();
            _rb.AddForceAtPosition(direction * force, hitPoint, ForceMode.Impulse);

            Vector3 torqueAxis = Vector3.Cross(Vector3.up, direction).normalized;
            if (torqueAxis.sqrMagnitude > 0.0001f)
                _rb.AddTorque(torqueAxis * angularImpulse * Mathf.Max(0.5f, forceScale), ForceMode.Impulse);
        }

        [ContextMenu("Configure Joint")]
        public void ConfigureJoint()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_joint == null)
                _joint = GetComponent<ConfigurableJoint>();
            if (_capsule == null)
                _capsule = GetComponent<CapsuleCollider>();

            if (_capsule == null)
                _capsule = gameObject.AddComponent<CapsuleCollider>();

            _rb.mass = 22f;
            _rb.linearDamping = 0.2f;
            _rb.angularDamping = 0.35f;
            _rb.useGravity = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionX;
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;

            _capsule.isTrigger = false;
            _capsule.direction = 1;
            _capsule.center = Vector3.zero;
            _capsule.radius = colliderRadius;
            _capsule.height = colliderHeight;

            _joint.autoConfigureConnectedAnchor = false;
            if (anchorTransform != null)
                _joint.connectedAnchor = anchorTransform.position;
            else
                _joint.connectedAnchor = transform.position + Vector3.up * anchorHeight;
            _joint.anchor = new Vector3(0f, anchorHeight, 0f);
            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;
            _joint.angularXMotion = ConfigurableJointMotion.Limited;
            _joint.angularYMotion = ConfigurableJointMotion.Limited;
            _joint.angularZMotion = ConfigurableJointMotion.Limited;
            _joint.enablePreprocessing = false;
            _joint.projectionMode = JointProjectionMode.PositionAndRotation;
            _joint.projectionDistance = 0.03f;
            _joint.projectionAngle = 3f;
            _joint.massScale = 1f;
            _joint.connectedMassScale = 1000f;

            SoftJointLimit lowX = _joint.lowAngularXLimit;
            lowX.limit = -swingLimit;
            _joint.lowAngularXLimit = lowX;

            SoftJointLimit highX = _joint.highAngularXLimit;
            highX.limit = swingLimit;
            _joint.highAngularXLimit = highX;

            SoftJointLimit yLimit = _joint.angularYLimit;
            yLimit.limit = swingLimit;
            _joint.angularYLimit = yLimit;

            SoftJointLimit zLimit = _joint.angularZLimit;
            zLimit.limit = twistLimit;
            _joint.angularZLimit = zLimit;
        }

        private void UpdateRopeVisual()
        {
            if (ropeVisual == null)
                return;

            Vector3 anchorPoint = _joint != null ? _joint.connectedAnchor : transform.position + Vector3.up * anchorHeight;
            if (anchorTransform != null)
                anchorPoint = anchorTransform.position;
            Vector3 bagTop = transform.TransformPoint(new Vector3(0f, anchorHeight, 0f));
            Vector3 delta = bagTop - anchorPoint;
            float length = delta.magnitude;
            if (length <= 0.001f)
                return;

            ropeVisual.position = anchorPoint + delta * 0.5f;
            ropeVisual.up = delta.normalized;
            Vector3 scale = ropeVisual.localScale;
            ropeVisual.localScale = new Vector3(scale.x, length * 0.5f, scale.z);

            if (anchorVisual != null)
                anchorVisual.position = anchorPoint;
        }
    }
}
