using System;
using System.Collections;
using SaintsField.Playa;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ContextMenuExample : SaintsMonoBehaviour
    {
        [CustomContextMenu(nameof(MyCallback))]  // use default name
        [CustomContextMenu(nameof(Func1), "Custom/Debug")]  // use sub item
        [CustomContextMenu(nameof(Func2), "Custom/Set")]  // use sub item
        [CustomContextMenu(":Debug.Log", "$" + nameof(DynamicMenuCallback))]
        [CustomContextMenu(nameof(DynamicMenuInfoClick), "$" + nameof(DynamicMenuInfo))]
        public string content;

        private void MyCallback()
        {
            Debug.Log("clicked on MyCallback");
        }
        private void Func1(string c)  // you can accept the current field's value
        {
            Debug.Log(c);
        }
        private void Func2()
        {
            content = "Hi There";
        }

        public string DynamicMenuCallback()  // dynamic control the item name;
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

        public (string menuName, EContextMenuStatus menuStatus) DynamicMenuInfo()  // control it's label & status
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
            return ($"My Menu {status}", status);
        }

        [CustomContextMenu(nameof(MyGenerator))] // use default name
        public int myInt;

        private IEnumerator MyGenerator()
        {
            for (int i = 0; i < 100; i++)
            {
                Debug.Log(i);
                myInt = i;
                yield return null;
            }
        }

        [CustomContextMenu("$" + nameof(ResetIntV))]
        [ShowInInspector] private int _intV;

        private void ResetIntV()
        {
            _intV = 0;
        }

        [CustomContextMenu]  // The whole component will have this context menu
        private void Wizard()
        {
        }

        public string f1;
        public string f2;
        [CustomContextMenu(":Debug.Log", "f3 Config")]
        public string[] f3;

        [Serializable]
        public struct MyStruct
        {
            public string myString;

            [CustomContextMenu("Set My String")]  // The whole `MyStruct` type will have this context menu
            public void SetString()
            {
                myString = "My Struct Value";
            }
        }

        public MyStruct myStructWithContextMenu;
    }
}
