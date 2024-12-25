using SaintsField.Editor.Playa.Renderer;
using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public interface IMakeRenderer
    {
        AbsRenderer MakeRenderer(SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo);
    }
}
