using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
    public class ExamplePlatform : MonoBehaviour
    {
        public float movementSpeed = 10f;
        public bool reverseDirection;
        public float waitTime = 1f;
        private bool _isWaiting;
        public List<Transform> waypoints = new List<Transform>();

        private readonly List<Rigidbody> _rigidbodiesInTriggerArea = new List<Rigidbody>();
        private Rigidbody _myRigidbody;
        private int _currentWaypointIndex;
        private Transform _currentWaypoint;

        #region Unity Methods

        private void Start()
        {
            if (!TryGetComponent(out _myRigidbody))
            {
                InternalDebug.Log("No rigidbody found on platform.", gameObject);
            }

            _myRigidbody.freezeRotation = true;
            _myRigidbody.useGravity = false;
            _myRigidbody.isKinematic = true;

            if (waypoints.Count <= 0)
            {
                InternalDebug.LogWarning("No waypoints have been assigned to Platform", gameObject);
            }
            else
            {
                _currentWaypoint = waypoints[_currentWaypointIndex];
            }

            StartCoroutine(WaitRoutine());
            StartCoroutine(LateFixedUpdate());
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.attachedRigidbody != null)
            {
                _rigidbodiesInTriggerArea.Add(col.attachedRigidbody);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.attachedRigidbody != null)
            {
                _rigidbodiesInTriggerArea.Remove(col.attachedRigidbody);
            }
        }

        #endregion

        private IEnumerator LateFixedUpdate()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (enabled)
            {
                yield return waitForFixedUpdate;
                MovePlatform();
            }
        }

        private void MovePlatform()
        {
            if (waypoints.Count <= 0)
            {
                return;
            }

            if (_isWaiting)
            {
                return;
            }

            var toCurrentWaypoint = _currentWaypoint.position - transform.position;

            //Get normalized movement direction;
            var movement = toCurrentWaypoint.normalized;

            //Get movement for this frame;
            movement *= movementSpeed * Time.deltaTime;

            //If the remaining distance to the next waypoint is smaller than this frame's movement, move directly to the next waypoint;
            //Else, move toward the next waypoint;
            if (movement.magnitude >= toCurrentWaypoint.magnitude || movement.magnitude == 0f)
            {
                _myRigidbody.transform.position = _currentWaypoint.position;
                UpdateWaypoint();
            }
            else
            {
                _myRigidbody.transform.position += movement;
            }

            foreach (var rigidBody in _rigidbodiesInTriggerArea)
            {
                rigidBody.MovePosition(rigidBody.position + movement);
            }
        }

        private void UpdateWaypoint()
        {
            if (reverseDirection)
            {
                _currentWaypointIndex--;
            }
            else
            {
                _currentWaypointIndex++;
            }

            if (_currentWaypointIndex >= waypoints.Count)
            {
                _currentWaypointIndex = 0;
            }

            if (_currentWaypointIndex < 0)
            {
                _currentWaypointIndex = waypoints.Count - 1;
            }

            _currentWaypoint = waypoints[_currentWaypointIndex];

            _isWaiting = true;
        }

        private IEnumerator WaitRoutine()
        {
            var waitInstruction = new WaitForSeconds(waitTime);

            while (enabled)
            {
                if (_isWaiting)
                {
                    yield return waitInstruction;
                    _isWaiting = false;
                }

                yield return null;
            }
        }
    }
}