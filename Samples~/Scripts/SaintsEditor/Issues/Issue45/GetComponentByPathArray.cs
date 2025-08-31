using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetComponentByPathArray : SaintsMonoBehaviour
    {
        [GetComponentByPath("*"), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentByPathArray;
        [GetComponentByPath("*[1]"), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentByPathList;

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

        // saintsInterface is broken atm
        // TODO: fix this
        // [GetComponentByPath("*"), PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        // public GeneralInterface[] getComponentIArray;

        // saintsInterface is broken atm
        // TODO: fix this
        // [GetComponentByPath("*[1]"), PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
        // public List<SaintsInterface<UnityEngine.Object, IDummy>> getComponentIList;

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
