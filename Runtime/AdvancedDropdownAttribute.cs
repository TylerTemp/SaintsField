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
    public class AdvancedDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        public const float DefaultTitleHeight = 45f;
        private const float DefaultSepHeight = 4f;

        public const float TitleHeight = DefaultTitleHeight;
        public const float ItemHeight = -1f;
        public const float SepHeight = DefaultSepHeight;
        public const float MinHeight = -1f;
        public const bool UseTotalItemCount = false;
        public EUnique EUnique;

        public enum Mode
        {
            Default,
            Options,
            Tuples,
        }

        public virtual Mode BehaveMode => Mode.Default;

        public IReadOnlyList<object> Options;
        public IReadOnlyList<(string path, object value)> Tuples;

        // public AdvancedDropdownAttribute(string funcName)
        // {
        //     FuncName = funcName;
        // }

        // [Obsolete]
        // public AdvancedDropdownAttribute(string funcName, float itemHeight=-1f, float titleHeight=DefaultTitleHeight, float sepHeight=DefaultSepHeight, bool useTotalItemCount=false, float minHeight=-1f)
        // {
        //     FuncName = funcName;
        //     ItemHeight = itemHeight;
        //     TitleHeight = titleHeight;
        //     SepHeight = sepHeight;
        //     UseTotalItemCount = useTotalItemCount;
        //     MinHeight = minHeight;
        // }

        public AdvancedDropdownAttribute(string funcName = null, EUnique unique = EUnique.None)
        {
            FuncName = RuntimeUtil.ParseCallback(funcName).content;
            EUnique = unique;
        }

        public AdvancedDropdownAttribute(EUnique unique) : this(null, unique) {}
    }
}
