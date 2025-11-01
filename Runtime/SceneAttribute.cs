﻿using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    /// <summary>
    /// <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Core/DrawerAttributes/SceneAttribute.cs#L6"/>
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SceneAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly bool FullPath;

        public SceneAttribute(bool fullPath = false)
        {
            FullPath = fullPath;
        }
    }
}
