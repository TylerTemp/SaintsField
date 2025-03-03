#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ArraySizeDrawer
{
    public partial class ArraySizeAttributeDrawer
    {
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}_ArraySize_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };

            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }

        private bool _dymamic = true;
        private int _min;
        private int _max;

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            // SerializedProperty targetProperty = property;
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                return;
            }

            (SerializedProperty arrProp, int _, string error) = Util.GetArrayProperty(property, info, parent);

            SetHelpBox(error, property, container);

            ArraySizeAttribute arraySizeAttribute = (ArraySizeAttribute)saintsAttribute;

            // ReSharper disable once MergeIntoPattern
            if (error != "")
            {
                return;
            }

            if(_dymamic)
            {
                (string callbackError, bool dynamic, int min, int max) =
                    GetMinMax(arraySizeAttribute, property, info, parent);
                _dymamic = dynamic;
                if (callbackError != "")
                {
                    SetHelpBox(callbackError, property, container);
                    return;
                }

                _min = min;
                _max = max;
            }

            bool changed = false;
            int curSize = arrProp.arraySize;

            // Debug.Log($"{curSize}: {_min}/{_max}");

            if (_min >= 0 && curSize < _min)
            {
                // Debug.Log($"change array size {curSize} to min {arraySizeAttribute.Min}");
                arrProp.arraySize = _min;
                changed = true;
            }

            if (_max >= 0 && curSize > _max)
            {
                // Debug.Log($"change array size {curSize} to max {arraySizeAttribute.Max}");
                arrProp.arraySize = _max;
                changed = true;
            }

            if (changed)
            {
                arrProp.serializedObject.ApplyModifiedProperties();
            }
        }

        private static void SetHelpBox(string error, SerializedProperty property, VisualElement container)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            // ReSharper disable once InvertIf
            if (error != helpBox.text)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

    }
}
#endif
