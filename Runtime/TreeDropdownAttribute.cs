using System;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class TreeDropdownAttribute: PathedDropdownAttribute
    {
        public TreeDropdownAttribute(string funcName = null, EUnique unique = EUnique.None): base(funcName, unique)
        {
        }

        public TreeDropdownAttribute(EUnique unique) : base(unique) {}
    }
}
