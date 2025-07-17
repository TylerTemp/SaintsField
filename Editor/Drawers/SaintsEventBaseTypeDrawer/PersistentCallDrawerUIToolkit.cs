using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer.UIToolkitElements;
using SaintsField.Editor.Drawers.TypeReferenceTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public partial class PersistentCallDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static string NameRoot(SerializedProperty property) => $"{property.propertyPath}__PersistentCall";

        private static VisualTreeAsset _containerTree;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            if (_containerTree == null)
            {
                _containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/PersistentCallContainer.uxml");
            }

            TemplateContainer element = _containerTree.CloneTree();
            element.name = NameRoot(property);

            UnityEventCallStateSelector callStateSelector = element.Q<UnityEventCallStateSelector>();
            SerializedProperty callStateProperty = property.FindPropertyRelative(PropNameCallState());
            callStateSelector.BindProperty(callStateProperty);
            callStateSelector.style.flexShrink = 0;

            CallbackTypeButton callbackTypeButton = element.Q<CallbackTypeButton>();
            SerializedProperty callbackTypeProperty = property.FindPropertyRelative(PropNameIsStatic);
            callbackTypeButton.BindProperty(callbackTypeProperty);
            callbackTypeButton.style.flexShrink = 0;

            SerializedProperty targetProperty = property.FindPropertyRelative(PropNameTarget);
            ObjectField of = element.Q<ObjectField>();
            of.BindProperty(targetProperty);
            of.style.width = new StyleLength(Length.Percent(100));

            VisualElement dropdownButtonField = element.Q<VisualElement>("TypeDropdownButton");
            dropdownButtonField.style.width = new StyleLength(Length.Percent(100));

            return element;
        }

        private static readonly Dictionary<Assembly, Type[]> ToFill = new Dictionary<Assembly, Type[]>();

        private readonly struct TypeDropdownInfo: IEquatable<TypeDropdownInfo>
        {
            public readonly string DropPath;
            public readonly Type Type;
            public readonly Object ObjectRef;

            public TypeDropdownInfo(string dropPath, Type type, Object objectRef)
            {
                DropPath = dropPath;
                Type = type;
                ObjectRef = objectRef;
            }

            public bool Equals(TypeDropdownInfo other)
            {
                return Type == other.Type;
            }

            public override bool Equals(object obj)
            {
                return obj is TypeDropdownInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Type != null ? Type.GetHashCode() : 0;
            }
        }

        private readonly struct TypeDropdownGroup
        {
            public readonly string GroupName;
            public readonly IReadOnlyList<TypeDropdownInfo> Types;

            public TypeDropdownGroup(string groupName, IReadOnlyList<TypeDropdownInfo> types)
            {
                GroupName = groupName;
                Types = types;
            }
        }

        private readonly struct MethodSelect: IEquatable<MethodSelect>
        {
            public readonly Type Type;
            public readonly MethodInfo MethodInfo;

            public MethodSelect(Type type, MethodInfo methodInfo)
            {
                Type = type;
                MethodInfo = methodInfo;
            }

            public bool Equals(MethodSelect other)
            {
                return Equals(MethodInfo, other.MethodInfo);
            }

            public override bool Equals(object obj)
            {
                return obj is MethodSelect other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(Type, MethodInfo);
            }
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement root = container.Q<TemplateContainer>(NameRoot(property));
            SerializedProperty targetProperty = property.FindPropertyRelative(PropNameTarget);

            VisualElement typeDropdownButtonField = root.Q<VisualElement>("TypeDropdownButton");
            SerializedProperty isStaticProperty = property.FindPropertyRelative(PropNameIsStatic);
            Label typeDropdownButtonLabel = typeDropdownButtonField.Q<Label>();

            SerializedProperty propTypeAndAss = property.FindPropertyRelative(PropNameTypeNameAndAssmble);
            typeDropdownButtonLabel.text = FormatTypeNameAndAssmbleLabel(propTypeAndAss.stringValue);
            typeDropdownButtonLabel.TrackPropertyValue(propTypeAndAss, sp => typeDropdownButtonLabel.text = FormatTypeNameAndAssmbleLabel(sp.stringValue));
            typeDropdownButtonLabel.text = propTypeAndAss.stringValue;
            typeDropdownButtonField.Q<Button>().clicked += () =>
            {
                bool isStatic = isStaticProperty.boolValue;
                Object uObj = targetProperty.objectReferenceValue;
                List<TypeDropdownGroup> typeDropdownGroups = new List<TypeDropdownGroup>();
                if (isStatic)
                {
                    Type staticRefType = uObj == null ? null : uObj.GetType();
                    if (staticRefType is null)
                    {
                        Dictionary<string, List<TypeDropdownInfo>> assToGroup = new Dictionary<string, List<TypeDropdownInfo>>();
                        IEnumerable<Assembly> allAss = TypeReferenceDrawer.GetAssembly(EType.AllAssembly, null);
                        TypeReferenceDrawer.FillAsssembliesTypes(allAss, ToFill);
                        foreach ((Assembly ass, Type[] assTypes) in ToFill)
                        {
                            string assName = TypeReference.GetShortAssemblyName(ass);
                            // IReadOnlyList<(string dropPath, Type type)> types = assTypes
                            //     .Select(each => (TypeReferenceDrawer.FormatPath(each, 0, false), each)).ToArray();
                            // typeDropdownGroups.Add(new TypeDropdownGroup(assName, types));

                            if (!assToGroup.TryGetValue(assName, out List<TypeDropdownInfo> lis))
                            {
                                assToGroup[assName] = lis = new List<TypeDropdownInfo>();
                            }

                            foreach (Type assType in assTypes)
                            {
                                lis.Add(new TypeDropdownInfo(TypeReferenceDrawer.FormatPath(assType, 0, false), assType, null));
                            }
                        }

                        foreach ((string k, List<TypeDropdownInfo> v)  in assToGroup.OrderBy(each => each.Key))
                        {
                            typeDropdownGroups.Add(new TypeDropdownGroup(k, v));
                        }
                    }
                    else
                    {
                        typeDropdownGroups.Add(new TypeDropdownGroup(null, GetUObjExpandTypes(uObj)));
                    }
                }
                else
                {
                    if (targetProperty.objectReferenceValue == null)
                    {
                        return;
                    }
                    typeDropdownGroups.Add(new TypeDropdownGroup(null, GetUObjExpandTypes(uObj)));
                }

                AdvancedDropdownMetaInfo meta = GetTypeDropdownMeta(Type.GetType(propTypeAndAss.stringValue), typeDropdownGroups);
                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(typeDropdownButtonField.worldBound);
                SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                    meta,
                    typeDropdownButtonField.worldBound.width,
                    maxHeight,
                    false,
                    (newDisplay, curItem) =>
                    {
                        typeDropdownButtonLabel.text = newDisplay;
                        TypeDropdownInfo newTypeInfo = (TypeDropdownInfo)curItem;
                        Type newType = newTypeInfo.Type;
                        propTypeAndAss.stringValue = TypeReference.GetTypeNameAndAssembly(newType);

                        if (newTypeInfo.ObjectRef != targetProperty.objectReferenceValue)
                        {
                            targetProperty.objectReferenceValue = newTypeInfo.ObjectRef;
                        }

                        property.serializedObject.ApplyModifiedProperties();
                    }
                );
                UnityEditor.PopupWindow.Show(worldBound, sa);
            };

            SerializedProperty propMethodName = property.FindPropertyRelative(PropMethodName);
            VisualElement methodDropdownButtonField = root.Q<VisualElement>("MethodDropdownButton");
            Label methodDropdownButtonLabel = methodDropdownButtonField.Q<Label>();
            methodDropdownButtonLabel.text = propMethodName.stringValue;

            GetMethodLabel(property);

            methodDropdownButtonLabel.TrackPropertyValue(property, GetMethodLabel);
            SerializedProperty propPersistentArguments = property.FindPropertyRelative(PropNamePersistentArguments);
            methodDropdownButtonField.Q<Button>().clicked += () =>
            {
                bool isStatic = isStaticProperty.boolValue;
                Object uObj = targetProperty.objectReferenceValue;
                string typeNameAndAss = propTypeAndAss.stringValue;

                Type type = uObj == null ? Type.GetType(typeNameAndAss) : uObj.GetType();

                if (type == null)
                {
                    return;
                }

                (bool isValidMethodInfo, IReadOnlyList<Type> methonParamTypes, Type returnType) = GetMethodParamsType(property);
                MethodSelect selectedMethod = default;

                const BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                const BindingFlags staticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                BindingFlags bf = isStatic ? staticFlags : instanceFlags;
                List<MethodSelect> methodSelects = new List<MethodSelect>();
                foreach (Type subType in ReflectUtils.GetSelfAndBaseTypesFromType(type))
                {
                    foreach (MethodInfo methodInfo in subType.GetMethods(bf))
                    {
                        if (methodInfo.IsGenericMethod)  // this is not supported, so far...
                        {
                            continue;
                        }

                        if (methodInfo.DeclaringType != subType)
                        {
                            continue;
                        }

                        MethodSelect methodSelect = new MethodSelect(subType, methodInfo);
                        // ReSharper disable once InvertIf
                        if(!methodSelects.Contains(methodSelect))
                        {
                            methodSelects.Add(methodSelect);

                            // ReSharper disable once InvertIf
                            if (isValidMethodInfo)
                            {
                                if (methodInfo.Name == propMethodName.stringValue &&
                                    methodInfo.ReturnType == returnType &&
                                    methodInfo.GetParameters().Select(p => p.ParameterType).SequenceEqual(methonParamTypes))
                                {
                                    selectedMethod = methodSelect;
                                }
                            }
                        }
                    }
                }

                AdvancedDropdownMetaInfo meta = GetMethodDropdownMeta(selectedMethod, methodSelects, false);
                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(methodDropdownButtonField.worldBound);
                SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                    meta,
                    methodDropdownButtonField.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        MethodSelect mi = (MethodSelect)curItem;
                        if (mi.MethodInfo == null)
                        {
                            propMethodName.stringValue = "";
                            propPersistentArguments.arraySize = 0;
                        }
                        else
                        {
                            propMethodName.stringValue = mi.MethodInfo.Name;
                            property.FindPropertyRelative(PropNameReturnTypeNameAndAssmbly).stringValue =
                                TypeReference.GetTypeNameAndAssembly(mi.MethodInfo.ReturnType);
                            ParameterInfo[] methodParams = mi.MethodInfo.GetParameters();
                            propPersistentArguments.arraySize = methodParams.Length;
                            for (int i = 0; i < methodParams.Length; i++)
                            {
                                SerializedProperty persistentArgument = propPersistentArguments.GetArrayElementAtIndex(i);
                                ParameterInfo param = methodParams[i];
                                persistentArgument.FindPropertyRelative(PropNamePersistentArgumentIsOptional).boolValue =
                                    param.IsOptional;
                                persistentArgument.FindPropertyRelative(PropNamePersistentArgumentNameAndAssmbly)
                                    .stringValue = TypeReference.GetTypeNameAndAssembly(param.ParameterType);
                            }

                        }

                        property.serializedObject.ApplyModifiedProperties();
                    }
                );
                UnityEditor.PopupWindow.Show(worldBound, sa);
            };
            return;
            // UnityEventCallStateSelector callStateSelector = container.Q<UnityEventCallStateSelector>();
            // SerializedProperty callStateProperty = property.FindPropertyRelative(PropNameCallState());
            // callStateSelector.Bind(callStateProperty.serializedObject);

            void GetMethodLabel(SerializedProperty prop)
            {
                (bool isValidMethodInfo, IReadOnlyList<Type> methonParamTypes, Type returnType) = GetMethodParamsType(prop);
                methodDropdownButtonLabel.text = isValidMethodInfo ? StringifyMethod(propMethodName.stringValue, methonParamTypes, returnType) : "-";
            }
        }

        private static string FormatTypeNameAndAssmbleLabel(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "-";
            }

            string[] splitC = name.Split(',');

            string firstPart = splitC[0].Trim();
            string[] firstDots = firstPart.Split('.');

            StringBuilder sb = new StringBuilder(firstDots[0]);
            sb.Append("<color=#808080>");
            for (int i = 1; i < firstDots.Length; i++)
            {
                sb.Append('.').Append(firstDots[i]);
            }
            sb.Append(",");
            for (int index = 1; index < splitC.Length; index++)
            {
                sb.Append(splitC[index]);
            }
            sb.Append("</color>");
            return sb.ToString();
        }

        private IReadOnlyList<TypeDropdownInfo> GetUObjExpandTypes(Object uObj) => GetUObjExpand(uObj)
            .Select(expandedObj =>
            {
                Type assType = expandedObj.GetType();
                return new TypeDropdownInfo(
                    TypeReferenceDrawer.FormatPath(assType, 0, false), assType, expandedObj);
            })
            .ToList();

        private static IEnumerable<Object> GetUObjExpand(Object uObj)
        {
            switch (uObj)
            {
                case GameObject go:
                {
                    yield return go;
                    foreach (Component component in go.GetComponents<Component>())
                    {
                        yield return component;
                    }
                }
                    break;
                case Component comp:
                {
                    GameObject go = comp.gameObject;
                    foreach (Object uObjExpand in GetUObjExpand(go))
                    {
                        yield return uObjExpand;
                    }
                }
                    break;
                default:
                    yield return uObj;
                    break;
            }
        }
    }
}
