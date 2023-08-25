using UnityEngine;

namespace ExtInspector.Standalone
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AnimStateSelector : PropertyAttribute
    {
        public readonly string AnimFieldName;

        public AnimStateSelector(string animator)
        {
            AnimFieldName = animator;
        }
    }
}
