using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts
{
    public class TableExampleSo : SaintsMonoBehaviour
    {
        [Table]
        // [GetScriptableObject]
        public Scriptable[] scriptableArray;

        // [GetScriptableObject, Expandable]
        // public Scriptable[] rawScriptableArray;
    }
}
