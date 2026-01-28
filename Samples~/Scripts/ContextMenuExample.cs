using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ContextMenuExample : MonoBehaviour
    {
        [FieldCustomContextMenu(nameof(MyCallback))]
        [FieldCustomContextMenu(nameof(Func1), "Custom/Debug")]
        [FieldCustomContextMenu(nameof(Func2), "Custom/Set")]
        [FieldCustomContextMenu(":Debug.Log", "$" + nameof(DynamicMenuCallback))]
        [FieldCustomContextMenu(nameof(DynamicMenuInfoClick), "$" + nameof(DynamicMenuInfo))]
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

        public string DynamicMenuCallback()
        {
            return $"Random {Random.Range(0, 9)}";
        }

        public bool hasContextMenu;
        public bool isChecked;
        public bool isDisabled;

        private void DynamicMenuInfoClick()
        {
            isChecked = !isChecked;
        }

        public (string, EContextMenuStatus) DynamicMenuInfo()
        {
            if (!hasContextMenu)
            {
                return (null, default);
            }
            EContextMenuStatus status = EContextMenuStatus.Normal;
            if (isChecked)
            {
                status = EContextMenuStatus.Checked;
            }
            else if (isDisabled)
            {
                status = EContextMenuStatus.Disabled;
            }
            return ("My Menu", status);
            // return $"Random {Random.Range(0, 9)}";
        }
    }
}
