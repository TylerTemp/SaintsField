using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class MaxValueAttribute: MinValueAttribute
    {
        public MaxValueAttribute(float value) : base(value)
        {
        }

        public MaxValueAttribute(string valueCallback) : base(valueCallback)
        {
        }
    }
}
