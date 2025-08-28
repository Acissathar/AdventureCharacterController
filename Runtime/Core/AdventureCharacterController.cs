using AdventureCharacterController.Runtime.Extras;
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

        // Ladder settings
        [SerializeField] private float ladderMovementSpeed = 15.0f;
        [SerializeField] private float ladderUseThreshold = 0.15f;
        [SerializeField] private float ladderAttachSpeed = 3.5f;
        [SerializeField] private float ladderMoveThreshold = 0.01f;
        
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
        ///     Momentum of the controller. Will calculate local/world-based vector depending on UseLocalMomentum.
        /// </summary>
        public Vector3 Momentum
        {
            get => useLocalMomentum ? myTransform.localToWorldMatrix * momentum : momentum;
            private set => momentum = useLocalMomentum ? myTransform.worldToLocalMatrix * value : value;
        }

        /// <summary>
        ///     Velocity of the controller from input. I.e., Sliding down a slope with no input will return a Velocity but Movement
        ///     Velocity will be 0.
        /// </summary>
        public Vector3 MovementVelocity { get; private set; }

        /// <summary>
        ///     Flag indicating if the controller is in a crouch zone trigger so that we can change controller state to crouching.
        /// </summary>
        public bool InCrouchZone { get; set; }

        /// <summary>
        ///     Current ladder info (if any) that the controller is in. 
        /// </summary>
        public ILadderInfo CurrentLadder { get; set; }

        /// <summary>
        ///     If the controller is currently on stable ground or sliding down a slope.
        /// </summary>
        public bool IsGrounded =>
            currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding ||
            currentControllerState == ControllerState.Crouching;

        /// <summary>
        ///     Checks if the controller is currently rising or falling (i.e., if any vertical movement is happening).
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

        private bool triggerLadderEnter;
        private bool triggerLadderExit;
        private bool usingLadder;
        
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
            Crouching,
            LadderStart,
            LadderClimbing,
            LadderEnd
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
        ///     Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        /// </summary>
        private void OnValidate()
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

        #region State Handling

        /// <summary>
        ///     Determine the current controller state based on momentum, grounded, etc.
        /// </summary>
        /// <returns>Current controller state.</returns>
        private ControllerState DetermineControllerState()
        {
            var isRising = IsRisingOrFalling && VectorMath.GetDotProduct(Momentum, myTransform.up) > 0.0f;
            var isSliding = mover.IsGrounded && IsGroundTooSteep();
            var isOnLadder = usingLadder && CurrentLadder != null;
            
            switch (currentControllerState)
            {
                case ControllerState.Grounded:
                {
                    if (InCrouchZone)
                    {
                        return ControllerState.Crouching;
                    }
                    
                    if (triggerLadderEnter)
                    {
                        return ControllerState.LadderStart;
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
                    if (triggerJump)
                    {
                        return ControllerState.Jumping;
                    }

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
                case ControllerState.LadderStart:
                {
                    if (mover.Velocity.sqrMagnitude <= ladderMoveThreshold)
                    {
                        return ControllerState.LadderClimbing;
                    }
                    
                    if (triggerLadderEnter)
                    {
                        return ControllerState.LadderStart;
                    }
                    
                    if (!mover.IsGrounded)
                    {
                        return ControllerState.Falling;
                    }

                    return ControllerState.Grounded;
                }
                case ControllerState.LadderClimbing:
                {
                    if (isOnLadder)
                    {
                        return ControllerState.LadderClimbing;
                    }

                    if (!mover.IsGrounded)
                    {
                        return ControllerState.Falling;
                    }

                    return ControllerState.Grounded;
                }
                case ControllerState.LadderEnd:
                {
                    if (triggerLadderExit)
                    {
                        return ControllerState.LadderEnd;
                    }

                    if (!mover.IsGrounded)
                    {
                        return ControllerState.Falling;
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
                        exitingState == ControllerState.Rising || exitingState == ControllerState.LadderClimbing)
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
                    OnGroundContactLost();
                    OnJumpStart();

                    timeSinceLastJump = 0.0f;
                    canJump = false;

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
                case ControllerState.LadderStart:
                {
                    usingLadder = true;
                    break;
                }
                case ControllerState.LadderClimbing:
                {
                    break;
                }
                case ControllerState.LadderEnd:
                {
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
                case ControllerState.LadderStart:
                {
                    triggerLadderEnter = false;
                    if (enteringState != ControllerState.LadderClimbing && enteringState != ControllerState.LadderEnd)
                    {
                        usingLadder = false;
                    }
                    break;
                }
                case ControllerState.LadderClimbing:
                {
                    if (enteringState != ControllerState.LadderStart && enteringState != ControllerState.LadderEnd)
                    {
                        usingLadder = false;
                    }
                    break;
                }
                case ControllerState.LadderEnd:
                {
                    triggerLadderExit = false;
                    if (enteringState != ControllerState.LadderStart && enteringState != ControllerState.LadderClimbing)
                    {
                        usingLadder = false;
                    }
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
        ///     Handle momentum calculations based on ControllerState, applying necessary friction and gravity, along with any
        ///     state-specific update functionality.
        /// </summary>
        private void StateUpdate()
        {
            var tempMomentum = Momentum;

            switch (currentControllerState)
            {
                case ControllerState.Grounded:
                {
                    tempMomentum = CalculateGroundedMomentum(tempMomentum);
                    if (useAutoJump && !canJump)
                    {
                        timeSinceLastJump += Time.deltaTime;
                        canJump = timeSinceLastJump >= autoJumpCooldown;
                    }

                    HandleLadder();

                    Velocity = tempMomentum + MovementVelocity;
                    break;
                }
                case ControllerState.Sliding:
                {
                    tempMomentum = CalculateSlidingMomentum(tempMomentum);

                    Velocity = tempMomentum;
                    break;
                }
                case ControllerState.Falling:
                {
                    tempMomentum = CalculateAirMomentum(tempMomentum);

                    Velocity = tempMomentum;
                    break;
                }
                case ControllerState.Rising:
                {
                    tempMomentum = CalculateAirMomentum(tempMomentum);

                    Velocity = tempMomentum;
                    break;
                }
                case ControllerState.Jumping:
                {
                    tempMomentum = CalculateAirMomentum(tempMomentum);
                    tempMomentum = VectorMath.RemoveDotVector(tempMomentum, myTransform.up);
                    tempMomentum += myTransform.up * JumpSpeed;

                    Velocity = tempMomentum;
                    break;
                }
                case ControllerState.Crouching:
                {
                    tempMomentum = CalculateGroundedMomentum(tempMomentum);
                    if (useAutoJump && !canJump)
                    {
                        timeSinceLastJump += Time.deltaTime;
                        canJump = timeSinceLastJump >= autoJumpCooldown;
                    }

                    Velocity = tempMomentum + MovementVelocity;
                    break;
                }
                case ControllerState.LadderStart:
                {
                    if (CurrentLadder == null)
                    {
                        usingLadder = false;
                    }
                    
                    tempMomentum = (CurrentLadder.LadderStartOffsetPoint + CurrentLadder.LadderTransform.position - myTransform.position).normalized * ladderAttachSpeed;
                    Velocity = tempMomentum;
                    break;
                }
                case ControllerState.LadderClimbing:
                {
                    if (CurrentLadder == null)
                    {
                        usingLadder = false;
                    }
                    
                    tempMomentum = CalculateLadderMomentum();
                    
                    Velocity = tempMomentum;
                    break;
                }
                case ControllerState.LadderEnd:
                {
                    if (CurrentLadder == null)
                    {
                        usingLadder = false;
                    }
                    
                    Velocity = Vector3.zero;
                    break;
                }
                default:
                {
                    InternalDebug.LogErrorFormat($"Invalid ControllerState: {currentControllerState}", gameObject);
                    break;
                }
            }

            Momentum = tempMomentum;
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
            MovementVelocity = CalculateMovementVelocity();

            var nextCharacterState = DetermineControllerState();
            TransitionToState(nextCharacterState);
            
            StateUpdate();

            // If the player is grounded or sliding on a slope, extend mover's sensor range as it enables the player to walk up or down stairs and slopes without losing ground contact
            mover.UseExtendedSensorRange = IsGrounded; 

            mover.Velocity = Velocity;

            // Reset ceiling detector if applicable
            if (useCeilingDetection)
            {
                ceilingWasHit = false;
            }
        }

        /// <summary>
        ///     Calculates momentum for when the controller is grounded. Notably, vertical momentum will be zeroed out in this
        ///     state.
        /// </summary>
        /// <param name="tempMomentum">Starting momentum to work with.</param>
        /// <returns>Calculated grounded momentum.</returns>
        private Vector3 CalculateGroundedMomentum(Vector3 tempMomentum)
        {
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

            if (VectorMath.GetDotProduct(verticalMomentum, myTransform.up) < 0.0f)
            {
                verticalMomentum = Vector3.zero;
            }

            horizontalMomentum =
                VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, groundFriction, Time.deltaTime, Vector3.zero);

            // Add horizontal and vertical momentum back together
            return horizontalMomentum + verticalMomentum;
        }

        /// <summary>
        ///     Calculates momentum for when the controller is in the air. Notably, if external momentum has been applied to the
        ///     controller, it will be adjusted by airControlMultiplier to help apply some additional weight. MovementSpeed
        ///     will cap horizontal velocity magnitude.
        /// </summary>
        /// <param name="tempMomentum">Starting momentum to work with.</param>
        /// <returns>Calculated air momentum.</returns>
        private Vector3 CalculateAirMomentum(Vector3 tempMomentum)
        {
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

            // If the controller has received additional momentum from somewhere else
            if (horizontalMomentum.magnitude > MovementSpeed)
            {
                // Prevent unwanted accumulation of speed in the direction of the current momentum
                if (VectorMath.GetDotProduct(MovementVelocity, horizontalMomentum.normalized) > 0.0f)
                {
                    MovementVelocity = VectorMath.RemoveDotVector(MovementVelocity, horizontalMomentum.normalized);
                }

                // Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller
                horizontalMomentum += MovementVelocity * (Time.deltaTime * airControlRate * airControlMultiplier);
            }
            else
            {
                // Clamp horizontal velocity to prevent accumulation of speed
                horizontalMomentum += MovementVelocity * (Time.deltaTime * airControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, MovementSpeed);
            }

            horizontalMomentum =
                VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero);

            // Add horizontal and vertical momentum back together
            return horizontalMomentum + verticalMomentum;
        }

        /// <summary>
        ///     Calculates momentum for when the controller is sliding. Upwards momentum is removed, and slide gravity is applied
        ///     in the direction of the slope.
        /// </summary>
        /// <param name="tempMomentum">Starting momentum to work with.</param>
        /// <returns>Calculated grounded momentum.</returns>
        private Vector3 CalculateSlidingMomentum(Vector3 tempMomentum)
        {
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

            // Calculate the vector pointing away from the slope
            var pointDownVector = Vector3.ProjectOnPlane(mover.GroundNormal, myTransform.up).normalized;

            // Remove all velocity pointing up the slope
            var slopeMovementVelocity = VectorMath.RemoveDotVector(MovementVelocity, pointDownVector);

            horizontalMomentum += slopeMovementVelocity * Time.fixedDeltaTime;

            horizontalMomentum =
                VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero);

            // Add horizontal and vertical momentum back together
            tempMomentum = horizontalMomentum + verticalMomentum;

            // Project the current momentum onto the current ground normal if the controller is sliding down a slope
            tempMomentum = Vector3.ProjectOnPlane(tempMomentum, mover.GroundNormal);

            // Remove any upwards momentum when sliding
            if (VectorMath.GetDotProduct(tempMomentum, myTransform.up) > 0.0f)
            {
                tempMomentum = VectorMath.RemoveDotVector(tempMomentum, myTransform.up);
            }

            // Apply additional slide gravity
            var slideDirection = Vector3.ProjectOnPlane(-myTransform.up, mover.GroundNormal).normalized;
            tempMomentum += slideDirection * (slideGravity * Time.deltaTime);

            return tempMomentum;
        }

        /// <summary>
        ///     Calculates momentum for when the controller is on a ladder. Notably, momentum from previous frames is ignored, and
        ///     only current momentum for this frame's velocity is used and translated into the Y direction.
        /// </summary>
        /// <returns>Calculated grounded momentum.</returns>
        private Vector3 CalculateLadderMomentum()
        {
            var verticalMomentum = Vector3.zero;
            var horizontalMomentum = Vector3.zero;

            if (CurrentLadder != null && mover.IsGrounded && MovementVelocity.z < 0.0f)
            {
                horizontalMomentum = -CurrentLadder.LadderTransform.forward * MovementSpeed;
            }
            else
            {
                // Take the Z of the MovementVelocity (the 'forward' and 'back' input from the player) and use it in place of the velocity rising/falling gravity axis
                var ladderMovementVelocity = new Vector3(0.0f, MovementVelocity.z, 0.0f);

                // Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller
                verticalMomentum += ladderMovementVelocity * (Time.deltaTime * ladderMovementSpeed);
            }

            // Add horizontal and vertical momentum back together
            return horizontalMomentum + verticalMomentum;
        }

        /// <summary>
        ///     Handles whether to transition into or out of the ladder state based on movement input.
        /// </summary>
        private void HandleLadder()
        {
            if (CurrentLadder == null)
            {
                usingLadder = false;
                triggerLadderEnter = false;
                triggerLadderExit = false;
            }
            else if (mover.IsGrounded && MovementVelocity.sqrMagnitude > 0.0f)
            {
                if (!usingLadder)
                {
                    var controllerToLadderDotProduct =
                        VectorMath.GetDotProduct(MovementVelocity.normalized, CurrentLadder.LadderTransform.forward);

                    InternalDebug.LogFormat($"Movement Velocity {MovementVelocity} - Normalized: {MovementVelocity.normalized}");
                    
                    if (controllerToLadderDotProduct <= ladderUseThreshold && controllerToLadderDotProduct >= -ladderUseThreshold)
                    {
                        triggerLadderEnter = true;
                    }
                }
            }
            else
            {
                InternalDebug.Log($"Retain ladder state - {usingLadder}");
            }
        }

        #endregion

        #region Internal Calculations

        /// <summary>
        ///     Calculate movement velocity based on player input, controller state, ground normal [...]
        /// </summary>
        /// <returns>Vector3 containing movement velocity with horizontal being the X element and vertical being the Z element.</returns>
        private Vector3 CalculateMovementVelocity()
        {
            var velocity = Vector3.zero;

            // If no relative transform has been assigned, or we're on a ladder, use the controller's transform axes to calculate the movement direction
            if (!relativeInputTransform || usingLadder)
            {
                velocity += myTransform.right * ControllerInput.Horizontal;
                velocity += myTransform.forward * ControllerInput.Vertical;
            }
            else
            {
                // If a relative transform has been assigned, use the assigned transform's axes for a movement direction
                // Project movement direction so the movement stays parallel to the ground
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
        ///     Gather component references and prep the input struct.
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
        /// <returns>If not grounded or slope is too steep.</returns>
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
        /// <param name="collision">Collision the controller collided with.</param>
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
                    // Calculate the angle between hit normal and character
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
                        // Calculate the angle between hit normal and character
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
                        // Calculate the angle between hit normal and character and add it to the total angle count
                        angle += Vector3.Angle(-myTransform.up, contactPoint.normal);
                    }

                    // If the average angle is smaller than the ceiling angle limit, register ceiling hit
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
            timeSinceLastJump = 0.0f;
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
        ///     If using ceiling detection, remove vertical momentum if we hit a ceiling.
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