using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedAttributeDrawer : SaintsPropertyDrawer
    {
        private string _error = "";

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            // Debug.Log($"OnValueChangedAttributeDrawer={valueChanged}");
            if (!valueChanged)
            {
                return true;
            }

            property.serializedObject.ApplyModifiedProperties();

            string callback = ((OnValueChangedAttribute)saintsAttribute).Callback;
            object target = GetParentTarget(property);

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            MethodInfo methodInfo =  target.GetType().GetMethod(callback, bindAttr);
            if (methodInfo == null)
            {
                _error = $"no method found `{callback}` on `{target}`";
                return true;
            }

            _error = "";

            ParameterInfo[] methodParams = methodInfo.GetParameters();
            Debug.Assert(methodParams.All(p => p.IsOptional));
            try
            {
                methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
            }
            catch (TargetInvocationException e)
            {
                Debug.Assert(e.InnerException != null);
                _error = e.InnerException.Message;
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                _error = e.Message;
                Debug.LogException(e);
            }

            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
