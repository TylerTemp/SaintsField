using UnityEngine;

namespace ExtInspector.Samples
{
    public class IconAttribute : PropertyAttribute
    {
        public string IconName { get; }

        public IconAttribute(string iconName)
        {
            IconName = iconName;
        }
    }
}
