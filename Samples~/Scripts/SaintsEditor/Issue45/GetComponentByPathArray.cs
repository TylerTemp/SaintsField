using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issue45
{
    public class GetComponentByPathArray : SaintsMonoBehavior
    {
        [GetComponentByPath("*"), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentByPathArray;
        [GetComponentByPath("*[1]"), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentByPathList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }

        [GetComponentByPath("*"), PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [GetComponentByPath("*[1]"), PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
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
