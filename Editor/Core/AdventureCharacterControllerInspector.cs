using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(Runtime.Core.AdventureCharacterController))]
    public class AdventureCharacterControllerInspector : UnityEditor.Editor
    {
        #region Private Fields

        private SerializedProperty movementSpeedProp;
        private SerializedProperty groundFrictionProp;
        private SerializedProperty useLocalMomentumProp;
        private SerializedProperty slideGravityProp;
        private SerializedProperty slopeLimitProp;
        private SerializedProperty airControlRateProp;
        private SerializedProperty airControlMultiplierProp;
        private SerializedProperty gravityProp;
        private SerializedProperty useAutoJumpProp;
        private SerializedProperty jumpSpeedProp;
        private SerializedProperty autoJumpMovementSpeedThresholdProp;
        private SerializedProperty autoJumpCooldownProp;
        private SerializedProperty verticalThresholdProp;
        private SerializedProperty airFrictionProp;
        private SerializedProperty useCeilingDetectionProp;
        private SerializedProperty ceilingAngleLimitProp;
        private SerializedProperty ceilingDetectionMethodProp;
        private SerializedProperty bounceOffWallCollisionsProp;
        private SerializedProperty currentControllerStateProp;
        
        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            movementSpeedProp = serializedObject.FindProperty("movementSpeed");
            groundFrictionProp = serializedObject.FindProperty("groundFriction");
            useLocalMomentumProp = serializedObject.FindProperty("useLocalMomentum");
            slideGravityProp = serializedObject.FindProperty("slideGravity");
            slopeLimitProp = serializedObject.FindProperty("slopeLimit");
            airControlRateProp = serializedObject.FindProperty("airControlRate");
            airControlMultiplierProp = serializedObject.FindProperty("airControlMultiplier");
            gravityProp = serializedObject.FindProperty("gravity");
            verticalThresholdProp = serializedObject.FindProperty("verticalThreshold");
            airFrictionProp = serializedObject.FindProperty("airFriction");
            useAutoJumpProp = serializedObject.FindProperty("useAutoJump");
            jumpSpeedProp = serializedObject.FindProperty("jumpSpeed");
            autoJumpMovementSpeedThresholdProp = serializedObject.FindProperty("autoJumpMovementSpeedThreshold");
            autoJumpCooldownProp = serializedObject.FindProperty("autoJumpCooldown");
            useCeilingDetectionProp = serializedObject.FindProperty("useCeilingDetection");
            ceilingAngleLimitProp = serializedObject.FindProperty("ceilingAngleLimit");
            ceilingDetectionMethodProp = serializedObject.FindProperty("ceilingDetectionMethod");
            bounceOffWallCollisionsProp = serializedObject.FindProperty("bounceOffWallCollisions");
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
                EditorGUILayout.PropertyField(autoJumpMovementSpeedThresholdProp, new GUIContent("Auto Jump Movement Speed Threshold",
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

            EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Controller State: " + currentControllerStateProp.enumNames[currentControllerStateProp.enumValueIndex], EditorStyles.boldLabel);
            
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}