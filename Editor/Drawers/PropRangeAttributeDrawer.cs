using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PropRangeAttribute))]
    public class PropRangeAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            PropRangeAttribute propRangeAttribute = (PropRangeAttribute) saintsAttribute;

            object parentTarget = GetParentTarget(property);

            float minValue;
            if (propRangeAttribute.MinCallback == null)
            {
                minValue = propRangeAttribute.Min;
            }
            else
            {
                (float getValue, string getError) = Util.GetCallbackFloat(parentTarget, propRangeAttribute.MinCallback);
                _error = getError;
                minValue = getValue;
            }

            float maxValue;
            if (propRangeAttribute.MaxCallback == null)
            {
                maxValue = propRangeAttribute.Max;
            }
            else
            {
                (float getValue, string getError) = Util.GetCallbackFloat(parentTarget, propRangeAttribute.MaxCallback);
                _error = getError;
                maxValue = getValue;
            }

            if (_error != "")
            {
                DefaultDrawer(position, property, label);
                return;
            }

            if (maxValue <= minValue)
            {
                _error = $"max({maxValue}) should be greater than min({minValue})";
                DefaultDrawer(position, property, label);
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool isFloat = property.propertyType == SerializedPropertyType.Float;
                float curValue = isFloat ? property.floatValue : property.intValue;
                float newValue = EditorGUI.Slider(position, label, curValue, minValue, maxValue);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    // property.floatValue = newValue;
                    float step = propRangeAttribute.Step;
                    // Debug.Log(step);
                    if (step <= 0)
                    {
                        if (isFloat)
                        {
                            property.floatValue = newValue;
                        }
                        else
                        {
                            property.intValue = (int)newValue;
                        }
                    }
                    else
                    {
                        if (isFloat)
                        {
                            property.floatValue = Util.BoundFloatStep(newValue, minValue, maxValue, step);
                        }
                        else
                        {
                            property.intValue = Util.BoundIntStep(newValue, minValue, maxValue, (int)step);
                        }
                    }
                }
            }
        }


        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);

    }
}
