using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue286
{
    [System.Serializable]
    public class FebucciUIStyles
    {
        public string styleTag;

        [TextArea] public string openingTag;
        [TextArea] public string closingTag;

        public FebucciUIStyles(string styleTag, string openingTag, string closingTag)
        {
            this.styleTag = styleTag;
            this.openingTag = openingTag;
            this.closingTag = closingTag;
        }
    }
}
