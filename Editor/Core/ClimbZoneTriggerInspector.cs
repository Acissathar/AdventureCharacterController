using AdventureCharacterController.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Core
{
    [CustomEditor(typeof(ClimbZoneTrigger))]
    [CanEditMultipleObjects]
    public class ClimbZoneTriggerInspector : UnityEditor.Editor
    {
        #region Private Fields

        private ClimbZoneTrigger climbZoneTrigger;

        private SerializedProperty allowFreeClimbingProp;
        private SerializedProperty climbZoneStartOffsetPointProp;
        private SerializedProperty climbZoneEndOffsetPointProp;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            climbZoneTrigger = (ClimbZoneTrigger)target;

            allowFreeClimbingProp = serializedObject.FindProperty("allowFreeClimbing");
            climbZoneStartOffsetPointProp = serializedObject.FindProperty("climbZoneStartOffsetPoint");
            climbZoneEndOffsetPointProp = serializedObject.FindProperty("climbZoneEndOffsetPoint");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (climbZoneTrigger.transform.localRotation != Quaternion.identity)
            {
                EditorGUILayout.HelpBox(
                    "Climb Zone Trigger should have an all 0 rotation so that the Forward direction 'faces the wall' as we use this direction for attaching and leaving the climb zone.\n" +
                    "The parent object can be rotated any direction, as long as the 'blue axis' faces into the wall on the object containing this component.",
                    MessageType.Warning);
            }

            EditorGUILayout.PropertyField(allowFreeClimbingProp, new GUIContent("Allow Free Climbing"));
            EditorGUILayout.Space();

            if (allowFreeClimbingProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Free Climbing is not yet supported. This option will be added in a future update.",
                    MessageType.Info);

                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Start Offset Point should be set to the point where you want the controller to begin climbing, as the controller's position will be lerped to this position when 'entering' the ladder.\n" +
                    "End Offset Point should be set to the point where you want the controller to get off the ladder. Note that the capsule collider size fits the torso of the controller, so this should be slightly higher than the immediate end point of the ladder.",
                    MessageType.Info);

                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(climbZoneStartOffsetPointProp, new GUIContent("Start Offset Point"));
            EditorGUILayout.PropertyField(climbZoneEndOffsetPointProp, new GUIContent("End Offset Point"));
            
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///     Allows custom rendering and interaction with handles in the Scene view for the associated ClimbZoneTrigger object.
        ///     This method is called whenever the Scene view is updated, providing a way to display and manipulate
        ///     custom position handles or other interactive elements for the object.
        /// </summary>
        private void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();
            var newStartPoint = Handles.PositionHandle(
                climbZoneTrigger.ClimbZoneStartOffsetPoint + climbZoneTrigger.transform.position,
                Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(climbZoneTrigger, "Change Climb Zone Start Point");
                climbZoneTrigger.ClimbZoneStartOffsetPoint = newStartPoint - climbZoneTrigger.transform.position;
            }

            EditorGUI.BeginChangeCheck();
            var newEndPoint = Handles.PositionHandle(climbZoneTrigger.ClimbZoneEndOffsetPoint + climbZoneTrigger.transform.position,
                Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(climbZoneTrigger, "Change Climb Zone End Point");
                climbZoneTrigger.ClimbZoneEndOffsetPoint = newEndPoint - climbZoneTrigger.transform.position;
            }
        }

        #endregion
    }
}