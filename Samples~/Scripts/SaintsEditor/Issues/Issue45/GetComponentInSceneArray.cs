using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetComponentInSceneArray : SaintsMonoBehaviour
    {
        [FindObjectsByType, EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInSceneArray;
        [FindObjectsByType, EndText(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInSceneList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        [Serializable]
        public class GeneralInterface : SaintsInterface<Object, IDummy>
        {
            public GeneralInterface(Object obj) : base(obj)
            {
            }
        }

        [FindObjectsByType, EndText(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [FindObjectsByType, EndText(nameof(DummyNumberG), isCallback: true)]
        public List<SaintsInterface<Object, IDummy>> getComponentIList;

        private string DummyNumberI(GeneralInterface dummyInter)
        {
            return dummyInter == null? "":  $"{dummyInter.I.GetComment()}";
        }

        private string DummyNumberG(SaintsInterface<Object, IDummy> dummyInter)
        {
            return dummyInter == null? "":  $"{dummyInter.I.GetComment()}";
        }
    }
}
