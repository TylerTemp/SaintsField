#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
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
            public static int InlineCount;

            public ForceInlineScoop(int inlineCount)
            {
                InlineCount = inlineCount;
            }

            public void Dispose()
            {
                InlineCount = 0;
            }
        }

        protected override bool UseCreateFieldUIToolKit => true;

        public const string SaintsRowClass = "saints-field--saintsrow";
        // private static string NameActualContainer(SerializedProperty property) => $"{property.propertyPath}__saints_row";

        public static VisualElement CreateElement(SerializedProperty property, string label, MemberInfo info, bool inHorizontalLayout, SaintsRowAttribute saintsRowAttribute, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent, IRichTextTagProvider richTextTagProvider)
        {
            bool inline = saintsRowAttribute?.Inline ?? false;

            if (ForceInlineScoop.InlineCount > 0)
            {
                if (!inline)
                {
                    inline = true;
                }
                ForceInlineScoop.InlineCount--;
            }

            VisualElement root;

            if (inline)
            {
                // root = new EmptyPrefabOverrideElement(property)
                root = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                    // name = NameActualContainer(property),
                };
                root.Add(new Label
                {
                    style =
                    {
                        display = DisplayStyle.None,
                    },
                });
            }
            else
            {
                Foldout foldout = new FoldoutPrefabOverrideElement(property)
                {
                    text = label,
                    value = property.isExpanded,
                    style =
                    {
                        flexGrow = 1,
                    },
                    // name = NameActualContainer(property),
                    viewDataKey = property.propertyPath,
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    property.isExpanded = evt.newValue;
                });

                UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => {});

                root = foldout;
            }

            root.AddToClassList(ClassLabelFieldUIToolkit);

            root.AddToClassList(SaintsRowClass);
            root.AddToClassList(ClassAllowDisable);

            FillElement(root, label, property, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder, parent, richTextTagProvider);

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
                        if (!SerializedUtils.IsOk(property))
                        {
                            return;
                        }
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

                            // VisualElement actualContainer = root.Q<VisualElement>(NameActualContainer(property));
                            root.Clear();
                            if (inline)
                            {
                                root.Add(new Label
                                {
                                    style =
                                    {
                                        display = DisplayStyle.None,
                                    },
                                });
                            }

                            SerializedProperty newProp = property.serializedObject.FindProperty(propPath);

                            FillElement(root, label, newProp, info, inHorizontalLayout, makeRenderer, doTweenPlayRecorder, parent, richTextTagProvider);
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

            VisualElement ele = CreateElement(property, property.displayName, info, InHorizontalLayout, saintsRowAttribute, this, this, parent, this);
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

        private static void FillElement(VisualElement root, string label, SerializedProperty property, MemberInfo info, bool inHorizontalLayout, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent, IRichTextTagProvider richTextTagProvider)
        {
            // Debug.Log(info.Name);
            // Debug.Log(info.DeclaringType);
            // Debug.Log(info.ReflectedType);

            // SaintsSerializedActualAttribute saintsSerializedActual = ReflectCache.GetCustomAttributes<SaintsSerializedActualAttribute>(info).FirstOrDefault();
            // // Debug.Log($"{saintsSerializedActual?.Path}/{saintsSerializedActual?.PathType}");
            // if (saintsSerializedActual != null)
            // {
            //     if (label.EndsWith("__Saints Serialized__"))
            //     {
            //         label = label[..^"__Saints Serialized__".Length];
            //     }
            //     // Debug.Log($"{info.Name}/{property.propertyPath}/{saintsSerializedActual.Name}/{saintsSerializedActual.ElementType}");
            //     VisualElement renderSerializedActual = RenderSerializedActual(saintsSerializedActual, label, property, (FieldInfo)info, inHorizontalLayout, parent, richTextTagProvider);
            //     root.Add(renderSerializedActual);
            //     return;
            // }

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
                    // Debug.Log($"Get {getValue}(error={getValueError}) from parentValue={parentValue}/{property.propertyPath}");
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

                if (value == null)
                {
                    // https://github.com/TylerTemp/SaintsField/issues/200
                    // Unity will re-render this whole, for no reason...
                    var errorHelpBox = new HelpBox($"Failed to get value from {property.propertyPath}",
                        HelpBoxMessageType.Error);
                    root.Add(errorHelpBox);
                    try
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parentValue,
                            Activator.CreateInstance(info is PropertyInfo pi
                                ? pi.PropertyType
                                : ((FieldInfo)info).FieldType));
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    errorHelpBox.schedule.Execute(() =>
                    {
                        try
                        {
                            property.serializedObject.Update();
                            object parentValueAgain = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                            (string __, int _, object getValue) = Util.GetValue(property, info, parentValueAgain);
                            if (getValue != null)
                            {
                                root.Clear();
                                FillElement(root, label, property, info, inHorizontalLayout, makeRenderer,
                                    doTweenPlayRecorder,
                                    parent, richTextTagProvider);
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
#if SAINTSFIELD_DEBUG
                        Debug.Log("Patched up #200");
#endif
                    }).StartingIn(150);
                    return;
                }

                // Debug.Assert(value != null);
            }

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            // Debug.Log(parent);
            int propIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            IReadOnlyList<ISaintsRenderer> renderer =
                SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, makeRenderer, parent, info, propIndex, new []{value});

            VisualElement bodyElement = new VisualElement();

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

            // VisualElement actualContainer = root.Q<VisualElement>(NameActualContainer(property));

            root.Add(bodyElement);
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(doTweenPlayRecorder));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(doTweenPlayRecorder));
#endif
        }
    }
}
#endif
