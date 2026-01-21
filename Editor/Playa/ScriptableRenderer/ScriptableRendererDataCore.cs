using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
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
            Type providerType = typeof(ScriptableRendererDataEditor).Assembly
                .GetType("UnityEditor.Rendering.Universal.ScriptableRendererFeatureProvider");
            // Type t = typeof(ScriptableRendererFeatureProvider);
            // Debug.Log(providerType);
            ConstructorInfo ctor =
                providerType?.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new[] { typeof(ScriptableRendererDataEditor) },
                    null
                );
            // Debug.Log(ctor);

            addBtn.clicked += () =>
            {
                Rect r = addBtn.worldBound;
                Vector2 pos = new Vector2(r.x + r.width / 2f, r.yMax + 18f);
                if (ctor == null)
                {
                    throw new MissingMemberException(
                        "ScriptableRendererFeatureProvider ctor not found (URP API changed). Please report this issue to SaintsField."
                    );
                }
                FilterWindow.Show(pos, (FilterWindow.IProvider)ctor.Invoke(new object[] { _editor }));
            };

            root.schedule.Execute(OnUpdateUIToolkit).Every(150);
            OnUpdateUIToolkit();

            root.Bind(_serializedObject);

            return root;
        }


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

            ScriptableRendererTitleElement titleElement = new ScriptableRendererTitleElement(serializedRendererFeaturesEditor, () =>
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

            titleElement.Add(new InspectorElement(rendererFeatureEditor)
            {
                style =
                {
                    marginLeft = -7,
                },
            });

            root.Bind(serializedRendererFeaturesEditor);

            return root;
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
