using UnityEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue233.Editor
{
    // No. This does not work. struct field needs a property drawer, not Editor
    // But SaintsPropertyDrawer does not support `Button` etc.

    // [CustomEditor(typeof(Issue233Test.Nest1), true)]
    // public class ApplyNest1: SaintsField.Editor.SaintsEditor
    // {
    //
    // }
    //
    // [CustomEditor(typeof(Issue233Test.Nest2), true)]
    // public class ApplyNest2: SaintsField.Editor.SaintsEditor
    // {
    //
    // }

}
