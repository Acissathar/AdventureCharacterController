using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
    public class ExampleAnimator : MonoBehaviour
    {
    
        [SerializeField] private AdventureCharacterController.Runtime.Core.AdventureCharacterController adventureCharacterController;
        [SerializeField] private Transform characterMesh;
        
        private void Update()
        {
            if (adventureCharacterController)
            {
                if (adventureCharacterController.InCrouchZone)
                {
                    characterMesh.localScale = new Vector3(1, 0.5f, 1);
                }
                else
                {
                    characterMesh.localScale = Vector3.one;
                }
            }
        }
    }
}
