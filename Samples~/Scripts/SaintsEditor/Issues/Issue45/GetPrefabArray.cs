using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetPrefabArray : SaintsMonoBehaviour
    {
        [GetPrefabWithComponent] public Dummy[] dummies;
        [GetPrefabWithComponent(typeof(Dummy))] public List<GameObject> dummyGos;


        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }

        [GetPrefabWithComponent, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getComponentIArray;

        [GetPrefabWithComponent(typeof(Dummy)), PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
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
