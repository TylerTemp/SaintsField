using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsArrayExample: MonoBehaviour
    {
        public SaintsArray<GameObject> saintsArrayGo;
        // two-dimensional array
        public SaintsArray<GameObject>[] gameObjects2;
        public SaintsArray<SaintsArray<GameObject>> gameObjects2Nest;
        // four dimensional array, if you like.
        // it can be used with array, but ensure the `[]` is always at the end.
        public SaintsArray<SaintsArray<SaintsArray<GameObject>>>[] gameObjects4;

        [ArraySize(2)] public SaintsArray<int> _arrSize;

        // private void Start()
        // {
        //     GameObject[] impArrDirect = saintsArrayGo;
        //     SaintsArray<GameObject> expArrDirect = (SaintsArray<GameObject>)impArrDirect;
        //
        //     saintsArrayGo[0] = new GameObject();
        //     GameObject firstGameObject = saintsArrayGo[0];
        // }
    }
}
