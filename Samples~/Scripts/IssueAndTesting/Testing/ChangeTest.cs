using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class ChangeTest: SaintsMonoBehaviour
    {
        [SerializeField, OnValueChanged(nameof(EditorWriteText))] private long _editorNumber;

        private void EditorWriteText(long v)
        {
            Debug.Log(v);
        }
    }
}
