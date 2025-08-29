using UnityEngine;

namespace AdventureCharacterController.Runtime.Core
{
    /// <summary>
    ///     Simple trigger script to set the InLadderZone property of the AdventureCharacterController if applicable.
    /// </summary>
    public class LadderZoneTrigger : MonoBehaviour
    {
        #region Editor - Settings

        public Vector3 LadderStartOffsetPoint
        {
            get => ladderStartOffsetPoint;
            set => ladderStartOffsetPoint = value;
        }

        public Vector3 LadderEndOffsetPoint
        {
            get => ladderEndOffsetPoint;
            set => ladderEndOffsetPoint = value;
        }

        public Transform LadderTransform { get; private set; }

        [SerializeField] private Vector3 ladderStartOffsetPoint;
        [SerializeField] private Vector3 ladderEndOffsetPoint;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Unity calls Awake when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            LadderTransform = transform;
        }

        /// <summary>
        ///     OnTriggerEnter is invoked when two GameObjects with a Collider component touch or overlap, and one of the Collider
        ///     components has the Collider.isTrigger property enabled. A trigger Collider doesn't register collisions with an
        ///     incoming Rigidbody and doesn't collide with any other GameObjects that have Colliders on them. The events are
        ///     invoked during simulation, which happens after all FixedUpdate methods are called, or within the scope of
        ///     Physics.Simulate, if you're using manual physics simulation.
        /// </summary>
        /// <param name="other">The object colliding with *this* object.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out AdventureCharacterController controller))
            {
                controller.CurrentLadder = this;
            }
        }

        /// <summary>
        ///     OnTriggerExit is called when the Collider other has stopped touching the trigger. This message is sent to the
        ///     trigger and the Collider that touches the trigger. Notes: Trigger events are only sent if one of the Colliders also
        ///     has a Rigidbody attached. Trigger events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in
        ///     response to collisions. OnTriggerExit occurs on the FixedUpdate after the Colliders have stopped touching. The
        ///     Colliders involved are not guaranteed to be at the point of initial separation. Deactivating or destroying a
        ///     Collider while it is inside a trigger volume will not register an on exit event.
        /// </summary>
        /// <param name="other">The object colliding with *this* object.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out AdventureCharacterController controller))
            {
                controller.CurrentLadder = null;
            }
        }

        #endregion
    }
}