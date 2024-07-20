using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetComponentInSceneArray : SaintsMonoBehaviour
    {
        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInSceneArray;
        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInSceneList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }

        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
        public List<SaintsInterface<UnityEngine.Object, IDummy>> getComponentIList;

        private string DummyNumberI(GeneralInterface dummyInter)
        {
            return dummyInter == null? "":  $"{dummyInter.I.GetComment()}";
        }

        private string DummyNumberG(SaintsInterface<UnityEngine.Object, IDummy> dummyInter)
        {
            return dummyInter == null? "":  $"{dummyInter.I.GetComment()}";
        }
    }
}
