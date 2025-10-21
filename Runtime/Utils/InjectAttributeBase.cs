using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    public class InjectAttributeBase: PropertyAttribute
    {
        public readonly Type Decorator;
        public readonly object[] Parameters;

        public InjectAttributeBase(Type decorator, params object[] parameters)
        {
            Decorator = decorator;
            Parameters = parameters;
        }
    }
}
