using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
    public class RollCrash : MonoBehaviour
    {
        public GameObject knockdownObject;
        public float resetTime = 5.0f;
        public Vector3 startPosition;
        public Vector3 endPosition;
    
        private bool isKnockedDown;
        private float knockdownTimer;
    
        private void Update()
        {
            if (isKnockedDown)
            {
                knockdownObject.transform.position = endPosition;

                knockdownTimer += Time.deltaTime;
                if (knockdownTimer >= resetTime)
                {
                    isKnockedDown = false;
                    knockdownTimer = 0.0f;
                    knockdownObject.transform.position = startPosition;
                }
            }
        }
    
        private void OnCollisionEnter(Collision other)
        {
            if (!isKnockedDown && other.gameObject.TryGetComponent(out Runtime.Core.AdventureCharacterController adventureCharacterController))
            {
                if (adventureCharacterController.IsRolling)
                {
                    isKnockedDown = true;
                }
            }
        }
    }
}
