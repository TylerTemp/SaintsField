﻿using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Addressable
{
    [Conditional("UNITY_EDITOR")]
    public class AddressableAddressAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string Group;

        public readonly string[][] LabelFilters;

        // public AddressableAddressAttribute()
        // {
        //     Group = null;
        //     LabelFilters = null;
        // }
        //
        // public AddressableAddressAttribute(string group = "")
        // {
        //     Group = group;
        //     LabelFilters = null;
        // }

        // public AddressableAddressAttribute(string group = "", string[][] labelFilters = null)
        // {
        //     Group = group;
        //     LabelFilters = labelFilters ?? Array.Empty<string[]>();
        // }

        // and
        public AddressableAddressAttribute(string group = null, params string[] orLabels)
        {
            Group = group;
            LabelFilters = orLabels?
                .Select(each => each
                    .Split(new [] { "&&" }, StringSplitOptions.None)
                    .Select(eachAnd => eachAnd.Trim())
                    .ToArray())
                .ToArray();
        }
    }
}
