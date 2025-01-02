#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {

        #region UIToolkit

        [Obsolete("No longer needed")]
        protected virtual bool TryFixUIToolkit => false;

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        public override VisualElement CreateInspectorGUI()
        {
            // Debug.Log("CreateInspectorGUI");

            if (target == null)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

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

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            IReadOnlyList<ISaintsRenderer> renderers = Setup();

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
            return root;
        }

#endif
        #endregion

    }
}
#endif
