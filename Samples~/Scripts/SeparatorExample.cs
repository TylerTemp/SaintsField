using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SeparatorExample: MonoBehaviour
    {
        [Separator]
        public string s1;

        [Separator(10)]
        [Separator("[ Hi <color=LightBlue>Above</color> ]", EColor.Aqua, EAlign.Center)]
        [BelowSeparator("[ Hi <color=Blue>Below</color> ]", EColor.Brown, EAlign.Center)]
        [BelowSeparator(10)]
        public string hi;

        public string s2;
    }
}
