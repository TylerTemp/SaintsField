using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetComponentInParentArray : SaintsMonoBehaviour
    {
        [GetComponentInParent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInParentArray;
        [GetComponentInParents, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInParentsList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }

        [GetComponentInParent, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [GetComponentInParents, PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
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
