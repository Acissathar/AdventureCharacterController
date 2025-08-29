using AdventureCharacterController.Runtime.Core;
using UnityEngine;

namespace AdventureCharacterController.Samples.Scripts
{
    public class BasicCharacterInput : MonoBehaviour
    {
        public string horizontalInputAxis = "Horizontal";
        public string verticalInputAxis = "Vertical";

        public Runtime.Core.AdventureCharacterController characterController;
        
        private ControllerInput controllerInput;
        
        //If this is enabled, Unity's internal input smoothing is bypassed;
        public bool useRawInput = true;
        
        // Update is called once per frame
        void Update()
        {
            characterController.ControllerInput = new ControllerInput
            {
                Horizontal = useRawInput ? Input.GetAxisRaw(horizontalInputAxis) : Input.GetAxis(horizontalInputAxis),
                Vertical = useRawInput ? Input.GetAxisRaw(verticalInputAxis) : Input.GetAxis(verticalInputAxis)
            };
        }
    }
}
