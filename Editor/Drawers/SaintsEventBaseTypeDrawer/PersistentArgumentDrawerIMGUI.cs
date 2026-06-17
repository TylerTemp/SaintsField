#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Events;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public partial class PersistentArgumentDrawer
    {
        private sealed class EventArgumentContextIMGUI
        {
            public Type[] EventParamTypes = Array.Empty<Type>();
            public IReadOnlyList<string> EventArgNames = Array.Empty<string>();
            public SerializedProperty PersistentCallProp;
            public int ArgumentIndex;
        }

        private const float ParamButtonWidth = 20f;
        private const float FieldGap = 2f;
        private const float MinLabelWidth = 60f;

        private static GUIStyle _richMiniButtonStyle;
        private static GUIStyle _richLabelStyle;
        private static GUIStyle _richPopupLabelStyle;

        protected override bool UseCreateFieldIMGUI => true;

        private static GUIStyle RichMiniButtonStyle => _richMiniButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
        {
            richText = true,
            padding = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
        };

        private static GUIStyle RichLabelStyle => _richLabelStyle ??= new GUIStyle(EditorStyles.label)
        {
            richText = true,
            clipping = TextClipping.Clip,
            alignment = TextAnchor.MiddleLeft,
        };

        private static GUIStyle RichPopupLabelStyle => _richPopupLabelStyle ??= new GUIStyle(EditorStyles.label)
        {
            richText = true,
            clipping = TextClipping.Clip,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(4, 16, 0, 0),
        };

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SerializedProperty callTypeProp = property.FindPropertyRelative(nameof(PersistentArgument.callType));
            SerializedProperty isOptionalProp = property.FindPropertyRelative(nameof(PersistentArgument.isOptional));
            SerializedProperty isUnityObjectProp = property.FindPropertyRelative(nameof(PersistentArgument.isUnityObject));
            SerializedProperty invokedParameterIndexProp =
                property.FindPropertyRelative(nameof(PersistentArgument.invokedParameterIndex));
            SerializedProperty unityObjectProp = property.FindPropertyRelative(nameof(PersistentArgument.unityObject));
            SerializedProperty serializeJsonDataProp =
                property.FindPropertyRelative(nameof(PersistentArgument.serializeJsonData));

            Type argumentType = GetArgumentType(property);
            EventArgumentContextIMGUI eventContext = GetEventArgumentContext(property);

            Rect buttonRect = new Rect(position)
            {
                width = ParamButtonWidth,
            };
            if (GUI.Button(buttonRect, GetCallTypeContent((PersistentArgument.CallType)callTypeProp.intValue),
                    RichMiniButtonStyle))
            {
                ShowCallTypeMenu(buttonRect, property, callTypeProp, isOptionalProp);
            }

            Rect contentRect = new Rect(position)
            {
                xMin = buttonRect.xMax + FieldGap,
            };
            float labelWidth = Mathf.Clamp(contentRect.width * 0.25f, MinLabelWidth,
                Mathf.Max(MinLabelWidth, contentRect.width * 0.45f));
            Rect labelRect = new Rect(contentRect)
            {
                width = Mathf.Min(labelWidth, contentRect.width),
            };
            Rect valueRect = new Rect(contentRect)
            {
                xMin = labelRect.xMax + FieldGap,
            };

            GUI.Label(labelRect, GetArgumentLabel(property, argumentType), RichLabelStyle);
            DrawValue(valueRect, property, (PersistentArgument.CallType)callTypeProp.intValue, argumentType,
                eventContext, callTypeProp, invokedParameterIndexProp, isUnityObjectProp, unityObjectProp,
                serializeJsonDataProp);
        }

        private void DrawValue(Rect rect, SerializedProperty property, PersistentArgument.CallType callType,
            Type argumentType, EventArgumentContextIMGUI eventContext, SerializedProperty callTypeProp,
            SerializedProperty invokedParameterIndexProp, SerializedProperty isUnityObjectProp,
            SerializedProperty unityObjectProp, SerializedProperty serializeJsonDataProp)
        {
            switch (callType)
            {
                case PersistentArgument.CallType.Dynamic:
                    if (DrawDropdownButton(rect, GetDynamicLabel(argumentType, eventContext,
                            invokedParameterIndexProp.intValue)))
                    {
                        ShowDynamicMenu(rect, property, argumentType, eventContext, callTypeProp,
                            invokedParameterIndexProp);
                    }
                    break;
                case PersistentArgument.CallType.Serialized:
                    DrawSerializedValue(rect, property, argumentType, isUnityObjectProp, unityObjectProp,
                        serializeJsonDataProp);
                    break;
                case PersistentArgument.CallType.OptionalDefault:
                    GUI.Label(rect, GetOptionalDefaultDisplay(eventContext), RichLabelStyle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(callType), callType, null);
            }
        }

        private static bool DrawDropdownButton(Rect rect, string richLabel)
        {
            bool clicked = GUI.Button(rect, GUIContent.none, EditorStyles.popup);
            GUI.Label(new Rect(rect) { xMin = rect.xMin + 2f, xMax = rect.xMax - 16f }, richLabel,
                RichPopupLabelStyle);
            return clicked;
        }

        private static GUIContent GetCallTypeContent(PersistentArgument.CallType callType) => callType switch
        {
            PersistentArgument.CallType.Dynamic => new GUIContent("<color=#00ffff>D</color>", "Dynamic"),
            PersistentArgument.CallType.Serialized => new GUIContent("<color=#00ff00>S</color>", "Serialized"),
            PersistentArgument.CallType.OptionalDefault => new GUIContent("", "Use Default"),
            _ => new GUIContent("?", callType.ToString()),
        };

        private void ShowCallTypeMenu(Rect rect, SerializedProperty property, SerializedProperty callTypeProp,
            SerializedProperty isOptionalProp)
        {
            GenericMenu menu = new GenericMenu();
            AddCallTypeMenuItem(menu, property, callTypeProp, PersistentArgument.CallType.Dynamic, "Dynamic");
            AddCallTypeMenuItem(menu, property, callTypeProp, PersistentArgument.CallType.Serialized, "Serialized");
            menu.AddSeparator("");
            if (isOptionalProp.boolValue)
            {
                AddCallTypeMenuItem(menu, property, callTypeProp, PersistentArgument.CallType.OptionalDefault,
                    "Use Default");
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Use Default"));
            }

            menu.DropDown(rect);
        }

        private void AddCallTypeMenuItem(GenericMenu menu, SerializedProperty property,
            SerializedProperty callTypeProp, PersistentArgument.CallType callType, string label)
        {
            menu.AddItem(new GUIContent(label), callTypeProp.intValue == (int)callType, () =>
            {
                callTypeProp.intValue = (int)callType;
                NotifyChanged(property);
            });
        }

        private static string GetArgumentLabel(SerializedProperty property, Type argumentType)
        {
            string typeLabel = argumentType == null
                ? ""
                : $" <color=#808080>({SaintsEventUtils.StringifyType(argumentType)})</color>";
            return $"{property.FindPropertyRelative(nameof(PersistentArgument.name)).stringValue}{typeLabel}";
        }

        private static string GetDynamicLabel(Type argumentType, EventArgumentContextIMGUI eventContext, int index)
        {
            if (index < 0 || index >= eventContext.EventParamTypes.Length)
            {
                return "?";
            }

            Type eventType = eventContext.EventParamTypes[index];
            bool canAssign = argumentType != null && argumentType.IsAssignableFrom(eventType);
            string prefix = canAssign ? "" : "<color=red>x</color> ";
            string argName = eventContext.EventArgNames.Count > index ? eventContext.EventArgNames[index] : "Arg";
            return $"{prefix}<color=#808080>[{index}]</color> {argName} <color=#808080>({SaintsEventUtils.StringifyType(eventType)})</color>";
        }

        private void ShowDynamicMenu(Rect rect, SerializedProperty property, Type argumentType,
            EventArgumentContextIMGUI eventContext, SerializedProperty callTypeProp,
            SerializedProperty invokedParameterIndexProp)
        {
            GenericMenu menu = new GenericMenu();
            for (int eventParamIndex = 0; eventParamIndex < eventContext.EventParamTypes.Length; eventParamIndex++)
            {
                string useName = eventContext.EventArgNames.Count > eventParamIndex
                    ? eventContext.EventArgNames[eventParamIndex]
                    : "Arg";
                Type type = eventContext.EventParamTypes[eventParamIndex];
                string labelText = $"[{eventParamIndex}] {useName} ({SaintsEventUtils.StringifyType(type)})";
                int thisIndex = eventParamIndex;
                bool canAssign = argumentType != null && argumentType.IsAssignableFrom(type);
                if (canAssign)
                {
                    menu.AddItem(new GUIContent(labelText), invokedParameterIndexProp.intValue == eventParamIndex,
                        () =>
                        {
                            invokedParameterIndexProp.intValue = thisIndex;
                            callTypeProp.intValue = (int)PersistentArgument.CallType.Dynamic;
                            NotifyChanged(property);
                        });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(labelText));
                }
            }

            menu.DropDown(rect);
        }

        private void DrawSerializedValue(Rect rect, SerializedProperty property, Type argumentType,
            SerializedProperty isUnityObjectProp, SerializedProperty unityObjectProp,
            SerializedProperty serializeJsonDataProp)
        {
            if (isUnityObjectProp.boolValue || typeof(Object).IsAssignableFrom(argumentType ?? typeof(object)))
            {
                Type objectType = argumentType != null && typeof(Object).IsAssignableFrom(argumentType)
                    ? argumentType
                    : typeof(Object);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Object newValue = EditorGUI.ObjectField(rect, GUIContent.none, unityObjectProp.objectReferenceValue,
                        objectType, true);
                    if (changed.changed)
                    {
                        unityObjectProp.objectReferenceValue = newValue;
                        if (newValue != null)
                        {
                            isUnityObjectProp.boolValue = true;
                        }

                        NotifyChanged(property);
                    }
                }

                return;
            }

            if (!TryDrawSerializedPrimitive(rect, property, argumentType, serializeJsonDataProp))
            {
                GUI.Label(rect, GetSerializedValueDisplay(argumentType, serializeJsonDataProp.stringValue),
                    RichLabelStyle);
            }
        }

        private bool TryDrawSerializedPrimitive(Rect rect, SerializedProperty property, Type argumentType,
            SerializedProperty serializeJsonDataProp)
        {
            if (argumentType == null)
            {
                return false;
            }

            object currentValue = LoadSerializedValue(argumentType, serializeJsonDataProp.stringValue);
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            object newValue;
            if (argumentType == typeof(int))
            {
                newValue = EditorGUI.IntField(rect, GUIContent.none, currentValue is int value ? value : 0);
            }
            else if (argumentType == typeof(long))
            {
                newValue = EditorGUI.LongField(rect, GUIContent.none, currentValue is long value ? value : 0L);
            }
            else if (argumentType == typeof(float))
            {
                newValue = EditorGUI.FloatField(rect, GUIContent.none, currentValue is float value ? value : 0f);
            }
            else if (argumentType == typeof(double))
            {
                newValue = EditorGUI.DoubleField(rect, GUIContent.none, currentValue is double value ? value : 0d);
            }
            else if (argumentType == typeof(bool))
            {
                newValue = EditorGUI.Toggle(rect, GUIContent.none, currentValue is bool value && value);
            }
            else if (argumentType == typeof(string))
            {
                newValue = EditorGUI.TextField(rect, GUIContent.none, currentValue as string ?? "");
            }
            else if (argumentType.IsEnum)
            {
                Enum enumValue = currentValue is Enum value
                    ? value
                    : (Enum)Enum.GetValues(argumentType).GetValue(0);
                newValue = EditorGUI.EnumPopup(rect, GUIContent.none, enumValue);
            }
            else if (argumentType == typeof(Vector2))
            {
                newValue = EditorGUI.Vector2Field(rect, GUIContent.none,
                    currentValue is Vector2 value ? value : Vector2.zero);
            }
            else if (argumentType == typeof(Vector3))
            {
                newValue = EditorGUI.Vector3Field(rect, GUIContent.none,
                    currentValue is Vector3 value ? value : Vector3.zero);
            }
            else
            {
                return false;
            }

            if (changed.changed)
            {
                serializeJsonDataProp.stringValue = SerializationUtil.ToJsonType(newValue);
                property.FindPropertyRelative(nameof(PersistentArgument.isUnityObject)).boolValue = false;
                NotifyChanged(property);
            }

            return true;
        }

        private static object LoadSerializedValue(Type argumentType, string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return SerializationUtil.FromJsonType(argumentType, json);
                }
                catch (ArgumentException)
                {
                }
            }

            return ActivatorCreateInstance(argumentType);
        }

        private static string GetSerializedValueDisplay(Type argumentType, string json)
        {
            object value = LoadSerializedValue(argumentType, json);
            return value == null ? "[Null]" : value.ToString();
        }

        private static string GetOptionalDefaultDisplay(EventArgumentContextIMGUI eventContext)
        {
            if (eventContext.PersistentCallProp == null)
            {
                return "";
            }

            MethodInfo methodInfo = GetPersistentCallMethod(eventContext.PersistentCallProp);
            if (methodInfo == null)
            {
                return "<color=red>Failed to obtain method info</color>";
            }

            ParameterInfo[] methodParams = methodInfo.GetParameters();
            if (eventContext.ArgumentIndex < 0 || eventContext.ArgumentIndex >= methodParams.Length)
            {
                return "<color=red>Argument index out of range</color>";
            }

            ParameterInfo methodParam = methodParams[eventContext.ArgumentIndex];
            if (!methodParam.IsOptional)
            {
                return $"<color=red>{methodParam.Name} is not optional</color>";
            }

            object defaultValue = methodParam.DefaultValue;
            return defaultValue == null ? "[Null]" : defaultValue.ToString();
        }

        private static MethodInfo GetPersistentCallMethod(SerializedProperty persistentCallProp)
        {
            SerializedProperty persistentArgumentsProp =
                persistentCallProp.FindPropertyRelative(nameof(PersistentCall.persistentArguments));
            List<Type> argumentTypes = new List<Type>();
            for (int argumentIndex = 0; argumentIndex < persistentArgumentsProp.arraySize; argumentIndex++)
            {
                SerializedProperty argumentProp = persistentArgumentsProp.GetArrayElementAtIndex(argumentIndex);
                Type argumentType = GetArgumentType(argumentProp);
                if (argumentType == null)
                {
                    return null;
                }

                argumentTypes.Add(argumentType);
            }

            return PersistentCall.GetMethod(
                persistentCallProp.FindPropertyRelative(nameof(PersistentCall.isStatic)).boolValue,
                Type.GetType(persistentCallProp.FindPropertyRelative(nameof(PersistentCall.staticType) +
                                                                     "._typeNameAndAssembly").stringValue),
                persistentCallProp.FindPropertyRelative(nameof(PersistentCall.target)).objectReferenceValue,
                persistentCallProp.FindPropertyRelative(nameof(PersistentCall.methodName)).stringValue,
                argumentTypes.ToArray()
            ).MethodInfo;
        }

        private static EventArgumentContextIMGUI GetEventArgumentContext(SerializedProperty property)
        {
            string[] split = property.propertyPath.Split('.').SkipLast(6).ToArray();
            bool selfInsideArray = false;
            if (split.Length > 0 && split[^1].EndsWith("]"))
            {
                split = split.SkipLast(2).ToArray();
                selfInsideArray = true;
            }

            (SerializedUtils.FieldOrProp rootFieldOrProp, object _) =
                SerializedUtils.GetFieldInfoAndDirectParentByPathSegments(property, split);
            Type rawType;
            MemberInfo rawMemberInfo;
            if (rootFieldOrProp.IsField)
            {
                rawType = rootFieldOrProp.FieldInfo.FieldType;
                rawMemberInfo = rootFieldOrProp.FieldInfo;
            }
            else
            {
                rawType = rootFieldOrProp.PropertyInfo.PropertyType;
                rawMemberInfo = rootFieldOrProp.PropertyInfo;
            }

            if (selfInsideArray)
            {
                rawType = ReflectUtils.GetElementType(rawType);
            }

            SaintsEventArgsAttribute saintsEventArgsAttribute = ReflectCache
                .GetCustomAttributes<SaintsEventArgsAttribute>(rawMemberInfo)
                .FirstOrDefault();
            string callPropPath = string.Join(".", property.propertyPath.Split('.').SkipLast(3));
            return new EventArgumentContextIMGUI
            {
                EventParamTypes = rawType?.GetGenericArguments() ?? Array.Empty<Type>(),
                EventArgNames = saintsEventArgsAttribute?.ArgNames ?? Array.Empty<string>(),
                PersistentCallProp = property.serializedObject.FindProperty(callPropPath),
                ArgumentIndex = SerializedUtils.PropertyPathIndex(property.propertyPath),
            };
        }

        private void NotifyChanged(SerializedProperty property)
        {
            property.serializedObject.ApplyModifiedProperties();
            TriggerChangedIMGUI(property, null);
        }
    }
}
#endif
