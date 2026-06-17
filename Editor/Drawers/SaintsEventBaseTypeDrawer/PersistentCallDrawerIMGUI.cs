#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.TypeReferenceTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Events;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public partial class PersistentCallDrawer
    {
        private const float CompactButtonWidth = 20f;
        private const float RowGap = 2f;
        private const float FieldGap = 2f;
        private const float MethodIndent = 20f;

        private static GUIStyle _richMiniButtonStyle;
        private static GUIStyle _richLabelStyle;

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
            padding = new RectOffset(4, 16, 0, 0),
        };

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            SerializedProperty argumentsProp = property.FindPropertyRelative(nameof(PersistentCall.persistentArguments));
            float height = EditorGUIUtility.singleLineHeight * 2f + RowGap;
            for (int argumentIndex = 0; argumentIndex < argumentsProp.arraySize; argumentIndex++)
            {
                SerializedProperty argumentProp = argumentsProp.GetArrayElementAtIndex(argumentIndex);
                height += RowGap + EditorGUI.GetPropertyHeight(argumentProp, GUIContent.none, true);
            }

            return height;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SerializedProperty callStateProp = property.FindPropertyRelative(PropNameCallState());
            SerializedProperty isStaticProp = property.FindPropertyRelative(nameof(PersistentCall.isStatic));
            SerializedProperty targetProp = property.FindPropertyRelative(nameof(PersistentCall.target));
            SerializedProperty typeNameProp = property.FindPropertyRelative(nameof(PersistentCall.staticType) + SubPropNameTypeNameAndAssmble);
            SerializedProperty methodNameProp = property.FindPropertyRelative(nameof(PersistentCall.methodName));
            SerializedProperty argumentsProp = property.FindPropertyRelative(nameof(PersistentCall.persistentArguments));

            Rect rowRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            DrawTargetRow(rowRect, property, callStateProp, isStaticProp, targetProp, typeNameProp);

            Rect methodRect = new Rect(position)
            {
                y = rowRect.yMax + RowGap,
                x = position.x + MethodIndent,
                width = Mathf.Max(0f, position.width - MethodIndent),
                height = EditorGUIUtility.singleLineHeight,
            };
            DrawMethodRow(methodRect, property, isStaticProp, targetProp, typeNameProp, methodNameProp, argumentsProp);

            float y = methodRect.yMax + RowGap;
            for (int argumentIndex = 0; argumentIndex < argumentsProp.arraySize; argumentIndex++)
            {
                SerializedProperty argumentProp = argumentsProp.GetArrayElementAtIndex(argumentIndex);
                float argumentHeight = EditorGUI.GetPropertyHeight(argumentProp, GUIContent.none, true);
                Rect argumentRect = new Rect(position.x + MethodIndent, y, Mathf.Max(0f, position.width - MethodIndent),
                    argumentHeight);
                EditorGUI.PropertyField(argumentRect, argumentProp, GUIContent.none, true);
                y += argumentHeight + RowGap;
            }
        }

        private void DrawTargetRow(Rect rowRect, SerializedProperty property, SerializedProperty callStateProp,
            SerializedProperty isStaticProp, SerializedProperty targetProp, SerializedProperty typeNameProp)
        {
            Rect callStateRect = new Rect(rowRect)
            {
                width = CompactButtonWidth,
            };
            if (GUI.Button(callStateRect, GetCallStateContent(callStateProp.intValue), RichMiniButtonStyle))
            {
                ShowCallStateMenu(callStateRect, property, callStateProp);
            }

            Rect callbackRect = new Rect(rowRect)
            {
                x = callStateRect.xMax + FieldGap,
                width = CompactButtonWidth,
            };
            if (GUI.Button(callbackRect, GetCallbackTypeContent(isStaticProp.boolValue), RichMiniButtonStyle))
            {
                isStaticProp.boolValue = !isStaticProp.boolValue;
                NotifyChanged(property);
            }

            Rect remainingRect = new Rect(rowRect)
            {
                xMin = callbackRect.xMax + FieldGap,
            };
            float objectWidth = Mathf.Max(70f, remainingRect.width * 0.45f);
            Rect targetRect = new Rect(remainingRect)
            {
                width = Mathf.Min(objectWidth, remainingRect.width),
            };
            Rect typeRect = new Rect(remainingRect)
            {
                xMin = targetRect.xMax + FieldGap,
            };

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object newTarget = EditorGUI.ObjectField(targetRect, GUIContent.none, targetProp.objectReferenceValue,
                    typeof(Object), true);
                if (changed.changed)
                {
                    targetProp.objectReferenceValue = newTarget;
                    NotifyChanged(property);
                }
            }

            DrawTypeDropdown(typeRect, property, isStaticProp, targetProp, typeNameProp);
        }

        private void DrawTypeDropdown(Rect rect, SerializedProperty property, SerializedProperty isStaticProp,
            SerializedProperty targetProp, SerializedProperty typeNameProp)
        {
            if (DrawDropdownButton(rect, FormatTypeNameAndAssmbleLabel(typeNameProp.stringValue)))
            {
                ShowTypeDropdown(rect, property, isStaticProp, targetProp, typeNameProp);
            }
        }

        private void DrawMethodRow(Rect rect, SerializedProperty property, SerializedProperty isStaticProp,
            SerializedProperty targetProp, SerializedProperty typeNameProp, SerializedProperty methodNameProp,
            SerializedProperty argumentsProp)
        {
            if (DrawDropdownButton(rect, GetMethodDisplay(property, isStaticProp, targetProp, typeNameProp, methodNameProp)))
            {
                ShowMethodDropdown(rect, property, isStaticProp, targetProp, typeNameProp, methodNameProp, argumentsProp);
            }
        }

        private static bool DrawDropdownButton(Rect rect, string richLabel)
        {
            bool clicked = GUI.Button(rect, GUIContent.none, EditorStyles.popup);
            GUI.Label(new Rect(rect) { xMin = rect.xMin + 2f, xMax = rect.xMax - 16f }, richLabel, RichLabelStyle);
            return clicked;
        }

        private static GUIContent GetCallStateContent(int callStateValue)
        {
            UnityEventCallState callState = (UnityEventCallState)callStateValue;
            return callState switch
            {
                UnityEventCallState.Off => new GUIContent("", "Off"),
                UnityEventCallState.EditorAndRuntime => new GUIContent("<color=#00ff00>E</color>", "Editor & Runtime"),
                UnityEventCallState.RuntimeOnly => new GUIContent("<color=#00ffff>R</color>", "Runtime Only"),
                _ => new GUIContent("?", callState.ToString()),
            };
        }

        private static GUIContent GetCallbackTypeContent(bool isStatic) => isStatic
            ? new GUIContent("<color=#ffa500>S</color>", "Static")
            : new GUIContent("I", "Instance");

        private void ShowCallStateMenu(Rect rect, SerializedProperty property, SerializedProperty callStateProp)
        {
            GenericMenu menu = new GenericMenu();
            AddCallStateMenuItem(menu, property, callStateProp, UnityEventCallState.Off, "Off");
            AddCallStateMenuItem(menu, property, callStateProp, UnityEventCallState.EditorAndRuntime,
                "Editor & Runtime");
            AddCallStateMenuItem(menu, property, callStateProp, UnityEventCallState.RuntimeOnly, "Runtime Only");
            menu.DropDown(rect);
        }

        private void AddCallStateMenuItem(GenericMenu menu, SerializedProperty property,
            SerializedProperty callStateProp, UnityEventCallState callState, string label)
        {
            menu.AddItem(new GUIContent(label), callStateProp.intValue == (int)callState, () =>
            {
                callStateProp.intValue = (int)callState;
                NotifyChanged(property);
            });
        }

        private void ShowTypeDropdown(Rect rect, SerializedProperty property, SerializedProperty isStaticProp,
            SerializedProperty targetProp, SerializedProperty typeNameProp)
        {
            List<TypeDropdownGroup> typeDropdownGroups = GetTypeDropdownGroups(property, isStaticProp, targetProp);
            if (typeDropdownGroups.Count == 0)
            {
                return;
            }

            AdvancedDropdownMetaInfo meta = GetTypeDropdownMeta(Type.GetType(typeNameProp.stringValue),
                typeDropdownGroups);
            PopupWindow.Show(rect, new SaintsTreeDropdownIMGUI(
                meta,
                Mathf.Max(rect.width, 220f),
                320f,
                false,
                (curItem, _) =>
                {
                    TypeDropdownInfo newTypeInfo = (TypeDropdownInfo)curItem;
                    Type newType = newTypeInfo.Type;
                    typeNameProp.stringValue = newType == null ? "" : TypeReference.GetTypeNameAndAssembly(newType);
                    if (newTypeInfo.ObjectRef != targetProp.objectReferenceValue)
                    {
                        targetProp.objectReferenceValue = newTypeInfo.ObjectRef;
                    }

                    NotifyChanged(property);
                    return new[] { curItem };
                }));
        }

        private void ShowMethodDropdown(Rect rect, SerializedProperty property, SerializedProperty isStaticProp,
            SerializedProperty targetProp, SerializedProperty typeNameProp, SerializedProperty methodNameProp,
            SerializedProperty argumentsProp)
        {
            (MethodSelect selectedMethod, IReadOnlyList<MethodSelect> methodSelects) =
                GetMethods(property, isStaticProp, targetProp, typeNameProp, methodNameProp);
            AdvancedDropdownMetaInfo meta = GetMethodDropdownMeta(selectedMethod, methodSelects);
            PopupWindow.Show(rect, new SaintsTreeDropdownIMGUI(
                meta,
                Mathf.Max(rect.width, 220f),
                320f,
                false,
                (curItem, _) =>
                {
                    ApplySelectedMethod(property, methodNameProp, argumentsProp, (MethodSelect)curItem);
                    return null;
                }));
        }

        private void ApplySelectedMethod(SerializedProperty property, SerializedProperty methodNameProp,
            SerializedProperty argumentsProp, MethodSelect methodSelect)
        {
            if (methodSelect.MethodInfo == null)
            {
                methodNameProp.stringValue = "";
                argumentsProp.arraySize = 0;
            }
            else
            {
                methodNameProp.stringValue = methodSelect.MethodInfo.Name;
                property.FindPropertyRelative(nameof(PersistentCall.returnType) + SubPropNameTypeNameAndAssmble)
                    .stringValue = TypeReference.GetTypeNameAndAssembly(methodSelect.MethodInfo.ReturnType);
                ParameterInfo[] methodParams = methodSelect.MethodInfo.GetParameters();
                argumentsProp.arraySize = methodParams.Length;
                for (int i = 0; i < methodParams.Length; i++)
                {
                    SerializedProperty persistentArgument = argumentsProp.GetArrayElementAtIndex(i);
                    ParameterInfo param = methodParams[i];
                    persistentArgument.FindPropertyRelative(nameof(PersistentArgument.isOptional)).boolValue =
                        param.IsOptional;
                    persistentArgument.FindPropertyRelative(nameof(PersistentArgument.typeReference) +
                                                           SubPropNameTypeNameAndAssmble)
                        .stringValue = TypeReference.GetTypeNameAndAssembly(param.ParameterType);
                    persistentArgument.FindPropertyRelative(nameof(PersistentArgument.name)).stringValue = param.Name;
                }
            }

            NotifyChanged(property);
        }

        private List<TypeDropdownGroup> GetTypeDropdownGroups(SerializedProperty property,
            SerializedProperty isStaticProp, SerializedProperty targetProp)
        {
            bool isStatic = isStaticProp.boolValue;
            Object uObj = targetProp.objectReferenceValue;
            List<TypeDropdownGroup> typeDropdownGroups = new List<TypeDropdownGroup>();
            if (isStatic)
            {
                Type staticRefType = uObj == null ? null : uObj.GetType();
                if (staticRefType is null)
                {
                    Dictionary<string, List<TypeDropdownInfo>> assToGroup =
                        new Dictionary<string, List<TypeDropdownInfo>>();
                    TypeReferenceAttribute typeReferenceAttribute = GetTypeReferenceAttribute(property);
                    IEnumerable<Assembly> allAss = TypeReferenceDrawer.GetAssembly(typeReferenceAttribute,
                        property.serializedObject);
                    TypeReferenceDrawer.FillAssembliesTypes(allAss, ToFill);
                    foreach ((Assembly ass, Type[] assTypes) in ToFill)
                    {
                        string assName = TypeReference.GetShortAssemblyName(ass);
                        if (!assToGroup.TryGetValue(assName, out List<TypeDropdownInfo> list))
                        {
                            assToGroup[assName] = list = new List<TypeDropdownInfo>();
                        }

                        foreach (Type assType in assTypes)
                        {
                            list.Add(new TypeDropdownInfo(
                                TypeReferenceDrawer.FormatPath(assType, 0, true, false), assType, null));
                        }
                    }

                    foreach ((string key, List<TypeDropdownInfo> value) in assToGroup.OrderBy(each => each.Key))
                    {
                        typeDropdownGroups.Add(new TypeDropdownGroup(key, value));
                    }
                }
                else
                {
                    typeDropdownGroups.Add(new TypeDropdownGroup(null, GetUObjExpandTypes(uObj)));
                }
            }
            else
            {
                if (targetProp.objectReferenceValue != null)
                {
                    typeDropdownGroups.Add(new TypeDropdownGroup(null, GetUObjExpandTypes(uObj)));
                }
            }

            return typeDropdownGroups;
        }

        private static TypeReferenceAttribute GetTypeReferenceAttribute(SerializedProperty property)
        {
            string[] split = property.propertyPath.Split('.').SkipLast(3).ToArray();
            if (split.Length > 0 && split[^1].EndsWith("]"))
            {
                split = split.SkipLast(2).ToArray();
            }

            (SerializedUtils.FieldOrProp rootFieldOrProp, object _) =
                SerializedUtils.GetFieldInfoAndDirectParentByPathSegments(property, split);
            MemberInfo rootMemberInfo = rootFieldOrProp.IsField
                ? rootFieldOrProp.FieldInfo
                : rootFieldOrProp.PropertyInfo;
            return ReflectCache.GetCustomAttributes<TypeReferenceAttribute>(rootMemberInfo).FirstOrDefault();
        }

        private string GetMethodDisplay(SerializedProperty property, SerializedProperty isStaticProp,
            SerializedProperty targetProp, SerializedProperty typeNameProp, SerializedProperty methodNameProp)
        {
            if (string.IsNullOrEmpty(methodNameProp.stringValue))
            {
                return "-";
            }

            (bool isValidMethodInfo, IReadOnlyList<Type> methodParamTypes, Type returnType) =
                GetMethodParamsType(property);
            if (!isValidMethodInfo)
            {
                return "-";
            }

            string useText = StringifyMethod(methodNameProp.stringValue, methodParamTypes, returnType);
            (MethodSelect selectedMethod, IReadOnlyList<MethodSelect> _) =
                GetMethods(property, isStaticProp, targetProp, typeNameProp, methodNameProp);
            if (selectedMethod.MethodInfo == null)
            {
                useText = $"<color=red>?</color> <color=#808080>{useText}</color>";
            }

            return useText;
        }

        private void NotifyChanged(SerializedProperty property)
        {
            property.serializedObject.ApplyModifiedProperties();
            TriggerChangedIMGUI(property, null);
        }
    }
}
#endif
