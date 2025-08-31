#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DisabledDrawers.ReadOnlyDrawer
{
    public partial class ReadOnlyAttributeDrawer
    {

        private static string NameReadOnly(SerializedProperty property, int index) =>
            $"{property.propertyType}_{index}__ReadOnly";

        private static string ClassReadOnly(SerializedProperty property) => $"{property.propertyType}__ReadOnly";

        private static string NameReadOnlyHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyType}_{index}__ReadOnly_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                name = NameReadOnly(property, index),
                userData = (ReadOnlyAttribute)saintsAttribute,
            };
            root.AddToClassList(ClassReadOnly(property));
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameReadOnlyHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info)
        {
            IReadOnlyList<VisualElement> visibilityElements =
                container.Query<VisualElement>(className: ClassReadOnly(property)).ToList();
            VisualElement topElement = visibilityElements[0];

            if (topElement.name != NameReadOnly(property, index))
            {
                return;
            }

            List<VisualElement> allPossibleDisable =
                container.Query<VisualElement>(className: ClassAllowDisable).ToList();

            bool curReadOnly = allPossibleDisable.All(each => !each.enabledSelf);

            List<string> errors = new List<string>();
            // List<bool> nowReadOnlyResult = new List<bool>();
            bool nowReadOnly = false;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
            Debug.Log($"curReadOnly={curReadOnly}");
#endif
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            foreach ((string error, bool readOnly) in visibilityElements.Select(each =>
                         IsDisabled(property, info, parent)))
            {
                if (error != "")
                {
                    errors.Add(error);
                    // nowReadOnlyResult.Add(false);
                }
                else
                {
                    if (readOnly)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
                        Debug.Log($"nowReadOnly=true");
#endif
                        nowReadOnly = true;
                        break;
                    }
                }
            }

            // bool nowReadOnly = nowReadOnlyResult.Any(b => b);

            // Debug.Log($"{curReadOnly}/{nowReadOnly}");

            if (curReadOnly != nowReadOnly)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
                Debug.Log($"setReadOnly={nowReadOnly}, count={container.Query<VisualElement>(className: ClassAllowDisable).ToList().Count()}");
#endif
                // container.SetEnabled(!nowReadOnly);
                allPossibleDisable.ForEach(each => each.SetEnabled(!nowReadOnly));
            }

            HelpBox helpBox = container.Q<HelpBox>(NameReadOnlyHelpBox(property, index));
            string joinedError = string.Join("\n\n", errors);
            // ReSharper disable once InvertIf
            if (helpBox.text != joinedError)
            {
                helpBox.text = joinedError;
                helpBox.style.display = joinedError == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
    }
}
#endif
