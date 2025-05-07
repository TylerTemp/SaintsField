using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue45
{
    public class GetScriptableObjectArray : MonoBehaviour
    {
        [GetScriptableObject] public ScriptableInter12 dummy;
        [GetScriptableObject] public ScriptableInter12[] dummies;
        [GetScriptableObject] public List<ScriptableInter12> dummyGos;

        [Serializable]
        public class GeneralInterface : SaintsInterface<Object, IDummy>
        {
            public GeneralInterface(Object obj) : base(obj)
            {
            }
        }

        [GetScriptableObject, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [GetScriptableObject, PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
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
