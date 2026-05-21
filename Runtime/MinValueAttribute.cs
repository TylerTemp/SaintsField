using System;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class MinValueAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly IReadOnlyList<NumberLimitParam> Positions;
        // public readonly NumberLimitParam Position2;
        // public readonly NumberLimitParam Position3;
        // public readonly NumberLimitParam Position4;

        // public readonly float Value;
        // public readonly string ValueCallback;

        public MinValueAttribute(object position0=null, object position1=null, object position2=null, object position3=null, string groupBy = "")
        {
            NumberLimitParam pos0 = new NumberLimitParam(position0);
            Debug.Assert(pos0.SourceType != SourceType.NotSupported, position0);

            NumberLimitParam pos1 = new NumberLimitParam(position1);
            Debug.Assert(pos1.SourceType != SourceType.NotSupported, position1);

            NumberLimitParam pos2 = new NumberLimitParam(position2);
            Debug.Assert(pos2.SourceType != SourceType.NotSupported, position2);

            NumberLimitParam pos3 = new NumberLimitParam(position3);
            Debug.Assert(pos3.SourceType != SourceType.NotSupported, position3);

            Positions = new[]
            {
                pos0,
                pos1,
                pos2,
                pos3,
            };

            GroupBy = groupBy;
        }

        // public MinValueAttribute(string valueCallback, string groupBy = "")
        // {
        //     Value = -1;
        //     ValueCallback = RuntimeUtil.ParseCallback(valueCallback).content;
        //
        //     GroupBy = groupBy;
        // }
    }
}
