using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetComponentArray : SaintsMonoBehaviour
    {
        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy>
        {
            public override string ToString()
            {
                return I?.GetComment() ?? "NULL";
            }
        }

        [GetComponent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentArray;
        [GetComponent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentList;

        [GetComponent, PostFieldRichLabel(nameof(DummyNumberI))]
        public GeneralInterface[] getComponentIArray;

        [GetComponent, PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
        public List<SaintsInterface<UnityEngine.Object, IDummy>> getComponentIList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        private string DummyNumberI(GeneralInterface dummyInter)
        {
            return $"{dummyInter.I?.GetComment()}";
        }

        private string DummyNumberG(SaintsInterface<UnityEngine.Object, IDummy> dummyInter)
        {
            return $"{dummyInter}";
        }
    }
}
