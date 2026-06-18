using System.Collections.Generic;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public partial class SaintsEditorCore : IDOTweenPlayRecorder, IMakeRenderer
    {
        private readonly UnityEditor.Editor _editor;

        private bool _saintsEditorIMGUI;
        private readonly bool _editorShowMonoScript;
        private readonly IMakeRenderer _makeRenderer;

        private SerializedObject SerializedObject => _editor.serializedObject;
        private UnityEngine.Object[] Targets => _editor.targets;
        private IMakeRenderer GetMakeRender() => _makeRenderer ?? this;

        public SaintsEditorCore(UnityEditor.Editor editor, bool editorShowMonoScript, IMakeRenderer makeRenderer=null)
        {
            _editor = editor;
            _editorShowMonoScript = editorShowMonoScript;
            _makeRenderer = makeRenderer;
        }

        public virtual IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(so, fieldWithInfo);
        }
    }
}
