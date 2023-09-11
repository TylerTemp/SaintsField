using System;
using System.Collections.Generic;
using ExtInspector.Utils;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: PropertyAttribute
    {
        public readonly string CallbackName;
        public RichLabelAttribute(string callbackName)
        {
            CallbackName = callbackName;
        }
    }
}
