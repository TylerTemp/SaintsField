using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Events;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsEventBase), true)]
    public partial class SaintsEventBaseDrawer: SaintsPropertyDrawer
    {
        private const string PropNamePersistentCalls = "_persistentCalls";
        private const string SubPropNameTypeNameAndAssmble = "._typeNameAndAssembly";
        private const string SubPropMonoScriptGuid = "._monoScriptGuid";

        internal sealed class SaintsEventContext
        {
            public string Error;
            public SerializedProperty RootProperty;
            public SerializedProperty PersistentCallsProp;
            public FieldInfo Info;
            public object Parent;
            public string Label;
        }

        internal static (string error, SaintsEventContext context) GetSaintsEventContext(SerializedProperty property,
            GUIContent label, FieldInfo info, object parent)
        {
            SerializedProperty persistentCallProp = property.FindPropertyRelative(PropNamePersistentCalls);
            if (persistentCallProp == null)
            {
                string error = $"{PropNamePersistentCalls} not found in {property.propertyPath}";
                return (error, new SaintsEventContext
                {
                    Error = error,
                    RootProperty = property,
                    Info = info,
                    Parent = parent,
                    Label = label?.text ?? "",
                });
            }

            return ("", new SaintsEventContext
            {
                Error = "",
                RootProperty = property,
                PersistentCallsProp = persistentCallProp,
                Info = info,
                Parent = parent,
                Label = GetEventLabel(property, label?.text ?? ""),
            });
        }

        internal static string GetEventLabel(SerializedProperty property, string rawLabel)
        {
            string useLabel = string.IsNullOrEmpty(rawLabel) ? null : rawLabel;
            IReadOnlyList<Type> genericTypes = GetEventParamTypes(property);
            if (string.IsNullOrEmpty(useLabel) || genericTypes.Count == 0)
            {
                return useLabel;
            }

            return $"{useLabel} ({string.Join(", ", genericTypes.Select(SaintsEventUtils.StringifyType))})";
        }

        internal static IReadOnlyList<Type> GetEventParamTypes(SerializedProperty property)
        {
            int propIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            (SerializedUtils.FieldOrProp rootFieldOrProp, object _) = SerializedUtils.GetFieldInfoAndDirectParent(property);
            Type rawType = rootFieldOrProp.IsField
                ? rootFieldOrProp.FieldInfo.FieldType
                : rootFieldOrProp.PropertyInfo.PropertyType;
            if (propIndex >= 0)
            {
                rawType = ReflectUtils.GetElementType(rawType);
            }

            return rawType?.GetGenericArguments() ?? Array.Empty<Type>();
        }

        internal static void PersistentCallAdd(SerializedProperty persistentCallProp, bool isStatic)
        {
            int index = persistentCallProp.arraySize;
            persistentCallProp.arraySize = index + 1;
            SerializedProperty persistentCallElement = persistentCallProp.GetArrayElementAtIndex(index);
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.isStatic)).boolValue = isStatic;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.callState)).intValue =
                (int)UnityEventCallState.RuntimeOnly;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.methodName)).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.target)).objectReferenceValue = null;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.persistentArguments)).arraySize = 0;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.staticType) + SubPropNameTypeNameAndAssmble).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.staticType) + SubPropMonoScriptGuid).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.returnType) + SubPropNameTypeNameAndAssmble).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.returnType) + SubPropMonoScriptGuid).stringValue = "";
            persistentCallElement.serializedObject.ApplyModifiedProperties();
        }
    }
}
