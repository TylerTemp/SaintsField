using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue72 : MonoBehaviour
    {
        [GetComponent(EXP.Silent | EXP.NoPicker)] public Dummy issue72Dummy;
        [GetComponent(EXP.Silent | EXP.NoPicker | EXP.NoResignButton)] public Dummy issue72DummySimple;
    }
}
