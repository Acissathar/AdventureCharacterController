using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(Runtime.Core.AdventureCharacterController))]
    public class AdventureCharacterControllerInspector : UnityEditor.Editor
    {
        #region Private Fields

        // Local editor variables
        private Runtime.Core.AdventureCharacterController _controller;
        private GUIStyle _boldFoldoutStyle;
        private bool _showGroundSettings;
        private bool _showAirSettings;
        private bool _showAutoJumpSettings;
        private bool _showCeilingDetectionSettings;
        private bool _showWallCollisionSettings;
        private bool _showCrouchSettings;
        private bool _showClimbSettings;
        private bool _showRollSettings;
        private bool _showDebugInfo;

        // Ground settings
        private SerializedProperty _movementSpeedProp;
        private SerializedProperty _groundFrictionProp;
        private SerializedProperty _useLocalMomentumProp;
        private SerializedProperty _slideGravityProp;
        private SerializedProperty _slopeLimitProp;

        // Air settings
        private SerializedProperty _airControlRateProp;
        private SerializedProperty _airControlMultiplierProp;
        private SerializedProperty _gravityProp;
        private SerializedProperty _verticalThresholdProp;
        private SerializedProperty _airFrictionProp;

        // Auto jump settings
        private SerializedProperty _useAutoJumpProp;
        private SerializedProperty _jumpSpeedProp;
        private SerializedProperty _autoJumpMovementSpeedThresholdProp;
        private SerializedProperty _autoJumpCooldownProp;

        // Ceiling detection settings
        private SerializedProperty _useCeilingDetectionProp;
        private SerializedProperty _ceilingAngleLimitProp;
        private SerializedProperty _ceilingDetectionMethodProp;

        // Wall collision settings
        private SerializedProperty _bounceOffWallCollisionsProp;

        // Crouch settings
        private SerializedProperty _crouchSpeedProp;
        private SerializedProperty _crouchColliderHeightProp;
        private SerializedProperty _crouchStepHeightRatioProp;

        // Climb settings
        private SerializedProperty _climbMovementSpeedProp;
        private SerializedProperty _climbUseThresholdProp;
        private SerializedProperty _climbAttachSpeedProp;
        private SerializedProperty _climbMoveThresholdProp;

        // Roll settings
        private SerializedProperty _rollSpeedMultiplierProp;
        private SerializedProperty _rollDurationProp;
        private SerializedProperty _rollCrashDurationProp;

        // Debug info
        private SerializedProperty _currentControllerStateProp;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            _controller = (Runtime.Core.AdventureCharacterController)target;

            // Ground settings
            _movementSpeedProp = serializedObject.FindProperty("movementSpeed");
            _groundFrictionProp = serializedObject.FindProperty("groundFriction");
            _useLocalMomentumProp = serializedObject.FindProperty("useLocalMomentum");
            _slideGravityProp = serializedObject.FindProperty("slideGravity");
            _slopeLimitProp = serializedObject.FindProperty("slopeLimit");

            // Air settings
            _airControlRateProp = serializedObject.FindProperty("airControlRate");
            _airControlMultiplierProp = serializedObject.FindProperty("airControlMultiplier");
            _gravityProp = serializedObject.FindProperty("gravity");
            _verticalThresholdProp = serializedObject.FindProperty("verticalThreshold");
            _airFrictionProp = serializedObject.FindProperty("airFriction");

            // Auto jump settings
            _useAutoJumpProp = serializedObject.FindProperty("useAutoJump");
            _jumpSpeedProp = serializedObject.FindProperty("jumpSpeed");
            _autoJumpMovementSpeedThresholdProp = serializedObject.FindProperty("autoJumpMovementSpeedThreshold");
            _autoJumpCooldownProp = serializedObject.FindProperty("autoJumpCooldown");

            // Ceiling detection settings
            _useCeilingDetectionProp = serializedObject.FindProperty("useCeilingDetection");
            _ceilingAngleLimitProp = serializedObject.FindProperty("ceilingAngleLimit");
            _ceilingDetectionMethodProp = serializedObject.FindProperty("ceilingDetectionMethod");

            // Wall collision settings
            _bounceOffWallCollisionsProp = serializedObject.FindProperty("bounceOffWallCollisions");

            // Crouch settings
            _crouchSpeedProp = serializedObject.FindProperty("crouchSpeed");
            _crouchColliderHeightProp = serializedObject.FindProperty("crouchColliderHeight");
            _crouchStepHeightRatioProp = serializedObject.FindProperty("crouchStepHeightRatio");

            // Climb settings
            _climbMovementSpeedProp = serializedObject.FindProperty("climbMovementSpeed");
            _climbUseThresholdProp = serializedObject.FindProperty("climbUseThreshold");
            _climbAttachSpeedProp = serializedObject.FindProperty("climbAttachSpeed");
            _climbMoveThresholdProp = serializedObject.FindProperty("climbMoveThreshold");

            // Roll settings
            _rollSpeedMultiplierProp = serializedObject.FindProperty("rollSpeedMultiplier");
            _rollDurationProp = serializedObject.FindProperty("rollDuration");
            _rollCrashDurationProp = serializedObject.FindProperty("rollCrashDuration");

            // Debug info
            _currentControllerStateProp = serializedObject.FindProperty("currentControllerState");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI-based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Expand All"))
            {
                _showGroundSettings = true;
                _showAirSettings = true;
                _showAutoJumpSettings = true;
                _showCeilingDetectionSettings = true;
                _showWallCollisionSettings = true;
                _showCrouchSettings = true;
                _showClimbSettings = true;
                _showRollSettings = true;
                _showDebugInfo = true;
            }

            if (GUILayout.Button("Collapse All"))
            {
                _showGroundSettings = false;
                _showAirSettings = false;
                _showAutoJumpSettings = false;
                _showCeilingDetectionSettings = false;
                _showWallCollisionSettings = false;
                _showCrouchSettings = false;
                _showClimbSettings = false;
                _showRollSettings = false;
                _showDebugInfo = false;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            _showGroundSettings = EditorGUILayout.Foldout(_showGroundSettings, "Ground Settings", _boldFoldoutStyle);
            if (_showGroundSettings)
            {
                EditorGUILayout.PropertyField(_movementSpeedProp, new GUIContent("Movement Speed"));
                EditorGUILayout.PropertyField(_groundFrictionProp, new GUIContent("Ground Friction"));
                EditorGUILayout.PropertyField(_useLocalMomentumProp,
                    new GUIContent("Use Local Momentum",
                        "Whether to calculate and apply momentum relative to the controller's transform."));
                EditorGUILayout.PropertyField(_slideGravityProp, new GUIContent("Slide Gravity"));
                _slopeLimitProp.floatValue =
                    EditorGUILayout.Slider("Slope Limit", _slopeLimitProp.floatValue, 0.0f, 90.0f);
                EditorGUILayout.Space();
            }

            _showAirSettings = EditorGUILayout.Foldout(_showAirSettings, "Air Settings", _boldFoldoutStyle);
            if (_showAirSettings)
            {
                EditorGUILayout.PropertyField(_airControlRateProp, new GUIContent("Air Control"));
                EditorGUILayout.PropertyField(_airControlMultiplierProp,
                    new GUIContent("Air Control Multiplier",
                        "This multiplier is applied to air control when outside momentum is applied to the controller, causing it to exceed Movement Speed."));
                EditorGUILayout.PropertyField(_gravityProp, new GUIContent("Gravity"));
                EditorGUILayout.PropertyField(_verticalThresholdProp,
                    new GUIContent("Vertical Threshold",
                        "Small threshold applied to decide if the controller is in the act of rising or falling compared to being grounded."));
                EditorGUILayout.PropertyField(_airFrictionProp, new GUIContent("Air Friction"));
                EditorGUILayout.Space();
            }

            _showAutoJumpSettings = EditorGUILayout.Foldout(_showAutoJumpSettings, "Auto Jump Settings", _boldFoldoutStyle);
            if (_showAutoJumpSettings)
            {
                EditorGUILayout.PropertyField(_useAutoJumpProp, new GUIContent("Use Auto Jump"));
                if (_useAutoJumpProp.boolValue)
                {
                    EditorGUILayout.PropertyField(_jumpSpeedProp, new GUIContent("Jump Speed"));
                    EditorGUILayout.PropertyField(_autoJumpMovementSpeedThresholdProp, new GUIContent(
                        "Auto Jump Movement Speed Threshold",
                        "If the controller's movement speed is at or above this value, the controller will automatically jump when leaving the Grounded state."));
                    EditorGUILayout.PropertyField(_autoJumpCooldownProp, new GUIContent("Auto Jump Cooldown",
                        "How long the controller must remain grounded before being able to auto jump again."));
                }

                EditorGUILayout.Space();
            }

            _showCeilingDetectionSettings =
                EditorGUILayout.Foldout(_showCeilingDetectionSettings, "Ceiling Detection Settings", _boldFoldoutStyle);
            if (_showCeilingDetectionSettings)
            {
                EditorGUILayout.PropertyField(_useCeilingDetectionProp, new GUIContent("Use Ceiling Detection"));
                if (_useCeilingDetectionProp.boolValue)
                {
                    _ceilingAngleLimitProp.floatValue =
                        EditorGUILayout.Slider("Ceiling Angle Limit", _ceilingAngleLimitProp.floatValue, 0.0f, 90.0f);

                    if (_ceilingDetectionMethodProp.enumValueIndex == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "Only check the very first collision contact. This option is slightly faster but less accurate than the other two options.",
                            MessageType.Info);
                    }
                    else if (_ceilingDetectionMethodProp.enumValueIndex == 1)
                    {
                        EditorGUILayout.HelpBox(
                            "Check all contact points and register a ceiling hit as long as just one contact qualifies.",
                            MessageType.Info);
                    }
                    else if (_ceilingDetectionMethodProp.enumValueIndex == 2)
                    {
                        EditorGUILayout.HelpBox("Calculate an average surface normal to check against.",
                            MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Unsupported Ceiling Detection Method.",
                            MessageType.Warning);
                    }

                    EditorGUILayout.PropertyField(_ceilingDetectionMethodProp, new GUIContent("Ceiling Detection Method"));
                }

                EditorGUILayout.Space();
            }

            _showWallCollisionSettings =
                EditorGUILayout.Foldout(_showWallCollisionSettings, "Wall Collision Settings", _boldFoldoutStyle);
            if (_showWallCollisionSettings)
            {
                EditorGUILayout.PropertyField(_bounceOffWallCollisionsProp, new GUIContent("Bounce Off Walls",
                    "Colliding with a wall will provide an opposite force from the normal to simulate Newton's 3rd Law.\n" +
                    "Note that this only really makes a difference when no input is supplied, as the input momentum will usually override this opposite force."));
                EditorGUILayout.Space();
            }

            _showCrouchSettings = EditorGUILayout.Foldout(_showCrouchSettings, "Crouch Settings", _boldFoldoutStyle);
            if (_showCrouchSettings)
            {
                EditorGUILayout.PropertyField(_crouchSpeedProp, new GUIContent("Crouch Speed"));
                EditorGUILayout.PropertyField(_crouchColliderHeightProp, new GUIContent("Crouch Collider Height"));
                EditorGUILayout.PropertyField(_crouchStepHeightRatioProp, new GUIContent("Crouch Step Height Ratio"));
                EditorGUILayout.Space();
            }

            _showClimbSettings = EditorGUILayout.Foldout(_showClimbSettings, "Climb Settings", _boldFoldoutStyle);
            if (_showClimbSettings)
            {
                EditorGUILayout.PropertyField(_climbMovementSpeedProp, new GUIContent("Climb Movement Speed"));
                EditorGUILayout.PropertyField(_climbUseThresholdProp, new GUIContent("Climb Use Threshold",
                    "Threshold to use for Dot product comparison from Movement Velocity and the trigger of a climbable area.\n" +
                    "No threshold means the Movement Velocity must exactly match the direction of the climbable area's forward, so it's recommended to have a small threshold (~0.15) to allow for a slight angle difference."));
                EditorGUILayout.PropertyField(_climbAttachSpeedProp,
                    new GUIContent("Climb Attach Speed",
                        "Multiplier for how fast the rigidbody should move towards the climb start point when attaching to the climbable surface."));
                EditorGUILayout.PropertyField(_climbMoveThresholdProp,
                    new GUIContent("Climb Move Threshold",
                        "Maximum speed at which point we consider the rigidbody no longer moving. This is used to determine when we have finished moving to the desired climbable area start point, account for potential physics collisions stopping us from reaching the point exactly."));
                EditorGUILayout.Space();
            }

            _showRollSettings = EditorGUILayout.Foldout(_showRollSettings, "Roll Settings", _boldFoldoutStyle);
            if (_showRollSettings)
            {
                EditorGUILayout.PropertyField(_rollSpeedMultiplierProp, new GUIContent("Roll Speed Multiplier",
                    "Multiplier that is applied to MovementVelocity when rolling."));
                EditorGUILayout.PropertyField(_rollDurationProp, new GUIContent("Roll Duration",
                    "How long the controller will roll when initiated."));
                EditorGUILayout.PropertyField(_rollCrashDurationProp, new GUIContent("Roll Crash Duration",
                    "How long the controller will remain in place (simulating a stun effect) when it crashes into something during a roll."));
                EditorGUILayout.Space();
            }

            _showDebugInfo = EditorGUILayout.Foldout(_showDebugInfo, "Debug Info", _boldFoldoutStyle);
            if (_showDebugInfo)
            {
                EditorGUILayout.LabelField("Current Controller State: " +
                                           _currentControllerStateProp.enumNames[_currentControllerStateProp.enumValueIndex]);
                EditorGUILayout.LabelField($"Velocity: {_controller.Velocity} - {_controller.Velocity.magnitude} m/s");
                EditorGUILayout.LabelField($"Momentum: {_controller.Momentum} - {_controller.Momentum.magnitude} m/s");
                EditorGUILayout.LabelField(
                    $"Movement Velocity: {_controller.MovementVelocity} - {_controller.MovementVelocity.magnitude} m/s");
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}