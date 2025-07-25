#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED && UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
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
            paramTypeButton.BindProperty(property.FindPropertyRelative(nameof(PersistentArgument.callType)));

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
            if (splited[^1].EndsWith("]"))
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

            SerializedProperty valueTypeProp = property.FindPropertyRelative(nameof(PersistentArgument.callType));

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
                        if (valueTypeProp.intValue != (int)PersistentArgument.CallType.Dynamic)
                        {
                            valueTypeProp.intValue = (int)PersistentArgument.CallType.Dynamic;
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
            VisualElement serializedObject = container.Q<VisualElement>("persistent-argument-value-serialized-unity-object");
            ObjectField serializedObjectField = serializedObject.Q<ObjectField>();
            serializedObjectField.RegisterValueChangedCallback(evt =>
            {
                // ReSharper disable once InvertIf
                if (evt.newValue != null)
                {
                    property.FindPropertyRelative(nameof(PersistentArgument.isUnityObject)).boolValue = true;
                    property.serializedObject.ApplyModifiedProperties();
                }
            });

            // value
            VisualElement serializedValue = container.Q<VisualElement>("persistent-argument-value-serialized-value");
            VisualElement serializedValueDropdownButton = serializedValue.Q<VisualElement>("DropdownButton");

            Label serializedValueDropdownButtonLabel = serializedValueDropdownButton.Q<Label>();
            SerializedProperty serializeValueTypeProp = property.FindPropertyRelative(nameof(PersistentArgument.typeReference) + "._typeNameAndAssembly");
            // SerializedProperty serializedAsJsonProp = property.FindPropertyRelative(nameof(PersistentArgument.serializedAsJson));
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

            SerializedValueEditorReInit();
            SerializedValueEditorRepaint();

            serializedValueEditor.TrackPropertyValue(serializeValueTypeProp, _ =>
            {
                if (SerializedValueEditorReInit())
                {
                    SerializedValueEditorRepaint();
                }
            });

            // persistent-argument-value-default
            VisualElement persistentArgumentValueDefault = container.Q<VisualElement>("persistent-argument-value-default");
            string callPropPath = string.Join(".",  property.propertyPath.Split('.').SkipLast(3));
            int persistentArgumentIndex = SerializedUtils.PropertyPathIndex(callPropPath);
            SerializedProperty persistentCallProp = property.serializedObject.FindProperty(callPropPath);
            SerializedProperty persistentArgumentsProp =
                persistentCallProp.FindPropertyRelative("_persistentArguments");
            void UpdateDefaultParamValueDisplay(SerializedProperty p)
            {
                if (!SerializedUtils.IsOk(valueTypeProp))  // Unity, just, please, fix your shit
                {
                    return;
                }
                if (valueTypeProp.intValue != (int)PersistentArgument.CallType.OptionalDefault)
                {
                    persistentArgumentValueDefault.Clear();
                    return;
                }

                List<Type> argumentTypes = new List<Type>();
                for (int argumentIndex = 0; argumentIndex < persistentArgumentsProp.arraySize; argumentIndex++)
                {
                    SerializedProperty argumentProp = persistentArgumentsProp.GetArrayElementAtIndex(argumentIndex);
                    string argumentTypeStr = argumentProp
                        .FindPropertyRelative(nameof(PersistentArgument.typeReference) + "._typeNameAndAssembly")
                        .stringValue;
                    Type argumentType = string.IsNullOrEmpty(argumentTypeStr) ? null : Type.GetType(argumentTypeStr);
                    if (argumentType == null)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning($"failed to load {argumentTypeStr}@{argumentIndex}: {property.propertyPath}");
#endif
                        return;
                    }
                    argumentTypes.Add(argumentType);
                }

                MethodInfo methodInfo = PersistentCall.GetMethod(
                    persistentCallProp.FindPropertyRelative("_isStatic").boolValue,
                    Type.GetType(persistentCallProp.FindPropertyRelative("_staticType._typeNameAndAssembly").stringValue),
                    persistentCallProp.FindPropertyRelative("_target").objectReferenceValue,
                    persistentCallProp.FindPropertyRelative("_methodName").stringValue,
                    argumentTypes.ToArray()
                ).MethodInfo;

                if (methodInfo == null)
                {
                    persistentArgumentValueDefault.Clear();
                    persistentArgumentValueDefault.Add(new HelpBox("Failed to obtain method info", HelpBoxMessageType.Error));
                    return;
                }

                ParameterInfo[] methodParams = methodInfo.GetParameters();
                if(persistentArgumentIndex >= methodParams.Length)
                {
                    persistentArgumentValueDefault.Clear();
                    persistentArgumentValueDefault.Add(new HelpBox($"Persistent argument index {persistentArgumentIndex} is out of range for method {methodInfo.Name} with {methodParams.Length} parameters", HelpBoxMessageType.Error));
                    return;
                }

                ParameterInfo methodParam = methodParams[persistentArgumentIndex];
                if (!methodParam.IsOptional)
                {
                    persistentArgumentValueDefault.Clear();
                    persistentArgumentValueDefault.Add(new HelpBox($"method {methodInfo.Name} {methodParam.Name}@{persistentArgumentIndex} is not optional", HelpBoxMessageType.Error));
                    return;
                }

                object paramDefaultValue = methodParam.DefaultValue;
                string paramDefaultDisplay = paramDefaultValue == null ? "[Null]" : paramDefaultValue.ToString();
                Label paramDefaultLabel = persistentArgumentValueDefault.Q<Label>();
                if (paramDefaultLabel == null)
                {
                    persistentArgumentValueDefault.Add(paramDefaultLabel = new Label());
                }
                if(paramDefaultLabel.text != paramDefaultDisplay)
                {
                    paramDefaultLabel.text = paramDefaultDisplay;
                }
            }

            UpdateDefaultParamValueDisplay(persistentCallProp);
            persistentArgumentValueDefault.TrackPropertyValue(persistentCallProp, UpdateDefaultParamValueDisplay);
            // This does not fix Unity's shit:
            // persistentArgumentValueDefault.RegisterCallback<DetachFromPanelEvent>(_ => UIToolkitUtils.Unbind(persistentArgumentValueDefault));

            VisualElement persistentArgumentValueEditor = container.Q<VisualElement>("persistent-argument-value-editor");
            SerializedProperty persistentArgumentCallType = property.FindPropertyRelative(nameof(PersistentArgument.callType));
            void UpdatePersistentArgumentValueEditorDisplay(SerializedProperty p)
            {
                if (p.intValue == (int)PersistentArgument.CallType.Dynamic)
                {
                    // Debug.Log("display valueDynamic");
                    valueDynamic.style.display = DisplayStyle.Flex;
                    serializedObject.style.display = DisplayStyle.None;
                    serializedValue.style.display = DisplayStyle.None;
                }
                else if (p.intValue == (int)PersistentArgument.CallType.Serialized)
                {
                    valueDynamic.style.display = DisplayStyle.None;
                    if (property.FindPropertyRelative(nameof(PersistentArgument.isUnityObject)).boolValue)
                    {
                        // Debug.Log("display serializedObject");
                        serializedObject.style.display = DisplayStyle.Flex;
                        serializedValue.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        // Debug.Log("display serializedValue");
                        serializedObject.style.display = DisplayStyle.None;
                        serializedValue.style.display = DisplayStyle.Flex;
                    }
                }
                else if (p.intValue == (int)PersistentArgument.CallType.OptionalDefault)
                {
                    // Debug.Log("display default value");
                    valueDynamic.style.display = DisplayStyle.None;
                    serializedObject.style.display = DisplayStyle.None;
                    serializedValue.style.display = DisplayStyle.None;
                }
            }

            UpdatePersistentArgumentValueEditorDisplay(persistentArgumentCallType);
            persistentArgumentValueEditor.TrackPropertyValue(persistentArgumentCallType, UpdatePersistentArgumentValueEditorDisplay);

            return;

            void SerializedValueEditorRepaint()
            {
                // SerializedValuePayload sp = (SerializedValuePayload)serializedValueEditor.userData;
                // Debug.Assert(sp != null);
                SerializedValuePayload payload = (SerializedValuePayload)serializedValueEditor.userData;

                if (payload.Type == null)
                {
                    // Debug.Log($"payload null: {payload.Type}/{payload.Value}");
                    serializedValueEditor.Clear();
                    payload.RenderElement = null;
                    return;
                }

                (VisualElement result, bool isNestedField) = AbsRenderer.UIToolkitValueEdit(
                    payload.RenderElement, payload.Type.Name, payload.Type, payload.Value, null, newValue =>
                    {
                        // Debug.Log($"Update Value {newValue}");

                        // Debug.Log(jsonC);
                        payload.Value = newValue;

                        // SerializedProperty serializeBinaryDataProp = property.FindPropertyRelative(nameof(PersistentArgument.serializeBinaryData));
                        SerializedProperty serializeJsonDataProp = property.FindPropertyRelative(nameof(PersistentArgument.serializeJsonData));
                        // byte[] binData = Array.Empty<byte>();
                        // string jsonData = "";
                        // bool useJson = false;
                        // try
                        // {
                        //     binData = SerializationUtil.ToBinaryType(newValue);
                        //     object rest = SerializationUtil.FromBinaryType(payload.Type, binData);
                        //     if (rest != newValue)
                        //     {
                        //         throw new Exception("WTF Unity");
                        //     }
                        // }
                        // catch (Exception)
                        // {
                        //     useJson = true;
                        //     jsonData = SerializationUtil.ToJsonType(newValue);
                        //     // Debug.Log(jsonV);
                        //     // object jsonC = SerializationUtil.FromJsonType(payload.Type, jsonV);
                        // }
                        string jsonData = SerializationUtil.ToJsonType(newValue);

                        // if (useJson)
                        {
                            // serializedAsJsonProp.boolValue = true;
                            serializeJsonDataProp.stringValue = jsonData;
                            // serializeBinaryDataProp.arraySize = 0;
                        }
                        // else
                        // {
                        //     serializedAsJsonProp.boolValue = false;
                        //     serializeJsonDataProp.stringValue = string.Empty;
                        //     serializeBinaryDataProp.arraySize = binData.Length;
                        //     for (int binIndex = 0; binIndex < binData.Length; binIndex++)
                        //     {
                        //         serializeBinaryDataProp.GetArrayElementAtIndex(binIndex).intValue = binData[binIndex];
                        //     }
                        // }

                        property.FindPropertyRelative(nameof(PersistentArgument.isUnityObject)).boolValue = false;
                        property.serializedObject.ApplyModifiedProperties();
                        // Debug.Log($"re-render SerializedValueEditorRepaint");
                        SerializedValueEditorRepaint();

                    }, false, true);

                if (result != null)
                {
                    if (isNestedField)
                    {
                        result.schedule.Execute(() => result.Q<Foldout>().value = true);
                    }
                    else
                    {
                        result.Q<Label>().style.display = DisplayStyle.None;
                    }

                    serializedValueEditor.Clear();
                    serializedValueEditor.Add(result);
                    payload.RenderElement = result;
                }
            }

            bool SerializedValueEditorReInit()
            {
                object serializedObj = null;
                string typeName = serializeValueTypeProp.stringValue;
                Type serializedFieldType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
                if (serializedValueEditor.userData != null)
                {
                    SerializedValuePayload oldPayload = (SerializedValuePayload)serializedValueEditor.userData;
                    if (oldPayload.Type == serializedFieldType)
                    {
                        return false;
                    }
                }

                // SerializedProperty serializeBinaryDataProp = property.FindPropertyRelative(nameof(PersistentArgument.serializeBinaryData));
                SerializedProperty serializeJsonDataProp = property.FindPropertyRelative(nameof(PersistentArgument.serializeJsonData));
                // Debug.Log($"serializeBinaryDataProp.arraySize={serializeBinaryDataProp.arraySize}");
                // if (serializedAsJsonProp.boolValue)
                {
                    string jsonV = serializeJsonDataProp.stringValue;
                    if (!string.IsNullOrEmpty(jsonV))
                    {
                        try
                        {
                            serializedObj = SerializationUtil.FromJsonType(serializedFieldType, jsonV);
                        }
                        catch (ArgumentException e)
                        {
                            Debug.LogError(e);
                            serializedObj = ActivatorCreateInstance(serializedFieldType);
                        }
                    }
                    else if(serializedFieldType != null)
                    {
                        serializedObj = ActivatorCreateInstance(serializedFieldType);
                    }
                }
                // else
                // {
                //     if (serializeBinaryDataProp.arraySize > 0 && serializedFieldType != null)
                //     {
                //         byte[] serializeBinaryData = Enumerable.Range(0, serializeBinaryDataProp.arraySize)
                //             .Select(i => (byte)serializeBinaryDataProp.GetArrayElementAtIndex(i).intValue)
                //             .ToArray();
                //         try
                //         {
                //             serializedObj = SerializationUtil.FromBinaryType(serializedFieldType, serializeBinaryData);
                //         }
                //         catch (ArgumentException e)
                //         {
                //             Debug.LogError(e);
                //             serializedObj = ActivatorCreateInstance(serializedFieldType);
                //         }
                //     }
                //     else if (serializedFieldType != null)
                //     {
                //         serializedObj = ActivatorCreateInstance(serializedFieldType);
                //     }
                // }

                serializedValueEditor.userData = new SerializedValuePayload
                {
                    Type = serializedFieldType,
                    Value = serializedObj,
                    RenderElement = null,
                };

                return true;
            }

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

            void SetDynamicButtonLabel(SerializedProperty prop)
            {
                int curIndex = prop.intValue;
                string curType = curIndex < 0 || curIndex >= eventParamTypes.Length
                    ? "?"
                    : $"Args[{curIndex}] <color=#808080>({PersistentCallDrawer.StringifyType(eventParamTypes[curIndex])})</color>";

                dropdownButtonLabel.text = curType;
            }
        }

        private static object ActivatorCreateInstance(Type serializedFieldType)
        {
            if (serializedFieldType == typeof(string))
            {
                return string.Empty;
            }

            return Activator.CreateInstance(serializedFieldType, true);
        }
    }
}
#endif
