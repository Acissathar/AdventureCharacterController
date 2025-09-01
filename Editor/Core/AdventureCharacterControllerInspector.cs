using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(Runtime.Core.AdventureCharacterController))]
    public class AdventureCharacterControllerInspector : UnityEditor.Editor
    {
        #region Private Fields

        private Runtime.Core.AdventureCharacterController controller;

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

        // Ladder settings
        private SerializedProperty ladderMovementSpeedProp;
        private SerializedProperty ladderUseThresholdProp;
        private SerializedProperty ladderAttachSpeedProp;
        private SerializedProperty ladderMoveThresholdProp;

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

            // Ladder settings
            ladderMovementSpeedProp = serializedObject.FindProperty("ladderMovementSpeed");
            ladderUseThresholdProp = serializedObject.FindProperty("ladderUseThreshold");
            ladderAttachSpeedProp = serializedObject.FindProperty("ladderAttachSpeed");
            ladderMoveThresholdProp = serializedObject.FindProperty("ladderMoveThreshold");

            // Roll settings
            rollSpeedMultiplierProp = serializedObject.FindProperty("rollSpeedMultiplier");
            rollDurationProp = serializedObject.FindProperty("rollDuration");
            rollCrashDurationProp = serializedObject.FindProperty("rollCrashDuration");

            // Debug info
            currentControllerStateProp = serializedObject.FindProperty("currentControllerState");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Ground Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(movementSpeedProp, new GUIContent("Movement Speed"));
            EditorGUILayout.PropertyField(groundFrictionProp, new GUIContent("Ground Friction"));
            EditorGUILayout.PropertyField(useLocalMomentumProp,
                new GUIContent("Use Local Momentum",
                    "Whether to calculate and apply momentum relative to the controller's transform."));
            EditorGUILayout.PropertyField(slideGravityProp, new GUIContent("Slide Gravity"));
            slopeLimitProp.floatValue =
                EditorGUILayout.Slider("Slope Limit", slopeLimitProp.floatValue, 0.0f, 90.0f);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Air Settings", EditorStyles.boldLabel);
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


            EditorGUILayout.LabelField("Optional Features", EditorStyles.boldLabel);
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
            EditorGUILayout.PropertyField(bounceOffWallCollisionsProp, new GUIContent("Bounce Off Walls",
                "Colliding with a wall will provide an opposite force from the normal to simulate Newton's 3rd Law.\n" +
                "Note that this only really makes a difference when no input is supplied, as the input momentum will usually override this opposite force."));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(crouchSpeedProp, new GUIContent("Crouch Speed"));
            EditorGUILayout.PropertyField(crouchColliderHeightProp, new GUIContent("Crouch Collider Height"));
            EditorGUILayout.PropertyField(crouchStepHeightRatioProp, new GUIContent("Crouch Step Height Ratio"));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ladderMovementSpeedProp, new GUIContent("Ladder Movement Speed"));
            EditorGUILayout.PropertyField(ladderUseThresholdProp, new GUIContent("Ladder Use Threshold",
                "Threshold to use for Dot product comparison from Movement Velocity and the trigger of a ladder.\n" +
                "No threshold means the Movement Velocity must exactly match the direction of the ladder's forward, so it's recommended to have a small threshold (~0.15) to allow for a slight angle difference."));
            EditorGUILayout.PropertyField(ladderAttachSpeedProp,
                new GUIContent("Ladder Attach Speed",
                    "Multiplier for how fast the rigidbody should move towards the ladder start point when attaching to the ladder."));
            EditorGUILayout.PropertyField(ladderMoveThresholdProp,
                new GUIContent("Ladder Move Threshold",
                    "Maximum speed at which point we consider the rigidbody no longer moving. This is used to determine when we have finished moving to the desired ladder start point, account for potential physics collisions stopping us from reaching the point exactly."));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(rollSpeedMultiplierProp, new GUIContent("Roll Speed Multiplier",
                "Multiplier that is applied to MovementVelocity when rolling."));
            EditorGUILayout.PropertyField(rollDurationProp, new GUIContent("Roll Duration",
                "How long the controller will roll when initiated."));
            EditorGUILayout.PropertyField(rollCrashDurationProp, new GUIContent("Roll Crash Duration",
                "How long the controller will remain in place (simulating a stun effect) when it crashes into something during a roll."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Controller State: " +
                                       currentControllerStateProp.enumNames[currentControllerStateProp.enumValueIndex]);
            EditorGUILayout.LabelField($"Velocity: {controller.Velocity} - {controller.Velocity.magnitude} m/s");
            EditorGUILayout.LabelField($"Momentum: {controller.Momentum} - {controller.Momentum.magnitude} m/s");
            EditorGUILayout.LabelField(
                $"Movement Velocity: {controller.MovementVelocity} - {controller.MovementVelocity.magnitude} m/s");
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}