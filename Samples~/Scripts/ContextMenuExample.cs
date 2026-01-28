using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ContextMenuExample : MonoBehaviour
    {
        [FieldCustomContextMenu(nameof(MyCallback))]
        [FieldCustomContextMenu(nameof(Func1), "Custom/Debug")]
        [FieldCustomContextMenu(nameof(Func2), "Custom/Set")]
        public string content;

        private void MyCallback()
        {
            Debug.Log("clicked on MyCallback");
        }

        private void Func1(string c)
        {
            Debug.Log(c);
        }
        private void Func2()
        {
            content = "Hi There";
        }
    }
}
