using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class MaxValueAttribute: MinValueAttribute
    {
        public MaxValueAttribute(object position0=null, object position1=null, object position2=null, object position3=null, string groupBy = "") : base(
                position0, position1, position2, position3, groupBy
            )
        {
        }
    }
}
