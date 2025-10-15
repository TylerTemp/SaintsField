using UnityEngine;

namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorExample: MonoBehaviour
    {
        [Space(50)]

        [FieldSeparator("Start")]
        [FieldSeparator("Center", EAlign.Center)]
        [FieldSeparator("End", EAlign.End)]
        [FieldBelowSeparator("$" + nameof(Callback))]
        public string s3;
        public string Callback() => s3;

        [Space(50)]

        [FieldSeparator]
        public string s1;

        [FieldSeparator(10)]  // this behaves like a space
        [FieldSeparator("[ Hi <color=LightBlue>Above</color> ]", EColor.Aqua, EAlign.Center)]
        [FieldBelowSeparator("[ Hi <color=Silver>Below</color> ]", EColor.Brown, EAlign.Center)]
        [FieldBelowSeparator(10)]
        public string hi;

        [FieldBelowSeparator]
        public string s2;
    }
}
