using UnityEngine;

namespace AdventureCharacterController.Runtime.Core
{
    public interface ILadderInfo
    {
        public Vector3 LadderStartOffsetPoint
        {
            get;
            set;
        }
    
        public Vector3 LadderEndOffsetPoint
        {
            get;
            set;
        }

        public Transform LadderTransform
        {
            get;
        }
    }
}
