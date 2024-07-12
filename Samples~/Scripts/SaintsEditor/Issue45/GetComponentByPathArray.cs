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
    }
}
