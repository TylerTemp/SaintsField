using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue292
{
    [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Issue/Sub", order = 0)]
    public class Sub : Parent
    {
        [Button]
        public virtual void InSub(){}

        public override void DoSomething()
        {
            Debug.Log("DoSomething Sub");
        }

        [Button]
        public new void InParentNew() => Debug.Log("InParentNew.SubNew");
    }
}
