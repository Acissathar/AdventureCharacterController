using System;
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

        #endregion

        #region Unity Methods

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            ladderZoneTrigger = (LadderZoneTrigger)target;
        }

        /// <summary>
        ///     Allows custom rendering and interaction with handles in the Scene view for the associated LadderZoneTrigger object.
        ///     This method is called whenever the Scene view is updated, providing a way to display and manipulate
        ///     custom position handles or other interactive elements for the object.
        /// </summary>
        private void OnSceneGUI()
        {
            EditorGUILayout.HelpBox(
                "Ladder Trigger's Forward direction needs to match the facing the controller will have while climbing it. Usually this means the forward of the ladder trigger will be facing the wall the ladder is up against.\n" +
                "This is so that the calculation to allow for automatically climbing up ladders is accurate. (We use the movement velocity which is relative to the camera and the dot product of the ladder's forward direction)",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            var newStartPoint = Handles.PositionHandle(ladderZoneTrigger.LadderStartOffsetPoint + ladderZoneTrigger.transform.position,
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