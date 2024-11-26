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
using SaintsField.Utils;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.XPathDrawers
{
    [CustomPropertyDrawer(typeof(GetByXPathAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentsAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute))]
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute))]
    [CustomPropertyDrawer(typeof(FindComponentAttribute))]
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


        private static void OpenPicker(SerializedProperty property, FieldInfo info, IReadOnlyList<GetByXPathAttribute> getByXPathAttributes, Type expectedType, Type interfaceType, Action<object> onValueChanged, object updatedParent)
        {
            (string getValueError, int _, object curValue) = Util.GetValue(property, info, updatedParent);

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

            GetXPathValuesResult r = GetXPathValues(getByXPathAttributes.SelectMany(each => each.XPathInfoAndList).ToArray(),
                expectedType, interfaceType, property, info, updatedParent);
            if (r.XPathError != "")
            {
                Debug.LogError(r.XPathError);
                return;
            }

            Object[] objResults = r.Results.OfType<Object>().ToArray();
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

            GetByPickerWindow.Open(curValueObj, EPick.Assets | EPick.Scene, assetObj, sceneObj, onValueChanged.Invoke);
        }

        #region IMGUI
        private Texture2D _refreshIcon;
        private Texture2D _removeIcon;

        private static readonly Dictionary<string, InitUserData> ImGuiSharedUserData = new Dictionary<string, InitUserData>();

#if UNITY_2019_2_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        [InitializeOnLoadMethod]
        private static void ImGuiClearSharedData() => ImGuiSharedUserData.Clear();

        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        // private static IReadOnlyList<GetByXPathAttribute> AttributesIfImTheFirst(SerializedProperty property, GetByXPathAttribute getByXPathAttribute)
        // {
        //     (GetByXPathAttribute[] iSaintsAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(property);
        //     var match = iSaintsAttributes.FirstOrDefault(each => ReferenceEquals(each, getByXPathAttribute));
        //     return match == null? Array.Empty<GetByXPathAttribute>(): iSaintsAttributes;
        //     // return Array.IndexOf(iSaintsAttributes, getByXPathAttribute) == 0
        //     //     ? iSaintsAttributes
        //     //     : Array.Empty<GetByXPathAttribute>();
        // }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return 0;
            }
            // IReadOnlyList<GetByXPathAttribute> allGetByXPathAttributes = AttributesIfImTheFirst(property, (GetByXPathAttribute)saintsAttribute);
            bool configExists = ImGuiSharedUserData.TryGetValue(GetKey(property), out InitUserData existedInitUserData);
            if(configExists && existedInitUserData.DecoratorIndex != index)
            {
                return 0;
            }

            // do the check and cache the result
            GetByXPathAttribute firstAttribute = (GetByXPathAttribute) saintsAttribute;
            double curTime = EditorApplication.timeSinceStartup;

            // IMGUI has much worse performance issue. Don't overwhelm the update
            int loopInterval = SaintsFieldConfigUtil.GetByXPathLoopIntervalMs();
            bool noLoop = loopInterval <= 0;
            bool loopDelayNotReached = configExists && curTime - existedInitUserData.ImGuiLastTime < loopInterval / 1000f;

            if(configExists && (noLoop || loopDelayNotReached))
            {
                return GetPostFieldWidthValue(firstAttribute, existedInitUserData);
            }

            (string error, SerializedProperty targetProperty, MemberInfo memberInfo, Type expectType, Type expectInterface) = GetExpectedType(property, info, parent);
            if (error != "")
            {
                ImGuiSharedUserData[GetKey(property)] = new InitUserData
                {
                    DecoratorIndex = index,
                    Error = error,
                    ImGuiLastTime = curTime,
                };
                return 0;
            }

            (GetByXPathAttribute[] allGetByXPathAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(property);

            GetXPathValuesResult r = GetXPathValues(allGetByXPathAttributes.SelectMany(each => each.XPathInfoAndList).ToArray(), expectType, expectInterface, property, info, parent);

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            // IReadOnlyList<object> results = r.Results.ToArray();

            SerializedProperty arrProp = null;
            object targetValue = null;
            bool targetValueInit = false;

            if (propertyIndex != -1 && r.XPathError == "")
            {
                // handle array size if this is the first element
                // if (firstAttribute.AutoResignToValue || firstAttribute.AutoResignToNull)
                // {
                (SerializedProperty arrayProperty, int _, string arrayError) = Util.GetArrayProperty(property, info, parent);
                if (arrayError != "")
                {
                    r.XPathError = arrayError;
                }
                else
                {
                    arrProp = arrayProperty;
                    if(propertyIndex == 0)
                    {
                        targetValueInit = true;
                        IReadOnlyList<object> results = r.Results.ToArray();
                        r.Results = results;
                        targetValue = results.Count > 0? results[0]: null;
                        if (arrProp.arraySize != results.Count)
                        {
                            arrProp.arraySize = results.Count;
                            arrProp.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            if(!targetValueInit)
            {
                targetValue = r.Results.ElementAtOrDefault(propertyIndex == -1 ? 0 : propertyIndex);
            }

            InitUserData initUserData = new InitUserData
            {
                Error = "",
                DecoratorIndex = index,
                TargetProperty = targetProperty,
                MemberInfo = memberInfo,
                ExpectType = expectType,
                ExpectInterface = expectInterface,
                ArrayProperty = arrProp,
                GetByXPathAttributes = allGetByXPathAttributes,
                CheckFieldResult = r.XPathError == ""
                    ? CheckField(property, info, parent, targetValue)
                    : new CheckFieldResult
                    {
                        Error = r.XPathError,
                    },
                ImGuiLastTime = curTime,
            };
            ImGuiSharedUserData[GetKey(property)] = initUserData;

            // Debug.Log($"initUserData.CheckFieldResult.MisMatch={initUserData.CheckFieldResult.MisMatch}, initUserData.CheckFieldResult.OriginalValue={initUserData.CheckFieldResult.OriginalValue}");

            if(!configExists && firstAttribute.InitSign && initUserData.CheckFieldResult.MisMatch && Util.IsNull(initUserData.CheckFieldResult.OriginalValue)
               || ((firstAttribute.AutoResignToValue || firstAttribute.AutoResignToNull) && initUserData.CheckFieldResult.MisMatch))
            {
                bool doResignValue = firstAttribute.AutoResignToValue &&
                                     !Util.IsNull(initUserData.CheckFieldResult.TargetValue);
                bool doResignNull = firstAttribute.AutoResignToNull &&
                                    Util.IsNull(initUserData.CheckFieldResult.TargetValue);
                // Debug.Log($"init sign {firstAttribute.AutoResignToValue}/{firstAttribute.AutoResignToNull}/{Util.IsNull(initUserData.CheckFieldResult.TargetValue)}");
                if(doResignValue || doResignNull)
                {
                    SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent,
                        initUserData.CheckFieldResult.TargetValue);
                    initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                    onGuiPayload.SetValue(initUserData.CheckFieldResult.TargetValue);
                }
            }

            return GetPostFieldWidthValue(firstAttribute, initUserData);
        }

        private static float GetPostFieldWidthValue(GetByXPathAttribute firstAttribute, InitUserData initUserData)
        {
            float useWidth = firstAttribute.UsePickerButton ? SingleLineHeight : 0;

            if (initUserData.Error != "")
            {
                return useWidth;
            }

            if (initUserData.CheckFieldResult.Error != "")
            {
                return useWidth;
            }

            if (!initUserData.CheckFieldResult.MisMatch)
            {
                return useWidth;
            }

            if (!firstAttribute.UseResignButton)
            {
                return useWidth;
            }

            return useWidth + SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }
            if(!ImGuiSharedUserData.TryGetValue(GetKey(property), out InitUserData existedInitUserData) || existedInitUserData.DecoratorIndex != index)
            {
                return false;
            }
            IReadOnlyList<GetByXPathAttribute> allGetByXPathAttributes = existedInitUserData.GetByXPathAttributes;

            InitUserData initUserData = ImGuiSharedUserData[GetKey(property)];
            GetByXPathAttribute firstAttribute = allGetByXPathAttributes[0];
            bool willDraw = false;
            bool drawPicker = firstAttribute.UsePickerButton;
            Rect pickerRect = position;

            if (initUserData.CheckFieldResult.Error == "")
            {
                if (initUserData.CheckFieldResult.MisMatch && firstAttribute.UseResignButton)
                {
                    willDraw = true;
                    (Rect actionButtonRect, Rect leftRect) = RectUtils.SplitWidthRect(position, SingleLineHeight);
                    pickerRect = leftRect;
                    if (Util.IsNull(initUserData.CheckFieldResult.TargetValue))
                    {
                        if (_removeIcon == null)
                        {
                            _removeIcon = Util.LoadResource<Texture2D>("close.png");
                        }

                        if (GUI.Button(actionButtonRect, _removeIcon))
                        {

                            int arrayIndex = initUserData.CheckFieldResult.Index;
                            if(arrayIndex == -1)
                            {
                                SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, null);
                                initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                                onGUIPayload.SetValue(null);
                            }
                            else
                            {
                                initUserData.ArrayProperty.DeleteArrayElementAtIndex(arrayIndex);
                                initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                    else
                    {
                        if (_refreshIcon == null)
                        {
                            _refreshIcon = Util.LoadResource<Texture2D>("refresh.png");
                        }

                        if (GUI.Button(actionButtonRect, _refreshIcon))
                        {
                            SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, initUserData.CheckFieldResult.TargetValue);
                            initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                            onGUIPayload.SetValue(null);
                        }
                    }
                }
            }

            if (drawPicker)
            {
                willDraw = true;
                if (GUI.Button(pickerRect, "●"))
                {
                    object updatedParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    if (updatedParent == null)
                    {
                        Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                    }
                    else
                    {
                        OpenPicker(property, info, allGetByXPathAttributes,
                            initUserData.ExpectType, initUserData.ExpectInterface,
                            newValue =>
                            {
                                SetValue(initUserData.TargetProperty, initUserData.MemberInfo, updatedParent, newValue);
                                initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                                onGUIPayload.SetValue(newValue);
                            }, updatedParent);
                    }
                }
            }

            return willDraw;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return 0;
            }
            string errorMessage = GetErrorMessage(property, (GetByXPathAttribute)saintsAttribute, index);
            return errorMessage == ""
                ? 0
                : ImGuiHelpBox.GetHeight(errorMessage, width, MessageType.Error);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }
            return GetErrorMessage(property, (GetByXPathAttribute)saintsAttribute, index) != "";
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return position;
            }
            string errorMessage = GetErrorMessage(property, (GetByXPathAttribute)saintsAttribute, index);
            return errorMessage == ""
                ? position
                : ImGuiHelpBox.Draw(position, errorMessage, MessageType.Error);
        }

        private static string GetErrorMessage(SerializedProperty property, GetByXPathAttribute getByXPathAttribute, int index)
        {
            if(!ImGuiSharedUserData.TryGetValue(GetKey(property), out InitUserData existedInitUserData) || existedInitUserData.DecoratorIndex != index)
            {
                return "";
            }

            string errorMessage = existedInitUserData.Error;
            if(string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = existedInitUserData.CheckFieldResult.Error;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            bool useErrorMessage = getByXPathAttribute.UseErrorMessage;

            if (string.IsNullOrEmpty(errorMessage) && useErrorMessage && existedInitUserData.CheckFieldResult.MisMatch)
            {
                return $"Expected {(Util.IsNull(existedInitUserData.CheckFieldResult.TargetValue)? "nothing": existedInitUserData.CheckFieldResult.TargetValue)}, but got {(Util.IsNull(existedInitUserData.CheckFieldResult.OriginalValue)? "Null": existedInitUserData.CheckFieldResult.OriginalValue)}";
            }

            return "";
        }

        #endregion

        private class InitUserData
        {
            public string Error;
            public SerializedProperty Property;
            public SerializedProperty TargetProperty;
            public FieldInfo Info;
            public MemberInfo MemberInfo;
            public Type ExpectType;
            public Type ExpectInterface;

            public int DecoratorIndex;

            public SerializedProperty ArrayProperty;
            public GetXPathValuesResult ArrayValues;
            public GetByXPathAttribute GetByXPathAttribute;
            public IReadOnlyList<GetByXPathAttribute> GetByXPathAttributes;

            public double ImGuiLastTime;

            public CheckFieldResult CheckFieldResult;
        }

        #region UIToolkit
#if UNITY_2021_3_OR_NEWER

        private static string ClassArrayContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";
        private static string ClassContainer(SerializedProperty property) => $"{property.propertyPath}__GetByXPath";
        // private static string ClassAttributesContainer(SerializedProperty property) => $"{property.propertyPath}__GetByXPath_Attributes";
        private static string NameContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_HelpBox";
        private static string NameResignButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_ResignButton";
        private static string NameRemoveButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_RemoveButton";
        private static string NameSelectorButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_SelectorButton";

        // private const string ClassGetByXPath = "saints-field-get-by-xpath-attribute-drawer";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            string className = ClassContainer(property);
            GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute)saintsAttribute;

            InitUserData initUserData = new InitUserData
            {
                GetByXPathAttribute = getByXPathAttribute,
                Property = property,
                Info = info,
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
            // root.AddToClassList(ClassGetByXPath);
            root.AddToClassList(className);

            // get array property
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != -1)
            {
                // array need to know all element to decide the size
                (SerializedProperty arrayProperty, int _, string arrayError) = Util.GetArrayProperty(property, info, parent);
                if (arrayError != "")
                {
                    initUserData.Error = arrayError;
                    return root;
                }

                initUserData.ArrayProperty = arrayProperty;

                root.AddToClassList(ClassArrayContainer(arrayProperty, index));
            }

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
            // Debug.Log($"memberInfo={memberInfo.Name}");
            if (error != "")
            {
                initUserData.Error = error;
                return root;
            }



            // CheckFieldResult checkResult = CheckField(property, info, parent, targetValue);
            initUserData.Error = "";
            initUserData.TargetProperty = targetProperty;
            // Debug.Log($"set target property {targetProperty.propertyPath}");
            initUserData.MemberInfo = memberInfo;
            initUserData.ExpectType = expectType;
            initUserData.ExpectInterface = expectInterface;

            bool alreadyHadElement = container.Q<VisualElement>(className: className) != null;
            if (alreadyHadElement)
            {
                root.style.display = DisplayStyle.None;
            }

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
            if (EditorApplication.isPlaying)
            {
                return;
            }
            VisualElement root = container.Q<VisualElement>(NameContainer(property, index));
            if (root == null || root.style.display == DisplayStyle.None)
            {
                // Debug.Log($"skip {root}");
                return;
            }

            InitUserData initUserData = (InitUserData) root.userData;

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            // if(propertyIndex == 0)
            // {
            //     ListView listView = UIToolkitUtils.IterUpWithSelf(container).OfType<ListView>().First();
            // }

            Button refreshButton = root.Q<Button>(NameResignButton(property, index));
            Button removeButton = root.Q<Button>(NameRemoveButton(property, index));
            Button selectorButton = root.Q<Button>(NameSelectorButton(property, index));

            refreshButton.clicked += () =>
            {
                object expectedData = initUserData.CheckFieldResult.TargetValue;
                // Debug.Log($"expectedData={expectedData}, targetProp={initUserData.TargetProperty.propertyPath} memberInfo={initUserData.MemberInfo.Name}");
                SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, expectedData);
                initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(expectedData);
            };

            removeButton.clicked += () =>
            {
                // int arrayIndex = initUserData.CheckFieldResult.Index;
                if(propertyIndex == -1)
                {
                    SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent, null);
                    initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(null);
                }
                else
                {
                    // Debug.Log($"Remove index {propertyIndex}");
                    initUserData.ArrayProperty.DeleteArrayElementAtIndex(propertyIndex);
                    initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
                }
            };

            GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute) saintsAttribute;
            InitUserData[] allXPathInitData = container
                .Query<VisualElement>(className: ClassContainer(property))
                .ToList()
                .Select(each => (InitUserData)each.userData)
                .ToArray();

            bool imTheFirst = allXPathInitData[0].DecoratorIndex == index;
            if (!imTheFirst)
            {
                return;
            }

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

                    OpenPicker(property, info, allXPathInitData.Select(each => each.GetByXPathAttribute).ToArray(), initUserData.ExpectType, initUserData.ExpectInterface,
                        newValue =>
                        {
                            SetValue(initUserData.TargetProperty, initUserData.MemberInfo, updatedParent, newValue);
                            initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(newValue);
                        }, updatedParent);
                };
            }

            if (propertyIndex > 0)  // element 0 response for everything. Other element's behavior is disabled
            {
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"Awake {property.propertyPath}");
#endif
            if (initUserData.Error != "")
            {
                UpdateErrorMessage(getByXPathAttribute, container, initUserData.CheckFieldResult, property, index);
                return;
            }

            if(!getByXPathAttribute.KeepOriginalPicker)
            {
                StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                container.Q<VisualElement>(name: NameLabelFieldUIToolkit(property)).styleSheets.Add(hideStyle);
            }

            GetXPathValuesResult getXPathValuesResult = GetXPathValues(allXPathInitData.SelectMany(each => each.GetByXPathAttribute.XPathInfoAndList).ToArray(), initUserData.ExpectType, initUserData.ExpectInterface, property, info, parent);
            initUserData.ArrayValues = getXPathValuesResult;

            object targetValue = null;
            bool targetValueInit = false;
            bool checkFieldResultInit = false;

            // init check
            // ReSharper disable once MergeIntoPattern
            // bool hasSizeChange = false;
            if (initUserData.Error == "")
            {
                // bool originIsNull = Util.IsNull(initUserData.CheckFieldResult.OriginalValue);

                // bool noMore = false;
                if (getByXPathAttribute.InitSign && initUserData.ArrayProperty != null && initUserData.ArrayProperty.arraySize == 1)
                {
                    object[] findValues = initUserData.ArrayValues.Results.ToArray();
                    Debug.Log($"findValues count {findValues.Length}");
                    initUserData.ArrayValues.Results = findValues;
                    targetValueInit = true;
                    targetValue = findValues.WithIndex().FirstOrDefault(each => each.index == propertyIndex).value;

                    initUserData.CheckFieldResult = CheckField(property, info, parent, targetValue);
                    checkFieldResultInit = true;

                    if (Util.IsNull(initUserData.CheckFieldResult.OriginalValue) && findValues.Length != initUserData.ArrayProperty.arraySize)
                    {
                        int newSize = findValues.Length;
                        container.schedule.Execute(() =>
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                            Debug.Log($"size to {newSize}");
#endif
                            initUserData.ArrayProperty.arraySize = newSize;
                            initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
                        });
                        // initUserData.ArrayProperty.arraySize = newSize;
                        // initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
                        // noMore = newSize == 0;
                        // hasSizeChange = true;
                    }
                }

                // if(getByXPathAttribute.InitSign && !noMore)
                // {
                //     if (!targetValueInit)
                //     {
                //         object[] findValues = initUserData.ArrayValues.Results.ToArray();
                //         // Debug.Log($"findValues count {findValues.Length}");
                //         initUserData.ArrayValues.Results = findValues;
                //         targetValueInit = true;
                //         targetValue = findValues.WithIndex().FirstOrDefault(each => each.index == propertyIndex).value;
                //     }
                //     if (!checkFieldResultInit)
                //     {
                //         initUserData.CheckFieldResult = CheckField(property, info, parent, targetValue);
                //         checkFieldResultInit = true;
                //     }
                //
                //     if(initUserData.CheckFieldResult.MisMatch && Util.IsNull(initUserData.CheckFieldResult.OriginalValue))
                //     {
                //         CheckFieldResult checkResult = new CheckFieldResult
                //         {
                //             Error = "",
                //             MisMatch = false,
                //             OriginalValue = initUserData.CheckFieldResult.TargetValue,
                //             TargetValue = initUserData.CheckFieldResult.TargetValue,
                //             Index = initUserData.CheckFieldResult.Index,
                //         };
                //
                //         initUserData.CheckFieldResult = checkResult;
                //
                //         // SetValue(initUserData.TargetProperty, initUserData.MemberInfo, parent,
                //         //     initUserData.CheckFieldResult.TargetValue);
                //         // initUserData.TargetProperty.serializedObject.ApplyModifiedProperties();
                //         // onValueChangedCallback.Invoke(initUserData.CheckFieldResult.TargetValue);
                //     }
                // }
            }

            if (!targetValueInit)
            {
                targetValue =
                    initUserData.ArrayValues.Results.ElementAtOrDefault(propertyIndex == -1 ? 0 : propertyIndex);
            }

            if(!checkFieldResultInit)
            {
                initUserData.CheckFieldResult = CheckField(property, info, parent, targetValue);
            }

            if (initUserData.Error != "")
            {
                UpdateErrorMessage(getByXPathAttribute, container, initUserData.CheckFieldResult, property, index);
                return;
            }

            // container.schedule.Execute(() =>
            //         ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info))
            //     .StartingIn(500);
            // container.schedule.Execute(() =>
            //         ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info));
            // if (hasSizeChange)
            // {
            //     container.schedule.Execute(() =>
            //             ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info));
            // }
            // else
            // {
            //     ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info);
            // }

            IVisualElementScheduledItem task = container.schedule.Execute(() =>
            {
                ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info, true);
                // int loop = SaintsFieldConfigUtil.GetByXPathLoopIntervalMs();
                // if (loop > 0)
                // {
                //     container.schedule.Execute(() =>
                //         ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback,
                //             info, false)).Every(loop);
                // }
            });
            int delay = SaintsFieldConfigUtil.GetByXPathDelayMs();

            if (delay > 0)
            {
                task.StartingIn(delay);
            }
            // ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info);
        }

        private static void ActualUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, bool isInit)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            try
            {
                string _ = property.propertyPath;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }

            VisualElement firstRoot = container.Q<VisualElement>(NameContainer(property, index));
            if (firstRoot == null)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"{property.propertyPath} no root");
#endif
                return;
            }

            InitUserData initUserData = (InitUserData) firstRoot.userData;
            if (initUserData.Error != "")
            {
                Debug.LogWarning($"{property.propertyPath} error {initUserData.Error}");
                return;
            }

            List<VisualElement> targetRoots = new List<VisualElement>();
            SerializedProperty arrayProp = initUserData.ArrayProperty;
            if (arrayProp == null)
            {
                targetRoots.Add(firstRoot);
            }
            else
            {
                targetRoots.AddRange(
                    UIToolkitUtils.IterUpWithSelf(container)
                        .OfType<ListView>()
                        .First()
                        .Query<VisualElement>(className: ClassArrayContainer(arrayProp, index))
                        .ToList()
                );
                // Debug.Log($"{targetRoots.Count}/{ClassArrayContainer(arrayProp)}");
            }

            InitUserData[] allXPathInitData = container
                .Query<VisualElement>(className: ClassContainer(property))
                .ToList()
                .Select(each => (InitUserData)each.userData)
                .ToArray();
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute) saintsAttribute;
            GetXPathValuesResult r = GetXPathValues(allXPathInitData.SelectMany(each => each.GetByXPathAttribute.XPathInfoAndList).ToArray(),  initUserData.ExpectType, initUserData.ExpectInterface, property, info, parent);

            if (r.XPathError != "")
            {
                initUserData.Error = r.XPathError;
                UpdateErrorMessage(getByXPathAttribute, container, initUserData.CheckFieldResult, property, index);
                return;
            }

            IEnumerable<object> xPathResults;
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (propertyIndex == -1)
            {
                xPathResults = new []{r.Results.ElementAtOrDefault(0)};
            }
            else
            {
                xPathResults = r.Results.ToArray();
            }

            int requireSizeCount = 0;

            foreach ((bool hasRoot, VisualElement root, bool hasValue, object targetValue) in ZipTwoLongest(targetRoots, xPathResults))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"{hasValue}:{targetValue}/{hasRoot}:{root}");
#endif
                if(hasValue)
                {
                    requireSizeCount += 1;
                }
                if (hasRoot)
                {
                    InitUserData userData = (InitUserData)root.userData;
                    // Debug.Log(userData.TargetProperty.propertyPath);
                    Button refreshButton = root.Q<Button>(NameResignButton(userData.Property, index));
                    Button removeButton = root.Q<Button>(NameRemoveButton(userData.Property, index));
                    CheckFieldResult checkResult = CheckField(userData.Property, userData.Info, parent, targetValue);

                    // ReSharper disable once MergeIntoPattern
                    if (checkResult.Error == "" && checkResult.MisMatch && (getByXPathAttribute.AutoResignToValue || getByXPathAttribute.AutoResignToNull))
                    {
                        bool targetIsNull = Util.IsNull(targetValue);
                        bool doResignValue = getByXPathAttribute.AutoResignToValue &&
                                             !targetIsNull;
                        bool doResignNull = getByXPathAttribute.AutoResignToNull &&
                                            targetIsNull;

                        bool doResignInit = isInit && getByXPathAttribute.InitSign;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                        Debug.Log($"{getByXPathAttribute.AutoResignToNull}/{Util.IsNull(targetValue)}");
#endif
                        if (doResignNull || doResignValue || doResignInit)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                            Debug.Log($"resign {checkResult.OriginalValue} -> {checkResult.TargetValue} ({userData.TargetProperty.propertyPath})");
#endif
                            SetValue(userData.TargetProperty, userData.MemberInfo, parent, checkResult.TargetValue);
                            userData.TargetProperty.serializedObject.ApplyModifiedProperties();
                            onValueChanged.Invoke(checkResult.TargetValue);
                            checkResult = new CheckFieldResult
                            {
                                Error = "",
                                MisMatch = false,
                                OriginalValue = checkResult.TargetValue,
                                TargetValue = checkResult.TargetValue,
                                Index = checkResult.Index,
                            };
                            // userData.CheckFieldResult = checkResult;
                        }
                    }

                    // Debug.Log($"{checkResult.TargetValue}///{checkResult.TargetValue.GetType()}");

                    initUserData.CheckFieldResult = checkResult;

                    if(getByXPathAttribute.UseResignButton)
                    {
                        UpdateButtons(checkResult, refreshButton, removeButton);
                    }
                    // UpdateErrorMessage(getByXPathAttribute, root, userData.CheckFieldResult, userData.Property, index);
                }
                // else  // has no root with value, need to increase the array size; non-array will always have a root
                // {
                //     Debug.Log($"hasRoot={hasRoot}, hasValue={hasValue}");
                //     requireSizeCount += 1;
                // }



                // int arraySize = int.MaxValue;

                // if (r.xPathError == ""
                //     && (
                //         getByXPathAttribute.AutoResignToValue
                //         || getByXPathAttribute.AutoResignToNull)
                //     && initUserData.ArrayProperty != null
                //     && initUserData.ArrayProperty.arraySize != xPathResults.Count)
                // {
                //     arraySize = xPathResults.Count;
                //     initUserData.ArrayProperty.arraySize = arraySize;
                //     initUserData.ArrayProperty.serializedObject.ApplyModifiedProperties();
                // }

                // if(propertyIndex >= arraySize)
                // {
                //     return;
                // }

                // int useIndex = propertyIndex == -1? 0: propertyIndex;
                // object targetValue = xPathResults.Count > useIndex ? xPathResults[useIndex] : null;

                // CheckFieldResult checkResult = r.xPathError == ""
                //     ? CheckField(property, info, parent, targetValue)
                //     : new CheckFieldResult
                //     {
                //         Error = r.xPathError,
                //         MisMatch = initUserData.CheckFieldResult.MisMatch,
                //         OriginalValue = initUserData.CheckFieldResult.OriginalValue,
                //         TargetValue = initUserData.CheckFieldResult.TargetValue,
                //         Index = initUserData.CheckFieldResult.Index,
                //     };

            }

            if (arrayProp != null && arrayProp.arraySize != requireSizeCount && (getByXPathAttribute.AutoResignToValue || getByXPathAttribute.AutoResignToNull))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"resize {arrayProp.arraySize} -> {requireSizeCount}");
#endif
                arrayProp.arraySize = requireSizeCount;
                arrayProp.serializedObject.ApplyModifiedProperties();
            }
        }


// #if !(SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH_NO_UPDATE)
//
//         protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
//             VisualElement container, Action<object> onValueChanged, FieldInfo info)
//         {
//             ActualUpdateUIToolkit(property, saintsAttribute, index, container, onValueChanged, info);
//         }
//
// #endif

        private static IEnumerable<(bool hasRoot, VisualElement root, bool hasValue, object value)> ZipTwoLongest(IEnumerable<VisualElement> left, IEnumerable<object> right)
        {

            // IEnumerator<T> leftEnumerator = left.GetEnumerator();
            // IEnumerator<T> rightEnumerator = right.GetEnumerator();

            // ReSharper disable once ConvertToUsingDeclaration
            using(IEnumerator<VisualElement> leftEnumerator = left.GetEnumerator())
            using(IEnumerator<object> rightEnumerator = right.GetEnumerator())
            {
                bool hasLeft = leftEnumerator.MoveNext();
                bool hasRight = rightEnumerator.MoveNext();

                while (hasLeft || hasRight)
                {
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (hasLeft && hasRight)
                    {
                        yield return (true, leftEnumerator.Current, true, rightEnumerator.Current);
                    }
                    else if (hasLeft)
                    {
                        yield return (true, leftEnumerator.Current, false, default);
                    }
                    else
                    {
                        yield return (false, default, true, rightEnumerator.Current);
                    }

                    hasLeft = leftEnumerator.MoveNext();
                    hasRight = rightEnumerator.MoveNext();
                }
            }
        }

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
#endif
        #endregion

        private static void SetValue(SerializedProperty targetProperty, MemberInfo memberInfo, object parent, object expectedData)
        {
            ReflectUtils.SetValue(targetProperty.propertyPath, targetProperty.serializedObject.targetObject, memberInfo, parent, expectedData);
            Util.SignPropertyValue(targetProperty, memberInfo, parent, expectedData);
        }

        private static (bool valid, object value) ValidateXPathResult(object each, Type expectType, Type expectInterface)
        {
            object result;
            // Debug.Log($"{each}");
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

            if (result is GameObject resultGo)
            {
                result = resultGo.transform;
            }
            if(result is Component resultComponent)
            {
                foreach (Component component in resultComponent.GetComponents<Component>())
                {
                    if (expectInterface.IsAssignableFrom(component.GetType()))
                    {
                        return (true, component);
                    }
                }
            }

            bool valid = expectInterface.IsAssignableFrom(result.GetType());
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

            if(propValue is IWrapProp wrapProp)
            {
                propValue = Util.GetWrapValue(wrapProp);
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

        // private class GetXPathValueException : Exception
        // {
        //     public GetXPathValueException()
        //     {
        //     }
        //
        //     public GetXPathValueException(string message) : base(message)
        //     {
        //     }
        //
        //     public GetXPathValueException(string message, Exception innerException) : base(message, innerException)
        //     {
        //     }
        // }

        private class GetXPathValuesResult
        {
            public string XPathError;
            // ReSharper disable once NotAccessedField.Local
            public bool AnyResult;
            public IEnumerable<object> Results;
        }

        private static GetXPathValuesResult GetXPathValues(IReadOnlyList<IReadOnlyList<GetByXPathAttribute.XPathInfo>> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, FieldInfo info, object parent)
        {
            // Debug.Log($"andXPathInfoList Count={andXPathInfoList.Count}");
            bool anyResult = false;
            List<string> errors = new List<string>();
            // IEnumerable<object> finalResults = Array.Empty<object>();
            List<IEnumerable<object>> finalResultsCollected = new List<IEnumerable<object>>();

            foreach (IReadOnlyList<GetByXPathAttribute.XPathInfo> orXPathInfoList in andXPathInfoList)
            {
                // Debug.Log($"loop andXPathInfoList");
                foreach (GetByXPathAttribute.XPathInfo xPathInfo in orXPathInfoList)
                {
                    IEnumerable<ResourceInfo> accValues = new []
                    {
                        new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = property.serializedObject.targetObject,
                        },
                    };

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

                    foreach (XPathStep xPathStep in xPathSteps)
                    {
    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"processing xpath {xPathStep}");
    #endif

                        IEnumerable<ResourceInfo> sepResources = GetValuesFromSep(xPathStep.SepCount, xPathStep.Axis, xPathStep.NodeTest, accValues);
                        // IEnumerable<ResourceInfo> axisResources = GetValuesFromAxis(xPathStep.Axis, sepResources);

                        IEnumerable<ResourceInfo> nodeTestResources = GetValuesFromNodeTest(xPathStep.NodeTest, sepResources);

                        IEnumerable<ResourceInfo> attrResources = GetValuesFromAttr(xPathStep.Attr, nodeTestResources);
                        IEnumerable<ResourceInfo> predicatesResources = GetValuesFromPredicates(xPathStep.Predicates, attrResources);
                        accValues = predicatesResources;
                        //                     accValues = predicatesResources.ToArray();
                        //                     if (accValues.Count == 0)
                        //                     {
                        //                         // Debug.Log($"Found 0 in {xPathStep}, break");
                        // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        //                         Debug.Log($"Found 0 in {xPathStep}");
                        // #endif
                        //                         break;
                        //                     }
                    }

                    IEnumerable<object> results = accValues
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
                        .Select(each => each.value);

                    // if (results.Length != 0)
                    // {
                    //     finalResults.AddRange(results);
                    //     break;
                    // }
                    (bool hasElement, IEnumerable<object> elements) = HasAnyElement(results);
                    if (hasElement)
                    {
                        anyResult = true;
                        // finalResults = finalResults.Concat(elements);
                        finalResultsCollected.Add(elements);
                        // Debug.Log($"has value, break on {finalResultsCollected.Count}");
                        break;
                    }
                }
            }

            // return (string.Join("\n", errors), Array.Empty<object>());

            return anyResult
                ? new GetXPathValuesResult
                {
                    XPathError = "",
                    AnyResult = true,
                    Results = finalResultsCollected.SelectMany(each => each),
                }
                : new GetXPathValuesResult
                {
                    XPathError = string.Join("\n", errors),
                    AnyResult = false,
                    Results = Array.Empty<object>(),
                };
        }

        private static (bool hasElement, IEnumerable<T> elements) HasAnyElement<T>(IEnumerable<T> elements)
        {
            IEnumerator<T> enumerator = elements.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return (false, Array.Empty<T>());
            }

            T first = enumerator.Current;
            return (true, RePrependEnumerable(first, enumerator));
        }

        private static IEnumerable<T> RePrependEnumerable<T>(T first, IEnumerator<T> enumerator)
        {
            yield return first;
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromSep(int sepCount, Axis axis, NodeTest nodeTest, IEnumerable<ResourceInfo> accValues)
        {
            switch (axis)
            {
                case Axis.None:
                    break;
                case Axis.Ancestor:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, false, false)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, false, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorOrSelf:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, true, false)))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"Axis AncestorOrSelf return [{resourceInfo.ResourceType}]{resourceInfo.Resource}");
#endif
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, true, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.Parent:
                {
                    foreach (Transform parentTransform in accValues.Select(GetParentFromResourceInfo).Where(each => !(each is null)))
                    {
                        yield return new ResourceInfo{
                            ResourceType = ResourceType.Object,
                            Resource = parentTransform.gameObject,
                        };
                    }
                }
                    yield break;
                case Axis.ParentOrSelf:
                {
                    foreach (ResourceInfo attrResource in accValues.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        yield return attrResource;
                    }
                }
                    yield break;

                case Axis.ParentOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo attrResource in accValues.SelectMany(GetParentOrSelfFromResourceInfo))
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
                    yield break;

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
                    yield break;

                case Axis.Prefab:
                {
                    foreach (ResourceInfo resourceInfo in accValues)
                    {
                        ResourceInfo top = GetGameObjectsAncestor(resourceInfo, true, false).Last();
                        // ReSharper disable once UseNegatedPatternInIsExpression
                        if (!(top is null))
                        {
                            yield return top;
                        }
                    }
                }
                    yield break;

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
                    yield break;
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
                    yield break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            // Debug.LogWarning($"sepCount={sepCount}");
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
                        if(resourceInfo.ResourceType != ResourceType.SceneRoot && resourceInfo.ResourceType != ResourceType.AssetsRoot)
                        {
                            yield return resourceInfo;
                        }

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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Axis return all children: {resourceInfo.Resource}");
#endif
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

                    foreach (Transform childTrans in thisTransform.GetComponentsInChildren<Transform>(true).Where(each => !ReferenceEquals(each, thisTransform)))
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"yield scene root direct child {rootInfo.Resource}");
#endif
                        yield return rootInfo;

                        foreach (ResourceInfo child in GetAllChildrenOfResourceInfo(rootInfo))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"yield child {child.Resource} of {rootInfo.Resource}");
#endif
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
                        Debug.Log($"AssetsRoot children return assets all child {directoryInfo}");
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
            if (axisResource.ResourceType == ResourceType.File)
            {
                Object uObject = AssetDatabase.LoadAssetAtPath<Object>(string.IsNullOrEmpty(axisResource.FolderPath)
                    ? (string) axisResource.Resource
                    : $"{axisResource.FolderPath}/{axisResource.Resource}");
                if (uObject == null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"FakeEval failed to load {axisResource.FolderPath}/{axisResource.Resource}, return nothing");
#endif
                    yield break;
                }

                target = uObject;
            }
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
                            components = go.GetComponents<Component>().Where(each => each != null).ToArray();
                        }
                        else if (result is Component comp)
                        {
                            components = comp.GetComponents<Component>().Where(each => each != null).ToArray();
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
            if (EditorApplication.isPlaying)
            {
                return false;
            }
            (GetByXPathAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(arrayProperty);

            Type arrayElementType = ReflectUtils.GetElementType(info.FieldType);
            bool arrayElementIsWrapProp = typeof(IWrapProp).IsAssignableFrom(arrayElementType);
            // Type expectInterface = null;

            // we can not get what prop is wrapped when array size is 0
            // so giving up on this type (SaintsInterface, SaintsArray)
            if (arrayElementIsWrapProp)
            {
                return false;
            }

            GetByXPathAttribute firstAttribute = attributes[0];
            if (!firstAttribute.InitSign && !(firstAttribute.AutoResignToValue || firstAttribute.AutoResignToNull))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_GET_BY_XPATH
                Debug.Log($"{arrayProperty.propertyPath} not init sign");
#endif
                return false;
            }

            GetXPathValuesResult r = GetXPathValues(attributes.SelectMany(each => each.XPathInfoAndList).ToArray(), arrayElementType, null, arrayProperty, info, parent);
            if (r.XPathError != "")
            {
                return true;
            }

            IReadOnlyList<object> results = r.Results.ToArray();

            bool needApply = false;
            // int resultSize = -1;
            bool needInitSign = firstAttribute.InitSign && arrayProperty.arraySize == 0;

            if (needInitSign && results.Count > 0)
            {
                arrayProperty.arraySize = results.Count;
                needApply = true;
            }
            if((firstAttribute.AutoResignToValue || firstAttribute.AutoResignToNull) && arrayProperty.arraySize != results.Count)
            {
                arrayProperty.arraySize = results.Count;
                needApply = true;
            }

            object curValues = info.GetValue(parent);
            if (curValues == null)
            {
                if (needApply)
                {
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                }
                return true;
            }

            bool anyChange = false;

            foreach ((object targetValue, int index) in results.WithIndex())
            {
                if (needApply)
                {
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                    arrayProperty.serializedObject.Update();
                    needApply = false;
                }

                if(index >= arrayProperty.arraySize)
                {
                    break;
                }

                (string getValueError, object originValue) = Util.GetValueAtIndex(curValues, index);
                if (getValueError != "")
                {
                    continue;
                }

                if (originValue is IWrapProp wrapProp)
                {
                    originValue = Util.GetWrapValue(wrapProp);
                }

                if(!Util.GetIsEqual(originValue, targetValue))
                {
                    bool changeValue = false;
                    if (needInitSign)
                    {
                        changeValue = true;
                    }
                    else
                    {
                        bool doResignValue = firstAttribute.AutoResignToValue &&
                                             !Util.IsNull(targetValue);
                        bool doResignNull = firstAttribute.AutoResignToNull &&
                                            Util.IsNull(targetValue);
                        if (doResignValue || doResignNull)
                        {
                            // SetValue(elementProperty, memberInfo, parent, targetValue);
                            changeValue = true;
                        }
                    }

                    if (changeValue)
                    {
                        anyChange = true;
                        if (curValues is Array arr)
                        {
                            arr.SetValue(targetValue, index);
                        }
                        else if (curValues is IList list)
                        {
                            list[index] = targetValue;
                        }
                    }
                }
            }

            if (anyChange)
            {
                int index = 0;
                foreach (object eachValue in (IEnumerable)curValues)
                {
                    Util.SignPropertyValue(arrayProperty.GetArrayElementAtIndex(index), null, parent, eachValue);
                    index++;
                }

                needApply = true;
            }

            if (needApply)
            {
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }
            return arrayProperty.arraySize == 0;
        }
    }
}
