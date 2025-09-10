using AdventureCharacterController.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(Mover))]
    public class MoverInspector : UnityEditor.Editor
    {
        #region Private Fields

        private float _previewPointSize = 3.0f;

        private SerializedProperty _stepHeightRatioProp;
        private SerializedProperty _colliderHeightProp;
        private SerializedProperty _colliderThicknessProp;
        private SerializedProperty _colliderOffsetProp;
        private SerializedProperty _sensorRadiusModifierProp;
        private SerializedProperty _sensorTypeProp;
        private SerializedProperty _sensorArrayRowsProp;
        private SerializedProperty _sensorArrayRayCountProp;
        private SerializedProperty _sensorArrayRowsAreOffsetProp;
        private SerializedProperty _isSensorInDebugProp;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            _stepHeightRatioProp = serializedObject.FindProperty("stepHeightRatio");
            _colliderHeightProp = serializedObject.FindProperty("colliderHeight");
            _colliderThicknessProp = serializedObject.FindProperty("colliderThickness");
            _colliderOffsetProp = serializedObject.FindProperty("colliderOffset");
            _sensorRadiusModifierProp = serializedObject.FindProperty("sensorRadiusModifier");
            _sensorTypeProp = serializedObject.FindProperty("sensorType");
            _sensorArrayRowsProp = serializedObject.FindProperty("sensorArrayRows");
            _sensorArrayRayCountProp = serializedObject.FindProperty("sensorArrayRayCount");
            _sensorArrayRowsAreOffsetProp = serializedObject.FindProperty("sensorArrayRowsAreOffset");
            _isSensorInDebugProp = serializedObject.FindProperty("isSensorInDebug");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Mover Settings", EditorStyles.boldLabel);
            _stepHeightRatioProp.floatValue = EditorGUILayout.Slider("Step Height", _stepHeightRatioProp.floatValue, 0.0f, 1.0f);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_colliderHeightProp, new GUIContent("Collider Height"));
            EditorGUILayout.PropertyField(_colliderThicknessProp, new GUIContent("Collider Thickness"));
            EditorGUILayout.PropertyField(_colliderOffsetProp, new GUIContent("Collider Offset"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sensor Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_sensorRadiusModifierProp,
                new GUIContent("Sensor Radius Modifier",
                    "An additional modifier applied to the sensor radius without needing to actually increase the thickness of the collider."));
            EditorGUILayout.PropertyField(_sensorTypeProp, new GUIContent("Sensor Type"));
            if (_sensorTypeProp.enumValueFlag == (int)CastType.RaycastArray)
            {
                EditorGUILayout.PropertyField(_sensorArrayRowsProp, new GUIContent("Sensor Array Rows"));
                EditorGUILayout.PropertyField(_sensorArrayRayCountProp, new GUIContent("Sensor Array Ray Count"));
                EditorGUILayout.PropertyField(_sensorArrayRowsAreOffsetProp, new GUIContent("Offset Sensor Array Rows"));
                _previewPointSize = EditorGUILayout.FloatField(new GUIContent("Preview Point Size"), _previewPointSize);
            }

            EditorGUILayout.PropertyField(_isSensorInDebugProp, new GUIContent("Sensor Debug Mode"));

            EditorGUILayout.Space();

            if (_sensorTypeProp.enumValueFlag == (int)CastType.RaycastArray)
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

            var previewPositions = Sensor.GetRaycastStartPositions(_sensorArrayRowsProp.intValue, _sensorArrayRayCountProp.intValue,
                _sensorArrayRowsAreOffsetProp.boolValue, 1.0f);

            var center = new Vector2(background.x + background.width / 2.0f, background.y + background.height / 2.0f);

            if (previewPositions != null && previewPositions.Length != 0)
            {
                for (var i = 0; i < previewPositions.Length; i++)
                {
                    var position = center + new Vector2(previewPositions[i].x, previewPositions[i].z) * background.width / 2.0f *
                        0.9f;

                    EditorGUI.DrawRect(
                        new Rect(position.x - _previewPointSize / 2.0f, position.y - _previewPointSize / 2.0f, _previewPointSize,
                            _previewPointSize), Color.white);
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