using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsListExample: MonoBehaviour
    {
        public SaintsList<GameObject> gameObjectsDirect;
        public SaintsList<GameObject>[] gameObjects1;
        public SaintsList<SaintsList<GameObject>> gameObjects2;

        private void Start()
        {
            List<GameObject> impArrDirect = gameObjectsDirect;
            SaintsList<GameObject> expArrDirect = (SaintsList<GameObject>)impArrDirect;
        }
    }
}
