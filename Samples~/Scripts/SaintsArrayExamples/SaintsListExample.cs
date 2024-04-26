using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsListExample: MonoBehaviour
    {
        [RichLabel("<color=cyan><label /><icon=star.png />"), SaintsArray]
        public SaintsList<GameObject> saintsListGo;

        public SaintsList<GameObject>[] gameObjects1;
        public SaintsList<SaintsList<GameObject>> gameObjects2;

        private void Start()
        {
            List<GameObject> impListDirect = saintsListGo;
            SaintsList<GameObject> expArrDirect = (SaintsList<GameObject>)impListDirect;

            saintsListGo.Add(new GameObject());
            saintsListGo.RemoveAt(0);
        }
    }
}
