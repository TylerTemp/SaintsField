using System;
using SaintsField.Playa;
using UnityEditor;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativeFieldPropertyRenderer: AbsRenderer
    {
        protected bool RenderField;

        public NativeFieldPropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            RenderField = fieldWithInfo.PlayaAttributes.Any(each => each is ShowInInspectorAttribute);
            if (RenderField && FieldWithInfo.PropertyInfo != null)
            {
                RenderField = FieldWithInfo.PropertyInfo.CanRead;
            }
        }

        public override void OnDestroy()
        {
        }

        private static object GetValue(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo != null)
                return fieldWithInfo.FieldInfo.GetValue(fieldWithInfo.Target);

            return fieldWithInfo.PropertyInfo.CanRead
                ? fieldWithInfo.PropertyInfo.GetValue(fieldWithInfo.Target)
                : null;
        }

        private static string GetName(SaintsFieldWithInfo fieldWithInfo) =>
            fieldWithInfo.PropertyInfo?.Name ?? fieldWithInfo.FieldInfo.Name;

        private static string GetNiceName(SaintsFieldWithInfo fieldWithInfo) =>
            ObjectNames.NicifyVariableName(GetName(fieldWithInfo));

        private static Action<object> GetSetterOrNull(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo != null)
            {
                if (fieldWithInfo.FieldInfo.IsLiteral || fieldWithInfo.FieldInfo.IsInitOnly)
                {
                    return null;
                }
                return value => fieldWithInfo.FieldInfo.SetValue(fieldWithInfo.Target, value);
            }

            if (fieldWithInfo.PropertyInfo.CanWrite)
            {
                return value => fieldWithInfo.PropertyInfo.SetValue(fieldWithInfo.Target, value);
            }

            return null;

            // MethodInfo prop = fieldWithInfo.PropertyInfo?.GetSetMethod(true);
            // if (prop != null)
            // {
            //     return value => prop.Invoke(fieldWithInfo.Target, new[] {value});
            // }

            // return null;
        }

        private static Type GetFieldType(SaintsFieldWithInfo fieldWithInfo) =>
            fieldWithInfo.FieldInfo?.FieldType ?? fieldWithInfo.PropertyInfo.PropertyType;

        public override string ToString()
        {
            return $"<NativeFP {GetFriendlyName(FieldWithInfo)}/>";
        }
    }
}
