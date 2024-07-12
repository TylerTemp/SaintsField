using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issue45
{
    public class GetComponentInSceneArray : SaintsMonoBehavior
    {
        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInParentArray;
        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInParentsList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
