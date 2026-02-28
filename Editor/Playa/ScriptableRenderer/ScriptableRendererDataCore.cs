using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.ScriptableRenderer;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
    public class ScriptableRendererDataCore
    {
        private HelpBox _noFeaturesBox;
        private SerializedProperty _mRendererFeatures;
        private List<UnityEditor.Editor> _editors;
        private ListView _listView;

        private readonly ScriptableRendererDataEditor _editor;
        private readonly SerializedObject _serializedObject;
        private readonly UnityEngine.Object _target;

        public ScriptableRendererDataCore(ScriptableRendererDataEditor editor)
        {
            _editor = editor;
            _serializedObject = editor.serializedObject;
            _target = editor.target;
        }

        public VisualElement CreateInspectorGUI()
        {
            // if (m_RendererFeatures == null)
            //     OnEnable();
            // else if (m_RendererFeatures.arraySize != m_Editors.Count)
            //     UpdateEditorList();
            Type type = typeof(ScriptableRendererDataEditor);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            // private SerializedProperty m_RendererFeatures
            FieldInfo rendererFeaturesField =
                type.GetField("m_RendererFeatures", flags);

            // List<Editor> m_Editors
            FieldInfo editorsField =
                type.GetField("m_Editors", flags);

            // private void OnEnable()
            MethodInfo onEnableMethod =
                type.GetMethod("OnEnable", flags);

            // private void UpdateEditorList()
            MethodInfo updateEditorListMethod =
                type.GetMethod("UpdateEditorList", flags);

            if (rendererFeaturesField == null ||
                editorsField == null ||
                onEnableMethod == null ||
                updateEditorListMethod == null)
            {
                return new HelpBox("Unity update its internal calls. Please report this issue.", HelpBoxMessageType.Error);
            }
            _mRendererFeatures =
                rendererFeaturesField.GetValue(_editor) as SerializedProperty;
            // Debug.Log(_mRendererFeatures);
            // Debug.Log(_mRendererFeatures.arraySize);

            if (_mRendererFeatures == null)
            {
                onEnableMethod.Invoke(_editor, null);
                _mRendererFeatures =
                    rendererFeaturesField.GetValue(_editor) as SerializedProperty;
            }
            if (_mRendererFeatures == null)
            {
                return new HelpBox("Unity upgrade its internal calls. Please report this issue.", HelpBoxMessageType.Error);
            }

            if (editorsField.GetValue(_editor) is not List<UnityEditor.Editor> outEditors)
            {
                return new HelpBox("Unity upgrade its internal calls. Please report this issue.", HelpBoxMessageType.Error);
            }

            _editors = outEditors;

            if (_mRendererFeatures.arraySize != _editors.Count)
            {
                updateEditorListMethod.Invoke(_target, null);
            }

            _serializedObject.Update();

            VisualElement root = new VisualElement();

            // var renderFeatures = ScriptableRendererDataEditor.Styles.RenderFeatures;
            Label mainTitle = new Label("Renderer Features")
            {
                tooltip = "A Renderer Feature is an asset that lets you add extra Render passes to a URP Renderer and configure their behavior.",
                style =
                {
                    fontSize = 13,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 5,
                    paddingBottom = 8,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(32/255f, 32/255f, 32/255f, 1),
                },
            };
            root.Add(mainTitle);

            _noFeaturesBox = new HelpBox("No Renderer Features added", HelpBoxMessageType.Info)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            root.Add(_noFeaturesBox);

            root.Add(_listView = MakeList());
            Button addBtn = new Button
            {
                text = "Add Renderer Feature",
                style =
                {
                    marginTop = 15,
                    height = 25,
                },
            };
            root.Add(addBtn);

            // static readonly Assembly UrpEditorAssembly = typeof(UniversalRenderPipelineEditor).Assembly;
            // Type providerType = typeof(ScriptableRendererDataEditor).Assembly
            //     .GetType("UnityEditor.Rendering.Universal.ScriptableRendererFeatureProvider");
            // Type t = typeof(ScriptableRendererFeatureProvider);
            // Debug.Log(providerType);
            // ConstructorInfo ctor =
            //     providerType?.GetConstructor(
            //         BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            //         null,
            //         new[] { typeof(ScriptableRendererDataEditor) },
            //         null
            //     );
            // Debug.Log(ctor);

            addBtn.clicked += () =>
            {
                // Debug.Log(_editor.target.GetType());
                // Debug.Log("==============");
                TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<ScriptableRendererFeature>();
                // var data = _editor.target as ScriptableRendererData;
                AdvancedDropdownList<Type> dropdown = new AdvancedDropdownList<Type>();
                foreach (Type t in types)
                {
                    if (t.IsAbstract)
                    {
                        continue;
                    }
                    // Debug.Log(t);
                    // Debug.Log(RendererFeatureSupported(t));
                    if (!RendererFeatureSupported(t))
                    {
                        continue;
                    }
                    if (DuplicateFeatureCheck(t))
                    {
                        continue;
                    }

                    string path = GetMenuNameFromType(t);
                    // _editor.AddComponent(t);
                    // Debug.Log(path);
                    dropdown.Add(path, t);
                }


                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    CurDisplay = "",
                    CurValues = Array.Empty<object>(),
                    DropdownListValue = dropdown,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(addBtn.worldBound);

                SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    addBtn.worldBound.width,
                    maxHeight,
                    false,
                    (curItem, _) =>
                    {
                        // _editor.AddComponent((Type)curItem);
                        AddComponentMethod.Invoke(_editor, new object[] { (Type)curItem });
                        return null;
                    }
                );

                // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
                // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
                // editorWindow.Show();

                UnityEditor.PopupWindow.Show(worldBound, sa);
                // Rect r = addBtn.worldBound;
                // Vector2 pos = new Vector2(r.x + r.width / 2f, r.yMax + 18f);
                // if (ctor == null)
                // {
                //     throw new MissingMemberException(
                //         "ScriptableRendererFeatureProvider ctor not found (URP API changed). Please report this issue to SaintsField."
                //     );
                // }
                // FilterWindow.Show(pos, (FilterWindow.IProvider)ctor.Invoke(new object[] { _editor }));
            };

            root.schedule.Execute(OnUpdateUIToolkit).Every(150);
            OnUpdateUIToolkit();

            root.Bind(_serializedObject);

            return root;
        }

        #region FilterWindow.IProvider

        private static readonly MethodInfo AddComponentMethod =
            typeof(ScriptableRendererDataEditor).GetMethod(
                "AddComponent",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(Type) },
                null
            );


        private bool RendererFeatureSupported(Type rendererFeatureType)
        {
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL_17_1_0_OR_NEWER
            // UnityEngine.Rendering.Universal.DecalRendererFeature
            Type urd = typeof(UniversalRendererData);
            Type rendererType = _editor.target.GetType();

            SupportedOnRendererAttribute rendererFilterAttribute = Attribute.GetCustomAttribute(rendererFeatureType, typeof(SupportedOnRendererAttribute)) as SupportedOnRendererAttribute;
            // ReSharper disable once InvertIf
            if (rendererFilterAttribute != null)
            {
                bool foundEditor = false;
                for (int i = 0; i < rendererFilterAttribute.rendererTypes.Length && !foundEditor; i++)
                {
                    // Debug.Log($"{rendererFilterAttribute.rendererTypes[i]}/{rendererType}");
                    foundEditor = rendererFilterAttribute.rendererTypes[i] == rendererType;
                    // ReSharper disable once InvertIf
                    if (!foundEditor)
                    {
                        // ReSharper disable once InvertIf
                        if (rendererFilterAttribute.rendererTypes[i] == urd)
                        {
                            // If it's used on UniversalRendererData, then it should be allowed to used on SaintsUniversalRendererData's direct children
                            // sub children is not allowed because SupportedOnRendererAttribute itself does not work on child object
                            if (_editor.target is SaintsUniversalRendererData && _editor.target.GetType().BaseType == typeof(SaintsUniversalRendererData))
                            {
                                return true;
                            }
                        }
                    }
                }

                return foundEditor;
            }
#endif
            return true;

        }

        private static readonly FieldInfo RendererFeaturesField =
            typeof(ScriptableRendererData).GetField(
                "m_RendererFeatures",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

        internal bool DuplicateFeatureCheck(Type type)
        {
            ScriptableRendererData data = _editor.target as ScriptableRendererData;

            Attribute isSingleFeature = type.GetCustomAttribute(typeof(DisallowMultipleRendererFeature));
            if (isSingleFeature == null)
                return false;

            // if (data.m_RendererFeatures == null)
            //     return false;
            //
            // for (int i = 0; i < m_RendererFeatures.Count; i++)
            // {
            //     ScriptableRendererFeature feature = m_RendererFeatures[i];
            //     if (feature == null)
            //         continue;
            //
            //     if (feature.GetType() == type)
            //         return true;
            // }

            if (data == null || RendererFeaturesField == null)
                return false;

            // ReSharper disable once UseNegatedPatternMatching
            // ReSharper disable once InconsistentNaming
            List<ScriptableRendererFeature> m_RendererFeatures = RendererFeaturesField.GetValue(data) as List<ScriptableRendererFeature>;
            if (m_RendererFeatures == null)
                return false;
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < m_RendererFeatures.Count; i++)
            {
                ScriptableRendererFeature feature = m_RendererFeatures[i];
                if (feature == null)
                    continue;

                if (feature.GetType() == type)
                    return true;
            }
            return false;
        }

        private static readonly MethodInfo GetCustomTitleMethod =
            typeof(ScriptableRendererDataEditor).GetMethod(
                "GetCustomTitle",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(Type), typeof(string).MakeByRefType() },
                null
            );

        private string GetMenuNameFromType(Type type)
        {
            // string path;
            // if (!_editor.GetCustomTitle(type, out path))
            // {
            //     path = ObjectNames.NicifyVariableName(type.Name);
            // }
            string path = null;

            bool hasCustomTitle = false;

            if (_editor != null && GetCustomTitleMethod != null)
            {
                object[] args = { type, null };
                hasCustomTitle = (bool)GetCustomTitleMethod.Invoke(_editor, args);
                path = args[1] as string;
            }
            if (!hasCustomTitle)
            {
                path = ObjectNames.NicifyVariableName(type.Name);
            }

            if (type.Namespace != null)
            {
                if (type.Namespace.Contains("Experimental"))
                    path += " (Experimental)";
            }

            return path;
        }
        #endregion


        // private int _rendererFeaturesCount = 0;
        private int _curSize;

        private void OnUpdateUIToolkit()
        {
            if (!SerializedUtils.IsOk(_mRendererFeatures))
            {
                return;
            }

            int newSize;
            try
            {
                newSize = _mRendererFeatures.arraySize;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }

            if (newSize == _curSize)
            {
                return;
            }

            // Debug.Log($"_curSize={_curSize}, newSize={newSize}");
            _curSize = newSize;
            DisplayStyle display = _curSize == 0? DisplayStyle.Flex : DisplayStyle.None;
            UIToolkitUtils.SetDisplayStyle(_noFeaturesBox, display);

            using SerializedPropertyChangeEvent pooled = SerializedPropertyChangeEvent.GetPooled(_mRendererFeatures);
            pooled.target = _listView;
            _listView.SendEvent(pooled);
        }

        private ListView MakeList()
        {
            ListView listView = new ListView
            {
                // focusable = false,
                showBorder = false,
                selectionType = SelectionType.None,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,

                makeItem = () => new VisualElement(),
                bindItem = (element, index) =>
                {
                    SerializedProperty renderFeatureProperty;
                    UnityEditor.Editor editor;
                    try
                    {
                        renderFeatureProperty = _mRendererFeatures.GetArrayElementAtIndex(index);
                        editor = _editors[index];
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                        return;
                    }

                    element.Clear();
                    // UIToolkitUtils.Unbind(element);

                    element.Add(MakeElement(renderFeatureProperty, editor, index));
                    // element.TrackPropertyValue(renderFeatureProperty, _ => _listView.Rebuild());
                },
                unbindItem = (element, _) =>
                {
                    element.Clear();
                    UIToolkitUtils.Unbind(element);
                    // Debug.Log(element);
                    // Debug.Log(i);
                },
            };

            listView.RegisterCallback<AttachToPanelEvent>(_ => listView.Blur());

            // UIToolkitUtils.AddContextualMenuReset(listViewToggle, _mRendererFeatures, fieldInfo, parent);


            listView.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

            SerializedProperty serializedProperty = _mRendererFeatures.Copy();
            // string str = PropertyField.listViewNamePrefix + property.propertyPath;
            string str = "saints-field--srd-list-view--" + _mRendererFeatures.propertyPath;
            listView.userData = serializedProperty;
            listView.bindingPath = _mRendererFeatures.propertyPath;
            listView.viewDataKey = str;
            listView.name = str;

            if (listView.itemsSource?.Count != _mRendererFeatures.arraySize)
            {
                listView.itemsSource = Enumerable.Range(0, _mRendererFeatures.arraySize)
                    .Select(_mRendererFeatures.GetArrayElementAtIndex).ToArray();
            }

            listView.BindProperty(_mRendererFeatures);
            return listView;
        }

        private VisualElement MakeElement(SerializedProperty renderFeatureProperty, UnityEditor.Editor rendererFeatureEditor, int index)
        {
            VisualElement root = new VisualElement();

            SerializedObject serializedRendererFeaturesEditor = rendererFeatureEditor?.serializedObject;
            // serializedRendererFeaturesEditor.Update();

            Urp.ScriptableRendererTitleElement titleElement = new Urp.ScriptableRendererTitleElement(serializedRendererFeaturesEditor, () =>
            {
                Type type = typeof(ScriptableRendererDataEditor);

                // Walk up to the parent class if needed
                MethodInfo method = type.GetMethod(
                    "RemoveComponent",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

                if (method == null)
                {
                    throw new MissingMethodException(type.FullName, "RemoveComponent");
                }

                method.Invoke(_editor, new object[] { index });
                // _mRendererFeatures.DeleteArrayElementAtIndex(index);
            });
            root.Add(titleElement);
            titleElement.SetCustomViewData(renderFeatureProperty.propertyPath);
            if (serializedRendererFeaturesEditor == null)
            {
                titleElement.Add(new HelpBox("Missing reference, due to compilation issues or missing files. you can attempt auto fix or choose to remove the feature.", HelpBoxMessageType.Error));
                titleElement.Add(new Button(() =>
                {
                    ScriptableRendererData data = _target as ScriptableRendererData;
                    if (!ScriptableRendererDataReflection.ValidateRendererFeatures(data))
                    {
                        if (EditorUtility.DisplayDialog(
                                "Remove Missing Renderer Feature",
                                "This renderer feature script is missing (likely deleted or failed to compile). Do you want to remove it from the list and delete the associated sub-asset?",
                                "Yes", "No"))
                        {
                            ScriptableRendererDataReflection.RemoveMissingRendererFeatures(data);
                        }
                    }
                })
                {
                    text = "Attempt Fix",
                });
                return root;
            }

            SerializedProperty nameProperty = serializedRendererFeaturesEditor.FindProperty("m_Name");
            PropertyField namePropertyField = new PropertyField(nameProperty)
            {
                style =
                {
                    marginLeft = 7,
                },
            };
            titleElement.Add(namePropertyField);
            // namePropertyField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
            namePropertyField.RegisterValueChangeCallback(evt =>
            {
                if (!SerializedUtils.IsOk(renderFeatureProperty))
                {
                    return;
                }

                if (renderFeatureProperty.objectReferenceValue == null)
                {
                    return;
                }
                renderFeatureProperty.objectReferenceValue.name = evt.changedProperty.stringValue;
            });

            if(IsUIToolkit(rendererFeatureEditor))
            {
                titleElement.Add(new InspectorElement(rendererFeatureEditor)
                {
                    style =
                    {
                        marginLeft = -7,
                    },
                });
            }
            else
            {
                titleElement.Add(new IMGUIContainer(() =>
                {
                    // WTF Unity
                    using (new DisableUnityLogScoop())
                    {
                        rendererFeatureEditor.serializedObject.Update();
                    }

                    try
                    {
                        rendererFeatureEditor.OnInspectorGUI();
                    }
                    catch (NullReferenceException e)
                    {
                        // ... No words...
                        if (e.Message.Contains("SerializedProperty has been Disposed"))
                        {
                            return;
                        }

                        throw;
                    }

                    // WTF Unity
                    using (new DisableUnityLogScoop())
                    {
                        rendererFeatureEditor.serializedObject.ApplyModifiedProperties();
                    }
                }));
            }

            root.Bind(serializedRendererFeaturesEditor);

            return root;
        }

        private bool IsUIToolkit(UnityEditor.Editor rendererFeatureEditor)
        {
            var type = rendererFeatureEditor.GetType();

            var method = type.GetMethod(
                "CreateInspectorGUI",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method == null)
                return false;

            // If this method is overridden, DeclaringType will differ
            return method.DeclaringType != method.GetBaseDefinition().DeclaringType;
        }

        private static class ScriptableRendererDataReflection
        {
            private static readonly MethodInfo ValidateRendererFeaturesMethod =
                typeof(ScriptableRendererData)
                    .GetMethod(
                        "ValidateRendererFeatures",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );

            private static readonly MethodInfo RemoveMissingRendererFeaturesMethod =
                typeof(ScriptableRendererData)
                    .GetMethod(
                        "RemoveMissingRendererFeatures",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );

            public static bool ValidateRendererFeatures(object data)
            {
                return (bool)ValidateRendererFeaturesMethod.Invoke(data, null);
            }

            public static void RemoveMissingRendererFeatures(object data)
            {
                RemoveMissingRendererFeaturesMethod.Invoke(data, null);
            }
        }

    }
}
