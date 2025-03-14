#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer
    {
        private const string ClassSaintsFieldPlaya = "saints-field-playa";
        public const string ClassSaintsFieldPlayaContainer = ClassSaintsFieldPlaya + "-container";

        private VisualElement _rootElement;

        public virtual VisualElement CreateVisualElement()
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
                name = ToString(),
            };
            root.AddToClassList(ClassSaintsFieldPlaya);
            bool hasAnyChildren = false;

            (VisualElement aboveTarget, bool aboveNeedUpdate) = CreateAboveUIToolkit();
            if (aboveTarget != null)
            {
                root.Add(aboveTarget);
                hasAnyChildren = true;
            }
            (VisualElement target, bool targetNeedUpdate) = CreateTargetUIToolkit();
            if (target != null)
            {
                VisualElement targetContainer = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 0,
                    },
                };
                targetContainer.AddToClassList(ClassSaintsFieldPlayaContainer);
                targetContainer.Add(target);
                root.Add(targetContainer);
                hasAnyChildren = true;
            }
            (VisualElement belowTarget, bool belowNeedUpdate) = CreateBelowUIToolkit();
            if (belowTarget != null)
            {
                root.Add(belowTarget);
                hasAnyChildren = true;
            }

            bool anyNeedUpdate = aboveNeedUpdate || targetNeedUpdate || belowNeedUpdate;
            if (anyNeedUpdate)
            {
                root.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    // OnUpdateUIToolKit();
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement));
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement)).Every(100);
                });
            }
            if(anyNeedUpdate || hasAnyChildren)
            {
                return _rootElement = root;
            }

            return null;
        }

        protected virtual (VisualElement target, bool needUpdate) CreateAboveUIToolkit()
        {
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList($"{ClassSaintsFieldPlaya}-above");

            Dictionary<string, VisualElement> groupElements = new Dictionary<string, VisualElement>();

            bool needUpdate = false;
            bool hasAnyChildren = false;

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case PlayaInfoBoxAttribute { Below: false } infoBoxAttribute:
                    {
                        (HelpBox helpBox, bool helpBoxNeedUpdate) = CreateInfoBox(FieldWithInfo, infoBoxAttribute);
                        hasAnyChildren = true;
                        MergeIntoGroup(groupElements, infoBoxAttribute.GroupBy, visualElement, helpBox);
                        if (helpBoxNeedUpdate)
                        {
                            needUpdate = true;
                        }
                    }
                        break;
                }
            }

            if(needUpdate || hasAnyChildren)
            {
                return (visualElement, needUpdate);
            }

            return (null, false);
        }

        protected abstract (VisualElement target, bool needUpdate) CreateTargetUIToolkit();

        protected virtual (VisualElement target, bool needUpdate) CreateBelowUIToolkit()
        {
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList($"{ClassSaintsFieldPlaya}-below");

            Dictionary<string, VisualElement> groupElements = new Dictionary<string, VisualElement>();

            bool needUpdate = false;
            bool hasAnyChildren = false;

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case PlayaInfoBoxAttribute { Below: true } infoBoxAttribute:
                    {
                        (HelpBox helpBox, bool helpBoxNeedUpdate) = CreateInfoBox(FieldWithInfo, infoBoxAttribute);
                        hasAnyChildren = true;
                        MergeIntoGroup(groupElements, infoBoxAttribute.GroupBy, visualElement, helpBox);
                        if (helpBoxNeedUpdate)
                        {
                            needUpdate = true;
                        }
                    }
                        break;
                }
            }

            if(needUpdate || hasAnyChildren)
            {
                return (visualElement, needUpdate);
            }

            return (null, false);
        }

        private static void MergeIntoGroup(Dictionary<string, VisualElement> groupElements, string groupBy, VisualElement root, VisualElement child)
        {
            if (string.IsNullOrEmpty(groupBy))
            {
                root.Add(child);
                return;
            }

            bool exists = groupElements.TryGetValue(groupBy, out VisualElement groupElement);
            if (!exists)
            {
                groupElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    }
                };
                groupElement.AddToClassList($"{ClassSaintsFieldPlaya}-group-{groupBy}");
                groupElements.Add(groupBy, groupElement);
                root.Add(groupElement);
            }

            groupElement.Add(child);
        }

        private class InfoBoxUserData
        {
            public string XmlContent;
            public EMessageType MessageType;

            public PlayaInfoBoxAttribute InfoBoxAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }

        private const string ClassInfoBox = ClassSaintsFieldPlaya + "-info-box";

        private static (HelpBox helpBox, bool needUpdate) CreateInfoBox(SaintsFieldWithInfo fieldWithInfo, PlayaInfoBoxAttribute infoBoxAttribute)
        {
            RichTextDrawer richTextDrawer = new RichTextDrawer();
            InfoBoxUserData infoBoxUserData = new InfoBoxUserData
            {
                XmlContent = "",
                MessageType = infoBoxAttribute.MessageType,

                InfoBoxAttribute = infoBoxAttribute,
                FieldWithInfo = fieldWithInfo,
                RichTextDrawer = richTextDrawer,
            };

            HelpBox helpBox = new HelpBox
            {
                userData = infoBoxUserData,
                messageType = infoBoxAttribute.MessageType.GetUIToolkitMessageType(),
                style =
                {
                    display = DisplayStyle.Flex,
                    flexGrow = 1,
                    flexShrink = 0,
                },
            };
            helpBox.AddToClassList(ClassInfoBox);

            UpdateInfoBox(helpBox);

            // helpBox.RegisterCallback<DetachFromPanelEvent>(evt =>
            // {
            //     richTextDrawer.Dispose();
            // });

            return (helpBox, !string.IsNullOrEmpty(infoBoxAttribute.ShowCallback) || infoBoxAttribute.IsCallback);
        }

        private static void UpdateInfoBox(HelpBox helpBox)
        {
            InfoBoxUserData infoBoxUserData = (InfoBoxUserData)helpBox.userData;

            bool willShow = true;
            bool showHasError = false;
            if (!string.IsNullOrEmpty(infoBoxUserData.InfoBoxAttribute.ShowCallback))
            {
                (string showError, bool show) = UpdateInfoBoxShow(helpBox, infoBoxUserData);
                showHasError = showError != "";
                willShow = show;
            }

            if (!willShow)
            {
                if (helpBox.style.display != DisplayStyle.None)
                {
                    helpBox.style.display = DisplayStyle.None;
                }
                return;
            }

            if (!showHasError)
            {
                UpdateInfoBoxContent(helpBox, infoBoxUserData);
            }
        }

        private static (string error, bool show) UpdateInfoBoxShow(HelpBox helpBox,
            InfoBoxUserData infoBoxUserData)
        {
            (string showError, object showResult) = Util.GetOfNoParams<object>(infoBoxUserData.FieldWithInfo.Target,
                infoBoxUserData.InfoBoxAttribute.ShowCallback, null);
            if (showError != "")
            {
                infoBoxUserData.XmlContent = showError;
                infoBoxUserData.MessageType = EMessageType.Error;

                helpBox.text = showError;
                helpBox.style.display = DisplayStyle.Flex;
                return (showError, true);
            }

            bool willShow = ReflectUtils.Truly(showResult);
            helpBox.style.display = willShow ? DisplayStyle.Flex : DisplayStyle.None;
            if (!willShow)
            {
                infoBoxUserData.XmlContent = "";
            }

            return ("", willShow);
        }

        private static void UpdateInfoBoxContent(HelpBox helpBox, InfoBoxUserData infoBoxUserData)
        {
            string xmlContent = ((InfoBoxUserData)helpBox.userData).InfoBoxAttribute.Content;

            if (infoBoxUserData.InfoBoxAttribute.IsCallback)
            {
                (string error, object rawResult) =
                    GetCallback(infoBoxUserData.FieldWithInfo, infoBoxUserData.InfoBoxAttribute.Content);

                if (error != "")
                {
                    infoBoxUserData.XmlContent = error;
                    infoBoxUserData.MessageType = EMessageType.Error;

                    helpBox.text = error;
                    helpBox.style.display = DisplayStyle.Flex;
                    return;
                }

                if (rawResult is ValueTuple<EMessageType, string> resultTuple)
                {
                    infoBoxUserData.MessageType = resultTuple.Item1;
                    HelpBoxMessageType helpBoxType = infoBoxUserData.MessageType.GetUIToolkitMessageType();
                    if (helpBoxType != helpBox.messageType)
                    {
                        helpBox.messageType = helpBoxType;
                    }

                    xmlContent = resultTuple.Item2;
                }
                else
                {
                    xmlContent = rawResult?.ToString() ?? "";
                }
            }

            if (infoBoxUserData.XmlContent == xmlContent)
            {
                return;
            }

            if (string.IsNullOrEmpty(xmlContent))
            {
                helpBox.style.display = DisplayStyle.None;
                infoBoxUserData.XmlContent = "";
                return;
            }

            infoBoxUserData.XmlContent = xmlContent;
            Label label = helpBox.Q<Label>();
            label.text = "";
            label.style.flexDirection = FlexDirection.Row;

            MemberInfo member = GetMemberInfo(infoBoxUserData.FieldWithInfo);
            string useLabel = ObjectNames.NicifyVariableName(member.Name);

            label.Clear();
            foreach (VisualElement richTextElement in infoBoxUserData.RichTextDrawer.DrawChunksUIToolKit(
                         RichTextDrawer.ParseRichXml(xmlContent, useLabel, infoBoxUserData.FieldWithInfo.SerializedProperty, member, infoBoxUserData.FieldWithInfo.Target))
                     )
            {
                label.Add(richTextElement);
            }
        }

        protected virtual PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            UpdateHelpBox();
            return UpdatePreCheckUIToolkit();
        }

        protected PreCheckResult HelperOnUpdateUIToolKitRawBase()
        {
            UpdateHelpBox();
            return UpdatePreCheckUIToolkit();
        }

        private void UpdateHelpBox()
        {
            foreach (HelpBox helpBox in _rootElement.Query<HelpBox>(className: ClassInfoBox).ToList())
            {
                UpdateInfoBox(helpBox);
            }
        }

        protected PreCheckResult UpdatePreCheckUIToolkit()
        {
            return UpdatePreCheckUIToolkitInternal(FieldWithInfo, _rootElement);
        }

        // TODO: paging & searching
        private PreCheckResult UpdatePreCheckUIToolkitInternal(SaintsFieldWithInfo fieldWithInfo, VisualElement result)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(fieldWithInfo, false);
            if(result.enabledSelf != !preCheckResult.IsDisabled)
            {
                result.SetEnabled(!preCheckResult.IsDisabled);
            }

            bool isShown = result.style.display != DisplayStyle.None;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_IS_SHOWN
            Debug.Log($"{fieldWithInfo} {result.name} isShown={isShown}, preCheckIsShown={preCheckResult.IsShown}");
#endif

            if(isShown != preCheckResult.IsShown)
            {
                result.style.display = preCheckResult.IsShown ? DisplayStyle.Flex : DisplayStyle.None;
            }

            return preCheckResult;
        }

        private static StyleSheet _nullUss;

        // Obsolete: Need to merge to UIToolkitValueEdit
        protected static VisualElement UIToolkitLayout(object value, string label, Type type=null)
        {
            if (type == null && value == null)
            {
                TextField textField = new TextField(label)
                {
                    value = "null",
                    pickingMode = PickingMode.Ignore,
                };

                if(_nullUss == null)
                {
                    _nullUss = Util.LoadResource<StyleSheet>("UIToolkit/UnityTextInputElementWarning.uss");
                }
                textField.styleSheets.Add(_nullUss);

                return WrapVisualElement(textField);
            }

            // VisualElement visualElement;
            Type valueType = type ?? value.GetType();

            // Debug.Log(valueBaseGenericType);

            if (valueType == typeof(bool))
            {
                return WrapVisualElement(new Toggle(label)
                {
                    value = (bool)value,
                });
            }

            if (valueType == typeof(sbyte))
            {
                // EditorGUILayout.IntField(label, (sbyte)value);
                return WrapVisualElement(new IntegerField(label)
                {
                    value = (sbyte)value,
                });
            }
            if (valueType == typeof(byte))
            {
                // EditorGUILayout.IntField(label, (byte)value);
                return WrapVisualElement(new IntegerField(label)
                {
                    value = (byte)value,
                });
            }
            if (valueType == typeof(short))
            {
                // EditorGUILayout.IntField(label, (short)value);
                return WrapVisualElement(new IntegerField(label)
                {
                    value = (short)value,
                });
            }
            if (valueType == typeof(ushort))
            {
                // EditorGUILayout.IntField(label, (ushort)value);
                return WrapVisualElement(new IntegerField(label)
                {
                    value = (ushort)value,
                });
            }
            if (valueType == typeof(int))
            {
                // EditorGUILayout.IntField(label, (int)value);
                return WrapVisualElement(new IntegerField(label)
                {
                    value = (int)value,
                });
            }
            if (valueType == typeof(uint))
            {
                // EditorGUILayout.LongField(label, (uint)value);
                return WrapVisualElement(new LongField(label)
                {
                    value = (uint)value,
                });
            }
            if (valueType == typeof(long))
            {
                // EditorGUILayout.LongField(label, (long)value);
                return WrapVisualElement(new LongField(label)
                {
                    value = (long)value,
                });
            }
            if (valueType == typeof(ulong))
            {
                // EditorGUILayout.TextField(label, ((ulong)value).ToString());
                return WrapVisualElement(new TextField(label)
                {
                    value = ((ulong)value).ToString(),
                });
            }
            if (valueType == typeof(float))
            {
                // EditorGUILayout.FloatField(label, (float)value);
                return WrapVisualElement(new FloatField(label)
                {
                    value = (float)value,
                });
            }
            if (valueType == typeof(double))
            {
                // EditorGUILayout.DoubleField(label, (double)value);
                return WrapVisualElement(new DoubleField(label)
                {
                    value = (double)value,
                });
            }
            if (valueType == typeof(string))
            {
                // EditorGUILayout.TextField(label, (string)value);
                return WrapVisualElement(new TextField(label)
                {
                    value = (string)value,
                });
            }
            if (valueType == typeof(Vector2))
            {
                // EditorGUILayout.Vector2Field(label, (Vector2)value);
                return WrapVisualElement(new Vector2Field(label)
                {
                    value = (Vector2)value,
                });
            }
            if (valueType == typeof(Vector3))
            {
                // EditorGUILayout.Vector3Field(label, (Vector3)value);
                return WrapVisualElement(new Vector3Field(label)
                {
                    value = (Vector3)value,
                });
            }
            if (valueType == typeof(Vector4))
            {
                // EditorGUILayout.Vector4Field(label, (Vector4)value);
                return WrapVisualElement(new Vector4Field(label)
                {
                    value = (Vector4)value,
                });
            }
            if (valueType == typeof(Vector2Int))
            {
                // EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
                return WrapVisualElement(new Vector2IntField(label)
                {
                    value = (Vector2Int)value,
                });
            }
            if (valueType == typeof(Vector3Int))
            {
                // EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
                return WrapVisualElement(new Vector3IntField(label)
                {
                    value = (Vector3Int)value,
                });
            }
            if (valueType == typeof(Color))
            {
                // EditorGUILayout.ColorField(label, (Color)value);
                return WrapVisualElement(new ColorField(label)
                {
                    value = (Color)value,
                });
            }
            if (valueType == typeof(Bounds))
            {
                // EditorGUILayout.BoundsField(label, (Bounds)value);
                return WrapVisualElement(new BoundsField(label)
                {
                    value = (Bounds)value,
                });
            }
            if (valueType == typeof(Rect))
            {
                // EditorGUILayout.RectField(label, (Rect)value);
                return WrapVisualElement(new RectField(label)
                {
                    value = (Rect)value,
                });
            }
            if (valueType == typeof(RectInt))
            {
                // EditorGUILayout.RectIntField(label, (RectInt)value);
                return WrapVisualElement(new RectIntField(label)
                {
                    value = (RectInt)value,
                });
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
            {
                // EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, valueType, true);
                return WrapVisualElement(new ObjectField(label)
                {
                    value = (UnityEngine.Object)value,
                    objectType = valueType,
                });
            }
            if (valueType.BaseType == typeof(Enum))
            {
                return WrapVisualElement(new EnumField((Enum)value)
                {
                    label = label,
                    value = (Enum)value,
                });
            }
            if (valueType.BaseType == typeof(TypeInfo))
            {
                // EditorGUILayout.TextField(label, value.ToString());
                return WrapVisualElement(new TextField(label)
                {
                    value = value.ToString(),
                });
            }
            if (Array.Exists(valueType.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                object[] kvPairs = (value as IEnumerable).Cast<object>().ToArray();

                Foldout foldout = new Foldout
                {
                    text = $"{label} <color=#808080ff>(Dictionary x{kvPairs.Length})</color>",
                };

                const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;


                foreach ((object kvPair, int index) in kvPairs.WithIndex())
                {
                    Type kvPairType = kvPair.GetType();
                    PropertyInfo keyProp = kvPairType.GetProperty("Key", bindAttr);
                    if (keyProp == null)
                    {
                        foldout.Add(new HelpBox($"Failed to obtain key on element {index}: {kvPair}", HelpBoxMessageType.Error));
                        continue;
                    }
                    PropertyInfo valueProp = kvPairType.GetProperty("Value", bindAttr);
                    if (valueProp == null)
                    {
                        foldout.Add(new HelpBox($"Failed to obtain value on element {index}: {kvPair}", HelpBoxMessageType.Error));
                        continue;
                    }

                    object dictKey = keyProp.GetValue(kvPair);
                    object dictValue = valueProp.GetValue(kvPair);
                    foldout.Add(UIToolkitLayout(dictKey, $"{dictKey} <color=#808080ff>(Key {index})</color>"));
                    VisualElement valueContainer = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = SaintsPropertyDrawer.IndentWidth,
                        },
                    };
                    valueContainer.Add(UIToolkitLayout(dictValue, $"{dictValue} <color=#808080ff>(Value {index})</color>"));
                    foldout.Add(valueContainer);
                }

                return foldout;
                // return new HelpBox($"IDictionary {valueType}", HelpBoxMessageType.Error);
            }
            if (value is IEnumerable enumerableValue)
            {
                // List<object> values = enumerableValue.Cast<object>().ToList();
                // Debug.Log($"!!!!!!!!!{value}/{valueType}/{valueType.IsArray}/{valueType.BaseType}");
                // return new ListView(((IEnumerable<object>)enumerableValue).ToList());
                VisualElement root = new VisualElement();

                Foldout foldout = new Foldout
                {
                    text = label,
                };

                // this is sooooo buggy.
                // ListView listView = new ListView(
                //     values,
                //     -1f,
                //     () => new VisualElement(),
                //     (element, index) => element.Add(UIToolkitLayout(values[index], $"Element {index}")))
                // {
                //     showBorder = true,
                //     showBoundCollectionSize  = true,
                // };
                VisualElement listView = new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f),

                        borderTopWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderBottomWidth = 1,
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3,
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        borderLeftColor = EColor.MidnightAsh.GetColor(),
                        borderRightColor = EColor.MidnightAsh.GetColor(),
                        borderTopColor = EColor.MidnightAsh.GetColor(),
                        borderBottomColor = EColor.MidnightAsh.GetColor(),

                        paddingTop = 2,
                        paddingBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                    },
                };

                foreach ((object item, int index) in enumerableValue.Cast<object>().WithIndex())
                {
                    VisualElement child = UIToolkitLayout(item, $"Element {index}");
                    listView.Add(child);
                }

                listView.SetEnabled(false);

                foldout.RegisterValueChangedCallback(evt =>
                {
                    listView.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                });

                root.Add(foldout);
                root.Add(listView);

                return WrapVisualElement(root);
            }

            if (RuntimeUtil.IsNull(value))
            {
                return null;
            }

            // Debug.Log(ReflectUtils.GetMostBaseType(valueType));
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            Foldout genFoldout = new Foldout
            {
                text = label,
            };
            foreach (FieldInfo fieldInfo in valueType.GetFields(bindAttrNormal))
            {
                object fieldValue;
                try
                {
                    fieldValue = fieldInfo.GetValue(value);
                }
#pragma warning disable CS0168
                catch (NullReferenceException e)
#pragma warning restore CS0168
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogException(e);
#endif
                    continue;
                }
                genFoldout.Add(UIToolkitLayout(fieldValue, fieldInfo.Name, fieldInfo.FieldType));
            }

            foreach (PropertyInfo propertyInfo in valueType.GetProperties(bindAttrNormal))
            {
                object propertyValue;
                try
                {
                    propertyValue = propertyInfo.GetValue(value);
                }
#pragma warning disable CS0168
                catch (NullReferenceException e)
#pragma warning restore CS0168
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogException(e);
#endif
                    continue;
                }
                genFoldout.Add(UIToolkitLayout(propertyValue, propertyInfo.Name, propertyInfo.PropertyType));
            }

            return genFoldout;
        }

        private static VisualElement WrapVisualElement(VisualElement visualElement)
        {
            visualElement.SetEnabled(false);
            // visualElement.AddToClassList("unity-base-field__aligned");
            visualElement.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
            return visualElement;
        }

        private class LabelButtonField : BaseField<object>
        {
            public LabelButtonField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        protected static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object value, Action<object> setterOrNull)
        {
            // if (RuntimeUtil.IsNull(value))
            // {
            //     return null;
            // }

            if (valueType == typeof(bool))
            {
                if (oldElement is Toggle oldToggle)
                {
                    oldToggle.SetValueWithoutNotify(Convert.ToBoolean(value));
                    return null;
                }

                Toggle element = new Toggle(label)
                {
                    value = (bool)value,
                };
                element.AddToClassList(Toggle.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(sbyte))
            {
                if (oldElement is IntegerField integerField)
                {
                    integerField.SetValueWithoutNotify((sbyte)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (sbyte)value,
                };
                element.AddToClassList(IntegerField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        sbyte newValue = (sbyte)evt.newValue;
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(byte))
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((byte)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (byte)value,
                };
                element.AddToClassList(IntegerField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        byte newValue = (byte)evt.newValue;
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(short))
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((short)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (short)value,
                };
                element.AddToClassList(IntegerField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        short newValue = (short)evt.newValue;
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }
                return element;
            }
            if (valueType == typeof(ushort))
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((ushort)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (ushort)value,
                };
                element.AddToClassList(IntegerField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        ushort newValue = (ushort)evt.newValue;
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(int))
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((int)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (int)value,
                };
                element.AddToClassList(IntegerField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(uint))
            {
                if (oldElement is LongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify((uint)value);
                    return null;
                }

                LongField element = new LongField(label)
                {
                    value = (uint)value,
                };
                element.AddToClassList(LongField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        uint newValue = (uint)evt.newValue;
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(long))
            {
                if (oldElement is LongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify((long)value);
                    return null;
                }

                LongField element = new LongField(label)
                {
                    value = (long)value,
                };
                element.AddToClassList(LongField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(ulong))
            {
                // wtf...
                // long longValue = Convert.ToInt64(value);
                // long longValue = unchecked((long) value);
                // long longValue = unchecked((long)Convert.ChangeType(value, typeof(long)));
                // long longValue = (long) value;
                ulong ulongRawValue = (ulong)value;
                long longValue = (long) ulongRawValue;
                if (oldElement is LongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify(longValue);
                    return null;
                }

                LongField element = new LongField(label)
                {
                    value = longValue,
                };

                element.AddToClassList(LongField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        // wtf x2...
                        long rawNewValue = evt.newValue;
                        ulong useNewValue;
                        if (rawNewValue < 0)
                        {
                            useNewValue = 0;
                        }
                        else
                        {
                            useNewValue = (ulong) rawNewValue;
                        }
                        long checkLong = (long) useNewValue;

                        setterOrNull(useNewValue);
                        if (rawNewValue != checkLong)
                        {
                            element.SetValueWithoutNotify(checkLong);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(float))
            {
                if (oldElement is FloatField oldFloatField)
                {
                    oldFloatField.SetValueWithoutNotify((float)value);
                    return null;
                }

                FloatField element = new FloatField(label)
                {
                    value = (float)value,
                };
                element.AddToClassList(FloatField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(double))
            {
                if (oldElement is DoubleField oldDoubleField)
                {
                    oldDoubleField.SetValueWithoutNotify((double)value);
                    return null;
                }

                DoubleField element = new DoubleField(label)
                {
                    value = (double)value,
                };
                element.AddToClassList(DoubleField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(string))
            {
                if (oldElement is TextField oldTextField)
                {
                    oldTextField.SetValueWithoutNotify((string)value);
                    return null;
                }

                TextField element = new TextField(label)
                {
                    value = (string)value,
                };
                element.AddToClassList(TextField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector2))
            {
                if (oldElement is Vector2Field oldVector2Field)
                {
                    oldVector2Field.SetValueWithoutNotify((Vector2)value);
                    return null;
                }

                Vector2Field element = new Vector2Field(label)
                {
                    value = (Vector2)value,
                };
                element.AddToClassList(Vector2Field.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector3))
            {
                if (oldElement is Vector3Field oldVector3Field)
                {
                    oldVector3Field.SetValueWithoutNotify((Vector3)value);
                    return null;
                }

                Vector3Field element = new Vector3Field(label)
                {
                    value = (Vector3)value,
                };
                element.AddToClassList(Vector3Field.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector4))
            {
                if (oldElement is Vector4Field oldVector4Field)
                {
                    oldVector4Field.SetValueWithoutNotify((Vector4)value);
                    return null;
                }

                Vector4Field element = new Vector4Field(label)
                {
                    value = (Vector4)value,
                };
                element.AddToClassList(Vector4Field.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector2Int))
            {
                if (oldElement is Vector2IntField oldVector2IntField)
                {
                    oldVector2IntField.SetValueWithoutNotify((Vector2Int)value);
                    return null;
                }

                Vector2IntField element = new Vector2IntField(label)
                {
                    value = (Vector2Int)value,
                };
                element.AddToClassList(Vector2IntField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector3Int))
            {
                if (oldElement is Vector3IntField oldVector3IntField)
                {
                    oldVector3IntField.SetValueWithoutNotify((Vector3Int)value);
                    return null;
                }

                Vector3IntField element = new Vector3IntField(label)
                {
                    value = (Vector3Int)value,
                };
                element.AddToClassList(Vector3IntField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Color))
            {
                if (oldElement is ColorField oldColorField)
                {
                    oldColorField.SetValueWithoutNotify((Color)value);
                    return null;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"Create color field for {label}");
#endif

                ColorField element = new ColorField(label)
                {
                    value = (Color)value,
                };
                element.AddToClassList(ColorField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                        Debug.Log($"Set Color {evt.newValue}");
#endif
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Bounds))
            {
                if (oldElement is BoundsField oldBoundsField)
                {
                    oldBoundsField.SetValueWithoutNotify((Bounds)value);
                    return null;
                }

                BoundsField element = new BoundsField(label)
                {
                    value = (Bounds)value,
                };
                element.AddToClassList(BoundsField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Rect))
            {
                if (oldElement is RectField oldRectField)
                {
                    oldRectField.SetValueWithoutNotify((Rect)value);
                    return null;
                }

                RectField element = new RectField(label)
                {
                    value = (Rect)value,
                };
                element.AddToClassList(RectField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(RectInt))
            {
                if (oldElement is RectIntField oldRectIntField)
                {
                    oldRectIntField.SetValueWithoutNotify((RectInt)value);
                    return null;
                }

                RectIntField element = new RectIntField(label)
                {
                    value = (RectInt)value,
                };
                element.AddToClassList(RectIntField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType.BaseType == typeof(Enum))
            {
                if (oldElement is EnumField oldEnumField)
                {
                    oldEnumField.SetValueWithoutNotify((Enum)value);
                    return null;
                }

                EnumField element = new EnumField(label, (Enum)value);
                // ReSharper disable once PossibleNullReferenceException
                typeof(EnumField).GetField("m_EnumType", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(element, valueType);
                element.AddToClassList(EnumField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
            {
                if (oldElement is ObjectField oldUnityEngineObjectField)
                {
                    oldUnityEngineObjectField.objectType = valueType;
                    oldUnityEngineObjectField.SetValueWithoutNotify((UnityEngine.Object)value);
                    return null;
                }

                ObjectField element = new ObjectField(label)
                {
                    value = (UnityEngine.Object)value,
                    objectType = valueType,
                };
                element.AddToClassList(ObjectField.alignedFieldUssClassName);
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (Array.Exists(valueType.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                object[] kvPairs = (value as IEnumerable).Cast<object>().ToArray();

                Foldout foldout = new Foldout
                {
                    text = $"{label} <color=#808080ff>(Dictionary x{kvPairs.Length})</color>",
                };

                const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;


                foreach ((object kvPair, int index) in kvPairs.WithIndex())
                {
                    Type kvPairType = kvPair.GetType();
                    PropertyInfo keyProp = kvPairType.GetProperty("Key", bindAttr);
                    if (keyProp == null)
                    {
                        foldout.Add(new HelpBox($"Failed to obtain key on element {index}: {kvPair}", HelpBoxMessageType.Error));
                        continue;
                    }
                    PropertyInfo valueProp = kvPairType.GetProperty("Value", bindAttr);
                    if (valueProp == null)
                    {
                        foldout.Add(new HelpBox($"Failed to obtain value on element {index}: {kvPair}", HelpBoxMessageType.Error));
                        continue;
                    }

                    object dictKey = keyProp.GetValue(kvPair);
                    object dictValue = valueProp.GetValue(kvPair);
                    foldout.Add(UIToolkitLayout(dictKey, $"{dictKey} <color=#808080ff>(Key {index})</color>"));
                    VisualElement valueContainer = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = SaintsPropertyDrawer.IndentWidth,
                        },
                    };
                    valueContainer.Add(UIToolkitLayout(dictValue, $"{dictValue} <color=#808080ff>(Value {index})</color>"));
                    foldout.Add(valueContainer);
                }

                return foldout;
                // return new HelpBox($"IDictionary {valueType}", HelpBoxMessageType.Error);
            }
            if (value is IEnumerable enumerableValue)
            {
                // Debug.Log($"oldElement={oldElement}, {oldElement is Foldout}");
                return MakeListView(oldElement as Foldout, label, valueType, enumerableValue, enumerableValue.Cast<object>().ToArray(), setterOrNull);
                // VisualElement root = new VisualElement();
                //
                // Foldout foldout = new Foldout
                // {
                //     text = label,
                // };
                //
                // VisualElement listView = new VisualElement
                // {
                //     style =
                //     {
                //         backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f),
                //
                //         borderTopWidth = 1,
                //         borderLeftWidth = 1,
                //         borderRightWidth = 1,
                //         borderBottomWidth = 1,
                //         borderTopLeftRadius = 3,
                //         borderTopRightRadius = 3,
                //         borderBottomLeftRadius = 3,
                //         borderBottomRightRadius = 3,
                //         borderLeftColor = EColor.MidnightAsh.GetColor(),
                //         borderRightColor = EColor.MidnightAsh.GetColor(),
                //         borderTopColor = EColor.MidnightAsh.GetColor(),
                //         borderBottomColor = EColor.MidnightAsh.GetColor(),
                //
                //         paddingTop = 2,
                //         paddingBottom = 2,
                //         paddingLeft = 2,
                //         paddingRight = 2,
                //     },
                // };
                //
                // foreach ((object item, int index) in enumerableValue.Cast<object>().WithIndex())
                // {
                //     VisualElement child = UIToolkitLayout(item, $"Element {index}");
                //     listView.Add(child);
                // }
                //
                // listView.SetEnabled(false);
                //
                // foldout.RegisterValueChangedCallback(evt =>
                // {
                //     listView.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                // });
                //
                // root.Add(foldout);
                // root.Add(listView);
                //
                // return WrapVisualElement(root);
            }
            if (valueType.BaseType == typeof(TypeInfo))  // generic type?
            {
                // EditorGUILayout.TextField(label, value.ToString());
                return WrapVisualElement(new TextField(label)
                {
                    value = value.ToString(),
                });
            }

            // TODO: Allow to select different type

            if (RuntimeUtil.IsNull(value))
            {
                if (setterOrNull is null)
                {
                    TextField textField = new TextField(label)
                    {
                        value = "null",
                        pickingMode = PickingMode.Ignore,
                    };

                    if(_nullUss == null)
                    {
                        _nullUss = Util.LoadResource<StyleSheet>("UIToolkit/UnityTextInputElementWarning.uss");
                    }
                    textField.styleSheets.Add(_nullUss);

                    return WrapVisualElement(textField);
                }

                LabelButtonField labelButtonField = new LabelButtonField(label, new Button(() =>
                {
                    if (valueType.IsArray)
                    {
                        setterOrNull(Array.CreateInstance(ReflectUtils.GetElementType(valueType), 0));
                        return;
                    }
                    setterOrNull(Activator.CreateInstance(valueType));
                })
                {
                    text = $"null (Click to Create)",
                    tooltip = "Click to Create",
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleLeft,
                    },
                });
                labelButtonField.AddToClassList(LabelButtonField.alignedFieldUssClassName);
                return labelButtonField;
            }

            // Debug.Log(ReflectUtils.GetMostBaseType(valueType));
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            Foldout genFoldout = oldElement as Foldout;
            bool useOld = genFoldout != null;
            if (!useOld)
            {
                genFoldout = new Foldout
                {
                    text = label,
                    style =
                    {
                        position = Position.Relative,
                    },
                };

                // Debug.Log($"new foldout valueType: {valueType.IsValueType}, setter={setterOrNull}");

                if (!valueType.IsValueType && setterOrNull != null)
                {
                    // nullable
                    genFoldout.Q<Toggle>().Add(new Button(() => setterOrNull(null))
                    {
                        // text = "x",
                        tooltip = "Set to null",
                        style =
                        {
                            position = Position.Absolute,
                            // top = -EditorGUIUtility.singleLineHeight,
                            top = 0,
                            right = 0,
                            width = EditorGUIUtility.singleLineHeight,
                            height = EditorGUIUtility.singleLineHeight,

                            backgroundImage = Util.LoadResource<Texture2D>("close.png"),
#if UNITY_2022_2_OR_NEWER
                            backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                            backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                            unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                        },
                    });
                }
            }
            foreach (FieldInfo fieldInfo in valueType.GetFields(bindAttrNormal))
            {
                string name = fieldInfo.Name;
                object fieldValue = fieldInfo.GetValue(value);
                VisualElement result = UIToolkitValueEdit(
                    oldElement?.Q<VisualElement>(name: name),
                    ObjectNames.NicifyVariableName(name),
                    fieldInfo.FieldType,
                    fieldValue,
                    newValue =>
                    {
                        fieldInfo.SetValue(value, newValue);
                        setterOrNull?.Invoke(value);
                    });
                // Debug.Log($"{name}: {result}: {fieldInfo.FieldType}");
                // ReSharper disable once InvertIf
                if(result != null)
                {
                    result.name = name;
                    genFoldout.Add(result);
                }
            }

            foreach (PropertyInfo propertyInfo in valueType.GetProperties(bindAttrNormal))
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                string name = propertyInfo.Name;
                object propertyValue = propertyInfo.GetValue(value);

                VisualElement result = UIToolkitValueEdit(
                    oldElement?.Q<VisualElement>(name: name),
                    ObjectNames.NicifyVariableName(name),
                    propertyInfo.PropertyType,
                    propertyValue,
                    propertyInfo.CanWrite
                        ? (newValue =>
                        {
                            propertyInfo.SetValue(value, newValue);
                            setterOrNull?.Invoke(newValue);
                        })
                        : null);
                // ReSharper disable once InvertIf
                if(result != null)
                {
                    result.name = name;
                    genFoldout.Add(result);
                }
            }

            bool enabled = setterOrNull != null;
            if (genFoldout.enabledSelf != enabled)
            {
                genFoldout.SetEnabled(enabled);
            }

            return useOld? null: genFoldout;
        }

        // private int _listCurPageIndex = 0;
        // private List<int> _listItemIndexToOriginIndex;

        private class ListViewPayload
        {
            public List<object> RawValues;
            public List<int> ItemIndexToOriginIndex;
            public object RawListValue;
        }

        private static Foldout MakeListView(Foldout oldElement, string label, Type valueType, object rawListValue, object[] listValue, Action<object> setterOrNull)
        {
            Foldout foldout = oldElement;
            if (foldout == null)
            {
                // Debug.Log($"Create new Foldout");
                foldout = new Foldout
                {
                    text = label,
                };
                VisualElement foldoutContent = foldout.Q<VisualElement>(className: "unity-foldout__content");
                if (foldoutContent != null)
                {
                    foldoutContent.style.marginLeft = 0;
                }

                // nullable
                foldout.Q<Toggle>().Add(new Button(() => setterOrNull(null))
                {
                    // text = "x",
                    tooltip = "Set to null",
                    style =
                    {
                        position = Position.Absolute,
                        // top = -EditorGUIUtility.singleLineHeight,
                        top = 0,
                        right = 0,
                        width = EditorGUIUtility.singleLineHeight,
                        height = EditorGUIUtility.singleLineHeight,

                        backgroundImage = Util.LoadResource<Texture2D>("close.png"),
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                            unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
                });
            }

            ListView listView = foldout.Q<ListView>();
            if (listView == null)
            {
                ListViewPayload payload = new ListViewPayload
                {
                    RawValues = listValue.ToList(),
                    ItemIndexToOriginIndex = listValue.Select((_, index) => index).ToList(),
                    RawListValue = rawListValue,
                };
                // Debug.Log($"Create new listView");
                listView = new ListView
                {
                    selectionType = SelectionType.Multiple,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                    showBoundCollectionSize = false,
                    showFoldoutHeader = false,
                    headerTitle = label,
                    showAddRemoveFooter = true,
                    reorderMode = ListViewReorderMode.Animated,
                    reorderable = true,
                    style =
                    {
                        flexGrow = 1,
                        position = Position.Relative,
                    },
                    itemsSource = listValue.Select(((o, index) => index)).ToList(),
                    makeItem = () => new VisualElement(),

                    userData = payload,
                };

                Type elementType = ReflectUtils.GetElementType(valueType);

                void BindItem(VisualElement visualElement, int index)
                {
                    // int actualIndex = (int)listView.itemsSource[index];
                    // Debug.Log($"{index} -> {actualIndex}");
                    // Debug.Log($"index={index}, ItemIndexToOriginIndex={string.Join(",", payload.ItemIndexToOriginIndex)}");

                    VisualElement firstChild = visualElement.Children().FirstOrDefault();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                    Debug.Log($"bind {index} with old child: {firstChild}");
#endif

                    int actualIndex = payload.ItemIndexToOriginIndex[index];
                    object actualValue = payload.RawValues[actualIndex];
                    VisualElement item = UIToolkitValueEdit(
                        firstChild,
                        $"Element {actualIndex}",
                        elementType,
                        actualValue,
                        newItemValue =>
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                            Debug.Log($"List {actualIndex} set newValue {newItemValue}");
#endif
                            IList rawListValueArray = (IList) payload.RawListValue;
                            rawListValueArray[actualIndex] = newItemValue;
                            payload.RawValues[actualIndex] = newItemValue;
                        });
                    if (item != null)
                    {
                        visualElement.Clear();
                        visualElement.Add(item);
                    }
                }

                listView.bindItem = BindItem;

                Button listViewAddButton = listView.Q<Button>("unity-list-view__add-button");

                listViewAddButton.clickable = new Clickable(() =>
                {
                    int oldSize = payload.RawValues.Count;
                    int newSize = oldSize + 1;
                    object addItem = elementType.IsValueType
                        ? Activator.CreateInstance(elementType)
                        : null;

                    if(valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                    {
                        Array newArray = Array.CreateInstance(elementType, newSize);
                        payload.RawValues.Add(addItem);
                        Array.Copy(payload.RawValues.ToArray(), newArray, oldSize);
                        payload.RawListValue = newArray;
                        setterOrNull?.Invoke(newArray);
                    }
                    else
                    {
                        IList rawListValueArray = (IList) payload.RawListValue;
                        rawListValueArray.Add(addItem);
                        payload.RawValues.Add(addItem);
                        payload.ItemIndexToOriginIndex = payload.RawValues.Select((_, index) => index).ToList();
                        listView.itemsSource = payload.ItemIndexToOriginIndex.ToList();
                    }
                });

                listView.itemsRemoved += objects =>
                {
                    List<int> removeIndexInRaw = objects
                        .Select(removeIndex => payload.ItemIndexToOriginIndex[removeIndex])
                        .OrderByDescending(each => each)
                        .ToList();

                    if(valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                    {
                        Array newArray = Array.CreateInstance(elementType, payload.RawValues.Count - removeIndexInRaw.Count());
                        Array rawArray = (Array) payload.RawListValue;
                        int copyIndex = 0;
                        foreach ((object rawValue, int rawIndex) in rawArray.Cast<object>().WithIndex())
                        {
                            if (removeIndexInRaw.Contains(rawIndex))
                            {
                                continue;
                            }

                            newArray.SetValue(rawValue, copyIndex);
                            copyIndex++;
                        }
                        // payload.RawValues.Add(addItem);
                        // Array.Copy(payload.RawValues.ToArray(), newArray, oldSize);
                        payload.RawListValue = newArray;
                        setterOrNull?.Invoke(newArray);
                    }
                    else
                    {
                        IList rawListValueArray = (IList) payload.RawListValue;
                        foreach (int removeIndex in removeIndexInRaw)
                        {
                            rawListValueArray.RemoveAt(removeIndex);
                        }
                    }
                };

                listView.itemIndexChanged += (first, second) =>
                {
                    int fromPropIndex = payload.ItemIndexToOriginIndex[first];
                    int toPropIndex = payload.ItemIndexToOriginIndex[second];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                    Debug.Log($"drag {fromPropIndex}({first}) -> {toPropIndex}({second}); ItemIndexToOriginIndex={string.Join(",", payload.ItemIndexToOriginIndex)}");
#endif

                    IList lis = (IList)payload.RawListValue;
                    MoveArrayElement(lis, fromPropIndex, toPropIndex);
                    // (lis[fromPropIndex], lis[toPropIndex]) = (lis[toPropIndex], lis[fromPropIndex]);
                    // payload.RawValues = lis.Cast<object>().ToList();
                    // (payload.RawValues[fromPropIndex], payload.RawValues[toPropIndex]) = (payload.RawValues[toPropIndex], payload.RawValues[fromPropIndex]);
                    // (payload.ItemIndexToOriginIndex[fromPropIndex], payload.ItemIndexToOriginIndex[toPropIndex]) = (payload.ItemIndexToOriginIndex[toPropIndex], payload.ItemIndexToOriginIndex[fromPropIndex]);
                    // payload.ItemIndexToOriginIndex = payload.RawValues.Select((_, index) => index).ToList();
                    // listView.Rebuild();
                };

                foldout.Add(listView);
            }

            ListViewPayload oldPayload = (ListViewPayload)listView.userData;
            oldPayload.RawValues = listValue.ToList();
            oldPayload.RawListValue = rawListValue;

            // Debug.Log($"Refresh count={listValue.Length}");
            oldPayload.ItemIndexToOriginIndex = oldPayload.RawValues.Select((o, index) => index).ToList();
            listView.itemsSource = oldPayload.ItemIndexToOriginIndex.ToList();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
            Debug.Log($"ItemIndexToOriginIndex={string.Join(",", oldPayload.ItemIndexToOriginIndex)}");
#endif
            // Debug.Log($"itemSource({listView.itemsSource.Count})={string.Join(",", listView.itemsSource)}");
            // if (listValue.Length > 0)
            // {
            //     Debug.Log($"0 listValue={listValue[0]}; listView.itemsSource={listView.itemsSource[0]}");
            // }
            // listView.Rebuild();

            return oldElement == null? foldout : null;
        }

        private static void MoveArrayElement(IList list, int fromIndex, int toIndex)
        {
            if (list == null)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentNullException(nameof(list));
#endif
                return;
            }
            if (fromIndex < 0 || fromIndex >= list.Count)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
#endif
                return;
            }
            if (toIndex < 0 || toIndex >= list.Count)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentOutOfRangeException(nameof(toIndex));
#endif
                return;
            }

            if (fromIndex == toIndex)
            {
                return;
            }

            // shifting
            object item = list[fromIndex];

            if (fromIndex < toIndex)
            {
                for (int i = fromIndex; i < toIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }

            list[toIndex] = item;
        }
    }
}
#endif
