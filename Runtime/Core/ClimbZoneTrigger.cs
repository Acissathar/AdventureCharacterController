using UnityEngine;

namespace AdventureCharacterController.Runtime.Core
{
    /// <summary>
    ///     Script responsible for setting and passing on information about the style of climbing the controller should use in
    ///     this trigger.
    /// </summary>
    public class ClimbZoneTrigger : MonoBehaviour
    {
        #region Editor - Settings

        /// <summary>
        ///     Flag to allow free climbing. When false, the controller will only climb as if it were on a ladder.
        /// </summary>
        public bool AllowFreeClimbing
        {
            get => allowFreeClimbing;
            set => allowFreeClimbing = value;
        }

        /// <summary>
        ///     When AllowFreeClimbing is true, only the Y position is taken into account and is used to mark the bottom of the
        ///     free climb area.
        ///     When AllowFreeClimbing is false, the entire position is used, and is the point on the ladder that the controller
        ///     will start climbing from.
        ///     Note that this Vector is set as an offset in relation to the transform of this object.
        /// </summary>
        public Vector3 ClimbZoneStartOffsetPoint
        {
            get => climbZoneStartOffsetPoint;
            set => climbZoneStartOffsetPoint = value;
        }

        /// <summary>
        ///     When AllowFreeClimbing is true, only the Y position is taken into account and is used to mark the top of the free
        ///     climb area.
        ///     When AllowFreeClimbing is false, the entire position is used, and is the point on the ladder that the controller
        ///     will exit climbing from.
        ///     Note that this Vector is set as an offset in relation to the transform of this object.
        /// </summary>
        public Vector3 ClimbZoneEndOffsetPoint
        {
            get => climbZoneEndOffsetPoint;
            set => climbZoneEndOffsetPoint = value;
        }

        /// <summary>
        ///     Cached transform of this object.
        /// </summary>
        public Transform ClimbZoneTransform { get; private set; }

        [SerializeField] private bool allowFreeClimbing;
        [SerializeField] private Vector3 climbZoneStartOffsetPoint;
        [SerializeField] private Vector3 climbZoneEndOffsetPoint;

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Unity calls Awake when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            ClimbZoneTransform = transform;
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
                controller.ClimbZoneTriggers.Add(this);
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
                controller.ClimbZoneTriggers.Remove(this);
            }
        }

        #endregion
    }
}