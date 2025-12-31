#if UNITY_2021_3_OR_NEWER
using System;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ReferencePicker
{

    public class FoldoutField: FoldoutPrefabOverrideElement
    {
        public FoldoutField(SerializedProperty property, string getPreferredLabel): base(property)
        {
            Toggle toggle = this.Q<Toggle>();
            VisualElement checkMark = toggle.Q<VisualElement>("unity-checkmark");

            if(checkMark != null)
            {
                PropertyChangedEvent.AddListener(RefreshFoldoutCheck);
                RefreshFoldoutCheck();
            }
            VisualElement firstChild = toggle.Children().First();
            firstChild.style.width = Length.Percent(100);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField(getPreferredLabel, property, this, () => ReferencePickerAttributeDrawer.GetTypes(property));
            firstChild.Add(dropdownBtn);

            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
            return;

            void RefreshFoldoutCheck()
            {
                if (!SerializedUtils.IsOk(property))
                {
                    UIToolkitUtils.Unbind(this);
                    return;
                }

                // Debug.Log(property.managedReferenceValue);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                try
                {
                    if (property.managedReferenceValue == null)
                    {
                        checkMark.style.visibility = Visibility.Hidden;
                    }
                    else
                    {
                        checkMark.style.visibility = Visibility.Visible;
                    }
                }
                catch (InvalidOperationException)
                {
                    // ignored
                }
            }
        }


    }
}
#endif
