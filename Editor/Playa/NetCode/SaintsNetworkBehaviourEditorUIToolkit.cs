using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
namespace SaintsField.Editor.Playa.NetCode
{
    public partial class SaintsNetworkBehaviourEditor
    {

        public override VisualElement CreateInspectorGUI()
        {
            // Debug.Log("CreateInspectorGUI");

            if (target == null)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                using(new ImGuiFoldoutStyleRichTextScoop())
                using(new ImGuiLabelStyleRichTextScoop())
                {
                    serializedObject.Update();
                    // IReadOnlyList<string> netCodeFields = GetNetCodeFields();
                    // if (netCodeFields.Count == 0)
                    // {
                    //     return;
                    // }

                    // float height = SaintsPropertyDrawer.SingleLineHeight * netCodeFields.Count;
                    //
                    // EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));

                    RenderNetCodeIMGUI();

                    serializedObject.ApplyModifiedProperties();
                }
            })
            {
                // style =
                // {
                //     flexGrow = 1,
                //     flexShrink = 0,
                // },
            };

            root.Add(imguiContainer);

            MonoScript monoScript = SaintsEditor.GetMonoScript(target);
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
                // if(!EditorShowMonoScript)
                // {
                //     objectField.style.display = DisplayStyle.None;
                // }
                root.Add(objectField);
            }

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            string[] netCodeFields = GetNetCodeVariableFields().Values
                .Where(each => each != null)
                .Select(each => each.Name)
                .ToArray();
            // Debug.Log($"{string.Join(",", fields)}");

            _renderers = SaintsEditor.Setup(netCodeFields, serializedObject, this, targets);

            // Debug.Log($"renderers.Count={renderers.Count}");
            foreach (ISaintsRenderer saintsRenderer in _renderers)
            {
                // Debug.Log($"renderer={saintsRenderer}");
                VisualElement ve = saintsRenderer.CreateVisualElement();
                if(ve != null)
                {
                    root.Add(ve);
                }
            }

            return root;
        }

    }
}
#endif
