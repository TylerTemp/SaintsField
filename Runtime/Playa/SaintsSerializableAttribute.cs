using UnityEngine;

namespace SaintsField.Playa
{
    public class SaintsEditorAttribute: PropertyAttribute
    {
        public readonly bool Inline;

        public SaintsEditorAttribute(bool inline=false)
        {
            Inline = inline;
        }
    }
}
