using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class SavedThenDontSave : MonoBehaviour
    {
        [TextArea]
        public string text = "I'm restored if you click \"Remove Saved Values\"";
    }
}
