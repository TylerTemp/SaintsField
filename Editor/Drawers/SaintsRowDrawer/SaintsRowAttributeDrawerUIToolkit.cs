#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsRowDrawer
{
    public partial class SaintsRowAttributeDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        public const string SaintsRowClass = "saints-field--saintsrow";

        public class ForceInlineScoop : IDisposable
        {
            public static bool Inline;

            public ForceInlineScoop(bool inline)
            {
                Inline = inline;
            }

            public void Dispose()
            {
                Inline = false;
            }
        }

        public static VisualElement CreateElement(SerializedProperty property, string label, MemberInfo info, bool inHorizontalLayout, SaintsRowAttribute saintsRowAttribute, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent)
        {
            bool inline = saintsRowAttribute?.Inline ?? false;

            if (!inline)
            {
                inline = ForceInlineScoop.Inline;
            }

            VisualElement root;

            if (inline)
            {
                root = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }
            else
            {
                Foldout foldout = new Foldout
                {
                    text = label,
                    value = property.isExpanded,
                    style =
                    {
                        flexGrow = 1,
                    },
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    property.isExpanded = evt.newValue;
                });

                UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => {});

                root = foldout;
            }

            root.AddToClassList(SaintsRowClass);
            root.AddToClassList(ClassAllowDisable);

            FillElement(root, property, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                string propPath = property.propertyPath;
                root.userData = property.managedReferenceId;
                root.schedule.Execute(() =>
                    {
                        long curId = (long) root.userData;
                        // ReSharper disable once InvertIf
                        if (curId != property.managedReferenceId)
                        {
                            // Debug.Log($"{property.propertyPath} Changed {curId} -> {property.managedReferenceId}/{property.managedReferenceFieldTypename}");
                            root.userData = property.managedReferenceId;
                            root.Clear();

                            SerializedProperty newProp = property.serializedObject.FindProperty(propPath);

                            FillElement(root, newProp, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder);
                        }
                        // Debug.Log(property.managedReferenceId);
                    })
                    .Every(100);
            }
            return root;
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsRowAttribute saintsRowAttribute = saintsAttribute as SaintsRowAttribute;

            VisualElement ele = CreateElement(property, property.displayName, info, InHorizontalLayout, saintsRowAttribute, this, this, parent);
            //
            // if (InHorizentalLayout)
            // {
            //     ele.style.marginLeft = IndentWidth;
            // }

            return ele;
        }

        // private static Type GetMemberType(MemberInfo member)
        // {
        //     return member.MemberType == MemberTypes.Property
        //         ? ((PropertyInfo) member).PropertyType
        //         : ((FieldInfo) member).FieldType;
        // }

        private static void FillElement(VisualElement root, SerializedProperty property, MemberInfo info, bool inHorizontalLayout, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder)
        {
            object value = null;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                value = property.managedReferenceValue;
                if (value == null)
                {
                    return;
                }
            }
            else
            {
                string error = "";
                object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                // Debug.Log($"parentValue={parentValue}/{property.propertyPath}");
                if (parentValue == null)
                {
                    error = $"Parent of {property.propertyPath} not found";
                }
                else
                {
                    (string getValueError, int _, object getValue) = Util.GetValue(property, info, parentValue);
                    if (getValueError != "")
                    {
                        error = getValueError;
                    }
                    else
                    {
                        value = getValue;

                        // Debug.Log(value);
                        if (value == null)
                        {
                            // foreach (SerializedProperty subProp in SerializedUtils.GetPropertyChildren(property))
                            // {
                            //     // switch (subProp.)
                            //     // {
                            //     //
                            //     // }
                            // }

                            // var p = new PropertyField(property);
                            // root.Add(p);
                            // return;

                            // Type rawType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                            //     ? ReflectUtils.GetElementType(GetMemberType(info))
                            //     : GetMemberType(info);
                            //
                            // // Undo.RecordObject(property.serializedObject.targetObject, property.propertyPath);
                            // // value = Activator.CreateInstance(rawType, true);
                            // // Util.SignPropertyValue(property, info, parent, value);
                            //
                            // property.boxedValue = value = Activator.CreateInstance(rawType, true);
                            // property.serializedObject.ApplyModifiedProperties();

                            // return;
                        }



                        // if (value == null)
                        // {
                        //     Type rawType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                        //         ? ReflectUtils.GetElementType(GetMemberType(info))
                        //         : GetMemberType(info);
                        //
                        //     Undo.RecordObject(property.serializedObject.targetObject, property.propertyPath);
                        //     value = Activator.CreateInstance(rawType, true);
                        //     Util.SignPropertyValue(property, info, parent, value);
                        // }
                    }
                }

                if (error != "")
                {
                    root.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                // value = getValue;
            }

            // Debug.Assert(value != null);

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            // SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute)attribute;

            IReadOnlyList<ISaintsRenderer> renderer =
                SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, makeRenderer, value);

//             // Debug.Log($"{renderer.Count}");
//
//             // VisualElement bodyElement = SaintsEditor.CreateVisualElement(renderer);


             VisualElement bodyElement = new VisualElement();
             // bodyElement.Add(new Label(property.displayName));

             // // this works just fine
             // foreach (KeyValuePair<string,SerializedProperty> kv in serializedFieldNames)
             // {
             //     Debug.Log($"{kv.Key} -> {kv.Value.propertyPath}");
             //     // bodyElement.Add(new Label(kv.Key));
             //     var prop = new PropertyField(kv.Value);
             //     prop.Bind(property.serializedObject);
             //     bodyElement.Add(prop);
             // }

             // this... fixed by adding Bind()... wtf...
             foreach (ISaintsRenderer saintsRenderer in renderer)
             {
                 saintsRenderer.InAnyHorizontalLayout = inHorizontalLayout;
                 VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                 if (rendererElement != null)
                 {
                     // Debug.Log($"add: {saintsRenderer}");
                     bodyElement.Add(rendererElement);
                 }
             }

             root.Add(bodyElement);

             // Foldout foldout = new Foldout
             // {
             //     value = true,
             // };
             // foldout.Add(bodyElement);
             // root.Add(foldout);
             //
             // return;

//             foreach (ISaintsRenderer saintsRenderer in renderer)
//             {
//                 VisualElement rendererElement = saintsRenderer.CreateVisualElement();
//                 if (rendererElement != null)
//                 {
//                     Debug.Log($"add {saintsRenderer}: {rendererElement}");
//                     bodyElement.Add(rendererElement);
//                 }
//             }
//
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(doTweenPlayRecorder));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(doTweenPlayRecorder));
#endif

            // if (saintsRowAttribute?.Inline ?? false)
            // {
            //     root.Add(bodyElement);
            //     return;
            // }

            // bodyElement.style.paddingLeft = SaintsPropertyDrawer.IndentWidth;

            // Debug.Log(property.isExpanded);

            // Foldout toggle = new Foldout
            // {
            //     text = property.displayName,
            //     // value = property.isExpanded,
            //     value = true,
            // };
            //
            // // bodyElement.style.display = property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            // toggle.RegisterValueChangedCallback(evt =>
            // {
            //     property.isExpanded = evt.newValue;
            //     // bodyElement.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            // });
            //
            // toggle.Add(bodyElement);
            //
            // root.Add(toggle);
            // root.Add(bodyElement);

        }
    }
}
#endif
