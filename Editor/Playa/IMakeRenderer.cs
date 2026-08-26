using System.Collections.Generic;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public readonly struct SaintsFieldWithRenderer
    {
        public readonly IPlayaAttribute Playa;
        public readonly AbsRenderer Renderer;

        public SaintsFieldWithRenderer(IPlayaAttribute playa, AbsRenderer renderer)
        {
            Playa = playa;
            Renderer = renderer;
        }

        public override string ToString()
        {
            return $"{Renderer}:{Playa}";
        }
    }

    public interface IMakeRenderer
    {
        IEnumerable<IReadOnlyList<SaintsFieldWithRenderer>> MakeRenderer(SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo);
    }
}
