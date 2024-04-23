using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsArrayExample: MonoBehaviour
    {
        public SaintsArray<GameObject> gameObjectsDirect;
        public SaintsArray<GameObject>[] gameObjects1;
        public SaintsArray<SaintsArray<GameObject>> gameObjects2;

        private void Start()
        {
            GameObject[] impArrDirect = gameObjectsDirect;
            SaintsArray<GameObject> expArrDirect = (SaintsArray<GameObject>)impArrDirect;
        }
    }
}
