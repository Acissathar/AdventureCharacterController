using AdventureCharacterController.Runtime.Core;
using AdventureCharacterController.Runtime.Extras;
using UnityEditor;
using UnityEngine;

namespace AdventureCharacterController.Editor.Extras
{
    [CustomEditor(typeof(LadderZoneTrigger))]
    [CanEditMultipleObjects]
    public class LadderZoneTriggerInspector : UnityEditor.Editor
    {
        #region Private Fields

        private LadderZoneTrigger ladderZoneTrigger;

        private SerializedProperty ladderStartOffsetPointProp;
        private SerializedProperty ladderEndOffsetPointProp;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            ladderZoneTrigger = (LadderZoneTrigger)target;

            ladderStartOffsetPointProp = serializedObject.FindProperty("ladderStartOffsetPoint");
            ladderEndOffsetPointProp = serializedObject.FindProperty("ladderEndOffsetPoint");
        }

        /// <summary>
        ///     Implement this function to make a custom inspector.
        ///     Inside this function you can add your own custom IMGUI based GUI for the inspector of a specific object class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ladderZoneTrigger.transform.localRotation != Quaternion.identity)
            {
                EditorGUILayout.HelpBox(
                    "Ladder Trigger should have an all 0 rotation so that the Forward direction 'faces the wall the ladder is up against' as we use this direction for attaching and leaving the ladder.\n" +
                    "The parent object can be rotated any direction, as long as the 'blue axis' faces into the wall on the object containing this component.",
                    MessageType.Warning);
            }

            EditorGUILayout.PropertyField(ladderStartOffsetPointProp, new GUIContent("Ladder Start Offset Point"));
            EditorGUILayout.PropertyField(ladderEndOffsetPointProp, new GUIContent("Ladder End Offset Point"));

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///     Allows custom rendering and interaction with handles in the Scene view for the associated LadderZoneTrigger object.
        ///     This method is called whenever the Scene view is updated, providing a way to display and manipulate
        ///     custom position handles or other interactive elements for the object.
        /// </summary>
        private void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();
            var newStartPoint = Handles.PositionHandle(
                ladderZoneTrigger.LadderStartOffsetPoint + ladderZoneTrigger.transform.position,
                Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ladderZoneTrigger, "Change Ladder Zone Start Point");
                ladderZoneTrigger.LadderStartOffsetPoint = newStartPoint - ladderZoneTrigger.transform.position;
            }

            EditorGUI.BeginChangeCheck();
            var newEndPoint = Handles.PositionHandle(ladderZoneTrigger.LadderEndOffsetPoint + ladderZoneTrigger.transform.position,
                Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ladderZoneTrigger, "Change Ladder Zone End Point");
                ladderZoneTrigger.LadderEndOffsetPoint = newEndPoint - ladderZoneTrigger.transform.position;
            }
        }

        #endregion
    }
}