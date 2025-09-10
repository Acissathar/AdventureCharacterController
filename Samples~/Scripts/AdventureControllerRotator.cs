using UnityEngine;
using UnityHelpers.Runtime.Math;

namespace AdventureCharacterController.Samples.Scripts
{
    /// <summary>
    ///     Rotates a gameobject based on an assigned AdventureCharacterController and desired rotation state.
    /// </summary>
    public class AdventureControllerRotator : MonoBehaviour
    {
        #region Editor - Settings

        [SerializeField] [Tooltip("Target Controller")]
        private Runtime.Core.AdventureCharacterController adventureCharacterController;

        [SerializeField] [Tooltip("Speed at which this game object turns toward the controller's velocity")]
        private float turnSpeed = 500.0f;

        [SerializeField]
        [Tooltip("If the angle between the current and target direction falls below this, turnSpeed becomes progressively slower.")]
        private float fallOffAngle = 90.0f;

        [SerializeField] [Tooltip("Whether the current controller momentum should be ignored when calculating the new direction")]
        private bool ignoreControllerMomentum;

        [SerializeField] [Tooltip("Magnitude the target's velocity needs to at or above to actually turn towards it")]
        private float magnitudeThreshold = 0.001f;

        #endregion

        #region Private Fields

        private Transform _parentTransform;
        private Transform _myTransform;
        private float _currentYRotation;

        private Transform _mainCameraTransform;
        private Transform _externalTargetTransform;

        private RotationMethod _rotationMethod;

        private enum RotationMethod
        {
            TowardsControllerVelocity,
            TowardsMainCameraDirection,
            TowardsExternalTransform
        }

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Unity calls Awake when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            _myTransform = transform;
            _parentTransform = _myTransform.parent;

            if (adventureCharacterController != null)
            {
                return;
            }

            InternalDebug.LogWarning("No controller script has been assigned. Disabling Rotator.", gameObject);
            enabled = false;
        }

        /// <summary>
        ///     LateUpdate is called every frame if the Behavior is enabled after all other Update functions.
        /// </summary>
        private void LateUpdate()
        {
            switch (_rotationMethod)
            {
                case RotationMethod.TowardsControllerVelocity:
                {
                    TowardsControllerVelocity();
                    break;
                }
                case RotationMethod.TowardsMainCameraDirection:
                {
                    TowardsMainCameraDirection();
                    break;
                }
                case RotationMethod.TowardsExternalTransform:
                {
                    TowardsExternalTransform();
                    break;
                }
                default:
                {
                    InternalDebug.LogErrorFormat($"Invalid Rotation Method: {_rotationMethod}. Disabling Rotator.", gameObject);
                    enabled = false;
                    break;
                }
            }
        }

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            _currentYRotation = transform.localEulerAngles.y;

            if (adventureCharacterController == null)
            {
                InternalDebug.LogWarning("No controller script has been assigned. Disabling Rotator.", gameObject);
                enabled = false;
            }
            else
            {
                adventureCharacterController.OnFreeClimbEnter += FreeClimbEnterSetExternal;
                adventureCharacterController.OnFreeClimbExit += SetTowardsControllerVelocity;
            }


            if (!Camera.main)
            {
                return;
            }

            _mainCameraTransform = Camera.main.transform;
        }

        /// <summary>
        ///     This function is called when the object becomes disabled and inactive.
        /// </summary>
        private void OnDisable()
        {
            if (adventureCharacterController != null)
            {
                adventureCharacterController.OnFreeClimbEnter -= FreeClimbEnterSetExternal;
                adventureCharacterController.OnFreeClimbExit -= SetTowardsControllerVelocity;
            }
        }

        #endregion

        #region Rotation State Methods

        private void TowardsControllerVelocity()
        {
            var velocity = ignoreControllerMomentum
                ? adventureCharacterController.MovementVelocity
                : adventureCharacterController.Velocity;
            velocity = Vector3.ProjectOnPlane(velocity, _parentTransform.up);

            if (velocity.magnitude < magnitudeThreshold)
            {
                return;
            }

            velocity.Normalize();

            var currentForward = _myTransform.forward;

            // Calculate the rotation step that's needed
            var angleDifference = VectorMath.GetAngle(currentForward, velocity, _parentTransform.up);
            var factor = Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(angleDifference));
            var step = Mathf.Sign(angleDifference) * factor * Time.deltaTime * turnSpeed;

            if (angleDifference < 0f && step < angleDifference || angleDifference > 0f && step > angleDifference)
            {
                step = angleDifference;
            }

            _currentYRotation += step;

            if (_currentYRotation > 360f)
            {
                _currentYRotation -= 360f;
            }

            if (_currentYRotation < -360f)
            {
                _currentYRotation += 360f;
            }

            _myTransform.localRotation = Quaternion.Euler(0f, _currentYRotation, 0f);
        }

        private void TowardsMainCameraDirection()
        {
            if (!_mainCameraTransform)
            {
                if (!Camera.main)
                {
                    InternalDebug.LogErrorFormat("No main camera found, but RotationMethod is TowardsMainCameraDirection.",
                        gameObject);
                    return;
                }

                _mainCameraTransform = Camera.main.transform;
            }

            _mainCameraTransform.rotation = Quaternion.LookRotation(_mainCameraTransform.forward, _mainCameraTransform.up);
        }

        private void TowardsExternalTransform()
        {
            if (!_externalTargetTransform)
            {
                InternalDebug.LogErrorFormat("ExternalTargetTransform is null, but RotationMethod is TowardsExternalTransform.",
                    gameObject);
                return;
            }

            var forwardDirection = Vector3.ProjectOnPlane(_externalTargetTransform.forward, _parentTransform.up).normalized;
            var upDirection = _parentTransform.up;

            _myTransform.rotation = Quaternion.LookRotation(forwardDirection, upDirection);
        }

        private void FreeClimbEnterSetExternal()
        {
            SetTowardsExternalTransform(adventureCharacterController.CurrentClimbZoneTrigger.ClimbZoneTransform);
        }

        #endregion

        #region Public Methods

        public void SetTowardsControllerVelocity()
        {
            _rotationMethod = RotationMethod.TowardsControllerVelocity;
        }

        public void SetTowardsCameraDirection()
        {
            _rotationMethod = RotationMethod.TowardsMainCameraDirection;
        }

        public void SetTowardsExternalTransform(Transform transformToFollow)
        {
            _externalTargetTransform = transformToFollow;
            _rotationMethod = RotationMethod.TowardsExternalTransform;
        }

        #endregion
    }
}