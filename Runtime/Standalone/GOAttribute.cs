using System;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GOAttribute: PropertyAttribute
    {
        public readonly Type requiredComp;

        public GOAttribute()
        {
            requiredComp = null;
        }

        public GOAttribute(Type requiredComponent)
        {
            requiredComp = requiredComponent;
        }

    }
}
