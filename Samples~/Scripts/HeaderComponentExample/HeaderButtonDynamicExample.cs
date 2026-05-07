
using System.Collections;
using System.Linq;
using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderButtonDynamicExample: SaintsMonoBehaviour
    {
#pragma warning disable 0414
        private string _editButtonIcon = "<icon=pencil.png/>";
        private bool _editing;
#pragma warning restore 0414

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

#pragma warning disable 0414
        private string _saveLabel = "";
#pragma warning restore 0414

        [FieldEnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public string nickName;
        [FieldEnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public string password;
        [FieldEnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public int age;

        private void OnChanged() => _saveLabel = "<color=lime><icon=save.png/>";
    }
}
