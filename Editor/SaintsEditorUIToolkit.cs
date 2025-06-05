#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.ComponentHeader;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {

        [Obsolete("No longer needed")]
        protected virtual bool TryFixUIToolkit => false;

        private bool _searchableShown;

        private string SearchableGetLabelXml() =>
            _searchableShown ? "<icon=search.png/>" : "<color=gray><icon=search.png/>";

        private void SearchableToggle()
        {
            // _searchableShown = true;
            _searchableShown = !_searchableShown;
            _toolbarSearchField.style.display = _searchableShown ? DisplayStyle.Flex : DisplayStyle.None;

        }

        private ToolbarSearchField _toolbarSearchField;

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

            foreach (ISaintsRenderer saintsRenderer in GetClassStructRenderer(objectType, playaClassAttributes, serializedObject, target))
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

            if (playaClassAttributes.Any(each => each is SearchableAttribute))
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

                DrawHeaderGUI.AddAttributeIfNot(
                    new HeaderGhostButtonAttribute("$" + nameof(SearchableGetLabelXml)),
                    typeof(SaintsEditor).GetMethod(nameof(SearchableToggle), BindingFlags.NonPublic | BindingFlags.Instance),
                    this,
                    -100
                );
            }

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            IReadOnlyList<ISaintsRenderer> renderers = Setup(Array.Empty<string>(), serializedObject, this, target);

            // Debug.Log($"renderers.Count={renderers.Count}");
            foreach (ISaintsRenderer saintsRenderer in renderers)
            {
                // Debug.Log($"renderer={saintsRenderer}");
                VisualElement ve = saintsRenderer.CreateVisualElement();
                if(ve != null)
                {
                    root.Add(ve);
                }
            }

            // root.Add(CreateVisualElement(renderers));

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            root.RegisterCallback<AttachToPanelEvent>(_ => AddInstance(this));
            root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance(this));
#endif

            root.schedule.Execute(DrawHeaderGUI.HelperUpdate).Every(1);

            return root;
        }
    }
}
#endif
