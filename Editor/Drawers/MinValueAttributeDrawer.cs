using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MinValueAttribute))]
    public class MinValueAttributeDrawer : SaintsPropertyDrawer
    {
        private string _error = "";

        // protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     EditorGUI.BeginChangeCheck();
        //     return (true, position);
        // }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            if (!valueChanged)
            {
                return true;
            }

            object parentTarget = GetParentTarget(property);

            if (property.propertyType == SerializedPropertyType.Float)
            {
                float curValue = property.floatValue;
                MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
                float valueLimit;
                if (minValueAttribute.ValueCallback == null)
                {
                    valueLimit = minValueAttribute.Value;
                }
                else
                {
                    (float getValueLimit, string getError) = Util.GetCallbackFloat(parentTarget, minValueAttribute.ValueCallback);
                    valueLimit = getValueLimit;
                    _error = getError;
                }

                if (_error != "")
                {
                    return true;
                }

                if (valueLimit > curValue)
                {
                    property.floatValue = valueLimit;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int curValue = property.intValue;
                MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
                float valueLimit;
                if (minValueAttribute.ValueCallback == null)
                {
                    valueLimit = minValueAttribute.Value;
                }
                else
                {
                    (float getValueLimit, string getError) = Util.GetCallbackFloat(parentTarget, minValueAttribute.ValueCallback);
                    valueLimit = getValueLimit;
                    _error = getError;
                }

                if (_error != "")
                {
                    return true;
                }

                if (valueLimit > curValue)
                {
                    property.intValue = (int)valueLimit;
                }
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
