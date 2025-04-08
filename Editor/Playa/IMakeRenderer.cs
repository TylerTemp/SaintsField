using System.Collections.Generic;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public interface IMakeRenderer
    {
        AbsRenderer MakeRenderer(SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo);
    }
}
