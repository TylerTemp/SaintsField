#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.XPathDrawers;
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
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
                    root.schedule.Execute(() => OnUpdateUIToolKit());
                    root.schedule.Execute(() => OnUpdateUIToolKit()).Every(100);
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
                         RichTextDrawer.ParseRichXml(xmlContent, useLabel, member, infoBoxUserData.FieldWithInfo.Target))
                     )
            {
                label.Add(richTextElement);
            }
        }

        protected virtual PreCheckResult OnUpdateUIToolKit()
        {
            foreach (HelpBox helpBox in _rootElement.Query<HelpBox>(className: ClassInfoBox).ToList())
            {
                UpdateInfoBox(helpBox);
            }

            return UpdatePreCheckUIToolkit(FieldWithInfo, _rootElement);
        }

        protected PreCheckResult UpdatePreCheckUIToolkit(SaintsFieldWithInfo fieldWithInfo, VisualElement result)
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
                catch (NullReferenceException e)
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
                catch (NullReferenceException e)
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
    }
}
#endif
