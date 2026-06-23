#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.NetCode
{
    public partial class SaintsNetworkBehaviourEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            _saintsEditorIMGUI = false;
            // Debug.Log("CreateInspectorGUI");

            if (target == null)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                serializedObject.Update();
                using(new ImGuiFoldoutStyleRichTextScoop())
                using(new ImGuiLabelStyleRichTextScoop())
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    RenderNetCodeIMGUI();
                    if(changed.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            })
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            root.Add(imguiContainer);

            // string[] netCodeFields = GetNetCodeVariableFields().Values
            //     .Where(each => each != null)
            //     .Select(each => each.Name)
            //     .ToArray();
            // Debug.Log($"{string.Join(",", fields)}");
            _coreEditor = new SaintsEditorCore(this, true, this);
            root.Add(_coreEditor.CreateInspectorGUI());

            return root;
        }
    }
}
#endif
