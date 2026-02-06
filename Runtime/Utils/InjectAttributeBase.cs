using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    public abstract class InjectAttributeBase: PropertyAttribute
    {
        public readonly Type Decorator;
        public readonly object[] Parameters;
        public readonly int Depth;

        protected InjectAttributeBase(int depth, Type decorator, params object[] parameters)
        {
            Depth = depth;
            Decorator = decorator;
            Parameters = parameters;
        }
    }
}
