using UnityEngine;

namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorExample: MonoBehaviour
    {
        [Space(50)]

        [Separator("Start")]
        [Separator("Center", EAlign.Center)]
        [Separator("End", EAlign.End)]
        [BelowSeparator("$" + nameof(Callback))]
        public string s3;
        public string Callback() => s3;

        [Space(50)]

        [Separator]
        public string s1;

        [Separator(10)]  // this behaves like a space
        [Separator("[ Hi <color=LightBlue>Above</color> ]", EColor.Aqua, EAlign.Center)]
        [BelowSeparator("[ Hi <color=Silver>Below</color> ]", EColor.Brown, EAlign.Center)]
        [BelowSeparator(10)]
        public string hi;

        [BelowSeparator]
        public string s2;
    }
}
