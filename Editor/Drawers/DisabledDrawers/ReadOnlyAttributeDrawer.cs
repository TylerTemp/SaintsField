using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DisabledDrawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    [CustomPropertyDrawer(typeof(DisableIfAttribute))]
    public class ReadOnlyAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        // protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        // {
        //     (string error, bool disabled) = IsDisabled(property, (ReadOnlyAttribute)saintsAttribute, info, parent.GetType(), parent);
        //     _error = error;
        //     EditorGUI.BeginDisabledGroup(disabled);
        //     return -1;
        // }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            (string error, bool disabled) = IsDisabled(property, info, parent);
            _error = error;
            EditorGUI.BeginDisabledGroup(disabled);
            return position;
        }

        // protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        // {
        //     EditorGUI.EndDisabledGroup();
        //     return true;
        // }

        protected override void OnPropertyEndImGui(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            EditorGUI.EndDisabledGroup();
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
            // return true;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            // EditorGUI.EndDisabledGroup();

            if (_error == "")
            {
                return position;
            }

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position, ImGuiHelpBox.GetHeight(_error, position.width, MessageType.Error));
            ImGuiHelpBox.Draw(errorRect, _error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            // Debug.Log("check extra height!");
            if (_error == "")
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected virtual (string error, bool disabled) IsDisabled(SerializedProperty property, FieldInfo info, object target)
        {
            List<bool> allResults = new List<bool>();

            ReadOnlyAttribute[] targetAttributes = SerializedUtils.GetAttributesAndDirectParent<ReadOnlyAttribute>(property).attributes;
            foreach (var targetAttribute in targetAttributes)
            {
                (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(targetAttribute.ConditionInfos, property, info, target);

                if (errors.Count > 0)
                {
                    return (string.Join("\n\n", errors), false);
                }

                bool editorModeOk = Util.ConditionEditModeChecker(targetAttribute.EditorMode);
                // And Mode
                bool boolResultsOk = boolResults.All(each => each);
                allResults.Add(editorModeOk && boolResultsOk);
            }

            // Or Mode
            bool truly = allResults.Any(each => each);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
            Debug.Log($"{property.name} final={truly}/ars={string.Join(",", allResults)}");
#endif
            return ("", truly);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameReadOnly(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ReadOnly";
        private static string ClassReadOnly(SerializedProperty property) => $"{property.propertyType}__ReadOnly";
        private static string NameReadOnlyHelpBox(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ReadOnly_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                name = NameReadOnly(property, index),
                userData = (ReadOnlyAttribute) saintsAttribute,
            };
            root.AddToClassList(ClassReadOnly(property));
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
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
            int index, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            IReadOnlyList<VisualElement> visibilityElements = container.Query<VisualElement>(className: ClassReadOnly(property)).ToList();
            VisualElement topElement = visibilityElements[0];

            if (topElement.name != NameReadOnly(property, index))
            {
                return;
            }

            List<VisualElement> allPossibleDisable = container.Query<VisualElement>(className: ClassAllowDisable).ToList();

            bool curReadOnly = allPossibleDisable.All(each => !each.enabledSelf);

            List<string> errors = new List<string>();
            // List<bool> nowReadOnlyResult = new List<bool>();
            bool nowReadOnly = false;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
            Debug.Log($"curReadOnly={curReadOnly}");
#endif
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            foreach ((string error, bool readOnly) in visibilityElements.Select(each => IsDisabled(property, info, parent)))
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

        #endregion

#endif
    }
}
