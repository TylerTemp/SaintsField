using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue45
{
    public class GetComponentInChildrenArray : SaintsMonoBehaviour
    {
        // [GetComponentInChildren, PostFieldRichLabel("$" + nameof(DummyNumber))] public Dummy getComponentInChildren;
        [GetComponentInChildren, PostFieldRichLabel("$" + nameof(DummyNumber))] public Dummy[] getComponentInChildrenArray;
        // [GetByXPath("@{GetComponents(Dummy)}", ".//*@{GetComponents(Dummy)}"), PostFieldRichLabel("$" + nameof(DummyNumber))] public Dummy[] getComponentInChildrenArrayXPath;
        // [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInChildrenList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        // [Serializable]
        // public class GeneralInterface : SaintsInterface<UnityEngine.Object, IDummy> { }
        //
        // // [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumberI), isCallback: true)]
        // // public GeneralInterface[] getComponentIArray;
        //
        // [GetComponentInChildren, PostFieldRichLabel("$" + nameof(DummyNumberG))]
        // public List<SaintsInterface<UnityEngine.Object, IDummy>> getComponentIList;
        //
        // private string DummyNumberI(GeneralInterface dummyInter)
        // {
        //     return dummyInter == null? "":  $"{dummyInter.I.GetComment()}";
        // }
        //
        // private string DummyNumberG(SaintsInterface<UnityEngine.Object, IDummy> dummyInter)
        // {
        //     return dummyInter == null? "":  $"{dummyInter.I.GetComment()}";
        // }
    }
}
