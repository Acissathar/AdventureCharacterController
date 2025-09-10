using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
    public class RollCrash : MonoBehaviour
    {
        public GameObject knockdownObject;
        public float resetTime = 5.0f;
        public Vector3 startPosition;
        public Vector3 endPosition;

        private bool _isKnockedDown;
        private float _knockdownTimer;

        private void Update()
        {
            if (_isKnockedDown)
            {
                knockdownObject.transform.position = endPosition;

                _knockdownTimer += Time.deltaTime;
                if (_knockdownTimer >= resetTime)
                {
                    _isKnockedDown = false;
                    _knockdownTimer = 0.0f;
                    knockdownObject.transform.position = startPosition;
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!_isKnockedDown &&
                other.gameObject.TryGetComponent(out Runtime.Core.AdventureCharacterController adventureCharacterController))
            {
                if (adventureCharacterController.IsRolling)
                {
                    _isKnockedDown = true;
                }
            }
        }
    }
}