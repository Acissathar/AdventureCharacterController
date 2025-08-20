using AdventureCharacterController.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(Mover))]
    public class MoverInspector : UnityEditor.Editor
    {
        #region Private Fields

        private float previewPointSize = 3.0f;

        private SerializedProperty stepHeightRatioProp;
        private SerializedProperty colliderHeightProp;
        private SerializedProperty colliderThicknessProp;
        private SerializedProperty colliderOffsetProp;
        private SerializedProperty sensorRadiusModifierProp;
        private SerializedProperty sensorTypeProp;
        private SerializedProperty sensorArrayRowsProp;
        private SerializedProperty sensorArrayRayCountProp;
        private SerializedProperty sensorArrayRowsAreOffsetProp;
        private SerializedProperty isSensorInDebugProp;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            stepHeightRatioProp = serializedObject.FindProperty("stepHeightRatio");
            colliderHeightProp = serializedObject.FindProperty("colliderHeight");
            colliderThicknessProp = serializedObject.FindProperty("colliderThickness");
            colliderOffsetProp = serializedObject.FindProperty("colliderOffset");
            sensorRadiusModifierProp = serializedObject.FindProperty("sensorRadiusModifier");
            sensorTypeProp = serializedObject.FindProperty("sensorType");
            sensorArrayRowsProp = serializedObject.FindProperty("sensorArrayRows");
            sensorArrayRayCountProp = serializedObject.FindProperty("sensorArrayRayCount");
            sensorArrayRowsAreOffsetProp = serializedObject.FindProperty("sensorArrayRowsAreOffset");
            isSensorInDebugProp = serializedObject.FindProperty("isSensorInDebug");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Mover Settings", EditorStyles.boldLabel);
            stepHeightRatioProp.floatValue = EditorGUILayout.Slider("Step Height", stepHeightRatioProp.floatValue, 0.0f, 1.0f);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(colliderHeightProp, new GUIContent("Collider Height"));
            EditorGUILayout.PropertyField(colliderThicknessProp, new GUIContent("Collider Thickness"));
            EditorGUILayout.PropertyField(colliderOffsetProp, new GUIContent("Collider Offset"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sensor Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sensorRadiusModifierProp,
                new GUIContent("Sensor Radius Modifier",
                    "An additional modifier applied to the sensor radius without needing to actually increase the thickness of the collider."));
            EditorGUILayout.PropertyField(sensorTypeProp, new GUIContent("Sensor Type"));
            if (sensorTypeProp.enumValueFlag == (int)CastType.RaycastArray)
            {
                EditorGUILayout.PropertyField(sensorArrayRowsProp, new GUIContent("Sensor Array Rows"));
                EditorGUILayout.PropertyField(sensorArrayRayCountProp, new GUIContent("Sensor Array Ray Count"));
                EditorGUILayout.PropertyField(sensorArrayRowsAreOffsetProp, new GUIContent("Offset Sensor Array Rows"));
                previewPointSize = EditorGUILayout.FloatField(new GUIContent("Preview Point Size"), previewPointSize);
            }

            EditorGUILayout.PropertyField(isSensorInDebugProp, new GUIContent("Sensor Debug Mode"));

            EditorGUILayout.Space();

            if (sensorTypeProp.enumValueFlag == (int)CastType.RaycastArray)
            {
                DrawRaycastArrayPreview();
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Draws a preview of the raycast array in the inspector of the Mover component to give a rough idea of how it will be
        ///     fired.
        /// </summary>
        private void DrawRaycastArrayPreview()
        {
            GUILayout.Space(5);
            var space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100));

            var background = new Rect(space.x + (space.width - space.height) / 2.0f, space.y, space.height, space.height);
            EditorGUI.DrawRect(background, Color.grey);

            var previewPositions = Sensor.GetRaycastStartPositions(sensorArrayRowsProp.intValue, sensorArrayRayCountProp.intValue,
                sensorArrayRowsAreOffsetProp.boolValue, 1.0f);

            var center = new Vector2(background.x + background.width / 2.0f, background.y + background.height / 2.0f);

            if (previewPositions != null && previewPositions.Length != 0)
            {
                for (var i = 0; i < previewPositions.Length; i++)
                {
                    var position = center + new Vector2(previewPositions[i].x, previewPositions[i].z) * background.width / 2.0f *
                        0.9f;

                    EditorGUI.DrawRect(
                        new Rect(position.x - previewPointSize / 2.0f, position.y - previewPointSize / 2.0f, previewPointSize,
                            previewPointSize), Color.white);
                }
            }

            if (previewPositions != null && previewPositions.Length != 0)
            {
                GUILayout.Label("Number of rays = " + previewPositions.Length, EditorStyles.centeredGreyMiniLabel);
            }
        }

        #endregion
    }
}