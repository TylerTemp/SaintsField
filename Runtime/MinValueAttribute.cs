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

        public MinValueAttribute(object position0=null, object position1=null, object position2=null, object position3=null, object position4=null, object position5=null, string groupBy = "")
        {
            // Debug.Log($"MIN 0{position0}-1{position1}-2{position2}-3{position3}-4{position4}-5{position5}");
            NumberLimitParam pos0 = new NumberLimitParam(position0);
            Debug.Assert(pos0.SourceType != SourceType.NotSupported, position0);

            NumberLimitParam pos1 = new NumberLimitParam(position1);
            Debug.Assert(pos1.SourceType != SourceType.NotSupported, position1);

            NumberLimitParam pos2 = new NumberLimitParam(position2);
            Debug.Assert(pos2.SourceType != SourceType.NotSupported, position2);

            NumberLimitParam pos3 = new NumberLimitParam(position3);
            Debug.Assert(pos3.SourceType != SourceType.NotSupported, position3);

            NumberLimitParam pos4 = new NumberLimitParam(position4);
            Debug.Assert(pos4.SourceType != SourceType.NotSupported, position4);

            NumberLimitParam pos5 = new NumberLimitParam(position5);
            Debug.Assert(pos5.SourceType != SourceType.NotSupported, position5);
            // Debug.Log(position0);
            // Debug.Log(position1);
            // Debug.Log(position2);
            // Debug.Log(position3);
            // Debug.Log(position4);
            // Debug.Log(position5);

            Positions = new[]
            {
                pos0,
                pos1,
                pos2,
                pos3,
                pos4,
                pos5,
            };

            // Debug.Log($"pos={string.Join(", ", Positions.Select(each => each.SourceType))}");

            GroupBy = groupBy;
        }
    }
}
