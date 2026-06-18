using System;
using System.Collections.Generic;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor
{
    public partial class SaintsEditorCore
    {
        private UnityEngine.Object Target => _editor.target;

        private ToolbarSearchField _toolbarSearchField;
        private IReadOnlyList<ISaintsRenderer> _hasElementRenderersUIToolkit = Array.Empty<ISaintsRenderer>();
        public IReadOnlyList<ISaintsRenderer> AllRenderersUIToolkit { get; private set; } = Array.Empty<ISaintsRenderer>();
#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateInspectorGUI()
        {
            if (!Target)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            Type objectType = Target.GetType();
            IPlayaClassAttribute[] playaClassAttributes = ReflectCache.GetCustomAttributes<IPlayaClassAttribute>(objectType);

            // foreach (ISaintsRenderer saintsRenderer in GetClassStructRenderer(objectType, playaClassAttributes, serializedObject, targets))
            // {
            //     VisualElement ve = saintsRenderer.CreateVisualElement();
            //     if(ve != null)
            //     {
            //         root.Add(ve);
            //     }
            // }

            MonoScript monoScript = SaintsEditor.GetMonoScript(Target);
            if(monoScript && _editorShowMonoScript)
            {
                ObjectField objectField = new ObjectField("Script")
                {
                    bindingPath = "m_Script",
                    value = monoScript,
                    allowSceneObjects = false,
                    objectType = typeof(MonoScript),
                };
                objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                objectField.Bind(SerializedObject);
                objectField.SetEnabled(false);
                objectField.AddManipulator(new ContextualMenuManipulator(evt =>
                        evt.menu.AppendAction("Edit Script", _ => AssetDatabase.OpenAsset(monoScript))
                    ));

                root.Add(objectField);
            }

            SearchableAttribute searchableAttribute = null;
            if (SaintsFieldConfigUtil.GetMonoBehaviorSearchable())
            {
                searchableAttribute = new SearchableAttribute();
            }
            else
            {
                foreach (IPlayaClassAttribute playaClassAttribute in playaClassAttributes)
                {
                    if (playaClassAttribute is SearchableAttribute sa)
                    {
                        searchableAttribute = sa;
                        break;
                    }
                }
            }

            if (searchableAttribute != null && _editor is ISearchable iSearchable)
            {
                _toolbarSearchField = new ToolbarSearchField
                {
                    style =
                    {
                        // flexGrow = 1,
                        display = DisplayStyle.None,
                        // width = Length.Percent(100),
                        width = StyleKeyword.None,
                    },
#if UNITY_6000_0_OR_NEWER
                    placeholderText = "Search Field Name",
#endif
                };
                root.Add(_toolbarSearchField);

                _toolbarSearchField.RegisterValueChangedCallback(evt =>
                {
                    string searchRaw = evt.newValue;
                    string searchUse = searchRaw.Contains(' ') ? searchRaw : ObjectNames.NicifyVariableName(searchRaw);
                    OnSearchUIToolkit(searchUse);
                });
                DrawHeaderGUI.SaintsEditorEnqueueSearchable(iSearchable);
            }

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            AllRenderersUIToolkit = SaintsEditor.Setup(Array.Empty<string>(), SerializedObject, GetMakeRender(), Targets);

            // Debug.Log($"renderers.Count={renderers.Count}");
            List<ISaintsRenderer> usedRenderers = new List<ISaintsRenderer>();
            foreach (ISaintsRenderer saintsRenderer in AllRenderersUIToolkit)
            {
                // Debug.Log($"renderer={saintsRenderer}");
                VisualElement ve = saintsRenderer.CreateVisualElement(root);
                if(ve != null)
                {
                    usedRenderers.Add(saintsRenderer);
                    root.Add(ve);
                }
            }

            _hasElementRenderersUIToolkit = usedRenderers;

            // root.Add(CreateVisualElement(renderers));

            root.schedule.Execute(DrawHeaderGUI.HelperUpdate).Every(1);

            // ReSharper disable once InvertIf
            if (_toolbarSearchField != null)
            {
                root.focusable = true;
                root.RegisterCallback<KeyUpEvent>(evt =>
                {
                    if(evt.keyCode == KeyCode.F && evt.actionKey)
                    {
                        OnHeaderButtonClick();
                    }
                }, TrickleDown.TrickleDown);
            }

            return root;
        }
#endif
        private void OnSearchUIToolkit(string search)
        {
            foreach (ISaintsRenderer saintsRenderer in _hasElementRenderersUIToolkit)
            {
                saintsRenderer.OnSearchField(search);
            }
        }

        private bool _searchableShown;

        public void OnHeaderButtonClick()
        {
            _searchableShown = !_searchableShown;
            OnHeaderButtonClickUIToolkit();

            if (!_searchableShown)
            {
                ResetSearchUIToolkit();
            }
        }

        private void OnHeaderButtonClickUIToolkit()
        {
            _toolbarSearchField.style.display = _searchableShown ? DisplayStyle.Flex : DisplayStyle.None;
            if(_searchableShown)
            {
                _toolbarSearchField.Focus();
            }
        }

        public void ResetSearchUIToolkit()
        {
            // ReSharper disable once InvertIf
            if (_toolbarSearchField.parent != null)
            {
                _toolbarSearchField.parent.Focus();
                _toolbarSearchField.value = "";
            }
        }
    }
}
