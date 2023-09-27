using UnityEngine;

namespace ExtInspector
{
    public class SuffixButtonAttribute: UnityEngine.PropertyAttribute
    {
        public readonly string Callback;
        public readonly string Content;
        public readonly string Icon;

        public SuffixButtonAttribute(string callback, string content = null, string icon = null)
        {
            if (icon is null)
            {
                Debug.Assert(content != "", $"Either set an icon or non-empty string content for {nameof(SuffixButtonAttribute)}");
            }
            Callback = callback;
            Content = content;
            Icon = icon;
        }
    }
}
