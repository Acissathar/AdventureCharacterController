using UnityEngine;

namespace AdventureCharacterController.Runtime.Core
{
    /// <summary>
    ///     This component handles all physics, collision detection, and ground detection. It expects a movement velocity every
    ///     FixedUpdate frame from an external script (like a BaseController component) to work. This calculated information is
    ///     exposed via Properties (IsGrounded, GroundNormal, etc.)
    ///     Object must have a Collider (Box, Sphere, or Capsule - checked/used in that order), or else a default Capsule
    ///     Collider will be put on the object.
    ///     Similarly, if a RigidBody is not found, a default one will be added.
    /// </summary>
    public class Mover : MonoBehaviour
    {
        #region Editor Settings

        // Mover / Collider settings
        [SerializeField] private float stepHeightRatio = 0.25f;
        [SerializeField] private float colliderHeight = 2.0f;
        [SerializeField] private float colliderThickness = 1.0f;
        [SerializeField] private Vector3 colliderOffset = Vector3.zero;

        // Sensor settings
        [SerializeField] private float sensorRadiusModifier = 0.8f;
        [SerializeField] public CastType sensorType = CastType.Raycast;
        [SerializeField] private float safetyDistanceFactor = 0.001f;
        [SerializeField] private int sensorArrayRows = 1;
        [SerializeField] private int sensorArrayRayCount = 6;
        [SerializeField] private bool sensorArrayRowsAreOffset;
        [SerializeField] private bool isSensorInDebug;

        #endregion

        #region Properties

        /// <summary>
        ///     Center of the mover's collider bounding box.
        /// </summary>
        private Vector3 ColliderCenter
        {
            get
            {
                if (!_myCollider)
                {
                    Setup();
                }

                return _myCollider.bounds.center;
            }
        }

        /// <summary>
        ///     Change the height of the mover's collider.
        ///     This will force a recalculation of collider dimensions.
        /// </summary>
        public float ColliderHeight
        {
            get => colliderHeight;
            set
            {
                if (Mathf.Approximately(colliderHeight, value))
                {
                    return;
                }

                colliderHeight = value;
                RecalculateColliderDimensions();
            }
        }

        /// <summary>
        ///     Change the thickness of the mover's collider.
        ///     This will force a recalculation of collider dimensions.
        /// </summary>
        public float ColliderThickness
        {
            get => colliderThickness;
            set
            {
                if (Mathf.Approximately(colliderThickness, value))
                {
                    return;
                }

                if (value < 0f)
                {
                    value = 0f;
                }

                colliderThickness = value;
                RecalculateColliderDimensions();
            }
        }

        /// <summary>
        ///     The most recent collider hit by the Sensor's casts.
        /// </summary>
        public Collider GroundCollider => _mySensor.HitCollider;

        /// <summary>
        ///     The surface normal of the HitCollider from the Sensor's casts.
        /// </summary>
        public Vector3 GroundNormal => _mySensor.HitNormal;

        /// <summary>
        ///     The world position of the HitCollider from the Sensor's casts.
        /// </summary>
        public Vector3 GroundPoint => _mySensor.HitPosition;

        /// <summary>
        ///     If the Mover is considered Grounded or not.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        ///     Change the step height of the mover's collider for step/stair movement.
        ///     This will force a recalculation of collider dimensions.
        /// </summary>
        public float StepHeightRatio
        {
            get => stepHeightRatio;
            set
            {
                stepHeightRatio = Mathf.Clamp(value, 0.0f, 1.0f);
                RecalculateColliderDimensions();
            }
        }

        /// <summary>
        ///     If the sensor should use the extended range. Most commonly set to true when grounded or sliding to keep ground
        ///     contacts.
        /// </summary>
        public bool UseExtendedSensorRange
        {
            set => _useExtendedSensorRange = value;
        }

        /// <summary>
        ///     LinearVelocity of the attached Rigidbody. When setting, ground adjustment velocity will be included.
        /// </summary>
        public Vector3 Velocity
        {
#if UNITY_6000_0_OR_NEWER
            get => _myRigidbody.linearVelocity;
            set => _myRigidbody.linearVelocity = value + _currentGroundAdjustmentVelocity;
#else
            get => myRigidbody.velocity;
            set => myRigidbody.velocity = value + currentGroundAdjustmentVelocity;
#endif
        }

        #endregion

        #region Private Fields

        // References to attached collider(s)
        private BoxCollider _boxCollider;
        private SphereCollider _sphereCollider;
        private CapsuleCollider _capsuleCollider;

        // References to attached components
        private Collider _myCollider;
        private Rigidbody _myRigidbody;
        private Transform _myTransform;
        private Sensor _mySensor;

        // Sensor range variables
        private bool _useExtendedSensorRange = true;
        private float _baseSensorRange;
        private int _currentLayer;

        // Current upwards (or downwards) velocity necessary to keep the correct distance to the ground;
        private Vector3 _currentGroundAdjustmentVelocity = Vector3.zero;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Unity calls Awake when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            Setup();
        }

        /// <summary>
        ///     Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component
        ///     the first time. This function is only called in editor mode.
        /// </summary>
        private void Reset()
        {
            Setup();
        }

        /// <summary>
        ///     Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                RecalculateColliderDimensions();
            }
        }

        /// <summary>
        ///     LateUpdate is called every frame if the Behavior is enabled after all other Update functions.
        /// </summary>
        private void LateUpdate()
        {
            if (isSensorInDebug)
            {
                _mySensor.DrawDebug();
            }
        }

        #endregion

        #region Setup and Recalibration

        /// <summary>
        ///     Initial setup of the mover. Gathers all necessary Components, generates the sensor, and then recalculates collider
        ///     dimensions.
        /// </summary>
        private void Setup()
        {
            _myTransform = transform;

            if (!TryGetComponent(out _myCollider))
            {
                _myCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            if (!TryGetComponent(out _myRigidbody))
            {
                _myRigidbody = gameObject.AddComponent<Rigidbody>();
            }

            _myRigidbody.freezeRotation = true;
            _myRigidbody.useGravity = false;

            TryGetComponent(out _boxCollider);
            TryGetComponent(out _sphereCollider);
            TryGetComponent(out _capsuleCollider);

            _mySensor = new Sensor(_myTransform, _myCollider);
            RecalculateColliderDimensions();
        }

        /// <summary>
        ///     Recalculate's collider dimensions. Should be called whenever any aspect of the collider changes to ensure values
        ///     are correct. This will also recalibrate the Sensor based on the updated collider values.
        /// </summary>
        private void RecalculateColliderDimensions()
        {
            if (_myCollider == null)
            {
                Setup();

                if (_myCollider == null)
                {
                    InternalDebug.LogWarning("There is no collider attached to " + gameObject.name + "!");
                    return;
                }
            }

            if (_boxCollider)
            {
                var size = Vector3.zero;
                size.x = colliderThickness;
                size.z = colliderThickness;

                _boxCollider.center = colliderOffset * colliderHeight;

                size.y = colliderHeight * (1.0f - stepHeightRatio);
                _boxCollider.size = size;

                _boxCollider.center += new Vector3(0.0f, stepHeightRatio * colliderHeight / 2.0f, 0.0f);
            }
            else if (_sphereCollider)
            {
                _sphereCollider.radius = colliderHeight / 2.0f;
                _sphereCollider.center = colliderOffset * colliderHeight;

                _sphereCollider.center += new Vector3(0.0f, stepHeightRatio * _sphereCollider.radius, 0.0f);
                _sphereCollider.radius *= 1.0f - stepHeightRatio;
            }
            else if (_capsuleCollider)
            {
                _capsuleCollider.height = colliderHeight;
                _capsuleCollider.center = colliderOffset * colliderHeight;
                _capsuleCollider.radius = colliderThickness / 2.0f;

                _capsuleCollider.center += new Vector3(0.0f, stepHeightRatio * _capsuleCollider.height / 2.0f, 0.0f);
                _capsuleCollider.height *= 1.0f - stepHeightRatio;

                if (_capsuleCollider.height / 2.0f < _capsuleCollider.radius)
                {
                    _capsuleCollider.radius = _capsuleCollider.height / 2.0f;
                }
            }

            if (_mySensor != null)
            {
                RecalibrateSensor();
            }
        }

        /// <summary>
        ///     Recalibrates the Sensor to ensure correct casts are done. Is called whenever the capsule settings are
        ///     recalculated.
        /// </summary>
        private void RecalibrateSensor()
        {
            _mySensor.CastOrigin = ColliderCenter;
            _mySensor.CastDirection = CastDirection.Down;

            RecalculateSensorLayerMask();

            _mySensor.CastType = sensorType;

            var radius = colliderThickness / 2.0f * sensorRadiusModifier;

            if (_boxCollider)
            {
                radius = Mathf.Clamp(radius, safetyDistanceFactor, _boxCollider.size.y / 2.0f * (1.0f - safetyDistanceFactor));
            }
            else if (_sphereCollider)
            {
                radius = Mathf.Clamp(radius, safetyDistanceFactor, _sphereCollider.radius * (1.0f - safetyDistanceFactor));
            }
            else if (_capsuleCollider)
            {
                radius = Mathf.Clamp(radius, safetyDistanceFactor, _capsuleCollider.height / 2.0f * (1.0f - safetyDistanceFactor));
            }

            _mySensor.SphereCastRadius = radius * _myTransform.localScale.x;

            // Calculate and set sensor length
            var length = 0f;
            length += colliderHeight * (1f - stepHeightRatio) * 0.5f;
            length += colliderHeight * stepHeightRatio;
            _baseSensorRange = length * (1f + safetyDistanceFactor) * _myTransform.localScale.x;
            _mySensor.CastLength = length * _myTransform.localScale.x;

            _mySensor.ArrayRows = sensorArrayRows;
            _mySensor.ArrayRayCount = sensorArrayRayCount;
            _mySensor.OffsetArrayRows = sensorArrayRowsAreOffset;
            _mySensor.IsInDebugMode = isSensorInDebug;

            _mySensor.SphereCastCalculateRealDistance = true;
            _mySensor.SphereCastCalculateRealSurfaceNormal = true;

            _mySensor.RecalibrateRaycastArrayPositions();
        }

        /// <summary>
        ///     Recalculates the Sensor's LayerMask based on this game object to ensure we only cast against the layers that we can
        ///     actually collider with. Ignore Raycast layer will always be excluded from this generated LayerMask.
        /// </summary>
        private void RecalculateSensorLayerMask()
        {
            var layerMask = 0;
            var objectLayer = gameObject.layer;

            for (var i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(objectLayer, i))
                {
                    layerMask |= 1 << i;
                }
            }

            // Ensure Ignore Raycast isn't included in the LayerMask
            if (layerMask == (layerMask | 1 << LayerMask.NameToLayer("Ignore Raycast")))
            {
                layerMask ^= 1 << LayerMask.NameToLayer("Ignore Raycast");
            }

            _mySensor.LayerMask = layerMask;

            _currentLayer = objectLayer;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if mover is grounded, stores all relevant collision information for later, and calculates the necessary
        ///     adjustment velocity to keep the correct distance to the ground
        /// </summary>
        public void CheckForGround()
        {
            if (_currentLayer != gameObject.layer)
            {
                RecalculateSensorLayerMask();
            }

            _currentGroundAdjustmentVelocity = Vector3.zero;

            if (_useExtendedSensorRange)
            {
                _mySensor.CastLength = _baseSensorRange + colliderHeight * _myTransform.localScale.x * stepHeightRatio;
            }
            else
            {
                _mySensor.CastLength = _baseSensorRange;
            }

            _mySensor.Cast();

            if (!_mySensor.HasDetectedHit)
            {
                IsGrounded = false;
                return;
            }

            IsGrounded = true;

            // Calculate how much mover needs to be moved up or down
            var upperLimit = colliderHeight * _myTransform.localScale.x * (1.0f - stepHeightRatio) * 0.5f;
            var middle = upperLimit + colliderHeight * _myTransform.localScale.x * stepHeightRatio;
            var distanceToGo = middle - _mySensor.HitDistance;

            // Sets the new ground adjustment velocity for the next frame
            _currentGroundAdjustmentVelocity = _myTransform.up * (distanceToGo / Time.fixedDeltaTime);
        }

        /// <summary>
        ///     Wrapper for the attached Rigidbody's MovePosition method.
        /// </summary>
        /// <param name="desiredPosition">Desired world position to move the rigidbody too.</param>
        public void MovePosition(Vector3 desiredPosition)
        {
            _myRigidbody.MovePosition(desiredPosition);
        }

        #endregion
    }
}