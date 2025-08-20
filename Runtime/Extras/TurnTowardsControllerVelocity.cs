using UnityEngine;
using UnityHelpers.Runtime.Math;

namespace AdventureCharacterController.Runtime.Extras
{
    /// <summary>
    ///     Turns a gameobject toward the target controller's velocity direction.
    /// </summary>
    public class TurnTowardControllerVelocity : MonoBehaviour
    {
        #region Editor - Settings

        [SerializeField] [Tooltip("Target Controller")]
        private Core.AdventureCharacterController adventureCharacterController;

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

        private Transform parentTransform;
        private Transform myTransform;
        private float currentYRotation;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Unity calls Awake when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            myTransform = transform;
            parentTransform = myTransform.parent;

            if (adventureCharacterController == null)
            {
                InternalDebug.LogWarning("No controller script has been assigned to this 'TurnTowardControllerVelocity' component!",
                    this);
                enabled = false;
            }
        }

        /// <summary>
        ///     LateUpdate is called every frame, if the Behaviour is enabled after all other Update functions.
        /// </summary>
        private void LateUpdate()
        {
            var velocity = ignoreControllerMomentum
                ? adventureCharacterController.MovementVelocity
                : adventureCharacterController.Velocity;
            velocity = Vector3.ProjectOnPlane(velocity, parentTransform.up);

            if (velocity.magnitude < magnitudeThreshold)
            {
                return;
            }

            velocity.Normalize();

            var currentForward = myTransform.forward;

            // Calculate the rotation step that's needed
            var angleDifference = VectorMath.GetAngle(currentForward, velocity, parentTransform.up);
            var factor = Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(angleDifference));
            var step = Mathf.Sign(angleDifference) * factor * Time.deltaTime * turnSpeed;

            if (angleDifference < 0f && step < angleDifference || angleDifference > 0f && step > angleDifference)
            {
                step = angleDifference;
            }

            currentYRotation += step;

            if (currentYRotation > 360f)
            {
                currentYRotation -= 360f;
            }

            if (currentYRotation < -360f)
            {
                currentYRotation += 360f;
            }

            myTransform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            currentYRotation = transform.localEulerAngles.y;
        }

        #endregion
    }
}