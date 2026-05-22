using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class MaxValueAttribute: MinValueAttribute
    {
        public MaxValueAttribute(
            object position0=null, object position1=null, object position2=null, object position3=null,
            object position4=null, object position5=null,
            string groupBy = "") : base(
                position0, position1, position2, position3, position4, position5, groupBy
            )
        {
            // Debug.Log($"0{position0}-1{position1}-2{position2}-3{position3}-4{position4}-5{position5}");
        }
    }
}
