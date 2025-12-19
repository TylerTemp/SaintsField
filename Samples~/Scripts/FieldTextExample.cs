using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FieldTextExample : MonoBehaviour
    {
        [FieldAboveText("<color=DodgerBlue>┌<field/></color> at [<index/>]")]
        [FieldBelowText("$" + nameof(GetAboveText))]
        public string[] lis;

        private string GetAboveText(string value, int index)
        {
            bool isEven = index % 2 == 0;
            return isEven ? $"<color={EColor.SoftRed}>└Event" : $"<color={EColor.HotPink}>└Odd";
        }

        [FieldAboveText("Can also be used on single fields: <label/>(raw value)=<field/>")]
        [FieldBelowText("$" + nameof(singleField))]  // callback, as parsed value
        public string singleField;
    }
}
