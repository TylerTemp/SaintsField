using System;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    public class KeyAttributeAttribute: InjectAttributeBase
    {
        public KeyAttributeAttribute(Type decorator, params object[] parameters) : base(decorator, parameters)
        {
        }
    }
}
