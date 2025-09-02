using System.Collections;
using System.Collections.Generic;
using AdventureCharacterController.Runtime.Core;
using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
	public class ExamplePlatform : MonoBehaviour 
	{
		public float movementSpeed = 10f;
		public bool reverseDirection = false;
		public float waitTime = 1f;
		private bool isWaiting = false;
		public List<Transform> waypoints = new List<Transform>();
		
		private readonly List <Rigidbody> rigidbodiesInTriggerArea = new List<Rigidbody>();
		private Rigidbody myRigidbody;
		private int currentWaypointIndex = 0;
		private Transform currentWaypoint;
		
		#region Unity Methods
		
		private void Start () 
		{
			if (!TryGetComponent(out myRigidbody))
			{
				InternalDebug.Log("No rigidbody found on platform.", gameObject);
			}
			
			myRigidbody.freezeRotation = true;
			myRigidbody.useGravity = false;
			myRigidbody.isKinematic = true;
			
			if(waypoints.Count <= 0)
			{
				InternalDebug.LogWarning("No waypoints have been assigned to Platform", gameObject);
			} 
			else
			{
				currentWaypoint = waypoints[currentWaypointIndex];
			}
			
			StartCoroutine(WaitRoutine());
			StartCoroutine(LateFixedUpdate());
		}
		
		private void OnTriggerEnter(Collider col)
		{
			if(col.attachedRigidbody != null && col.TryGetComponent(out Mover mover))
			{
				rigidbodiesInTriggerArea.Add(col.attachedRigidbody);
			}
		}
		
		private void OnTriggerExit(Collider col)
		{
			if(col.attachedRigidbody != null && col.TryGetComponent(out Mover mover))
			{
				rigidbodiesInTriggerArea.Remove(col.attachedRigidbody);
			}
		}
		
		#endregion
		
		IEnumerator LateFixedUpdate()
		{
			var waitForFixedUpdate = new WaitForFixedUpdate();
			while(true)
			{
				yield return waitForFixedUpdate;
				MovePlatform();
			}
		}

		private void MovePlatform () 
		{
			if (waypoints.Count <= 0)
			{
				return;
			}

			if (isWaiting)
			{
				return;
			}

			var toCurrentWaypoint = currentWaypoint.position - transform.position;

			//Get normalized movement direction;
			var movement = toCurrentWaypoint.normalized;

			//Get movement for this frame;
			movement *= movementSpeed * Time.deltaTime;

			//If the remaining distance to the next waypoint is smaller than this frame's movement, move directly to next waypoint;
			//Else, move toward next waypoint;
			if(movement.magnitude >= toCurrentWaypoint.magnitude || movement.magnitude == 0f)
			{
				myRigidbody.transform.position = currentWaypoint.position;
				UpdateWaypoint();
			}
			else
			{
				myRigidbody.transform.position += movement;
			}
			
			foreach (var rigidBody in rigidbodiesInTriggerArea)
			{
				rigidBody.MovePosition(rigidBody.position + movement);
			}
		}

		private void UpdateWaypoint()
		{
			if (reverseDirection)
			{
				currentWaypointIndex--;
			}
			else
			{
				currentWaypointIndex++;
			}

			if (currentWaypointIndex >= waypoints.Count)
			{
				currentWaypointIndex = 0;
			}

			if (currentWaypointIndex < 0)
			{
				currentWaypointIndex = waypoints.Count - 1;
			}

			currentWaypoint = waypoints[currentWaypointIndex];
			
			isWaiting = true;
		}
		
		IEnumerator WaitRoutine()
		{
			var waitInstruction = new WaitForSeconds(waitTime);

			while(true)
			{
				if(isWaiting)
				{
					yield return waitInstruction;
					isWaiting = false;
				}

				yield return null;
			}
		}	
	}
}