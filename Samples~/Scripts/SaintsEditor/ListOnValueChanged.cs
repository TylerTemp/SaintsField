using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ListOnValueChanged : SaintsMonoBehaviour
    {
        [OnValueChanged(nameof(OnArrayChanged))] public string[] arrayChanged;

        private void OnArrayChanged(string content, int index)
        {
            Debug.Log($"array[{index}]={content}");
        }

        private void OnArrayChanged(string[] arrayItself, int removedCount)
        {
            Debug.Log($"array.length removed {-removedCount}, current length={arrayItself.Length}");
        }
    }
}
