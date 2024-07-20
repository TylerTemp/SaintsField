using System;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue45
{
    public class GetScriptableObjectArray : SaintsMonoBehaviour
    {
        // [GetScriptableObject] public ScriptableInter12[] dummies;
        // [GetScriptableObject] public List<ScriptableInter12> dummyGos;

        [Serializable]
        public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }

        [GetScriptableObject, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        public GeneralInterface[] getSoIArray;

        // [GetScriptableObject, PostFieldRichLabel(nameof(DummyNumberG), isCallback: true)]
        // public List<SaintsInterface<UnityEngine.Object, IDummy>> getSoIList;

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
