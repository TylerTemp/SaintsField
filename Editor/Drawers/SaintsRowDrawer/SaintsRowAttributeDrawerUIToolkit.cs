#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsRowDrawer
{
    public partial class SaintsRowAttributeDrawer
    {
        public class ForceInlineScoop : IDisposable
        {
            // ReSharper disable once NotAccessedField.Global
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

        protected override bool UseCreateFieldUIToolKit => true;

        public const string SaintsRowClass = "saints-field--saintsrow";
        private static string NameActualContaier(SerializedProperty property) => $"${property.propertyPath}__saintsrow";


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
                root = new EmptyPrefabOverrideElement(property)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                    name = NameActualContaier(property),
                };
            }
            else
            {
                VisualElement foldoutWrapper = new VisualElement();

                Foldout foldout = new Foldout
                {
                    text = label,
                    value = property.isExpanded,
                    style =
                    {
                        flexGrow = 1,
                    },
                    name = NameActualContaier(property),
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    property.isExpanded = evt.newValue;
                });

                foldoutWrapper.Add(foldout);
                foldoutWrapper.Add(new EmptyPrefabOverrideElement(property)
                {
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        bottom = 0,
                        left = 0,
                        right = 0,
                        height = 18,
                    },
                    pickingMode = PickingMode.Ignore,
                });

                // foldout.Add(new EmptyPrefabOverrideElement(property)
                // {
                //     style =
                //     {
                //         position = Position.Absolute,
                //         top = -20,
                //         bottom = 0,
                //         left = -15,
                //         right = 0,
                //         height = 18,
                //     },
                //     pickingMode = PickingMode.Ignore,
                // });

                // Toggle toggle = foldout.Q<Toggle>();
                // if (toggle != null)
                // {
                //     toggle.Add(
                //         new EmptyPrefabOverrideElement(property)
                //         {
                //             style =
                //             {
                //                 position = Position.Absolute,
                //                 top = 0,
                //                 bottom = 0,
                //                 left = 0,
                //                 right = 0,
                //             },
                //             pickingMode = PickingMode.Ignore,
                //         });
                //     // Label toggleLabel = toggle.Q<Label>();
                //     // if (toggleLabel != null)
                //     // {
                //     //     // EmptyPrefabOverrideElement emptyPrefabOverrideElement =
                //     //     //     new EmptyPrefabOverrideElement(property)
                //     //     //     {
                //     //     //         style =
                //     //     //         {
                //     //     //             position = Position.Absolute,
                //     //     //             top = 0,
                //     //     //             bottom = 0,
                //     //     //             left = 0,
                //     //     //             right = 0,
                //     //     //         },
                //     //     //         pickingMode = PickingMode.Ignore,
                //     //     //     };
                //     //     // toggleLabel.Add(emptyPrefabOverrideElement);
                //     // }
                // }

                UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => {});

                // root = foldout;
                root = foldoutWrapper;
            }

            root.AddToClassList(SaintsRowClass);
            root.AddToClassList(ClassAllowDisable);

            FillElement(root, property, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder);

            // ReSharper disable once InvertIf
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                string propPath = property.propertyPath;
                root.userData = property.managedReferenceId;
                root.schedule.Execute(() =>
                    {
                        long curId = (long) root.userData;
                        // ReSharper disable once InvertIf
                        long newId;
                        try
                        {
                            newId = property.managedReferenceId;
                        }
                        catch (InvalidOperationException)
                        {
                            return;
                        }
                        // ReSharper disable once InvertIf
                        if (curId != newId)
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
            // Debug.Log($"{property.propertyPath}: {inHorizontalLayout}");
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
                    }
                }

                if (error != "")
                {
                    root.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                Debug.Assert(value != null);
                // value = getValue;
            }

            // Debug.Assert(value != null);

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            IReadOnlyList<ISaintsRenderer> renderer =
                SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, makeRenderer, new []{value});

             VisualElement bodyElement = new VisualElement();

             Type objectType = value.GetType();
             IPlayaClassAttribute[] playaClassAttributes = ReflectCache.GetCustomAttributes<IPlayaClassAttribute>(objectType);

             foreach (ISaintsRenderer saintsRenderer in SaintsEditor.GetClassStructRenderer(objectType, playaClassAttributes, property.serializedObject, new[]{value}))
             {
                 VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                 if (rendererElement != null)
                 {
                     bodyElement.Add(rendererElement);
                 }
             }

            // this... fixed by adding Bind()... wtf...
            foreach (ISaintsRenderer saintsRenderer in renderer)
            {
                saintsRenderer.InAnyHorizontalLayout = inHorizontalLayout;
                saintsRenderer.SetSerializedProperty(property);
                VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                if (rendererElement != null)
                {
                    // Debug.Log($"add: {saintsRenderer}");
                    bodyElement.Add(rendererElement);
                }
            }

            VisualElement actualContainer = root.Q<VisualElement>(NameActualContaier(property));

            actualContainer.Add(bodyElement);
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(doTweenPlayRecorder));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(doTweenPlayRecorder));
#endif
        }
    }
}
#endif
