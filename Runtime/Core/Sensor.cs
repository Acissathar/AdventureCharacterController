using System;
using System.Collections.Generic;
using UnityEngine;
using UnityHelpers.Runtime.Math;

namespace AdventureCharacterController.Runtime.Core
{
    /// <summary>
    ///     Responsible for casting rays and spherecasts to be used by the Mover, it is instantiated by the 'Mover' component
    ///     at runtime
    /// </summary>
    [Serializable]
    public class Sensor
    {
        #region Constructor

        /// <summary>
        ///     Constructor for Sensor.
        /// </summary>
        /// <param name="transform">This game object's transform. Must be passed because it's not a MonoBehaviour.</param>
        /// <param name="collider">
        ///     This game object's collider will be tracked and ignored in all casts so we don't collider
        ///     against ourselves.
        /// </param>
        public Sensor(Transform transform, Collider collider)
        {
            _myTransform = transform;

            if (!collider)
            {
                return;
            }

            _ignoreList = new[]
            {
                collider
            };

            _ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");

            _ignoreListLayers = new int[_ignoreList.Length];
        }

        #endregion

        #region Internal

        /// <summary>
        ///     Resets all previously tracked Hit information.
        /// </summary>
        private void ResetHitFlags()
        {
            HasDetectedHit = false;
            HitPosition = Vector3.zero;
            HitNormal = -GetCastDirection();
            HitDistance = 0f;

            if (_hitColliders.Count > 0)
            {
                _hitColliders.Clear();
            }

            if (_hitTransforms.Count > 0)
            {
                _hitTransforms.Clear();
            }
        }

        #endregion

        #region Editor Debug

        /// <summary>
        ///     Draw debug information in the Editor (hit positions and ground surface normals)
        /// </summary>
        public void DrawDebug()
        {
            if (HasDetectedHit && IsInDebugMode)
            {
                Debug.DrawRay(HitPosition, HitNormal, Color.red, Time.deltaTime);
                var markerSize = 0.2f;
                Debug.DrawLine(HitPosition + Vector3.up * markerSize, HitPosition - Vector3.up * markerSize, Color.green,
                    Time.deltaTime);
                Debug.DrawLine(HitPosition + Vector3.right * markerSize, HitPosition - Vector3.right * markerSize, Color.green,
                    Time.deltaTime);
                Debug.DrawLine(HitPosition + Vector3.forward * markerSize, HitPosition - Vector3.forward * markerSize, Color.green,
                    Time.deltaTime);
            }
        }

        #endregion

        #region Private Fields

        // References to attached components;
        private Transform _myTransform;
        private Collider _myCollider;

        private Vector3 _castOrigin = Vector3.zero;
        private CastDirection _castDirection;
        private int _ignoreRaycastLayer;

        // Raycast hit information
        private List<Collider> _hitColliders = new List<Collider>();
        private List<Transform> _hitTransforms = new List<Transform>();

        // Backup normal used for specific edge cases when using spherecasts
        private Vector3 _backupNormal;

        private Vector3[] _raycastArrayStartPositions;
        private List<Vector3> _arrayNormals = new List<Vector3>();
        private List<Vector3> _arrayPoints = new List<Vector3>();

        // Optional list of colliders to ignore when raycasting
        private Collider[] _ignoreList;
        private int[] _ignoreListLayers;

        #endregion

        #region Properties

        /// <summary>
        ///     How far the cast should be fired.
        /// </summary>
        public float CastLength { get; set; }

        /// <summary>
        ///     Type of cast to use.
        /// </summary>
        public CastType CastType { get; set; }

        /// <summary>
        ///     LayerMask to be used for Casts. Should not include Ignore Raycasts layer.
        /// </summary>
        public LayerMask LayerMask { get; set; }

        /// <summary>
        ///     Set to True to calculate the actual surface normal from a spherecast with an additional raycast.
        ///     When false, use the Normal returned by the SphereCast call itself.
        /// </summary>
        public bool SphereCastCalculateRealSurfaceNormal { get; set; }

        /// <summary>
        ///     Set to True to calculate the actual distance from a spherecast by extracting the dot vector.
        ///     When false, use the distance returned by the SphereCast call itself.
        /// </summary>
        public bool SphereCastCalculateRealDistance { get; set; }

        /// <summary>
        ///     Radius of the sphere used for sphere casts. Not used when RayCasting.
        /// </summary>
        public float SphereCastRadius { get; set; }

        /// <summary>
        ///     How many total rays to use when using an array of RayCasts as the casting mode.
        ///     These will be centered around CastOrigin.
        /// </summary>
        public int ArrayRayCount { get; set; }

        /// <summary>
        ///     How many rows to split the ArrayRayCount in when using an array of RayCasts as the casting mode.
        ///     These will be centered around CastOrigin.
        /// </summary>
        public int ArrayRows { get; set; }

        /// <summary>
        ///     Whether to offset the Array RayCast rows when using an array of RayCasts as the casting mode.
        /// </summary>
        public bool OffsetArrayRows { get; set; }

        /// <summary>
        ///     Whether to draw Unity Debug versions of the Ray/Spherecasts that are cast.
        /// </summary>
        public bool IsInDebugMode { get; set; }

        /// <summary>
        ///     If the sensor has detected a hit with a cast.
        /// </summary>
        public bool HasDetectedHit { get; private set; }

        /// <summary>
        ///     Distance from the collider the raycast hit
        /// </summary>
        public float HitDistance { get; private set; }

        /// <summary>
        ///     Surface normal of the collider the raycast hit
        /// </summary>
        public Vector3 HitNormal { get; private set; }

        /// <summary>
        ///     World position of the collider where the raycast hit
        /// </summary>
        public Vector3 HitPosition { get; private set; }

        /// <summary>
        ///     The collider that was hit by the raycast;
        /// </summary>
        public Collider HitCollider => _hitColliders[0];

        /// <summary>
        ///     Transform component attached to the collider that was hit by the raycast
        /// </summary>
        public Transform HitTransform => _hitTransforms[0];

        /// <summary>
        ///     The world position of the raycast to start from. This will be converted to local coordinates.
        /// </summary>
        public Vector3 CastOrigin
        {
            set
            {
                if (!_myTransform)
                {
                    return;
                }

                _castOrigin = _myTransform.InverseTransformPoint(value);
            }
        }

        /// <summary>
        ///     Which axis of this gameobject's transform to use as the direction for the raycast;
        /// </summary>
        public CastDirection CastDirection
        {
            set
            {
                if (!_myTransform)
                {
                    return;
                }

                _castDirection = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Generates the array of Raycasts for use when casting mode is RaycastArray.
        /// </summary>
        /// <param name="sensorRows">Number of rows for the raycast array.</param>
        /// <param name="sensorRayCount">Number of rays to use for the raycast array.</param>
        /// <param name="offsetRows">Should the rows be offset?</param>
        /// <param name="sensorRadius">Radius of the sensor to spread the array around.</param>
        public static Vector3[] GetRaycastStartPositions(int sensorRows, int sensorRayCount, bool offsetRows, float sensorRadius)
        {
            var positions = new List<Vector3>();

            var startPosition = Vector3.zero;
            positions.Add(startPosition);

            for (var i = 0; i < sensorRows; i++)
            {
                //Calculate radius for all positions on this row;
                var rowRadius = (float)(i + 1) / sensorRows;

                for (var j = 0; j < sensorRayCount * (i + 1); j++)
                {
                    // Calculate the angle (in degrees) for this individual position;
                    var angle = 360f / (sensorRayCount * (i + 1)) * j;

                    if (offsetRows && i % 2 == 0)
                    {
                        angle += 360f / (sensorRayCount * (i + 1)) / 2f;
                    }

                    // Combine radius and angle into one position and add it to the list;
                    var x = rowRadius * Mathf.Cos(Mathf.Deg2Rad * angle);
                    var y = rowRadius * Mathf.Sin(Mathf.Deg2Rad * angle);

                    positions.Add(new Vector3(x, 0f, y) * sensorRadius);
                }
            }

            return positions.ToArray();
        }

        /// <summary>
        ///     Recalibrates the Raycast Array. The mover calls this when the Sensor is being recalibrated, such as from
        ///     collider changes.
        /// </summary>
        public void RecalibrateRaycastArrayPositions()
        {
            _raycastArrayStartPositions = GetRaycastStartPositions(ArrayRows, ArrayRayCount, OffsetArrayRows, SphereCastRadius);
        }

        #endregion

        #region Handle Casting

        /// <summary>
        ///     Performs the actual casting for collisions. Processing the data isn't done here, this is just simply gathering said
        ///     data.
        /// </summary>
        public void Cast()
        {
            ResetHitFlags();

            // Calculate the origin and direction of ray in world coordinates;
            var worldDirection = GetCastDirection();
            var worldOrigin = _myTransform.TransformPoint(_castOrigin);

            if (_ignoreListLayers.Length != _ignoreList.Length)
            {
                _ignoreListLayers = new int[_ignoreList.Length];
            }

            // (Temporarily) move all objects in the ignore list to the 'Ignore Raycast' layer so we can easily ignore them without needing to mess with physics matrix or filtering and let Unity handle it
            for (var i = 0; i < _ignoreList.Length; i++)
            {
                _ignoreListLayers[i] = _ignoreList[i].gameObject.layer;
                _ignoreList[i].gameObject.layer = _ignoreRaycastLayer;
            }

            switch (CastType)
            {
                case CastType.Raycast:
                {
                    CastRay(worldOrigin, worldDirection);
                    break;
                }
                case CastType.Spherecast:
                {
                    CastSphere(worldOrigin, worldDirection);
                    break;
                }
                case CastType.RaycastArray:
                {
                    CastRayArray(worldOrigin, worldDirection);
                    break;
                }
                default:
                {
                    HasDetectedHit = false;
                    break;
                }
            }

            // Reset collider layers in ignoreList
            for (var i = 0; i < _ignoreList.Length; i++)
            {
                _ignoreList[i].gameObject.layer = _ignoreListLayers[i];
            }
        }

        /// <summary>
        ///     Casting Method for casting an array of rays into 'direction' that is centered around 'origin'
        /// </summary>
        /// <param name="rayArrayOrigin">Origin point for the center of the raycast array.</param>
        /// <param name="rayArrayDirection">Direction to fire the array of raycasts in.</param>
        private void CastRayArray(Vector3 rayArrayOrigin, Vector3 rayArrayDirection)
        {
            var rayDirection = GetCastDirection();

            _arrayNormals.Clear();
            _arrayPoints.Clear();

            foreach (var raycastStartPosition in _raycastArrayStartPositions)
            {
                var rayStartPosition = rayArrayOrigin + _myTransform.TransformDirection(raycastStartPosition);

                if (Physics.Raycast(rayStartPosition, rayDirection, out var hit, CastLength, LayerMask,
                        QueryTriggerInteraction.Ignore))
                {
                    if (IsInDebugMode)
                    {
                        Debug.DrawRay(hit.point, hit.normal, Color.red, Time.fixedDeltaTime * 1.01f);
                    }

                    _hitColliders.Add(hit.collider);
                    _hitTransforms.Add(hit.transform);
                    _arrayNormals.Add(hit.normal);
                    _arrayPoints.Add(hit.point);
                }
            }

            HasDetectedHit = _arrayPoints.Count > 0;

            if (HasDetectedHit)
            {
                var averageNormal = Vector3.zero;
                foreach (var normal in _arrayNormals)
                {
                    averageNormal += normal;
                }

                averageNormal.Normalize();

                var averagePoint = Vector3.zero;
                foreach (var arrayPoint in _arrayPoints)
                {
                    averagePoint += arrayPoint;
                }

                averagePoint /= _arrayPoints.Count;

                HitPosition = averagePoint;
                HitNormal = averageNormal;
                HitDistance = VectorMath.ExtractDotVector(rayArrayOrigin - HitPosition, rayArrayDirection).magnitude;
            }
        }

        /// <summary>
        ///     Casting method for a single raycast.
        /// </summary>
        /// <param name="rayOrigin">Origin point of the raycast.</param>
        /// <param name="rayDirection">Direction to fire the raycast in.</param>
        private void CastRay(Vector3 rayOrigin, Vector3 rayDirection)
        {
            HasDetectedHit = Physics.Raycast(rayOrigin, rayDirection, out var hit, CastLength, LayerMask,
                QueryTriggerInteraction.Ignore);

            if (HasDetectedHit)
            {
                HitPosition = hit.point;
                HitNormal = hit.normal;

                _hitColliders.Add(hit.collider);
                _hitTransforms.Add(hit.transform);

                HitDistance = hit.distance;
            }
        }

        /// <summary>
        ///     Casting method for spherecasts.
        /// </summary>
        /// <param name="sphereOrigin">Origin point of the spherecast.</param>
        /// <param name="sphereDirection">Direction to fire the sphere cast in.</param>
        private void CastSphere(Vector3 sphereOrigin, Vector3 sphereDirection)
        {
            HasDetectedHit = Physics.SphereCast(sphereOrigin, SphereCastRadius, sphereDirection, out var hit,
                CastLength - SphereCastRadius, LayerMask, QueryTriggerInteraction.Ignore);

            if (HasDetectedHit)
            {
                HitPosition = hit.point;
                HitNormal = hit.normal;
                _hitColliders.Add(hit.collider);
                _hitTransforms.Add(hit.transform);

                HitDistance = hit.distance;

                HitDistance += SphereCastRadius;

                if (SphereCastCalculateRealDistance)
                {
                    HitDistance = VectorMath.ExtractDotVector(sphereOrigin - HitPosition, sphereDirection).magnitude;
                }

                var col = _hitColliders[0];

                if (SphereCastCalculateRealSurfaceNormal)
                {
                    if (col.Raycast(new Ray(HitPosition - sphereDirection, sphereDirection), out hit, 1.5f))
                    {
                        HitNormal = Vector3.Angle(hit.normal, -sphereDirection) >= 89f ? _backupNormal : hit.normal;
                    }
                    else
                    {
                        HitNormal = _backupNormal;
                    }

                    _backupNormal = HitNormal;
                }
            }
        }

        /// <summary>
        ///     Calculate a direction in world coordinates based on the local axes of this gameobject's transform component from
        ///     desired CastDirection.
        /// </summary>
        /// <returns>
        ///     CastDirection as applied to the transform's direction. e.g., CastDirection.Forward returns this transform's
        ///     forward.
        /// </returns>
        private Vector3 GetCastDirection()
        {
            switch (_castDirection)
            {
                case CastDirection.Forward:
                {
                    return _myTransform.forward;
                }
                case CastDirection.Right:
                {
                    return _myTransform.right;
                }
                case CastDirection.Up:
                {
                    return _myTransform.up;
                }
                case CastDirection.Backward:
                {
                    return -_myTransform.forward;
                }
                case CastDirection.Left:
                {
                    return -_myTransform.right;
                }
                case CastDirection.Down:
                {
                    return -_myTransform.up;
                }
                default:
                {
                    return Vector3.one;
                }
            }
        }

        #endregion
    }

    #region Enums

    /// <summary>
    ///     Desired ray/sphere cast direction for this gameobject.
    /// </summary>
    public enum CastDirection
    {
        Forward,
        Right,
        Up,
        Backward,
        Left,
        Down
    }

    /// <summary>
    ///     Type of cast to use for generating collision information.
    /// </summary>
    public enum CastType
    {
        Raycast,
        RaycastArray,
        Spherecast
    }

    #endregion
}