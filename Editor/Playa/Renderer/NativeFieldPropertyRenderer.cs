using System;
using SaintsField.Playa;
using UnityEditor;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

#if UNITY_2021_3_OR_NEWER
        private UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif
        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }

        private static (string error, object value) GetValue(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo != null)
            {
                return ("", fieldWithInfo.FieldInfo.GetValue(fieldWithInfo.Targets[0]));
            }

            if (fieldWithInfo.PropertyInfo.CanRead)
            {
                try
                {
                    return ("", fieldWithInfo.PropertyInfo.GetValue(fieldWithInfo.Targets[0]));
                }
                catch (Exception e)
                {
                    string message = e.InnerException?.Message ?? e.Message;
                    return (message, null);
                }
            }

            return ($"Can not get value", null);
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
                return value => fieldWithInfo.FieldInfo.SetValue(fieldWithInfo.Targets[0], value);
            }

            if (fieldWithInfo.PropertyInfo.CanWrite)
            {
                return value => fieldWithInfo.PropertyInfo.SetValue(fieldWithInfo.Targets[0], value);
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
