using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue45
{
    public class GetPrefabArray : SaintsMonoBehaviour
    {
        [GetPrefabWithComponent] public Dummy dummy;
        [GetPrefabWithComponent] public Dummy[] dummies;
        [GetPrefabWithComponent(typeof(Dummy))] public List<GameObject> dummyGos;

        [Serializable]
        public class GeneralInterface : SaintsInterface<Object, IDummy>
        {
            public GeneralInterface(Object obj) : base(obj)
            {
            }
        }

        [GetPrefabWithComponent, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;
        [GetPrefabWithComponent(typeof(Dummy)), PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
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
