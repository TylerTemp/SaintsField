using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue383
{
    public class Derived : BaseM
    {
        protected override void Editor_ButtonClicked() => Debug.Log("Clicked Derived Class!");
    }
}
