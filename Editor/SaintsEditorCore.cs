using System.Collections.Generic;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public partial class SaintsEditorCore : IDOTweenPlayRecorder, IMakeRenderer
    {
        public virtual IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(so, fieldWithInfo);
        }
    }
}
