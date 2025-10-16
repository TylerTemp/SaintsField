#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
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
            if (property.managedReferenceValue == null && checkMark != null)
            {
                checkMark.style.visibility = Visibility.Hidden;
            }
            VisualElement firstChild = toggle.Children().First();
            firstChild.style.width = Length.Percent(100);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.MakeDropdownButtonUIToolkit(getPreferredLabel);
            firstChild.Add(dropdownBtn);

            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
            dropdownBtn.ButtonLabelElement.text = GetLabel(property);

            dropdownBtn.ButtonElement.clicked += () =>
            {
                (Rect dropBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(worldBound);
                // dropBound.height = SaintsPropertyDrawer.SingleLineHeight;

                object managedReferenceValue = property.managedReferenceValue;
                AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>
                {
                    {"[Null]", null},
                };
                Dictionary<string, List<Type>> nameSpaceToTypes = new Dictionary<string, List<Type>>();
                foreach (Type type in ReferencePickerAttributeDrawer.GetTypes(property))
                {
                    string typeNamespace = type.Namespace;
                    if (string.IsNullOrEmpty(typeNamespace))
                    {
                        typeNamespace = "";
                    }
                    if (!nameSpaceToTypes.TryGetValue(typeNamespace, out List<Type> list))
                    {
                        nameSpaceToTypes[typeNamespace] = list = new List<Type>();
                    }
                    list.Add(type);
                    // string displayName = FormatTypeName(type);
                    // dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
                }

                IOrderedEnumerable<string> nameSpaceToTypesSorted = nameSpaceToTypes.Keys.OrderBy(each => each);
                foreach (string @namespace in nameSpaceToTypesSorted)
                {
                    List<Type> types = nameSpaceToTypes[@namespace];
                    // types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    AdvancedDropdownList<Type> namespaceTypes = new AdvancedDropdownList<Type>(@namespace == ""? "[No Namespace]": @namespace);
                    foreach (Type eachType in types)
                    {
                        namespaceTypes.Add(eachType.Name, eachType);
                    }
                    dropdownList.Add(namespaceTypes);
                }

                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurDisplay = managedReferenceValue == null
                        ? "-"
                        : managedReferenceValue.GetType().Name,
                    CurValues = managedReferenceValue == null? Array.Empty<object>(): new []{managedReferenceValue},
                    DropdownListValue = dropdownList,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                UnityEditor.PopupWindow.Show(dropBound, new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    worldBound.width,
                    maxHeight,
                    false,
                    (curItem, _) =>
                    {
                        object instance = curItem == null
                            ? null
                            : ReferencePickerAttributeDrawer.CopyObj(managedReferenceValue, Activator.CreateInstance((Type)curItem));

                        property.managedReferenceValue = instance;
                        property.serializedObject.ApplyModifiedProperties();
                        return null;
                    }
                ));
            };
        }

        private static string GetLabel(SerializedProperty property)
        {
            if (!SerializedUtils.IsOk(property))
            {
                return "";
            }

            object v = property.managedReferenceValue;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (v == null)
            {
                return "-";
            }

            return FormatTypeName(v.GetType());
        }

        private static string FormatTypeName(Type type)
        {
            return $"{type.Name} <color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>{type.Namespace}</color>";
        }
    }
}
#endif
