#if SAINTSFIELD_SERIALIZATION && SAINTSFIELD_SERIALIZATION_ENABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer.UIToolkitElements;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Events;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public partial class PersistentArgumentDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static VisualTreeAsset _containerTree;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            if (_containerTree == null)
            {
                _containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/PersistentArgumentContainer.uxml");
            }

            VisualElement element = _containerTree.CloneTree();

            ParamTypeButton paramTypeButton = element.Q<ParamTypeButton>();
            paramTypeButton.IsOptionalProp = property.FindPropertyRelative(nameof(PersistentArgument.isOptional));
            paramTypeButton.BindProperty(property.FindPropertyRelative(nameof(PersistentArgument.valueType)));

            VisualElement serialized = element.Q<VisualElement>("persistent-argument-value-serialized-unity-object");
            ObjectField serializedObjectField = serialized.Q<ObjectField>();
            serializedObjectField.BindProperty(property.FindPropertyRelative(nameof(PersistentArgument.unityObject)));

            return element;
        }

        private class SerializedValuePayload
        {
            public Type Type;
            public object Value;
            public VisualElement RenderElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Label label = container.Q<Label>("persistent-argument-label");

            TrackLabelDisplay(property);
            label.TrackPropertyValue(property, TrackLabelDisplay);

            // dynamic

            // Debug.Log(property.propertyPath);
            // Debug.Log(string.Join(".", property.propertyPath.Split('.').SkipLast(6)));
            string[] splited = property.propertyPath.Split('.').SkipLast(6).ToArray();
            bool selfInsideArray = false;
            if (splited[splited.Length - 1].EndsWith("]"))
            {
                splited = splited.SkipLast(2).ToArray();
                selfInsideArray = true;
            }
            (SerializedUtils.FieldOrProp rootFieldOrProp, object _) = SerializedUtils.GetFieldInfoAndDirectParentByPathSegments(property, splited);
            Type rawType = rootFieldOrProp.IsField
                ? rootFieldOrProp.FieldInfo.FieldType
                : rootFieldOrProp.PropertyInfo.PropertyType;
            if (selfInsideArray)
            {
                rawType = ReflectUtils.GetElementType(rawType);
            }
            Type[] eventParamTypes = rawType.GetGenericArguments();

            SerializedProperty valueTypeProp = property.FindPropertyRelative(nameof(PersistentArgument.valueType));

            VisualElement valueDynamic = container.Q<VisualElement>("persistent-argument-value-dynamic");
            VisualElement dropdownButton = valueDynamic.Q<VisualElement>("DropdownButton");
            Label dropdownButtonLabel = dropdownButton.Q<Label>();
            SerializedProperty invokedParameterIndexProp = property.FindPropertyRelative(nameof(PersistentArgument.invokedParameterIndex));
            SetDynamicButtonLabel(invokedParameterIndexProp);
            dropdownButtonLabel.TrackPropertyValue(invokedParameterIndexProp, SetDynamicButtonLabel);
            dropdownButton.Q<Button>().clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                for (int eventParamIndex = 0; eventParamIndex < eventParamTypes.Length; eventParamIndex++)
                {
                    Type type = eventParamTypes[eventParamIndex];
                    string labelText = $"Args[{eventParamIndex}] <color=#808080>({PersistentCallDrawer.StringifyType(type)})</color>";
                    int thisIndex = eventParamIndex;
                    genericDropdownMenu.AddItem(labelText, invokedParameterIndexProp.intValue == eventParamIndex, () =>
                    {
                        invokedParameterIndexProp.intValue = thisIndex;
                        if (valueTypeProp.intValue != (int)PersistentArgument.ValueType.Dynamic)
                        {
                            valueTypeProp.intValue = (int)PersistentArgument.ValueType.Dynamic;
                        }
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                Rect bound = dropdownButton.worldBound;
                if (bound.width < 150)
                {
                    bound.width = 150;
                }

                genericDropdownMenu.DropDown(bound, dropdownButton, true);
            };

            // serialized
            // VisualElement serialized = container.Q<VisualElement>("persistent-argument-value-serialized-unity-objet");
            // ObjectField serializedObjectField = serialized.Q<ObjectField>();

            // value
            VisualElement serializedValue = container.Q<VisualElement>("persistent-argument-value-serialized-value");
            VisualElement serializedValueDropdownButton = serializedValue.Q<VisualElement>("DropdownButton");

            Label serializedValueDropdownButtonLabel = serializedValueDropdownButton.Q<Label>();
            SerializedProperty serializeValueTypeProp = property.FindPropertyRelative(nameof(PersistentArgument.typeReference) + "._typeNameAndAssembly");
            SerializedValueDropdownButtonLabelDisplay(serializeValueTypeProp);
            serializedValueDropdownButtonLabel.TrackPropertyValue(serializeValueTypeProp, SerializedValueDropdownButtonLabelDisplay);

            Button serializedValueDropdownButtonButton = serializedValueDropdownButton.Q<Button>();
            serializedValueDropdownButtonButton.clicked += () =>
            {
                string typeName = serializeValueTypeProp.stringValue;
                Type fieldType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
                if (fieldType == null)
                {
                    return;
                }

                Type[] optionTypes = ReferencePickerAttributeDrawer
                    .GetTypesDerivedFrom(fieldType)
                    .ToArray();

                AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>();
                bool canBeNull = !fieldType.IsValueType;
                if(canBeNull)
                {
                    dropdownList.Add("[Null]", null);
                    if (optionTypes.Length > 0)
                    {
                        dropdownList.AddSeparator();
                    }
                }

                foreach (Type type in optionTypes)
                {
                    string displayName = AbsRenderer.GetDropdownTypeLabel(type);
                    dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
                }

                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurDisplay = "null",
                    CurValues = new[]{fieldType},
                    DropdownListValue = dropdownList,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(serializedValueDropdownButtonButton.worldBound);

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    serializedValueDropdownButtonButton.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        Type newType = (Type)curItem;
                        serializeValueTypeProp.stringValue = newType == null
                            ? ""
                            : TypeReference.GetTypeNameAndAssembly(newType);

                        serializeValueTypeProp.serializedObject.ApplyModifiedProperties();
                    }
                ));
            };

            VisualElement serializedValueEditor = serializedValue.Q<VisualElement>("persistent-argument-value-serialized-value-editor");
            object serializedObj = null;
            Type serializedFieldType = null;
            SerializedProperty serializeBinaryDataProp = property.FindPropertyRelative(nameof(PersistentArgument.serializeBinaryData));
            if (serializeBinaryDataProp.arraySize > 0)
            {
                string typeName = serializeValueTypeProp.stringValue;
                serializedFieldType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
                if(serializedFieldType != null)
                {
                    byte[] serializeBinaryData = Enumerable.Range(0, serializeBinaryDataProp.arraySize)
                        .Select(i => (byte)serializeBinaryDataProp.GetArrayElementAtIndex(i).intValue)
                        .ToArray();
                    try
                    {
                        serializedObj = SerializationUtil.FromBinaryType(serializedFieldType, serializeBinaryData);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogError(e);
                        serializedObj = Activator.CreateInstance(serializedFieldType, true);
                    }
                }
            }
            serializedValueEditor.userData = new SerializedValuePayload
            {
                Type = serializedFieldType,
                Value = serializedObj,
                RenderElement = null,
            };

            void SerializedValueEditorRepaint()
            {
                SerializedValuePayload sp = (SerializedValuePayload)serializedValueEditor.userData;
                Debug.Assert(sp != null);

                if (sp.Type == null || sp.Value == null)
                {
                    serializedValueEditor.Clear();
                    sp.RenderElement = null;
                    return;
                }

                SerializedValuePayload payload = (SerializedValuePayload)serializedValueEditor.userData;

                (VisualElement result, bool isNestedField) = AbsRenderer.UIToolkitValueEdit(
                    payload.RenderElement, "", sp.Type, sp.Value, null, Debug.Log, false, true);

                if (result != null)
                {
                    serializedValueEditor.Add(result);
                    sp.RenderElement = result;
                }
            }

            SerializedValueEditorRepaint();

            serializedValueEditor.TrackPropertyValue(serializeValueTypeProp, typeProp =>
            {

            });

            return;


            void SerializedValueDropdownButtonLabelDisplay(SerializedProperty p)
            {
                string typeName = p.stringValue;
                Type type = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
                string typeNameAndAssembly = type == null
                    ? "?"
                    : $"{PersistentCallDrawer.StringifyType(type)} <color=#808080>({type.Namespace})</color>";
                serializedValueDropdownButtonLabel.text = typeNameAndAssembly;
            }

            void TrackLabelDisplay(SerializedProperty p)
            {
                string argTypeStr = property
                    .FindPropertyRelative(nameof(PersistentArgument.typeReference) + "._typeNameAndAssembly").stringValue;
                Type argType = string.IsNullOrEmpty(argTypeStr) ? null : Type.GetType(argTypeStr);
                string argTypeName = argType == null
                    ? ""
                    : $" <color=#808080>({PersistentCallDrawer.StringifyType(argType)})</color>";
                label.text = $"{property.FindPropertyRelative(nameof(PersistentArgument.name)).stringValue}{argTypeName}";
            }

            // Type fieldType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
            //     ? ReflectUtils.GetElementType(info.FieldType)
            //     : info.FieldType;
            //
            // Debug.Log(fieldType);
            // Debug.Log(property.propertyPath);
            void SetDynamicButtonLabel(SerializedProperty prop)
            {
                int curIndex = prop.intValue;
                string curType = curIndex < 0 || curIndex >= eventParamTypes.Length
                    ? "?"
                    : $"Args[{curIndex}] <color=#808080>({PersistentCallDrawer.StringifyType(eventParamTypes[curIndex])})</color>";

                dropdownButtonLabel.text = curType;
            }
        }
    }
}
#endif
