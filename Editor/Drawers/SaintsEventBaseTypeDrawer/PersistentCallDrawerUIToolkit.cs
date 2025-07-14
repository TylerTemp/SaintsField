using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer.UIToolkitElements;
using SaintsField.Editor.Drawers.TypeReferenceTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
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

            VisualElement dropdownButtonField = element.Q<VisualElement>("DropdownButton");
            dropdownButtonField.style.width = new StyleLength(Length.Percent(100));

            return element;
        }

        private static readonly Dictionary<Assembly, Type[]> ToFill = new Dictionary<Assembly, Type[]>();

        private readonly struct TypeDropdownGroup
        {
            public readonly string GroupName;
            public readonly IReadOnlyList<(string dropPath, Type type, MethodInfo methodInfo)> Types;

            public TypeDropdownGroup(string groupName, IReadOnlyList<(string dropPath, Type type, MethodInfo methodInfo)> types)
            {
                GroupName = groupName;
                Types = types;
            }
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement root = container.Q<TemplateContainer>(NameRoot(property));
            SerializedProperty targetProperty = property.FindPropertyRelative(PropNameTarget);

            VisualElement dropdownButtonField = root.Q<VisualElement>("DropdownButton");
            SerializedProperty isStaticProperty = property.FindPropertyRelative(PropNameIsStatic);
            dropdownButtonField.Q<Button>().clicked += () =>
            {
                bool isStatic = isStaticProperty.boolValue;
                Object uObj = targetProperty.objectReferenceValue;
                List<TypeDropdownGroup> typeDropdownGroups = new List<TypeDropdownGroup>();
                if (isStatic)
                {
                    Type staticRefType = uObj == null ? null : uObj.GetType();
                    if (staticRefType is null)
                    {
                        IEnumerable<Assembly> allAss = TypeReferenceDrawer.GetAssembly(EType.AllAssembly, null);
                        TypeReferenceDrawer.FillAsssembliesTypes(allAss, ToFill);
                        foreach ((Assembly ass, Type[] assTypes) in ToFill)
                        {
                            string assName = TypeReference.GetShortAssemblyName(ass);
                            // IReadOnlyList<(string dropPath, Type type)> types = assTypes
                            //     .Select(each => (TypeReferenceDrawer.FormatPath(each, 0, false), each)).ToArray();
                            // typeDropdownGroups.Add(new TypeDropdownGroup(assName, types));

                            foreach (Type assType in assTypes)
                            {
                                foreach (MethodInfo methodInfo in assType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Object expandedObj in GetUObjExpand(uObj))
                        {

                        }
                    }
                }
                else
                {

                }
            };
            // UnityEventCallStateSelector callStateSelector = container.Q<UnityEventCallStateSelector>();
            // SerializedProperty callStateProperty = property.FindPropertyRelative(PropNameCallState());
            // callStateSelector.Bind(callStateProperty.serializedObject);
        }

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
