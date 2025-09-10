using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(Runtime.Core.AdventureCharacterController))]
    public class AdventureCharacterControllerInspector : UnityEditor.Editor
    {
        #region Private Fields

        // Local editor variables
        private Runtime.Core.AdventureCharacterController controller;
        private GUIStyle boldFoldoutStyle;
        private bool showGroundSettings;
        private bool showAirSettings;
        private bool showAutoJumpSettings;
        private bool showCeilingDetectionSettings;
        private bool showWallCollisionSettings;
        private bool showCrouchSettings;
        private bool showClimbSettings;
        private bool showRollSettings;
        private bool showDebugInfo;

        // Ground settings
        private SerializedProperty movementSpeedProp;
        private SerializedProperty groundFrictionProp;
        private SerializedProperty useLocalMomentumProp;
        private SerializedProperty slideGravityProp;
        private SerializedProperty slopeLimitProp;

        // Air settings
        private SerializedProperty airControlRateProp;
        private SerializedProperty airControlMultiplierProp;
        private SerializedProperty gravityProp;
        private SerializedProperty verticalThresholdProp;
        private SerializedProperty airFrictionProp;

        // Auto jump settings
        private SerializedProperty useAutoJumpProp;
        private SerializedProperty jumpSpeedProp;
        private SerializedProperty autoJumpMovementSpeedThresholdProp;
        private SerializedProperty autoJumpCooldownProp;

        // Ceiling detection settings
        private SerializedProperty useCeilingDetectionProp;
        private SerializedProperty ceilingAngleLimitProp;
        private SerializedProperty ceilingDetectionMethodProp;

        // Wall collision settings
        private SerializedProperty bounceOffWallCollisionsProp;

        // Crouch settings
        private SerializedProperty crouchSpeedProp;
        private SerializedProperty crouchColliderHeightProp;
        private SerializedProperty crouchStepHeightRatioProp;

        // Climb settings
        private SerializedProperty climbMovementSpeedProp;
        private SerializedProperty climbUseThresholdProp;
        private SerializedProperty climbAttachSpeedProp;
        private SerializedProperty climbMoveThresholdProp;

        // Roll settings
        private SerializedProperty rollSpeedMultiplierProp;
        private SerializedProperty rollDurationProp;
        private SerializedProperty rollCrashDurationProp;

        // Debug info
        private SerializedProperty currentControllerStateProp;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            controller = (Runtime.Core.AdventureCharacterController)target;

            // Ground settings
            movementSpeedProp = serializedObject.FindProperty("movementSpeed");
            groundFrictionProp = serializedObject.FindProperty("groundFriction");
            useLocalMomentumProp = serializedObject.FindProperty("useLocalMomentum");
            slideGravityProp = serializedObject.FindProperty("slideGravity");
            slopeLimitProp = serializedObject.FindProperty("slopeLimit");

            // Air settings
            airControlRateProp = serializedObject.FindProperty("airControlRate");
            airControlMultiplierProp = serializedObject.FindProperty("airControlMultiplier");
            gravityProp = serializedObject.FindProperty("gravity");
            verticalThresholdProp = serializedObject.FindProperty("verticalThreshold");
            airFrictionProp = serializedObject.FindProperty("airFriction");

            // Auto jump settings
            useAutoJumpProp = serializedObject.FindProperty("useAutoJump");
            jumpSpeedProp = serializedObject.FindProperty("jumpSpeed");
            autoJumpMovementSpeedThresholdProp = serializedObject.FindProperty("autoJumpMovementSpeedThreshold");
            autoJumpCooldownProp = serializedObject.FindProperty("autoJumpCooldown");

            // Ceiling detection settings
            useCeilingDetectionProp = serializedObject.FindProperty("useCeilingDetection");
            ceilingAngleLimitProp = serializedObject.FindProperty("ceilingAngleLimit");
            ceilingDetectionMethodProp = serializedObject.FindProperty("ceilingDetectionMethod");

            // Wall collision settings
            bounceOffWallCollisionsProp = serializedObject.FindProperty("bounceOffWallCollisions");

            // Crouch settings
            crouchSpeedProp = serializedObject.FindProperty("crouchSpeed");
            crouchColliderHeightProp = serializedObject.FindProperty("crouchColliderHeight");
            crouchStepHeightRatioProp = serializedObject.FindProperty("crouchStepHeightRatio");

            // Climb settings
            climbMovementSpeedProp = serializedObject.FindProperty("climbMovementSpeed");
            climbUseThresholdProp = serializedObject.FindProperty("climbUseThreshold");
            climbAttachSpeedProp = serializedObject.FindProperty("climbAttachSpeed");
            climbMoveThresholdProp = serializedObject.FindProperty("climbMoveThreshold");

            // Roll settings
            rollSpeedMultiplierProp = serializedObject.FindProperty("rollSpeedMultiplier");
            rollDurationProp = serializedObject.FindProperty("rollDuration");
            rollCrashDurationProp = serializedObject.FindProperty("rollCrashDuration");

            // Debug info
            currentControllerStateProp = serializedObject.FindProperty("currentControllerState");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI-based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Expand All"))
            {
                showGroundSettings = true;
                showAirSettings = true;
                showAutoJumpSettings = true;
                showCeilingDetectionSettings = true;
                showWallCollisionSettings = true;
                showCrouchSettings = true;
                showClimbSettings = true;
                showRollSettings = true;
                showDebugInfo = true;
            }

            if (GUILayout.Button("Collapse All"))
            {
                showGroundSettings = false;
                showAirSettings = false;
                showAutoJumpSettings = false;
                showCeilingDetectionSettings = false;
                showWallCollisionSettings = false;
                showCrouchSettings = false;
                showClimbSettings = false;
                showRollSettings = false;
                showDebugInfo = false;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            showGroundSettings = EditorGUILayout.Foldout(showGroundSettings, "Ground Settings", boldFoldoutStyle);
            if (showGroundSettings)
            {
                EditorGUILayout.PropertyField(movementSpeedProp, new GUIContent("Movement Speed"));
                EditorGUILayout.PropertyField(groundFrictionProp, new GUIContent("Ground Friction"));
                EditorGUILayout.PropertyField(useLocalMomentumProp,
                    new GUIContent("Use Local Momentum",
                        "Whether to calculate and apply momentum relative to the controller's transform."));
                EditorGUILayout.PropertyField(slideGravityProp, new GUIContent("Slide Gravity"));
                slopeLimitProp.floatValue =
                    EditorGUILayout.Slider("Slope Limit", slopeLimitProp.floatValue, 0.0f, 90.0f);
                EditorGUILayout.Space();
            }

            showAirSettings = EditorGUILayout.Foldout(showAirSettings, "Air Settings", boldFoldoutStyle);
            if (showAirSettings)
            {
                EditorGUILayout.PropertyField(airControlRateProp, new GUIContent("Air Control"));
                EditorGUILayout.PropertyField(airControlMultiplierProp,
                    new GUIContent("Air Control Multiplier",
                        "This multiplier is applied to air control when outside momentum is applied to the controller, causing it to exceed Movement Speed."));
                EditorGUILayout.PropertyField(gravityProp, new GUIContent("Gravity"));
                EditorGUILayout.PropertyField(verticalThresholdProp,
                    new GUIContent("Vertical Threshold",
                        "Small threshold applied to decide if the controller is in the act of rising or falling compared to being grounded."));
                EditorGUILayout.PropertyField(airFrictionProp, new GUIContent("Air Friction"));
                EditorGUILayout.Space();
            }

            showAutoJumpSettings = EditorGUILayout.Foldout(showAutoJumpSettings, "Auto Jump Settings", boldFoldoutStyle);
            if (showAutoJumpSettings)
            {
                EditorGUILayout.PropertyField(useAutoJumpProp, new GUIContent("Use Auto Jump"));
                if (useAutoJumpProp.boolValue)
                {
                    EditorGUILayout.PropertyField(jumpSpeedProp, new GUIContent("Jump Speed"));
                    EditorGUILayout.PropertyField(autoJumpMovementSpeedThresholdProp, new GUIContent(
                        "Auto Jump Movement Speed Threshold",
                        "If the controller's movement speed is at or above this value, the controller will automatically jump when leaving the Grounded state."));
                    EditorGUILayout.PropertyField(autoJumpCooldownProp, new GUIContent("Auto Jump Cooldown",
                        "How long the controller must remain grounded before being able to auto jump again."));
                }

                EditorGUILayout.Space();
            }

            showCeilingDetectionSettings =
                EditorGUILayout.Foldout(showCeilingDetectionSettings, "Ceiling Detection Settings", boldFoldoutStyle);
            if (showCeilingDetectionSettings)
            {
                EditorGUILayout.PropertyField(useCeilingDetectionProp, new GUIContent("Use Ceiling Detection"));
                if (useCeilingDetectionProp.boolValue)
                {
                    ceilingAngleLimitProp.floatValue =
                        EditorGUILayout.Slider("Ceiling Angle Limit", ceilingAngleLimitProp.floatValue, 0.0f, 90.0f);

                    if (ceilingDetectionMethodProp.enumValueIndex == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "Only check the very first collision contact. This option is slightly faster but less accurate than the other two options.",
                            MessageType.Info);
                    }
                    else if (ceilingDetectionMethodProp.enumValueIndex == 1)
                    {
                        EditorGUILayout.HelpBox(
                            "Check all contact points and register a ceiling hit as long as just one contact qualifies.",
                            MessageType.Info);
                    }
                    else if (ceilingDetectionMethodProp.enumValueIndex == 2)
                    {
                        EditorGUILayout.HelpBox("Calculate an average surface normal to check against.",
                            MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Unsupported Ceiling Detection Method.",
                            MessageType.Warning);
                    }

                    EditorGUILayout.PropertyField(ceilingDetectionMethodProp, new GUIContent("Ceiling Detection Method"));
                }

                EditorGUILayout.Space();
            }

            showWallCollisionSettings =
                EditorGUILayout.Foldout(showWallCollisionSettings, "Wall Collision Settings", boldFoldoutStyle);
            if (showWallCollisionSettings)
            {
                EditorGUILayout.PropertyField(bounceOffWallCollisionsProp, new GUIContent("Bounce Off Walls",
                    "Colliding with a wall will provide an opposite force from the normal to simulate Newton's 3rd Law.\n" +
                    "Note that this only really makes a difference when no input is supplied, as the input momentum will usually override this opposite force."));
                EditorGUILayout.Space();
            }

            showCrouchSettings = EditorGUILayout.Foldout(showCrouchSettings, "Crouch Settings", boldFoldoutStyle);
            if (showCrouchSettings)
            {
                EditorGUILayout.PropertyField(crouchSpeedProp, new GUIContent("Crouch Speed"));
                EditorGUILayout.PropertyField(crouchColliderHeightProp, new GUIContent("Crouch Collider Height"));
                EditorGUILayout.PropertyField(crouchStepHeightRatioProp, new GUIContent("Crouch Step Height Ratio"));
                EditorGUILayout.Space();
            }

            showClimbSettings = EditorGUILayout.Foldout(showClimbSettings, "Climb Settings", boldFoldoutStyle);
            if (showClimbSettings)
            {
                EditorGUILayout.PropertyField(climbMovementSpeedProp, new GUIContent("Climb Movement Speed"));
                EditorGUILayout.PropertyField(climbUseThresholdProp, new GUIContent("Climb Use Threshold",
                    "Threshold to use for Dot product comparison from Movement Velocity and the trigger of a climbable area.\n" +
                    "No threshold means the Movement Velocity must exactly match the direction of the climbable area's forward, so it's recommended to have a small threshold (~0.15) to allow for a slight angle difference."));
                EditorGUILayout.PropertyField(climbAttachSpeedProp,
                    new GUIContent("Climb Attach Speed",
                        "Multiplier for how fast the rigidbody should move towards the climb start point when attaching to the climbable surface."));
                EditorGUILayout.PropertyField(climbMoveThresholdProp,
                    new GUIContent("Climb Move Threshold",
                        "Maximum speed at which point we consider the rigidbody no longer moving. This is used to determine when we have finished moving to the desired climbable area start point, account for potential physics collisions stopping us from reaching the point exactly."));
                EditorGUILayout.Space();
            }

            showRollSettings = EditorGUILayout.Foldout(showRollSettings, "Roll Settings", boldFoldoutStyle);
            if (showRollSettings)
            {
                EditorGUILayout.PropertyField(rollSpeedMultiplierProp, new GUIContent("Roll Speed Multiplier",
                    "Multiplier that is applied to MovementVelocity when rolling."));
                EditorGUILayout.PropertyField(rollDurationProp, new GUIContent("Roll Duration",
                    "How long the controller will roll when initiated."));
                EditorGUILayout.PropertyField(rollCrashDurationProp, new GUIContent("Roll Crash Duration",
                    "How long the controller will remain in place (simulating a stun effect) when it crashes into something during a roll."));
                EditorGUILayout.Space();
            }

            showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Debug Info", boldFoldoutStyle);
            if (showDebugInfo)
            {
                EditorGUILayout.LabelField("Current Controller State: " +
                                           currentControllerStateProp.enumNames[currentControllerStateProp.enumValueIndex]);
                EditorGUILayout.LabelField($"Velocity: {controller.Velocity} - {controller.Velocity.magnitude} m/s");
                EditorGUILayout.LabelField($"Momentum: {controller.Momentum} - {controller.Momentum.magnitude} m/s");
                EditorGUILayout.LabelField(
                    $"Movement Velocity: {controller.MovementVelocity} - {controller.MovementVelocity.magnitude} m/s");
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}