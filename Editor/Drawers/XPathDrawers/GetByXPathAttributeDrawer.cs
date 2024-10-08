using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.XPathDrawers
{
    [CustomPropertyDrawer(typeof(GetByXPathAttribute))]
    public class GetByXPathAttributeDrawer: SaintsPropertyDrawer
    {
        private class GetByPickerWindow : ObjectSelectWindow
        {
            private Action<Object> _onSelected;
            private EPick _editorPick;

            private IReadOnlyList<Object> _assetObjects;
            private IReadOnlyList<Object> _sceneObjects;

            public static void Open(Object curValue, EPick editorPick, IReadOnlyList<Object> assetObjects, IReadOnlyList<Object> sceneObjects, Action<Object> onSelected)
            {
                GetByPickerWindow thisWindow = CreateInstance<GetByPickerWindow>();
                thisWindow.titleContent = new GUIContent("Select");
                // thisWindow._expectedTypes = expectedTypes;
                thisWindow._assetObjects = assetObjects;
                thisWindow._sceneObjects = sceneObjects;
                thisWindow._onSelected = onSelected;
                thisWindow._editorPick = editorPick;
                thisWindow.SetDefaultActive(curValue);
                // Debug.Log($"call show selector window");
                thisWindow.ShowAuxWindow();
            }

            protected override bool AllowScene =>
                // Debug.Log(_editorPick);
                _editorPick.HasFlag(EPick.Scene);

            protected override bool AllowAssets =>
                // Debug.Log(_editorPick);
                _editorPick.HasFlag(EPick.Assets);

            protected override IEnumerable<ItemInfo> FetchAllAssets()
            {
                // HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);
                return _assetObjects.Select(each => new ItemInfo
                {
                    Object = each,
                    Label = each.name,
                    // Icon = property.icon,
                    InstanceID = each.GetInstanceID(),
                    GuiLabel = new GUIContent(each.name),
                });
            }

            protected override IEnumerable<ItemInfo> FetchAllSceneObject()
            {
                // HierarchyProperty property = new HierarchyProperty(HierarchyType.GameObjects, false);
                return _sceneObjects.Select(each => new ItemInfo
                {
                    Object = each,
                    Label = each.name,
                    // Icon = property.icon,
                    InstanceID = each.GetInstanceID(),
                    GuiLabel = new GUIContent(each.name),
                });
            }

            protected override string Error => "";

            protected override bool IsEqual(ItemInfo itemInfo, Object target)
            {
                return ReferenceEquals(itemInfo.Object, target);
            }

            protected override void OnSelect(ItemInfo itemInfo)
            {
                _onSelected(itemInfo.Object);
            }

            protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => true;

            protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => true;
        }



#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_HelpBox";
        private static string NameResignButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_ResignButton";
        private static string NameRemoveButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_RemoveButton";
        private static string NameSelectorButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_SelectorButton";

        private const string ClassGetByXPath = "saints-field-get-by-xpath-attribute-drawer";

        private class InitUserData
        {
            public string Error;
            public SerializedProperty TargetProperty;
            public MemberInfo MemberInfo;
            public Type ExpectType;
            public Type ExpectInterface;

            public SerializedProperty ArrayProperty;
            public IReadOnlyList<object> ArrayValues;
            public GetByXPathAttribute GetByXPathAttribute;
            public int DecoratorIndex;

            public CheckFieldResult CheckFieldResult;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute)saintsAttribute;
            InitUserData initUserData = new InitUserData
            {
                GetByXPathAttribute = getByXPathAttribute,
                DecoratorIndex = index,
            };
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 1,
                },
                name = NameContainer(property, index),
                userData = initUserData,
            };
            root.AddToClassList(ClassGetByXPath);

            Button refreshButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameResignButton(property, index),
            };
            refreshButton.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("refresh.png"),
            });

            Button removeButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameRemoveButton(property, index),
            };
            removeButton.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("close.png"),
            });

            Button selectorButton = new Button
            {
                text = "●",
                style =
                {
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NameSelectorButton(property, index),
            };

            root.Add(refreshButton);
            root.Add(removeButton);
            root.Add(selectorButton);
            root.AddToClassList(ClassAllowDisable);

            (string error, SerializedProperty targetProperty, MemberInfo memberInfo, Type expectType, Type expectInterface) = GetExpectedType(property, info, parent);
            if (error != "")
            {
                initUserData.Error = error;
                return null;
            }

            // CheckFieldResult checkResult = CheckField(property, info, parent, targetValue);
            initUserData.Error = "";
            initUserData.TargetProperty = targetProperty;
            initUserData.MemberInfo = memberInfo;
            initUserData.ExpectType = expectType;
            initUserData.ExpectInterface = expectInterface;
            // initUserData.CheckFieldResult = checkResult;

            // if(getByXPathAttribute.UseResignButton)
            // {
            //     UpdateButtons(checkResult, refreshButton, removeButton);
            // }

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        private static void UpdateButtons(CheckFieldResult result, Button refreshButton, Button removeButton)
        {
            if (result.Error != "")
            {
                // Debug.Log($"{result.OriginalValue}: error={result.Error}");
                if (refreshButton.style.display != DisplayStyle.None)
                {
                    refreshButton.style.display = DisplayStyle.None;
                }

                if (removeButton.style.display != DisplayStyle.Flex)
                {
                    removeButton.style.display = DisplayStyle.Flex;
                }
                return;
            }

            if (!result.MisMatch)
            {
                // Debug.Log($"{result.OriginalValue}: matched");
                if(refreshButton.style.display != DisplayStyle.None)
                {
                    refreshButton.style.display = DisplayStyle.None;
                }

                if (removeButton.style.display != DisplayStyle.None)
                {
                    removeButton.style.display = DisplayStyle.None;
                }
                return;
            }

            if (Util.IsNull(result.TargetValue))
            {
                // Debug.Log($"{result.OriginalValue} -> [null]");
                if (refreshButton.style.display != DisplayStyle.None)
                {
                    refreshButton.style.display = DisplayStyle.None;
                }

                if (removeButton.style.display != DisplayStyle.Flex)
                {
                    removeButton.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                // Debug.Log($"{result.OriginalValue} -> {result.TargetValue}");
                if (refreshButton.style.display != DisplayStyle.Flex)
                {
                    refreshButton.style.display = DisplayStyle.Flex;
                }

                if (removeButton.style.display != DisplayStyle.None)
                {
                    removeButton.style.display = DisplayStyle.None;
                }
            }
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement root = container.Q<VisualElement>(NameContainer(property, index));
            if (root == null)
            {
                return;
            }

            InitUserData initUserData = (InitUserData) root.userData;
            GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute) saintsAttribute;

            if (initUserData.Error != "")
            {
                UpdateErrorMessage(getByXPathAttribute, container, initUserData.CheckFieldResult, property, index);
                return;
            }

            if(getByXPathAttribute.UsePickerButton)
            {
                StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                container.Q<VisualElement>(name: NameLabelFieldUIToolkit(property)).styleSheets.Add(hideStyle);
            }

            InitUserData[] allXPathInitData = container
                .Query<VisualElement>(className: ClassGetByXPath)
                .ToList()
                .Select(each => (InitUserData)each.userData)
                .ToArray();

            bool imTheFirst = allXPathInitData[0].DecoratorIndex == index;
            if (!imTheFirst)
            {
                return;
            }

            (string xPathError, IReadOnlyList<object> results) = GetXPathValues(allXPathInitData.Select(each => each.GetByXPathAttribute.XPathInfoList).ToArray(), initUserData.ExpectType, initUserData.ExpectInterface, property, info, parent);
            initUserData.ArrayValues = results;

            object targetValue = null;
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex == 0 && xPathError == "")
            {
                // handle array size if this is the first element
                if (getByXPathAttribute.AutoResign)
                {
                    (SerializedProperty arrayProperty, int _, string arrayError) = Util.GetArrayProperty(property, info, parent);
                    if (arrayError != "")
                    {
                        initUserData.Error = arrayError;
                    }
                    else
                    {
                        initUserData.ArrayProperty = arrayProperty;
                    }
                }
            }
            else
            {
                targetValue = results.ElementAtOrDefault(propertyIndex == -1? 0: propertyIndex);
            }

            initUserData.CheckFieldResult = CheckField(property, info, parent, targetValue);

            if (initUserData.Error != "")
            {
                UpdateErrorMessage(getByXPathAttribute, container, initUserData.CheckFieldResult, property, index);
                return;
            }

            // init check
            // ReSharper disable once MergeIntoPattern
            if (initUserData.CheckFieldResult.Error == "")
            {
                bool originIsNull = Util.IsNull(initUserData.CheckFieldResult.OriginalValue);

                bool noMore = false;
                if (getByXPathAttribute.InitSign && initUserData.ArrayProperty != null && originIsNull && initUserData.ArrayProperty.arraySize == 1)
                {
                    if (initUserData.ArrayValues.Count != initUserData.ArrayProperty.arraySize)
                    {
                        int newSize = initUserData.ArrayValues.Count;
                        container.schedule.Execute(() =>
                        {
                            initUserData.ArrayProperty.arraySize = newSize;
                            initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
                        });
                        noMore = newSize == 0;
                    }
                }

                if(getByXPathAttribute.InitSign && !noMore && originIsNull && initUserData.CheckFieldResult.MisMatch)
                {
                    CheckFieldResult checkResult = new CheckFieldResult
                    {
                        Error = "",
                        MisMatch = false,
                        OriginalValue = initUserData.CheckFieldResult.TargetValue,
                        TargetValue = initUserData.CheckFieldResult.TargetValue,
                        Index = initUserData.CheckFieldResult.Index,
                    };

                    initUserData.CheckFieldResult = checkResult;

                    SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent,
                        initUserData.CheckFieldResult.TargetValue);
                    onValueChangedCallback.Invoke(initUserData.CheckFieldResult.TargetValue);
                }
            }

            Button refreshButton = root.Q<Button>(NameResignButton(property, index));
            Button removeButton = root.Q<Button>(NameRemoveButton(property, index));
            Button selectorButton = root.Q<Button>(NameSelectorButton(property, index));

            refreshButton.clicked += () =>
            {
                object expectedData = initUserData.CheckFieldResult.TargetValue;
                SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, expectedData);
                onValueChangedCallback.Invoke(expectedData);
            };

            removeButton.clicked += () =>
            {
                int arrayIndex = initUserData.CheckFieldResult.Index;
                if(arrayIndex == -1)
                {
                    SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, null);
                }
                else
                {
                    initUserData.TargetProperty.DeleteArrayElementAtIndex(arrayIndex);
                    initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                }
                onValueChangedCallback.Invoke(null);
            };

            if (getByXPathAttribute.UsePickerButton)
            {
                selectorButton.style.display = DisplayStyle.Flex;
                selectorButton.clicked += () =>
                {
                    object updatedParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    if (updatedParent == null)
                    {
                        Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                        return;
                    }

                    (string getValueError, int _, object curValue) = Util.GetValue(property, info, parent);

                    if (!(curValue is Object) && curValue != null)
                    {
                        Debug.LogError($"targetValue is not Object: {curValue}");
                        return;
                    }

                    Object curValueObj = (Object)curValue;

                    if (getValueError != "")
                    {
                        Debug.LogError(getValueError);
                        return;
                    }

                    (string error, IReadOnlyList<object> getXPathResults) = GetXPathValues(allXPathInitData.Select(each => each.GetByXPathAttribute.XPathInfoList).ToArray(),
                        initUserData.ExpectType, initUserData.ExpectInterface, property, info, updatedParent);
                    if (error != "")
                    {
                        Debug.LogError(error);
                        return;
                    }

                    Object[] objResults = getXPathResults.OfType<Object>().ToArray();
                    List<Object> assetObj = new List<Object>();
                    List<Object> sceneObj = new List<Object>();
                    foreach (Object objResult in objResults)
                    {
                        if (AssetDatabase.GetAssetPath(objResult) != "")
                        {
                            assetObj.Add(objResult);
                        }
                        else
                        {
                            sceneObj.Add(objResult);
                        }
                    }

                    GetByPickerWindow.Open(curValueObj, EPick.Assets | EPick.Scene, assetObj, sceneObj, obj =>
                    {
                        SetValue(initUserData.TargetProperty, initUserData.MemberInfo, updatedParent, obj);
                        onValueChangedCallback.Invoke(obj);
                    });
                };
            }
        }

#if !(SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH_NO_UPDATE)

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            VisualElement root = container.Q<VisualElement>(NameContainer(property, index));
            if (root == null)
            {
                Debug.Log($"{property.propertyPath} no root");
                return;
            }

            InitUserData initUserData = (InitUserData) root.userData;
            if (initUserData.Error != "")
            {
                Debug.Log($"{property.propertyPath} error {initUserData.Error}");;
                return;
            }

            InitUserData[] allXPathInitData = container
                .Query<VisualElement>(className: ClassGetByXPath)
                .ToList()
                .Select(each => (InitUserData)each.userData)
                .ToArray();

            bool imTheFirst = allXPathInitData[0].DecoratorIndex == index;
            if (!imTheFirst)
            {
                return;
            }

            Button refreshButton = root.Q<Button>(NameResignButton(property, index));
            Button removeButton = root.Q<Button>(NameRemoveButton(property, index));

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute) saintsAttribute;

            (string xPathError, IReadOnlyList<object> xPathResults) = GetXPathValues(allXPathInitData.Select(each => each.GetByXPathAttribute.XPathInfoList).ToArray(),  initUserData.ExpectType, initUserData.ExpectInterface, property, info, parent);

            int arraySize = int.MaxValue;
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            if (xPathError == "" && getByXPathAttribute.AutoResign && initUserData.ArrayProperty != null &&
                initUserData.ArrayProperty.arraySize != xPathResults.Count)
            {
                arraySize = xPathResults.Count;
                initUserData.ArrayProperty.arraySize = arraySize;
                initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
            }

            if(propertyIndex >= arraySize)
            {
                return;
            }

            int useIndex = propertyIndex == -1? 0: propertyIndex;
            object targetValue = xPathResults.Count > useIndex ? xPathResults[useIndex] : null;

            CheckFieldResult checkResult = xPathError == ""
                ? CheckField(property, info, parent, targetValue)
                : new CheckFieldResult
                {
                    Error = xPathError,
                    MisMatch = initUserData.CheckFieldResult.MisMatch,
                    OriginalValue = initUserData.CheckFieldResult.OriginalValue,
                    TargetValue = initUserData.CheckFieldResult.TargetValue,
                    Index = initUserData.CheckFieldResult.Index,
                };

            // ReSharper disable once MergeIntoPattern
            if (checkResult.Error == "" && checkResult.MisMatch && getByXPathAttribute.AutoResign)
            {
                SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, checkResult.TargetValue);
                onValueChanged.Invoke(checkResult.TargetValue);
                checkResult = new CheckFieldResult
                {
                    Error = "",
                    MisMatch = false,
                    OriginalValue = checkResult.TargetValue,
                    TargetValue = checkResult.TargetValue,
                    Index = checkResult.Index,
                };
            }

            // Debug.Log($"{checkResult.TargetValue}///{checkResult.TargetValue.GetType()}");

            initUserData.CheckFieldResult = checkResult;

            if(getByXPathAttribute.UseResignButton)
            {
                UpdateButtons(checkResult, refreshButton, removeButton);
            }

            UpdateErrorMessage(getByXPathAttribute, container, initUserData.CheckFieldResult, property, index);
        }

#endif

        private static void UpdateErrorMessage(GetByXPathAttribute getByXPathAttribute, VisualElement root, CheckFieldResult checkFieldResult, SerializedProperty property, int index)
        {
            string error = checkFieldResult.Error;
            // ReSharper disable once MergeIntoPattern
            if(checkFieldResult.Error == "" && getByXPathAttribute.UseErrorMessage)
            {
                if(checkFieldResult.MisMatch)
                {
                    error = $"Expected {(Util.IsNull(checkFieldResult.TargetValue)? "nothing": checkFieldResult.TargetValue)}, but got {(Util.IsNull(checkFieldResult.OriginalValue)? "Null": checkFieldResult.OriginalValue)}";
                }
            }

            HelpBox helpBox = root.Q<HelpBox>(NameHelpBox(property, index));
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }

        #endregion
#endif

        private static void SetValue(SerializedProperty targetProperty, MemberInfo memberInfo, object parent, object expectedData)
        {
            ReflectUtils.SetValue(targetProperty.propertyPath, targetProperty.serializedObject.targetObject, memberInfo, parent, expectedData);
            Util.SignPropertyValue(targetProperty, memberInfo, parent, expectedData);
        }

        private static (bool valid, object value) ValidateXPathResult(object each, Type expectType, Type expectInterface)
        {
            object result;
            if (expectType.IsInstanceOfType(each))
            {
                result = each;
            }
            else if (each is Object uObject)
            {
                Object r = Util.GetTypeFromObj(uObject, expectType);
                if (r == null)
                {
                    return (false, null);
                }

                result = r;
            }
            else
            {
                return (false, null);
            }

            if (expectInterface == null)
            {
                return (true, result);
            }

            bool valid = expectInterface.IsAssignableFrom(each.GetType());
            return valid ? (true, result) : (false, null);
        }

                private static (string error, SerializedProperty targetProperty, MemberInfo targetMemberInfo, Type expectType, Type expectInterface) GetExpectedType(SerializedProperty property, FieldInfo info, object parent)
        {
            Type rawType = ReflectUtils.GetElementType(info.FieldType);
            MemberInfo targetMemberInfo = info;
            if (!typeof(IWrapProp).IsAssignableFrom(rawType))
            {
                return ("", property, targetMemberInfo, rawType, null);
            }

            (string error, int _, object value) = Util.GetValue(property, info, parent);
            if (error != "")
            {
                return (error, property, targetMemberInfo, rawType, null);
            }
            IWrapProp wrapProp = (IWrapProp) value;
            string prop = wrapProp.EditorPropertyName;
            Type expectedType;
            PropertyInfo wrapPropertyInfo = value.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
            if (wrapPropertyInfo == null)
            {
                FieldInfo wrapFieldInfo = value.GetType().GetField(prop, BindingFlags.Public | BindingFlags.Instance);
                Debug.Assert(wrapFieldInfo != null);
                targetMemberInfo = wrapFieldInfo;
                expectedType = wrapFieldInfo.FieldType;
            }
            else
            {
                expectedType = wrapPropertyInfo.PropertyType;
                targetMemberInfo = wrapPropertyInfo;
            }

            SerializedProperty targetProperty = property.FindPropertyRelative(prop) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, prop);

            Type expectedInterface = null;
            Type mostBaseType = ReflectUtils.GetMostBaseType(rawType);
            if(ReflectUtils.IsSubclassOfRawGeneric(typeof(SaintsInterface<,>), mostBaseType))
            {
                expectedInterface = mostBaseType.GetGenericArguments()[1];
            }

            return ("", targetProperty, targetMemberInfo, expectedType, expectedInterface);
        }
        private struct CheckFieldResult
        {
            public string Error;
            public bool MisMatch;
            public object OriginalValue;
            public object TargetValue;
            public int Index;
        }

        private static CheckFieldResult CheckField(SerializedProperty property, FieldInfo info, object parent, object targetValue)
        {
            (string propError, int propIndex, object propValue) = Util.GetValue(property, info, parent);

            if (propError != "")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"Error {property.propertyPath}: {propError}");
#endif
                return new CheckFieldResult
                {
                    Error = propError,
                    MisMatch = false,
                    OriginalValue = propValue,
                    TargetValue = targetValue,
                    Index = propIndex,
                };
            }

            if (Util.GetIsEqual(propValue, targetValue))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"Value Equal {property.propertyPath}: {propValue} == {targetValue}");
#endif
                return new CheckFieldResult
                {
                    Error = "",
                    MisMatch = false,
                    OriginalValue = propValue,
                    TargetValue = targetValue,
                    Index = propIndex,
                };
            }

#if SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"Mismatch {property.propertyPath}: {propValue} != {targetValue}; {propValue is UnityEngine.Object}/{targetValue is UnityEngine.Object}");
#endif
            return new CheckFieldResult
            {
                Error = "",
                MisMatch = true,
                OriginalValue = propValue,
                TargetValue = targetValue,
                Index = propIndex,
            };
        }

        #region XPath Raw Processors

        private enum ResourceType
        {
            Folder,
            File,
            Object,
            SceneRoot,
            AssetsRoot,
        }

        private class ResourceInfo
        {
            public ResourceType ResourceType;
            public object Resource;
            public string FolderPath;
        }

        private static (string error, IReadOnlyList<object>) GetXPathValues(IReadOnlyList<IReadOnlyList<GetByXPathAttribute.XPathInfo>> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, FieldInfo info, object parent)
        {
            List<string> errors = new List<string>();
            List<object> finalResults = new List<object>();

            foreach (IReadOnlyList<GetByXPathAttribute.XPathInfo> orXPathInfoList in andXPathInfoList)
            {
                foreach (GetByXPathAttribute.XPathInfo xPathInfo in orXPathInfoList)
                {
                    IEnumerable<XPathStep> xPathSteps;
                    if (xPathInfo.IsCallback)
                    {
                        (string error, string xPathString) = Util.GetOf(xPathInfo.Callback, "", property, info, parent);

                        if (error != "")
                        {
                            errors.Add(error);
                            continue;
                        }

                        xPathSteps = XPathParser.Parse(xPathString);
                    }
                    else
                    {
                        xPathSteps = xPathInfo.XPathSteps;
                    }

                    IReadOnlyList<ResourceInfo> accValues = new []
                    {
                        new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = property.serializedObject.targetObject,
                        },
                    };

                    foreach (XPathStep xPathStep in xPathSteps)
                    {
    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"processing xpath {xPathStep}");
    #endif

                        IEnumerable<ResourceInfo> sepResources = GetValuesFromSep(xPathStep.SepCount, xPathStep.NodeTest, accValues);

                        // foreach (ResourceInfo resourceInfo in sepResources)
                        // {
                        //     Debug.Log(resourceInfo.Resource);
                        // }
                        IEnumerable<ResourceInfo> axisResources = GetValuesFromAxis(xPathStep.Axis, sepResources);

                        IEnumerable<ResourceInfo> nodeTestResources = GetValuesFromNodeTest(xPathStep.NodeTest, axisResources);

                        // foreach (ResourceInfo resourceInfo in axisResources)
                        // {
                        //     Debug.Log(resourceInfo.Resource);
                        // }

                        IEnumerable<ResourceInfo> attrResources = GetValuesFromAttr(xPathStep.Attr, nodeTestResources);
                        IEnumerable<ResourceInfo> predicatesResources = GetValuesFromPredicates(xPathStep.Predicates, attrResources);
                        accValues = predicatesResources.ToArray();
                        if (accValues.Count == 0)
                        {
    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"Found 0 in {xPathStep}");
    #endif
                            break;
                        }
                    }

                    object[] results = accValues
                        .Select(each =>
                        {
                            // ReSharper disable once InvertIf
                            if (each.ResourceType == ResourceType.File)
                            {
                                string assetPath = string.IsNullOrEmpty(each.FolderPath)
                                    ? (string)each.Resource
                                    : $"{each.FolderPath}/{each.Resource}";
                                return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                            }

                            return each.Resource;
                        })
                        .Where(each => !Util.IsNull(each))
                        .Select(each => ValidateXPathResult(each, expectedType, expectedInterface))
                        .Where(each => each.valid)
                        .Select(each => each.value)
                        .ToArray();

                    if (results.Length != 0)
                    {
                        finalResults.AddRange(results);
                    }
                }
            }

            // return (string.Join("\n", errors), Array.Empty<object>());

            return finalResults.Count > 0
                ? ("", finalResults)
                : (string.Join("\n", errors), finalResults);
        }

        private static IEnumerable<ResourceInfo> GetValuesFromSep(int sepCount, NodeTest nodeTest, IEnumerable<ResourceInfo> accValues)
        {
            if (nodeTest.NameEmpty || nodeTest.ExactMatch == "." || nodeTest.ExactMatch == "..")
            {
                if(sepCount <= 1)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("empty name or dot name with 1 step, return originals");
#endif
                    foreach (ResourceInfo resourceInfo in accValues)
                    {
                        yield return resourceInfo;
                    }
                }
                else
                {
                    Debug.Assert(sepCount == 2);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("empty name or dot name with 2 step, return originals with all their children");
#endif
                    foreach (ResourceInfo resourceInfo in accValues)
                    {
                        yield return resourceInfo;
                        foreach (ResourceInfo childInfo in GetAllChildrenOfResourceInfo(resourceInfo))
                        {
                            yield return childInfo;
                        }
                    }
                }
                yield break;
            }

            if (sepCount <= 1)  // direct child
            {
                foreach (ResourceInfo resourceInfo in accValues)
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.Folder:
                        {
                            foreach (ResourceInfo info in GetChildInFolder(resourceInfo))
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"Get direct child {info.Resource} from {resourceInfo.Resource}");
#endif
                                yield return info;
                            }
                        }
                            break;

                        case ResourceType.File:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"Skip direct child for file from {resourceInfo.Resource}");
#endif
                            break;

                        case ResourceType.Object:
                        {
                            Object uObject = (Object) resourceInfo.Resource;
                            if (uObject is ScriptableObject)
                            {
                                // do nothing
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"skip scriptable object {uObject}");
#endif
                                break;
                            }

                            Transform thisTransform;
                            if (uObject is GameObject uGo)
                            {
                                thisTransform = uGo.transform;
                            }
                            else if (uObject is Component comp)
                            {
                                thisTransform = comp.transform;
                            }
                            else
                            {
                                break;
                            }

                            foreach (GameObject go in thisTransform.Cast<Transform>().Select(each => each.gameObject))
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"return {go} from children of {thisTransform}");
#endif
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = go,
                                    FolderPath = resourceInfo.FolderPath,
                                };
                            }
                        }
                            break;

                        case ResourceType.SceneRoot:
                        {
                            foreach (GameObject rootGameObject in ((Scene)resourceInfo.Resource).GetRootGameObjects())
                            {
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = rootGameObject,
                                };
                            }
                        }
                            break;

                        case ResourceType.AssetsRoot:
                        {
                            foreach (string directoryInfo in GetDirectoriesWithRelative("Assets"))
                            {
        #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"Axis return assets root child {directoryInfo}");
        #endif
                                yield return new ResourceInfo
                                {
                                    Resource = directoryInfo,
                                    ResourceType = ResourceType.Folder,
                                };
                            }
                        }
                            break;
                    }
                }
            }

            else  // any child
            {
                Debug.Assert(sepCount == 2);
                foreach (ResourceInfo resourceInfo in accValues.SelectMany(GetAllChildrenOfResourceInfo))
                {
                    yield return resourceInfo;
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetAllChildrenOfResourceInfo(ResourceInfo resourceInfo)
        {
            switch (resourceInfo.ResourceType)
            {
                case ResourceType.Folder:
                {
                    foreach (ResourceInfo info in GetChildInFolderRecursion(resourceInfo))
                    {
                        yield return info;
                    }
                }
                    break;

                case ResourceType.File:
                    break;

                case ResourceType.Object:
                {
                    Object uObject = (Object) resourceInfo.Resource;
                    if (uObject is ScriptableObject)  // no sub. Empty axis already been handled
                    {
                        // do nothing
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"skip scriptable object {uObject}");
#endif
                        break;
                    }

                    Transform thisTransform;
                    if (uObject is GameObject uGo)
                    {
                        thisTransform = uGo.transform;
                    }
                    else if (uObject is Component comp)
                    {
                        thisTransform = comp.transform;
                    }
                    else
                    {
                        break;
                    }

                    foreach (Transform childTrans in thisTransform.GetComponentsInChildren<Transform>().Where(each => !ReferenceEquals(each, thisTransform)))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"return {childTrans} from children of {thisTransform}");
#endif
                        yield return new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = childTrans.gameObject,

                        };
                    }
                }
                    break;

                case ResourceType.SceneRoot:
                {
                    foreach (GameObject rootGameObject in ((Scene)resourceInfo.Resource).GetRootGameObjects())
                    {
                        ResourceInfo rootInfo = new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = rootGameObject,
                        };
                        yield return rootInfo;

                        foreach (ResourceInfo child in GetAllChildrenOfResourceInfo(rootInfo))
                        {
                            yield return child;
                        }
                    }
                }
                    break;

                case ResourceType.AssetsRoot:
                {
                    foreach (string directoryInfo in GetDirectoriesWithRelative("Assets"))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"Axis return assets all child {directoryInfo}");
#endif
                        ResourceInfo subInfo = new ResourceInfo
                        {
                            Resource = directoryInfo,
                            ResourceType = ResourceType.Folder,
                        };
                        yield return subInfo;

                        foreach (ResourceInfo child in GetChildInFolderRecursion(subInfo))
                        {
                            yield return child;
                        }
                    }
                }
                    break;
            }
        }

        private static IEnumerable<string> GetDirectoriesWithRelative(string directory)
        {
            foreach (string subFolder in Directory.GetDirectories(directory))
            {
                string subFolderName = subFolder.Substring(directory.Length);
                if(subFolderName.StartsWith("/"))
                {
                    subFolderName = subFolderName.Substring(1);
                }

                if (!subFolderName.StartsWith(".") && !subFolderName.EndsWith("~"))
                {
                    yield return subFolder.Replace("\\", "/");
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetChildInFolder(ResourceInfo resourceInfo)
        {
            string directoryInfo = string.IsNullOrEmpty(resourceInfo.FolderPath)? (string) resourceInfo.Resource: $"{resourceInfo.FolderPath}/{resourceInfo.Resource}";
            foreach (string name in GetDirectoriesWithRelative(directoryInfo))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"go to folder {name}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Folder,
                    Resource = name,
                };
            }

            foreach (string assetPath in Directory
                         .GetFiles(directoryInfo)
                         .Where(each => !each.StartsWith(".") && !each.EndsWith(".meta"))
                         .Select(each => each.Replace("\\", "/")))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"return file {assetPath}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.File,
                    Resource = assetPath,
                };
            }
        }

        private static IEnumerable<ResourceInfo> GetChildInFolderRecursion(ResourceInfo resourceInfo)
        {
            foreach (ResourceInfo info in GetChildInFolder(resourceInfo))
            {
                yield return info;
                if (info.ResourceType == ResourceType.Folder)
                {
                    foreach (ResourceInfo subInfo in GetChildInFolderRecursion(info))
                    {
                        yield return subInfo;
                    }
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromNodeTest(NodeTest nodeTest, IEnumerable<ResourceInfo> sepResources)
        {
            if (nodeTest.NameEmpty || nodeTest.NameAny)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log("NodeTest name empty or any, return originals");
#endif
                foreach (ResourceInfo resourceInfo in sepResources)
                {
                    yield return resourceInfo;
                }
                yield break;
            }

            foreach (ResourceInfo resourceInfo in sepResources)
            {
                string resourceName = null;
                switch (resourceInfo.ResourceType)
                {
                    case ResourceType.Folder:
                    case ResourceType.File:
                        resourceName = ((string)resourceInfo.Resource).Split('/').Last();
                        break;
                    case ResourceType.Object:
                        if(resourceInfo.Resource is Object uObject)
                        {
                            resourceName = uObject.name;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resourceInfo.ResourceType), resourceInfo.ResourceType, null);
                }

                if (resourceName is null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"NodeTest no resource name, skip {resourceInfo.Resource}");
#endif
                    continue;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"NodeTest resourceName={resourceName}; type={resourceInfo.ResourceType}");
#endif

                if (nodeTest.ExactMatch == "..")
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.File:
                        {
                            string[] split = resourceName.Split('/');
                            string resFolder = string.Join("/", split.Take(split.Length - 1));
                            yield return new ResourceInfo
                            {
                                ResourceType = ResourceType.Folder,
                                Resource = resFolder,
                                FolderPath = resourceInfo.FolderPath,
                            };
                        }
                            break;
                        case ResourceType.Folder:
                        {
                            if (resourceName == "" || resourceName == ".")
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    string[] split = resourceInfo.FolderPath.Split('/');
                                    string resFolder = string.Join("/", split.Take(split.Length - 1));
                                    if(resFolder.StartsWith("Assets"))
                                    {
                                        yield return new ResourceInfo
                                        {
                                            ResourceType = ResourceType.Folder,
                                            Resource = resFolder,
                                        };
                                    }
                                }
                            }
                            else
                            {
                                string[] split = resourceName.Split('/');
                                string resFolder = string.Join("/", split.Take(split.Length - 1));
                                if(resFolder != "" || !string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new ResourceInfo
                                    {
                                        ResourceType = ResourceType.Folder,
                                        Resource = resFolder,
                                        FolderPath = resourceInfo.FolderPath,
                                    };
                                }
                            }
                        }
                            break;
                        case ResourceType.Object:
                        {
                            Transform parentTransform = GetParentFromResourceInfo(resourceInfo);
                            if (parentTransform != null)
                            {
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = parentTransform.gameObject,
                                };
                            }
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(resourceInfo.ResourceType), resourceInfo.ResourceType, null);
                    }

                    continue;
                }

                if (NodeTestMatch.NodeMatch(resourceName, nodeTest))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"name check passed, return {resourceInfo.Resource}. {nodeTest}");
#endif
                    yield return resourceInfo;
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromAttr(XPathAttrBase attr, IEnumerable<ResourceInfo> axisResources)
        {
            switch (attr)
            {
                case XPathAttrFakeEval fakeEval:
                    foreach (ResourceInfo axisResource in GetValuesFromFakeEval(fakeEval, axisResources))
                    {
                        yield return axisResource;
                    }
                    yield break;
                case XPathAttrLayer _:
                    foreach (ResourceInfo result in axisResources.Select(GetValueFromLayer).Where(each => each != null))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"GetValuesFromAttr Layer {result.Resource}");
#endif
                        yield return result;
                    }
                    break;
                case XPathAttrResourcePath _:
                {
                    foreach (ResourceInfo resourceInfo in axisResources)
                    {
                        switch (resourceInfo.ResourceType)
                        {
                            case ResourceType.File:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return resourceInfo;
                                }
                                else
                                {
                                    string filePath = (string)resourceInfo.Resource;
                                    ResourceInfo resourceFilePath = GetResourceInfoFromFilePath(filePath);
                                    if (resourceFilePath != null)
                                    {
                                        yield return resourceFilePath;
                                    }
                                }
                            }
                                break;
                            case ResourceType.Folder:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return resourceInfo;
                                }
                                else
                                {
                                    string folderPath = (string)resourceInfo.Resource;
                                    Queue<string> pathSplits = new Queue<string>(folderPath.Split('/'));
                                    List<string> parentFolders = new List<string>();
                                    bool found = false;
                                    while (pathSplits.Count > 0)
                                    {
                                        if (pathSplits.Peek().ToLower() == "resources")
                                        {
                                            found = true;
                                        }
                                        parentFolders.Add(pathSplits.Dequeue());
                                    }

                                    if (found)
                                    {
                                        parentFolders.Add(pathSplits.Dequeue());
                                        List<string> leftSplits = pathSplits.ToList();
                                        yield return new ResourceInfo
                                        {
                                            ResourceType = ResourceType.File,
                                            Resource = string.Join("/", leftSplits),
                                            FolderPath = string.Join("/", parentFolders),
                                        };
                                    }
                                }
                            }
                                break;
                            case ResourceType.Object:
                            {
                                if (resourceInfo.Resource is Object unityObject)
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(unityObject);
                                    if (assetPath != "")
                                    {
                                        ResourceInfo getResourceInfo = GetResourceInfoFromFilePath(assetPath);
                                        if(getResourceInfo != null)
                                        {
                                            yield return getResourceInfo;
                                        }
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                    break;
                case XPathAttrAssetPath _:
                {
                    foreach (ResourceInfo resourceInfo in axisResources)
                    {
                        switch (resourceInfo.ResourceType)
                        {
                            case ResourceType.File:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new ResourceInfo
                                    {
                                        ResourceType = ResourceType.File,
                                        Resource = $"{resourceInfo.FolderPath}/{resourceInfo.Resource}",
                                    };
                                }
                                else
                                {
                                    yield return resourceInfo;
                                }
                            }
                                break;
                            case ResourceType.Folder:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new ResourceInfo
                                    {
                                        ResourceType = ResourceType.Folder,
                                        Resource = $"{resourceInfo.FolderPath}/{resourceInfo.Resource}",
                                    };
                                }
                                else
                                {
                                    yield return resourceInfo;
                                }
                            }
                                break;
                            case ResourceType.Object:
                            {
                                if (resourceInfo.Resource is Object unityObject)
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(unityObject);
                                    if (assetPath != "")
                                    {
                                        yield return new ResourceInfo
                                        {
                                            ResourceType = ResourceType.File,
                                            Resource = assetPath,
                                        };
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                    break;
                default:
                {
                    foreach (ResourceInfo resourceInfo in axisResources)
                    {
                        yield return resourceInfo;
                    }
                    yield break;
                }
            }
        }

        private static ResourceInfo GetValueFromLayer(ResourceInfo resourceInfo)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (resourceInfo.ResourceType)
            {
                case ResourceType.File:
                {
                    Object uObject = AssetDatabase.LoadAssetAtPath<Object>(string.IsNullOrEmpty(resourceInfo.FolderPath)
                        ? (string) resourceInfo.Resource
                        : $"{resourceInfo.FolderPath}/{resourceInfo.Resource}");
                    if (uObject != null)
                    {
                        switch (uObject)
                        {
                            case GameObject go:
                                return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = LayerMask.LayerToName(go.layer),
                                };
                            case Component comp:
                                return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = LayerMask.LayerToName(comp.gameObject.layer),
                                };
                        }
                    }
                }
                    break;
                case ResourceType.Object:
                {
                    GameObject go = resourceInfo.Resource as GameObject;
                    if (go != null)
                    {
                        return new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = LayerMask.LayerToName(go.layer),
                        };
                    }
                }
                    break;
            }

            return null;
        }

        private static ResourceInfo GetResourceInfoFromFilePath(string filePath)
        {
            Queue<string> pathSplits = new Queue<string>(filePath.Split('/'));
            List<string> parentFolders = new List<string>();
            while (pathSplits.Count > 0 && pathSplits.Peek().ToLower() != "resources")
            {
                parentFolders.Add(pathSplits.Dequeue());
            }

            List<string> leftSplits = pathSplits.ToList();
            if (leftSplits.Count <= 0)
            {
                return null;
            }

            parentFolders.Add(leftSplits[0]);
            leftSplits.RemoveAt(0);
            return new ResourceInfo
            {
                ResourceType = ResourceType.File,
                Resource = string.Join("/", leftSplits),
                FolderPath = string.Join("/", parentFolders),
            };

        }

        private static IEnumerable<ResourceInfo> GetValuesFromFakeEval(XPathAttrFakeEval fakeEval, IEnumerable<ResourceInfo> axisResources)
        {
            foreach (ResourceInfo axisResource in axisResources.SelectMany(axisResource => GetValueFromFakeEval(fakeEval, axisResource)))
            {
                yield return axisResource;
            }
        }

        private static IEnumerable<ResourceInfo> GetValueFromFakeEval(XPathAttrFakeEval fakeEval, ResourceInfo axisResource)
        {
            object target = axisResource.Resource;
            object result = target;

            foreach (XPathAttrFakeEval.ExecuteFragment executeFragment in fakeEval.ExecuteFragments)
            {
                switch (executeFragment.ExecuteType)
                {
                    case XPathAttrFakeEval.ExecuteType.GetComponents:
                    {
                        Component[] components;
                        if (result is GameObject go)
                        {
                            components = go.GetComponents<Component>();
                        }
                        else if (result is Component comp)
                        {
                            components = comp.GetComponents<Component>();
                        }
                        else
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"FakeEval {result} is not GameObject or Component");
#endif
                            yield break;
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"FakeEval find {result}'s components {string.Join(",", components.Select(each => each.GetType().Name))}");
#endif

                        IReadOnlyList<Component> matchTypeComponent =
                            string.IsNullOrEmpty(executeFragment.ExecuteString)
                                ? components
                                : FilterComponentsByTypeName(components, executeFragment.ExecuteString).ToArray();
                        if (matchTypeComponent.Count == 0)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"FakeEval get no results from [{string.Join("][", executeFragment.ExecuteIndexer)}]");
#endif
                            yield break;
                        }

                        result = FilterByIndexer(matchTypeComponent, executeFragment.ExecuteIndexer);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"FakeEval get {result} from [{string.Join("][", executeFragment.ExecuteIndexer)}]");
#endif

                    }
                        break;

                    case XPathAttrFakeEval.ExecuteType.Method:
                    case XPathAttrFakeEval.ExecuteType.FieldOrProperty:
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(result, executeFragment.ExecuteString, null);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"{result}.{executeFragment.ExecuteString}: {error}");
#endif
                            yield break;
                        }

                        result = value;
                    }
                        break;
                }

                if (result == null)
                {
                    yield break;
                }
            }

            if (result == null)
            {
                yield break;
            }

            if (result is Array arr)
            {
                foreach (object obj in arr)
                {
                    yield return new ResourceInfo
                    {
                        Resource = obj,
                        ResourceType = ResourceType.Object,
                    };
                }
            }
            else if (result is IList list)
            {
                foreach (object obj in list)
                {
                    yield return new ResourceInfo
                    {
                        Resource = obj,
                        ResourceType = ResourceType.Object,
                    };
                }
            }
            else
            {
                yield return new ResourceInfo
                {
                    Resource = result,
                    ResourceType = ResourceType.Object,
                };
            }


            // return result == null
            //     ? null
            //     : new ResourceInfo
            //     {
            //         Resource = result,
            //         ResourceType = ResourceType.Object,
            //     };
        }

        private static object FilterByIndexer(object target, IReadOnlyList<FilterComparerBase> executeFragmentExecuteIndexer)
        {
            object result = target;
            foreach (FilterComparerBase filterComparerBase in executeFragmentExecuteIndexer)
            {
                switch (filterComparerBase)
                {
                    case FilterComparerInt filterComparerInt:
                    {
                        if (result is Array array)
                        {
                            result = array.GetValue(filterComparerInt.Value);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"array index {filterComparerInt.Value} -> {result}");
#endif
                        }
                        else if (result is IList<object> list)
                        {
                            result = list[filterComparerInt.Value];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"list index {filterComparerInt.Value} -> {result}");
#endif
                        }
                        else
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"can not index {filterComparerInt.Value} of {result}");
#endif
                            return null;
                        }
                    }
                        break;

                    case FilterComparerString filterComparerString:
                    {
                        Type dictionaryType = ReflectUtils.GetDictionaryType(result.GetType());
                        if (dictionaryType is null)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"can not string index {filterComparerString.Value} of {result}: not a dictionary");
#endif
                            return null;
                        }

                        Type keyType = dictionaryType.GetGenericArguments()[0];
                        Type stringType = typeof(string);
                        if (keyType != stringType && !keyType.IsSubclassOf(stringType))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"can not string index {filterComparerString.Value} of {result}: key is not string");
#endif
                            return null;
                        }

                        try
                        {
                            result = ((IDictionary)result)[filterComparerString.Value];
                        }
                        catch (KeyNotFoundException)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"can not find {filterComparerString.Value} in dictionary {result}");
#endif
                            return null;
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"dictionary index {filterComparerString.Value} -> {result}");
#endif
                    }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(filterComparerBase), filterComparerBase, null);
                }

                if (result == null)
                {
                    return null;
                }
            }

            return result;
        }

        private static IEnumerable<Component> FilterComponentsByTypeName(Component[] components, string executeFragmentExecuteString)
        {
            // a simple implement. Inheritance/Interface/Generic type not considered
            foreach (Component eachComp in components)
            {
                Type type = eachComp.GetType();
                string fullNamePrefixDot = $".{type.FullName}";
                string checkNamePrefixDot = $".{executeFragmentExecuteString}";
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"type name check: {checkNamePrefixDot} <- {fullNamePrefixDot} with component {eachComp}");
#endif
                if (fullNamePrefixDot.EndsWith(checkNamePrefixDot))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"type name check passed: {checkNamePrefixDot} <- {fullNamePrefixDot}, return {eachComp}");
#endif
                    yield return eachComp;
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromAxis(Axis axis, IEnumerable<ResourceInfo> attrResources)
        {
            switch (axis)
            {
                case Axis.None:
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("No axis given, return origins");
#endif
                    foreach (ResourceInfo resourceInfo in attrResources)
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.Ancestor:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, false, false)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.AncestorInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, false, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.AncestorOrSelf:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, true, false)))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"Axis AncestorOrSelf return [{resourceInfo.ResourceType}]{resourceInfo.Resource}");
#endif
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.AncestorOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, true, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.Parent:
                {
                    foreach (Transform parentTransform in attrResources.Select(GetParentFromResourceInfo).Where(each => !(each is null)))
                    {
                        yield return new ResourceInfo{
                            ResourceType = ResourceType.Object,
                            Resource = parentTransform.gameObject,
                        };
                    }
                }
                    break;
                case Axis.ParentOrSelf:
                {
                    foreach (ResourceInfo attrResource in attrResources.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        yield return attrResource;
                    }
                }
                    break;

                case Axis.ParentOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo attrResource in attrResources.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        switch (attrResource.Resource)
                        {
                            case GameObject go:
                                if(PrefabUtility.GetPrefabInstanceHandle(go)) {
                                    yield return attrResource;
                                }

                                break;
                            case Component comp:
                                if(PrefabUtility.GetPrefabInstanceHandle(comp.gameObject)) {
                                    yield return attrResource;
                                }

                                break;
                        }
                    }
                }
                    break;

                case Axis.Scene:
                {
                    Scene scene = SceneManager.GetActiveScene();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Axis return scene {scene}");
#endif
                    yield return new ResourceInfo
                    {
                        ResourceType = ResourceType.SceneRoot,
                        Resource = scene,
                    };
                    // foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                    // {
                    //
                    // }
                }
                    break;

                case Axis.Prefab:
                {
                    foreach (ResourceInfo resourceInfo in attrResources)
                    {
                        ResourceInfo top = GetGameObjectsAncestor(resourceInfo, true, false).Last();
                        // ReSharper disable once UseNegatedPatternInIsExpression
                        if (!(top is null))
                        {
                            yield return top;
                        }
                    }
                }
                    break;

                case Axis.Resources:
                {
                    foreach (string resourceDirectoryInfo in GetResourcesRootFolders("Assets"))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"Axis Resources return {resourceDirectoryInfo}");
#endif

                        (string resourceFolder, string subFolder) = SplitResources(resourceDirectoryInfo);

                        ResourceInfo info = new ResourceInfo
                        {
                            FolderPath = resourceFolder,
                            Resource = subFolder,
                            ResourceType = ResourceType.Folder,
                        };
                        yield return info;

                        foreach (ResourceInfo resourceInfo in GetChildInFolder(info))
                        {
                            string rawPath = (string)resourceInfo.Resource;
                            string resourcePath = rawPath.Substring(resourceFolder.Length,
                                rawPath.Length - resourceFolder.Length);
                            resourceInfo.Resource = resourcePath;
                            resourceInfo.FolderPath = resourceFolder;
                            yield return resourceInfo;
                        }
                    }
                }
                    break;
                case Axis.Asset:
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("Axis return assets root");
#endif
                    yield return new ResourceInfo
                    {
                        Resource = "Assets",
                        ResourceType = ResourceType.AssetsRoot,
                    };
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        private static (string resourceFolder, string subFolder) SplitResources(string resourcePath)
        {
            if (resourcePath.ToLower().EndsWith("/resources"))
            {
                return (resourcePath, "");
            }
            if (resourcePath.ToLower().EndsWith("/resources/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return (resourcePath.Substring(0, resourcePath.Length - 1), "");
            }

            List<string> resourceParts = new List<string>();
            Queue<string> pathSplits = new Queue<string>(resourcePath.Split('/'));
            while (pathSplits.Count > 0)
            {
                string part = pathSplits.Dequeue();
                resourceParts.Add(part);
                if (part.ToLower() == "resources")
                {
                    break;
                }
            }

            return (string.Join("/", resourceParts), string.Join("/", pathSplits));
        }

        private static IEnumerable<ResourceInfo> GetGameObjectsAncestor(ResourceInfo resourceInfo, bool withSelf, bool insidePrefab)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (resourceInfo.Resource)
            {
                case GameObject go:
                    return GetGameObjectsAncestorFromGameObject(go, withSelf, insidePrefab);
                case Component comp:
                    return GetGameObjectsAncestorFromGameObject(comp.gameObject, withSelf, insidePrefab);
                default:
                    return Array.Empty<ResourceInfo>();
            }
        }

        private static IEnumerable<ResourceInfo> GetGameObjectsAncestorFromGameObject(GameObject go, bool withSelf, bool insidePrefab)
        {
            if (withSelf)
            {
                if (!insidePrefab || PrefabUtility.GetPrefabInstanceHandle(go) != null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Ancestor {go} return itself");
#endif
                    yield return new ResourceInfo
                    {
                        ResourceType = ResourceType.Object,
                        Resource = go,
                    };
                }
            }

            foreach (GameObject gameObject in GetRecursivelyParentGameObject(go))
            {
                if (insidePrefab)
                {
                    bool isInsidePrefab = PrefabUtility.GetPrefabInstanceHandle(go) != null;
                    if (!isInsidePrefab)
                    {
                        yield break;
                    }
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"Ancestor {go} return parent(s) {gameObject}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Object,
                    Resource = gameObject,
                };
            }
        }

        private static IEnumerable<GameObject> GetRecursivelyParentGameObject(GameObject go)
        {
            Transform cur = go.transform.parent;
            while (cur != null)
            {
                yield return cur.gameObject;
                cur = cur.parent;
            }
        }

        private static Transform GetParentFromResourceInfo(ResourceInfo resourceInfo)
        {
            switch (resourceInfo.Resource)
            {
                case GameObject go:
                    return go.transform.parent;
                case Component comp:
                    return comp.transform.parent;
                default:
                {

                }
                    return null;
            }
        }

        private static IEnumerable<ResourceInfo> GetParentOrSelfFromResourceInfo(ResourceInfo resourceInfo)
        {
            switch (resourceInfo.Resource)
            {
                // ReSharper disable once RedundantDiscardDesignation
                case GameObject _:
                // ReSharper disable once RedundantDiscardDesignation
                case Component _:
                    yield return resourceInfo;
                    break;
                default:
                    yield break;
            }

            Transform parent = GetParentFromResourceInfo(resourceInfo);
            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(parent is null))
            {

                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Object,
                    Resource = parent.gameObject,
                };
            }
        }

        private static IEnumerable<string> GetResourcesRootFolders(string currentFolder)
        {
            IEnumerable<string> subFolders = GetDirectoriesWithRelative(currentFolder);
            foreach (string subFolderPath in subFolders)
            {
                if (subFolderPath.ToLower().EndsWith("/resources")) // resources ends here
                {
                    yield return subFolderPath;
                }
                else
                {
                    foreach (string subSubFolder in GetResourcesRootFolders(subFolderPath))
                    {
                        yield return subSubFolder;
                    }
                }
            }
        }

//         private static IEnumerable<string> GetFoldersRecursively(string currentFolder)
//         {
//             IEnumerable<string> subFolders = GetDirectoriesWithRelative(currentFolder);
//             foreach (string subFolder in subFolders)
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
//                 Debug.Log($"Folder get {subFolder} from {currentFolder}");
// #endif
//                 // string subPath = $"{currentFolder}/{subFolder}";
//                 yield return subFolder;
//                 foreach (string subSubFolder in GetFoldersRecursively(subFolder))
//                 {
//                     yield return subSubFolder;
//                 }
//             }
//         }

        private static IEnumerable<ResourceInfo> GetValuesFromPredicates(IReadOnlyList<IReadOnlyList<XPathPredicate>> andPredicates, IEnumerable<ResourceInfo> nodeTestResources)
        {
            IReadOnlyList<ResourceInfo> accValues = nodeTestResources.ToArray();
            foreach (IReadOnlyList<XPathPredicate> orPredicates in andPredicates)
            {
                IEnumerable<ResourceInfo> predicateResources = GetValuesFromOrPredicate(orPredicates, accValues);
                accValues = predicateResources.ToArray();
            }

            return accValues;
        }

        private static IEnumerable<ResourceInfo> GetValuesFromOrPredicate(IReadOnlyList<XPathPredicate> orPredicate, IReadOnlyList<ResourceInfo> accValues)
        {
            HashSet<int> matchedIndexes = new HashSet<int>();
            foreach (XPathPredicate predicate in orPredicate)
            {
                matchedIndexes.UnionWith(GetValuesFromPredicates(predicate, accValues));
            }

            return matchedIndexes.Select(each => accValues[each]);
        }

        private static IEnumerable<int> GetValuesFromPredicates(XPathPredicate predicate, IReadOnlyList<ResourceInfo> accValues)
        {
            if (accValues.Count == 0)
            {
                yield break;
            }

            switch (predicate.Attr)
            {
                case XPathAttrIndex attrIndex:
                {
                    if (attrIndex.Last)
                    {
                        yield return accValues.Count - 1;
                        yield break;
                    }

                    foreach ((ResourceInfo _, int index) in accValues.WithIndex())
                    {
                        if (FilterMatch(new ResourceInfo
                            {
                                Resource = index,
                            }, predicate.FilterComparer))
                        {
                            yield return index;
                        }
                    }
                }
                    break;

                case XPathAttrLayer _:
                {
                    foreach ((ResourceInfo _, int index) in accValues
                                .Select((each, index) => (GetValueFromLayer(each), index))
                                .Where(each => each.Item1 != null)
                                .Where(each => FilterMatch(each.Item1, predicate.FilterComparer))
                             )
                    {
                        yield return index;
                    }

                    break;
                }
                case XPathAttrFakeEval attrFakeEval:
                {
                    foreach ((ResourceInfo eachResource, int index) in accValues.WithIndex())
                    {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
//                         Debug.Log($"Predicates {eachResource.Resource} -> {attrFakeEval} {predicate.FilterComparer}");
// #endif
                        if (GetValueFromFakeEval(attrFakeEval, eachResource).Any(each => FilterMatch(each, predicate.FilterComparer)))
                        {
                            yield return index;
                        }
                        // ResourceInfo evalResource = ;
                        // if(evalResource != null && FilterMatch(evalResource, predicate.FilterComparer))
                        // {
                        //     yield return index;
                        // }
                    }
                }
                    break;
            }
        }

        private static bool FilterMatch(ResourceInfo eachResource, FilterComparerBase predicateFilterComparer)
        {
            switch (predicateFilterComparer)
            {
                case FilterComparerInt filterComparerInt:
                {
                    if (eachResource.Resource is IComparable sourceCompare)
                    {
                        return filterComparerInt.CompareToComparable(sourceCompare);
                    }

                    return false;
                }
                case FilterComparerString filterComparerString:
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"FilterMatch {eachResource.Resource} -> {filterComparerString}");
#endif
                    if (eachResource.Resource is string s)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"FilterMatch {s} -> {filterComparerString}");
#endif
                        return filterComparerString.CompareToString(s);
                    }

                    return false;
                }
                case FilterComparerTruly _:
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (eachResource.ResourceType == ResourceType.File)
                    {
                        return File.Exists((string)eachResource.Resource);
                    }
                    return ReflectUtils.Truly(eachResource.Resource);
                case FilterComparerBasePath filterBasePath:
                    if (eachResource.Resource is string sourceString)
                    {
                        return filterBasePath.CompareToString(sourceString);
                    }

                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(predicateFilterComparer), predicateFilterComparer, null);
            }
        }

        #endregion

        public static bool HelperGetArraySize(SerializedProperty arrayProperty, FieldInfo info)
        {
            (GetByXPathAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(arrayProperty);

            (string error, SerializedProperty targetProperty, MemberInfo memberInfo, Type expectType, Type expectInterface) = GetExpectedType(arrayProperty, info, parent);
            if (error != "")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_GET_BY_XPATH
                Debug.LogError(error);
#endif
                return true;
            }

            GetByXPathAttribute firstAttribute = attributes[0];
            if (!firstAttribute.InitSign && !firstAttribute.AutoResign)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_GET_BY_XPATH
                Debug.Log($"{arrayProperty.propertyPath} not init sign");
#endif
                return false;
            }

            (string xPathError, IReadOnlyList<object> results) = GetXPathValues(attributes.Select(each => each.XPathInfoList).ToArray(), expectType, expectInterface, arrayProperty, info, parent);
            if (xPathError != "")
            {
                return true;
            }

            bool needApply = false;
            // int resultSize = -1;
            bool needInitSign = firstAttribute.InitSign && targetProperty.arraySize == 0;

            if (needInitSign && results.Count > 0)
            {
                targetProperty.arraySize = results.Count;
                needApply = true;
            }
            if(firstAttribute.AutoResign && targetProperty.arraySize != results.Count)
            {
                targetProperty.arraySize = results.Count;
                needApply = true;
            }

            foreach ((object targetValue, int index) in results.WithIndex())
            {
                if (needApply)
                {
                    targetProperty.serializedObject.ApplyModifiedProperties();
                    targetProperty.serializedObject.Update();
                    needApply = false;
                }

                if(index >= targetProperty.arraySize)
                {
                    break;
                }

                SerializedProperty elementProperty = targetProperty.GetArrayElementAtIndex(index);

                (string getValueError, int _, object originValue) = Util.GetValue(elementProperty, memberInfo, parent);
                if (getValueError != "")
                {
                    continue;
                }

                if ((needInitSign || firstAttribute.AutoResign) && !Util.GetIsEqual(originValue, targetValue))
                {
                    SetValue(elementProperty, memberInfo, parent, targetValue);
                }
            }

            if (needApply)
            {
                targetProperty.serializedObject.ApplyModifiedProperties();
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }
            return firstAttribute.AutoResign;

        }
    }
}
