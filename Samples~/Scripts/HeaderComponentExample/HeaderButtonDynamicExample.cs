
using System.Collections;
using System.Linq;
using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderButtonDynamicExample: SaintsMonoBehaviour
    {
        private string _editButtonIcon = "<icon=pencil.png/>";
        private bool _editing;

        [HeaderGhostButton("$" + nameof(_editButtonIcon), "Edit")]
        private void StartEdit()
        {
            _editing = true;
            _editButtonIcon = "";

            _saveLabel = "<color=brown><icon=save.png/>";
        }

        [HeaderGhostButton("$" + nameof(_saveLabel), "Save")]
        private IEnumerator Click()
        {
            _editing = false;
            _saveLabel = "<color=gray><icon=save.png/>";
            foreach (int i in Enumerable.Range(0, 200))
            {
                // Debug.Log($"saving {i}");
                yield return null;
            }
            _saveLabel = "<color=lime><icon=check.png/>";
            foreach (int i in Enumerable.Range(0, 200))
            {
                // Debug.Log($"checked {i}");
                yield return null;
            }
            _saveLabel = "";

            _editButtonIcon = "<icon=pencil.png/>";
        }

        private string _saveLabel = "";

        [EnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public string nickName;
        [EnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public string password;
        [EnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public int age;

        private void OnChanged() => _saveLabel = "<color=lime><icon=save.png/>";
    }
}
