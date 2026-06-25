using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.OnValueChangedCollectionFakeRenderer
{
    public partial class OnValueChangedCollectionRenderer : AbsRenderer
    {
        protected override bool AllowGuiColor => false;

        private readonly OnValueChangedAttribute _onValueChangedAttribute;
        private int _curLength = -1;

        public OnValueChangedCollectionRenderer(OnValueChangedAttribute onValueChangedAttribute,
            SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject,
            fieldWithInfo)
        {
            SerializedProperty prop = fieldWithInfo.SerializedProperty;
            Debug.Assert(prop.propertyType == SerializedPropertyType.Generic && prop.isArray,
                $"{prop.propertyPath} is not an array/list");
            _onValueChangedAttribute = onValueChangedAttribute;
        }

        public override void OnSearchField(string searchString)
        {
        }

        protected (bool changed, string error) CheckCollectionLengthChanged()
        {
            SerializedProperty prop = FieldWithInfo.SerializedProperty;
            if (_curLength < 0)
            {
                _curLength = prop.arraySize;
                return (false, "");
            }

            int newLength = prop.arraySize;
            int diffCount = newLength - _curLength;
            if (diffCount == 0)
            {
                return (false, "");
            }

            int oldLength = _curLength;
            _curLength = newLength;

            MemberInfo memberInfo = (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo;
            object parent = FieldWithInfo.Targets[0];

            if (diffCount > 0)
            {
                List<string> errors = new List<string>();
                for (int index = oldLength; index < newLength; index++)
                {
                    (string addError, MemberInfo _, object _) = Util.GetOf<object>(
                        _onValueChangedAttribute.Callback,
                        null,
                        prop.GetArrayElementAtIndex(index),
                        memberInfo,
                        parent,
                        null);
                    if (addError != "")
                    {
                        errors.Add(addError);
                    }
                }

                return (true, string.Join("\n", errors));
            }

            (string valueError, int _, object curValue) = Util.GetValue(prop, memberInfo, parent);
            if (valueError != "")
            {
                return (true, valueError);
            }

            (string error, MemberInfo _, object _) = Util.GetOf<object>(
                _onValueChangedAttribute.Callback,
                null,
                prop,
                memberInfo,
                parent,
                new[] { curValue, diffCount });
            return (true, error);
        }
    }
}
