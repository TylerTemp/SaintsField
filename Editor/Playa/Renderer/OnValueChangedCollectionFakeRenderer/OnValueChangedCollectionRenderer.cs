using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.OnValueChangedCollectionFakeRenderer
{
    public class OnValueChangedCollectionRenderer: AbsRenderer
    {
        protected override bool AllowGuiColor => false;

        private readonly OnValueChangedAttribute _onValueChangedAttribute;

        public OnValueChangedCollectionRenderer(OnValueChangedAttribute onValueChangedAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            SerializedProperty prop = fieldWithInfo.SerializedProperty;
            Debug.Assert(prop.propertyType == SerializedPropertyType.Generic && prop.isArray, $"{prop.propertyPath} is not an array/list");
            _onValueChangedAttribute = onValueChangedAttribute;
        }

        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }

        #region Deprecated
        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }
        #endregion

        private int _curLength;
        private HelpBox _helpBox;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            // Debug.Log(FieldWithInfo.SerializedProperty.propertyPath);
            SerializedProperty prop = FieldWithInfo.SerializedProperty;
            _curLength = prop.arraySize;
            // Debug.Log($"watch {prop.propertyPath}; curLength={curLength}");
            // container.TrackPropertyValue(prop, _ =>
            // {
            //     int newLength = prop.arraySize;
            //     Debug.Log($"newLength={newLength}");
            //     if (curLength == newLength)
            //     {
            //         return;
            //     }
            //
            //     Debug.Log($"{curLength} -> {newLength}");
            //     curLength = newLength;
            // });
            return (_helpBox = new HelpBox
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            }, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);
            SerializedProperty prop = FieldWithInfo.SerializedProperty;
            int newLength = prop.arraySize;
            // Debug.Log($"newLength={newLength}");
            int diffCount = newLength - _curLength;
            if (diffCount == 0)
            {
                return baseResult;
            }

            int oldLength = _curLength;
            _curLength = newLength;

            // Debug.Log($"{oldLength} -> {newLength}");

            MemberInfo memberInfo = (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo;
            object parent = FieldWithInfo.Targets[0];
            (string valueError, int _, object curValue) = Util.GetValue(prop, memberInfo, parent);
            if (valueError != "")
            {
                UIToolkitUtils.SetHelpBox(_helpBox, valueError);
                return baseResult;
            }


            if (diffCount > 0)  // Add more
            {
                List<string> errors = new List<string>();
                // IReadOnlyList<object> curValueList = ((IEnumerable)curValue).Cast<object>().ToArray();
                for (int index = oldLength; index < newLength; index++)
                {
                    // object addValue = curValueList[index];
                    (string addError, object _) = Util.GetOf<object>(_onValueChangedAttribute.Callback, null, prop.GetArrayElementAtIndex(index), memberInfo, parent, null);
                    if (addError != "")
                    {
                        errors.Add(addError);
                    }
                }

                string addErrorJoin = string.Join("\n", errors);
                UIToolkitUtils.SetHelpBox(_helpBox, addErrorJoin);
                return baseResult;
            }
            // remove some
            (string error, object _) = Util.GetOf<object>(_onValueChangedAttribute.Callback, null, prop, memberInfo, parent, new[]{curValue, diffCount});
            UIToolkitUtils.SetHelpBox(_helpBox, error);
            return baseResult;
        }
    }
}
