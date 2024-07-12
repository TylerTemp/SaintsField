using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issue45
{
    public class GetComponentInChildrenArray : SaintsMonoBehavior
    {
        [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInChildrenArray;
        [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInChildrenList;

        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }

        [GetComponent, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [GetComponent, PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
        public List<SaintsInterface<UnityEngine.Object, IDummy>> getComponentIList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

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
