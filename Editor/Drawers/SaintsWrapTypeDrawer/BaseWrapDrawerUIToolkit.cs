#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BaseWrapTypeDrawer
{
    public partial class BaseWrapDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            InHorizontalLayout = true;

            VisualElement root = new VisualElement();
            // Debug.Log($"allAttributes={string.Join(",", allAttributes)}");
            root.Add(CreateElement(property, saintsAttribute, allAttributes, container, info, parent));
            root.TrackPropertyValue(property.FindPropertyRelative("wrapType"), _ =>
            {
                root.Clear();
                VisualElement nc = CreateElement(property, saintsAttribute, allAttributes, container, info, parent);
                root.Add(nc);
            });
            return root;
        }

        private VisualElement CreateElement(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            Type elementType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

            // Debug.Log(info.FieldType);
            // Debug.Log(elementType);
            Type wrapType = elementType.GetGenericArguments()[0];

            SerializedProperty wrapTypeProp = property.FindPropertyRelative("wrapType");

            switch (wrapTypeProp.intValue)
            {
                case (int)WrapType.Field:
                    return new SaintsWrapElement(
                        true,
                        wrapType,
                        property.FindPropertyRelative("valueField"),
                        allAttributes,
                        this,
                        this,
                        parent
                    );
                case (int)WrapType.Array:
                case (int)WrapType.List:
                {
                    bool isArray = wrapTypeProp.intValue == (int)WrapType.Array;
                    string arrayPropName = isArray ? "valueArray" : "valueList";
                    SerializedProperty arrayProp = property.FindPropertyRelative(arrayPropName);
                    Type arrayOrListElementType = wrapType.IsArray? wrapType.GetElementType(): wrapType.GetGenericArguments()[0];
                    ListView listView = new ListView
                    {
                        showBorder = true,
                        selectionType = SelectionType.Multiple,
                        showAddRemoveFooter = true,
                        showBoundCollectionSize = true,
                        showFoldoutHeader = true,
                        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                        showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                        reorderable = true,
                        reorderMode = ListViewReorderMode.Animated,

                        makeItem = () => new VisualElement(),
                        bindItem = (element, index) =>
                        {
                            SerializedProperty itemProp = arrayProp.GetArrayElementAtIndex(index);
                            // Debug.Log(itemProp.propertyPath);
                            UnbindChildren(element);
                            element.Clear();

                            // Debug.Log($"bind {itemProp.propertyPath}");
                            SaintsWrapElement result = new SaintsWrapElement(
                                true,
                                arrayOrListElementType,
                                itemProp,
                                allAttributes,
                                this,
                                this,
                                parent
                            );

                            // result.Bind(property.serializedObject);
                            // VisualElement result = new Label(itemProp.FindPropertyRelative("value").propertyPath);
                            // VisualElement result = new Label(itemProp.propertyPath);
                            element.Add(result);
                        },
                        unbindItem = (element, _) =>
                        {
                            UnbindChildren(element);
                            element.Clear();
                            // Debug.Log(element);
                            // Debug.Log(i);
                        },
                    };
                    Toggle listViewToggle = listView.Q<Toggle>();
                    if (listViewToggle != null && listViewToggle.style.marginLeft != -12)
                    {
                        listViewToggle.style.marginLeft = -12;
                    }
                    listView.AddToClassList(ClassAllowDisable);

                    Debug.Assert(arrayOrListElementType != null);
                    string str = "saints-field--list-view--" + arrayProp.propertyPath;
                    listView.headerTitle = isArray? $"{arrayOrListElementType.Name} (Array)": $"{arrayOrListElementType.Name} (List)";
                    listView.userData = arrayProp;
                    listView.bindingPath = arrayProp.propertyPath;
                    // Debug.Log(arrayProp.arraySize);
                    listView.viewDataKey = str;
                    listView.name = str;
                    listView.BindProperty(arrayProp);
                    return listView;
                }
                case (int)WrapType.T:
                {
                    // (SerializedProperty realProp, FieldInfo realInfo) = GetBasicInfo(property, info);
                    // PropertyAttribute[] fieldAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(info);

                    SerializedProperty valueProp = property.FindPropertyRelative("value");
                    Type valueType = info.FieldType.GetGenericArguments()[0];
                    // Debug.Log(valueType);
                    FieldInfo fInfo = valueType.GetField("value", BindingFlags.Instance | BindingFlags.Public);
                    (PropertyAttribute[] _, object valueParent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(valueProp);

                    using(new SaintsRowAttributeDrawer.ForceInlineScoop(1))
                    {
                        VisualElement r =
                            UIToolkitUtils.CreateOrUpdateFieldProperty(
                                valueProp,
                                // fieldAttributes,
                                allAttributes,
                                valueType,
                                null,
                                fInfo,
                                true,
                                this,
                                this,
                                null,
                                true,
                                valueParent
                            );
                        if (r != null)
                        {
                            r.style.width = new StyleLength(Length.Percent(100));
                            return r;
                        }
                    }
                    // return new PropertyField(property.FindPropertyRelative("value"));

                    goto default;
                }
                default:
                    return new HelpBox($"Failed to render {property.propertyPath}, please report this issue", HelpBoxMessageType.Error);
            }
        }

        private static void UnbindChildren(VisualElement ve)
        {
            foreach (VisualElement child in ve.Children())
            {
                UIToolkitUtils.Unbind(child);
            }
            // UIToolkitUtils.Unbind(ve);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
        }
    }
}
#endif
