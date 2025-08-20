using UnityEngine;
using UnityHelpers.Runtime.Math;

namespace AdventureCharacterController.Runtime.Core
{
    /// <summary>
    ///     Character Controller that takes in CharacterInput and handles movement using the Mover and Sensor components. This
    ///     controller is based on older style adventure games like Ocarina of Time / Twilight Princess and as such, takes some
    ///     liberties with auto movement like auto jumping and auto crouching, rather than letting them be separate inputs.
    ///     Most features can be toggled on/off as desired, and properties are exposed for most settings to allow for runtime
    ///     tweaking (such as through triggers).
    /// </summary>
    public class AdventureCharacterController : MonoBehaviour
    {
        #region Editor Settings
        
        // Grounded settings
        [SerializeField] private float movementSpeed = 7.0f;
        [SerializeField] private float groundFriction = 100f;
        [SerializeField] private bool useLocalMomentum;
        [SerializeField] private float slideGravity = 5.0f;
        [SerializeField] private float slopeLimit = 80f;
        // Air control settings
        [SerializeField] private float airControlRate = 2f;
        [SerializeField] private float airControlMultiplier = 0.25f;
        [SerializeField] private float gravity = 30.0f;
        [SerializeField] private float verticalThreshold = 0.001f;
        [SerializeField] private float airFriction = 0.5f;
        // Auto jump settings
        [SerializeField] private bool useAutoJump = true;
        [SerializeField] private float jumpSpeed = 10.0f;
        [SerializeField] private float autoJumpMovementSpeedThreshold = 2.0f;
        [SerializeField] private float autoJumpCooldown = 0.2f;
        // Ceiling detection settings 
        [SerializeField] private bool useCeilingDetection = true;
        [SerializeField] private float ceilingAngleLimit = 10.0f;
        [SerializeField] private CeilingDetectionMethod ceilingDetectionMethod;
        // Wall collision settings  
        [SerializeField] private bool bounceOffWallCollisions = true;
        // Crouch settings
        [SerializeField] private float crouchSpeed = 3.5f;
        [SerializeField] private float crouchColliderHeight = 1.0f;
        [SerializeField] private float crouchStepHeightRatio = 0.1f;
        // Debug info
        [SerializeField] private ControllerState currentControllerState;

        #endregion

        #region Events

        public delegate void VectorEvent(Vector3 v);

        public VectorEvent OnJump;
        public VectorEvent OnLand;

        #endregion

        #region Properties

        /// <summary>
        ///     The desired velocity of the rigidbody.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        ///     Multiplier to apply to controller for all gravity calculations.
        /// </summary>
        public float Gravity
        {
            get => gravity;
            set => gravity = value;
        }

        /// <summary>
        ///     Multiplier applied to Jump inputs of the controller to determine jump height.
        /// </summary>
        public float JumpSpeed
        {
            get => jumpSpeed;
            set => jumpSpeed = value;
        }

        /// <summary>
        ///     Movement speed of the controller that is applied to Momentum and Velocity. If crouched, returns crouchSpeed,
        ///     otherwise movementSpeed.
        /// </summary>
        public float MovementSpeed
        {
            get => InCrouchZone ? crouchSpeed : movementSpeed;
            set => movementSpeed = value;
        }

        private Vector3 momentum = Vector3.zero;

        /// <summary>
        ///     Momentum of the controller. Will calculate local/world based vector depending on UseLocalMomentum.
        /// </summary>
        public Vector3 Momentum
        {
            get => useLocalMomentum ? myTransform.localToWorldMatrix * momentum : momentum;
            set => momentum = useLocalMomentum ? myTransform.worldToLocalMatrix * value : value;
        }

        /// <summary>
        ///     Velocity of the controller from input. i.e. Sliding down a slope with no input will return a Velocity but Movement
        ///     Velocity will be 0.
        /// </summary>
        public Vector3 MovementVelocity { get; private set; }

        public bool InCrouchZone { get; set; }

        /// <summary>
        ///     If the controller is currently on stable ground or sliding down a slope.
        /// </summary>
        public bool IsGrounded =>
            currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding ||
            currentControllerState == ControllerState.Crouching;

        /// <summary>
        ///     Checks if the controller is currently rising or falling (i.e. if any vertical movement is happening).
        /// </summary>
        private bool IsRisingOrFalling => VectorMath.ExtractDotVector(Momentum, myTransform.up).magnitude > verticalThreshold;

        /// <summary>
        ///     If the controller is sliding.
        /// </summary>
        public bool IsSliding => currentControllerState == ControllerState.Sliding;

        /// <summary>
        ///     Struct to hold player input for moving the controller. Must be externally supplied.
        /// </summary>
        public ControllerInput ControllerInput { get; set; }

        #endregion

        #region Private Fields

        private Transform myTransform;
        private Mover mover;
        private Transform relativeInputTransform;

        private bool triggerJump;
        private float timeSinceLastJump;
        private bool canJump;

        private bool ceilingWasHit;

        private float originalStepHeightRatio;
        private float originalColliderHeight;

        private enum CeilingDetectionMethod
        {
            OnlyCheckFirstContact,
            CheckAllContacts,
            CheckAverageOfAllContacts
        }

        private enum ControllerState
        {
            Grounded,
            Sliding,
            Falling,
            Rising,
            Jumping,
            Crouching
        }

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
        ///     MonoBehaviour.FixedUpdate has the frequency of the physics system; it is called every fixed frame-rate frame.
        ///     Compute Physics system calculations after FixedUpdate.
        /// </summary>
        private void FixedUpdate()
        {
            ControllerUpdate();
        }

        /// <summary>
        ///     OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (useCeilingDetection)
            {
                CheckCeilingCollisionAngles(collision);
            }

            if (bounceOffWallCollisions)
            {
                BounceOffWall(collision);
            }
        }

        /// <summary>
        ///     OnCollisionStay is called once per frame for every Collider or Rigidbody that touches another Collider or
        ///     Rigidbody.
        /// </summary>
        private void OnCollisionStay(Collision collision)
        {
            if (useCeilingDetection)
            {
                CheckCeilingCollisionAngles(collision);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Add external momentum to the controller, UseLocalMomentum will be respected.
        /// </summary>
        /// <param name="extraMomentum">Extra momentum to add to the controller.</param>
        public void AddMomentum(Vector3 extraMomentum)
        {
            var tempMomentum = Momentum;

            tempMomentum += extraMomentum;

            Momentum = tempMomentum;
        }

        #endregion

        #region Base State Handling

        /// <summary>
        ///     Handles movement state transitions and enter/exit callbacks
        /// </summary>
        private void TransitionToState(ControllerState newState)
        {
            if (currentControllerState != newState)
            {
                var previousState = currentControllerState;
                OnStateEnter(newState, previousState);
                currentControllerState = newState;
                OnStateExit(previousState, newState);
            }
        }

        /// <summary>
        ///     Event when entering a state
        /// </summary>
        private void OnStateEnter(ControllerState enteringState, ControllerState exitingState)
        {
            switch (enteringState)
            {
                case ControllerState.Grounded:
                {
                    if (exitingState == ControllerState.Sliding || exitingState == ControllerState.Falling ||
                        exitingState == ControllerState.Rising)
                    {
                        OnGroundContactRegained();
                    }

                    break;
                }
                case ControllerState.Sliding:
                {
                    if (exitingState == ControllerState.Grounded || exitingState == ControllerState.Crouching)
                    {
                        OnGroundContactLost();
                    }

                    break;
                }
                case ControllerState.Falling:
                {
                    if (exitingState == ControllerState.Grounded || exitingState == ControllerState.Sliding ||
                        exitingState == ControllerState.Crouching)
                    {
                        OnGroundContactLost();
                    }

                    if (exitingState == ControllerState.Rising && useCeilingDetection && ceilingWasHit)
                    {
                        OnCeilingContact();
                    }

                    break;
                }
                case ControllerState.Rising:
                {
                    if (exitingState == ControllerState.Grounded || exitingState == ControllerState.Sliding ||
                        exitingState == ControllerState.Crouching)
                    {
                        OnGroundContactLost();
                    }

                    break;
                }
                case ControllerState.Jumping:
                {
                    if (exitingState == ControllerState.Rising && useCeilingDetection && ceilingWasHit)
                    {
                        OnCeilingContact();
                    }

                    break;
                }
                case ControllerState.Crouching:
                {
                    if (exitingState == ControllerState.Sliding || exitingState == ControllerState.Falling ||
                        exitingState == ControllerState.Rising)
                    {
                        OnGroundContactRegained();
                    }

                    mover.ColliderHeight = crouchColliderHeight;
                    mover.StepHeightRatio = crouchStepHeightRatio;
                    break;
                }
                default:
                {
                    InternalDebug.LogError("Invalid Entering ControllerState: " + enteringState, gameObject);
                    break;
                }
            }
        }

        /// <summary>
        ///     Event when exiting a state
        /// </summary>
        private void OnStateExit(ControllerState exitingState, ControllerState enteringState)
        {
            switch (exitingState)
            {
                case ControllerState.Grounded:
                {
                    if (enteringState == ControllerState.Falling && useAutoJump)
                    {
                        if (canJump && Velocity.magnitude >= autoJumpMovementSpeedThreshold)
                        {
                            triggerJump = true;
                        }
                    }

                    break;
                }
                case ControllerState.Sliding:
                {
                    break;
                }
                case ControllerState.Falling:
                {
                    break;
                }
                case ControllerState.Rising:
                {
                    break;
                }
                case ControllerState.Jumping:
                {
                    triggerJump = false;
                    break;
                }
                case ControllerState.Crouching:
                {
                    mover.ColliderHeight = originalColliderHeight;
                    mover.StepHeightRatio = originalStepHeightRatio;
                    break;
                }
                default:
                {
                    InternalDebug.LogError("Invalid Exiting ControllerState: " + exitingState, gameObject);
                    break;
                }
            }
        }

        /// <summary>
        ///     Determine the current controller state based on momentum, grounded, etc.
        /// </summary>
        /// <returns>Current controller state.</returns>
        private ControllerState DetermineControllerState()
        {
            var isRising = IsRisingOrFalling && VectorMath.GetDotProduct(Momentum, myTransform.up) > 0.0f;
            var isSliding = mover.IsGrounded && IsGroundTooSteep();

            switch (currentControllerState)
            {
                case ControllerState.Grounded:
                {
                    if (InCrouchZone)
                    {
                        return ControllerState.Crouching;
                    }

                    if (isRising)
                    {
                        return ControllerState.Rising;
                    }

                    if (!mover.IsGrounded)
                    {
                        return ControllerState.Falling;
                    }

                    if (isSliding)
                    {
                        return ControllerState.Sliding;
                    }

                    return ControllerState.Grounded;
                }
                case ControllerState.Sliding:
                {
                    if (isRising)
                    {
                        return ControllerState.Rising;
                    }

                    if (!mover.IsGrounded)
                    {
                        return ControllerState.Falling;
                    }

                    if (mover.IsGrounded && !isSliding)
                    {
                        return ControllerState.Grounded;
                    }

                    return ControllerState.Sliding;
                }
                case ControllerState.Falling:
                {
                    if (isRising)
                    {
                        return ControllerState.Rising;
                    }

                    if (mover.IsGrounded && !isSliding)
                    {
                        return ControllerState.Grounded;
                    }

                    if (isSliding)
                    {
                        return ControllerState.Sliding;
                    }

                    return ControllerState.Falling;
                }
                case ControllerState.Rising:
                {
                    if (!isRising)
                    {
                        if (mover.IsGrounded && !isSliding)
                        {
                            if (InCrouchZone)
                            {
                                return ControllerState.Crouching;
                            }

                            return ControllerState.Grounded;
                        }

                        if (isSliding)
                        {
                            return ControllerState.Sliding;
                        }

                        if (!mover.IsGrounded)
                        {
                            return ControllerState.Falling;
                        }
                    }

                    if (useCeilingDetection && ceilingWasHit)
                    {
                        return ControllerState.Falling;
                    }

                    return ControllerState.Rising;
                }
                case ControllerState.Jumping:
                {
                    if (triggerJump)
                    {
                        return ControllerState.Rising;
                    }

                    if (useCeilingDetection && ceilingWasHit)
                    {
                        return ControllerState.Falling;
                    }

                    return ControllerState.Jumping;
                }
                case ControllerState.Crouching:
                {
                    if (InCrouchZone)
                    {
                        return ControllerState.Crouching;
                    }

                    if (isRising)
                    {
                        return ControllerState.Rising;
                    }

                    if (!mover.IsGrounded)
                    {
                        return ControllerState.Falling;
                    }

                    if (isSliding)
                    {
                        return ControllerState.Sliding;
                    }

                    return ControllerState.Grounded;
                }
                default:
                {
                    InternalDebug.LogWarningFormat(
                        $"Invalid ControllerState {currentControllerState} detected, defaulting to falling.", gameObject);
                    return ControllerState.Falling;
                }
            }
        }

        #endregion

        #region Movement

        /// <summary>
        ///     This function must be called every fixed update in order for the controller to work correctly. Determines state and
        ///     handles movement calculations.
        /// </summary>
        private void ControllerUpdate()
        {
            mover.CheckForGround();

            var nextCharacterState = DetermineControllerState();
            TransitionToState(nextCharacterState);

            HandleMomentum();

            if (triggerJump)
            {
                HandleJumping();
            }

            var velocity = Vector3.zero;
            if (currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Crouching)
            {
                velocity = CalculateMovementVelocity();

                if (useAutoJump && !canJump)
                {
                    timeSinceLastJump += Time.deltaTime;
                    canJump = timeSinceLastJump >= autoJumpCooldown;
                }
            }

            var newMomentum = Momentum;

            velocity += newMomentum;

            // If the player is grounded or sliding on a slope, extend mover's sensor range as it enables the player to walk up or down stairs and slopes without losing ground contact
            mover.UseExtendedSensorRange = IsGrounded;

            mover.Velocity = velocity;

            // Store current velocity for next frame
            Velocity = velocity;
            MovementVelocity = CalculateMovementVelocity();

            // Reset ceiling detector if applicable
            if (useCeilingDetection)
            {
                ceilingWasHit = false;
            }
        }

        /// <summary>
        ///     Handle momentum calculations based on ControllerState, applying necessary friction and gravity.
        /// </summary>
        private void HandleMomentum()
        {
            var tempMomentum = Momentum;

            var verticalMomentum = Vector3.zero;
            var horizontalMomentum = Vector3.zero;

            // Split momentum into vertical and horizontal components
            if (tempMomentum != Vector3.zero)
            {
                verticalMomentum = VectorMath.ExtractDotVector(tempMomentum, myTransform.up);
                horizontalMomentum = tempMomentum - verticalMomentum;
            }

            // Add gravity to vertical momentum
            verticalMomentum -= myTransform.up * (Gravity * Time.deltaTime);

            // Remove any downward force if the controller is grounded
            if ((currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Crouching) &&
                VectorMath.GetDotProduct(verticalMomentum, myTransform.up) < 0.0f)
            {
                verticalMomentum = Vector3.zero;
            }

            // Manipulate momentum to steer controller in the air (if controller is not grounded or sliding)
            if (!IsGrounded)
            {
                var movementVelocity = CalculateMovementVelocity();

                // If controller has received additional momentum from somewhere else
                if (horizontalMomentum.magnitude > MovementSpeed)
                {
                    // Prevent unwanted accumulation of speed in the direction of the current momentum
                    if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0.0f)
                    {
                        movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                    }

                    // Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller
                    horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * airControlMultiplier);
                }
                else
                {
                    // Clamp horizontal velocity to prevent accumulation of speed
                    horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
                    horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, MovementSpeed);
                }
            }

            // Steer controller on slopes
            if (currentControllerState == ControllerState.Sliding)
            {
                // Calculate vector pointing away from the slope
                var pointDownVector = Vector3.ProjectOnPlane(mover.GroundNormal, myTransform.up).normalized;

                var slopeMovementVelocity = CalculateMovementVelocity();
                // Remove all velocity pointing up the slope
                slopeMovementVelocity = VectorMath.RemoveDotVector(slopeMovementVelocity, pointDownVector);

                horizontalMomentum += slopeMovementVelocity * Time.fixedDeltaTime;
            }

            // Apply appropriate friction to horizontal momentum based on whether the controller is grounded or not
            if (currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Crouching)
            {
                horizontalMomentum =
                    VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, groundFriction, Time.deltaTime, Vector3.zero);
            }
            else
            {
                horizontalMomentum =
                    VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero);
            }

            // Add horizontal and vertical momentum back together
            tempMomentum = horizontalMomentum + verticalMomentum;

            // Additional momentum calculations for sliding
            if (currentControllerState == ControllerState.Sliding)
            {
                // Project the current momentum onto the current ground normal if the controller is sliding down a slope;
                tempMomentum = Vector3.ProjectOnPlane(tempMomentum, mover.GroundNormal);

                // Remove any upwards momentum when sliding;
                if (VectorMath.GetDotProduct(tempMomentum, myTransform.up) > 0.0f)
                {
                    tempMomentum = VectorMath.RemoveDotVector(tempMomentum, myTransform.up);
                }

                // Apply additional slide gravity
                var slideDirection = Vector3.ProjectOnPlane(-myTransform.up, mover.GroundNormal).normalized;
                tempMomentum += slideDirection * (slideGravity * Time.deltaTime);
            }

            // If the controller is jumping, override vertical velocity with jumpSpeed;
            if (currentControllerState == ControllerState.Jumping)
            {
                tempMomentum = VectorMath.RemoveDotVector(tempMomentum, myTransform.up);
                tempMomentum += myTransform.up * JumpSpeed;
            }

            Momentum = tempMomentum;
        }

        /// <summary>
        ///     Handle jumping state
        /// </summary>
        private void HandleJumping()
        {
            OnGroundContactLost();
            OnJumpStart();

            currentControllerState = ControllerState.Jumping;
            timeSinceLastJump = 0.0f;
            canJump = false;
        }

        #endregion

        #region Internal Calculations

        /// <summary>
        ///     Calculate and return movement velocity based on player input, controller state, ground normal [...]
        /// </summary>
        /// <returns></returns>
        private Vector3 CalculateMovementVelocity()
        {
            var velocity = Vector3.zero;

            // If no relative transform has been assigned, use the controller's transform axes to calculate the movement direction
            if (!relativeInputTransform)
            {
                velocity += myTransform.right * ControllerInput.Horizontal;
                velocity += myTransform.forward * ControllerInput.Vertical;
            }
            else
            {
                // If a relative transform has been assigned, use the assigned transform's axes for a movement direction
                // Project movement direction so movement stays parallel to the ground
                velocity += Vector3.ProjectOnPlane(relativeInputTransform.right, myTransform.up).normalized *
                            ControllerInput.Horizontal;
                velocity += Vector3.ProjectOnPlane(relativeInputTransform.forward, myTransform.up).normalized *
                            ControllerInput.Vertical;
            }

            // Clamp any movement above 1 so that moving diagonally isn't faster than normal movement
            if (velocity.magnitude > 1.0f)
            {
                velocity.Normalize();
            }

            // Multiply (normalized) velocity with movement speed
            velocity *= MovementSpeed;

            return velocity;
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        ///     Gather component references and prep input struct.
        /// </summary>
        private void Setup()
        {
            // Mandatory references
            ControllerInput = new ControllerInput();
            myTransform = transform;
            if (!TryGetComponent(out mover))
            {
                InternalDebug.LogError("Mover component not found", gameObject);
            }

            originalColliderHeight = mover.ColliderHeight;
            originalStepHeightRatio = mover.StepHeightRatio;

            // Optional references
            if (Camera.main != null)
            {
                relativeInputTransform = Camera.main.transform;
            }

            TransitionToState(ControllerState.Grounded);
        }

        /// <summary>
        ///     Checks if the current ground is too steep for the controller, either because the mover is not grounded or the slope
        ///     is above the slope limit.
        /// </summary>
        /// <returns>True if not grounded or slope is too steep.</returns>
        private bool IsGroundTooSteep()
        {
            if (!mover.IsGrounded)
            {
                return true;
            }

            return Vector3.Angle(mover.GroundNormal, myTransform.up) > slopeLimit;
        }

        #endregion

        #region Collision Modifiers

        /// <summary>
        ///     Gets the contact normal of the wall and applies opposite force to the controller.
        /// </summary>
        /// <param name="collision"></param>
        private void BounceOffWall(Collision collision)
        {
            var normal = collision.GetContact(0).normal;
            var contactForce = -(normal * Vector3.Dot(Velocity, normal));
            AddMomentum(contactForce);
        }

        /// <summary>
        ///     Checks if a given collision qualifies as a ceiling hit.
        /// </summary>
        /// <param name="collision">Collision the controller collided with.</param>
        private void CheckCeilingCollisionAngles(Collision collision)
        {
            var angle = 0.0f;

            switch (ceilingDetectionMethod)
            {
                case CeilingDetectionMethod.OnlyCheckFirstContact:
                {
                    // Calculate angle between hit normal and character
                    angle = Vector3.Angle(-myTransform.up, collision.contacts[0].normal);

                    if (angle < ceilingAngleLimit)
                    {
                        ceilingWasHit = true;
                    }

                    break;
                }
                case CeilingDetectionMethod.CheckAllContacts:
                {
                    foreach (var contactPoint in collision.contacts)
                    {
                        // Calculate angle between hit normal and character
                        angle = Vector3.Angle(-myTransform.up, contactPoint.normal);

                        if (angle < ceilingAngleLimit)
                        {
                            ceilingWasHit = true;
                        }
                    }

                    break;
                }
                case CeilingDetectionMethod.CheckAverageOfAllContacts:
                {
                    foreach (var contactPoint in collision.contacts)
                    {
                        // Calculate angle between hit normal and character and add it to total angle count
                        angle += Vector3.Angle(-myTransform.up, contactPoint.normal);
                    }

                    // If average angle is smaller than the ceiling angle limit, register ceiling hit
                    if (angle / collision.contacts.Length < ceilingAngleLimit)
                    {
                        ceilingWasHit = true;
                    }

                    break;
                }
                default:
                {
                    InternalDebug.LogWarning(
                        $"Unknown CeilingDetectionMethod {ceilingDetectionMethod.ToString()}. CeilingWasHit will always be false.");
                    ceilingWasHit = false;
                    break;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Controller has initiated a jump, add upwards momentum to make controller 'jump'. Also calls the OnJump VectorEvent.
        /// </summary>
        private void OnJumpStart()
        {
            var tempMomentum = Momentum;

            tempMomentum += myTransform.up * JumpSpeed;

            if (OnJump != null)
            {
                OnJump(tempMomentum);
            }

            Momentum = tempMomentum;
        }

        /// <summary>
        ///     Recalculate momentum when ground contact is lost to prevent unwanted speed accumulation.
        /// </summary>
        private void OnGroundContactLost()
        {
            var tempMomentum = Momentum;

            var velocity = MovementVelocity;

            // Check if the controller has both momentum and a current movement velocity
            if (velocity.sqrMagnitude >= 0f && tempMomentum.sqrMagnitude > 0f)
            {
                // Project momentum onto movement direction
                var projectedMomentum = Vector3.Project(tempMomentum, velocity.normalized);
                // Calculate dot product to determine whether momentum and movement are aligned
                var dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

                // If current momentum is already pointing in the same direction as movement velocity,
                // Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f)
                {
                    velocity = Vector3.zero;
                }
                else if (dot > 0f)
                {
                    velocity -= projectedMomentum;
                }
            }

            tempMomentum += velocity;

            Momentum = tempMomentum;
        }

        /// <summary>
        ///     Fires off OnLand VectorEvent when ground contact is regained by the controller.
        /// </summary>
        private void OnGroundContactRegained()
        {
            if (OnLand != null)
            {
                OnLand(Momentum);
            }
        }

        /// <summary>
        ///     If a using ceiling detection, remove vertical momentum if we hit a ceiling.
        /// </summary>
        private void OnCeilingContact()
        {
            var tempMomentum = Momentum;

            // Remove all vertical parts of momentum;
            tempMomentum = VectorMath.RemoveDotVector(tempMomentum, myTransform.up);

            Momentum = tempMomentum;
        }

        #endregion
    }

    /// <summary>
    ///     Storage struct for controller-specific inputs.
    /// </summary>
    public struct ControllerInput
    {
        public float Vertical;
        public float Horizontal;
    }
}