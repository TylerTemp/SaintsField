using System;
using SaintsField.Playa;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    public abstract class InjectAttributeBase: Attribute, IPlayaAttribute
    {
        public readonly Type Decorator;
        public readonly object[] Parameters;
        public readonly int Depth;

        protected InjectAttributeBase(int depth, Type decorator, params object[] parameters)
        {
            // Debug.Log($"{depth}/{decorator}/{parameters?.Length}");
            Depth = depth;
            Decorator = decorator;
            Parameters = parameters;
        }
    }
}
