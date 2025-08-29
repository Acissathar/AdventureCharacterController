using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
    public class SwingingObstacle : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        
        private void Update ()
        {
            transform.Rotate (0,0,speed * Time.deltaTime); 
        }
    }
}
