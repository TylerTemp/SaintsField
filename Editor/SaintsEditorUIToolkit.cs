#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {

        [Obsolete("No longer needed")]
        protected virtual bool TryFixUIToolkit => false;

        private void OnHeaderButtonClickUIToolkit()
        {
            _toolbarSearchField.style.display = _searchableShown ? DisplayStyle.Flex : DisplayStyle.None;
            if(_searchableShown)
            {
                _toolbarSearchField.Focus();
            }
        }

        private ToolbarSearchField _toolbarSearchField;

        private IReadOnlyList<ISaintsRenderer> _hasElementRenderersUIToolkit = Array.Empty<ISaintsRenderer>();
        private IReadOnlyList<ISaintsRenderer> _allRenderersUIToolkit = Array.Empty<ISaintsRenderer>();

        public override VisualElement CreateInspectorGUI()
        {
            _saintsEditorIMGUI = false;
            // Debug.Log("CreateInspectorGUI");

            if (!target)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            Type objectType = target.GetType();
            IPlayaClassAttribute[] playaClassAttributes = ReflectCache.GetCustomAttributes<IPlayaClassAttribute>(objectType);

            foreach (ISaintsRenderer saintsRenderer in GetClassStructRenderer(objectType, playaClassAttributes, serializedObject, targets))
            {
                VisualElement ve = saintsRenderer.CreateVisualElement();
                if(ve != null)
                {
                    root.Add(ve);
                }
            }

            MonoScript monoScript = GetMonoScript(target);
            if(monoScript)
            {
                ObjectField objectField = new ObjectField("Script")
                {
                    bindingPath = "m_Script",
                    value = monoScript,
                    allowSceneObjects = false,
                    objectType = typeof(MonoScript),
                };
                objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                objectField.Bind(serializedObject);
                objectField.SetEnabled(false);
                if(!EditorShowMonoScript)
                {
                    objectField.style.display = DisplayStyle.None;
                }
                root.Add(objectField);
            }

            SearchableAttribute searchableAttribute = null;
            foreach (IPlayaClassAttribute playaClassAttribute in playaClassAttributes)
            {
                if (playaClassAttribute is SearchableAttribute sa)
                {
                    searchableAttribute = sa;
                    break;
                }
            }

            if (searchableAttribute != null)
            {
                _toolbarSearchField = new ToolbarSearchField
                {
                    style =
                    {
                        // flexGrow = 1,
                        display = DisplayStyle.None,
                        width = Length.Percent(100),
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
                    OnSearch(searchUse);
                });
                DrawHeaderGUI.SaintsEditorEnqueueSearchable(this);
            }

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            _allRenderersUIToolkit = Setup(Array.Empty<string>(), serializedObject, this, targets);

            // Debug.Log($"renderers.Count={renderers.Count}");
            List<ISaintsRenderer> usedRenderers = new List<ISaintsRenderer>();
            foreach (ISaintsRenderer saintsRenderer in _allRenderersUIToolkit)
            {
                // Debug.Log($"renderer={saintsRenderer}");
                VisualElement ve = saintsRenderer.CreateVisualElement();
                if(ve != null)
                {
                    usedRenderers.Add(saintsRenderer);
                    root.Add(ve);
                }
            }

            _hasElementRenderersUIToolkit = usedRenderers;

            // root.Add(CreateVisualElement(renderers));

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            root.RegisterCallback<AttachToPanelEvent>(_ => AddInstance(this));
            root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance(this));
#endif

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

        private void OnDestroyUIToolkit()
        {
            foreach (ISaintsRenderer saintsRenderer in _allRenderersUIToolkit)
            {
                saintsRenderer.OnDestroy();
            }
        }

        private void OnSearchUIToolkit(string search)
        {
            foreach (ISaintsRenderer saintsRenderer in _hasElementRenderersUIToolkit)
            {
                saintsRenderer.OnSearchField(search);
            }
        }

        private void ResetSearchUIToolkit()
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
#endif
