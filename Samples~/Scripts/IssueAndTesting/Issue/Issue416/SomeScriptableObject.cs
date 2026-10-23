using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue416
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(menuName = "SaintsField Debug/Test Scriptable Object")]
#endif
    public class SomeScriptableObject : ScriptableObject, ISomeInterface
    {

    }
}
