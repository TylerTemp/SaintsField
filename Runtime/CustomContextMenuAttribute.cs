using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class CustomContextMenuAttribute: Attribute, IPlayaAttribute
    {
        public readonly string FuncName;
        public readonly string MenuName;
        public readonly bool MenuNameIsCallback;

        public CustomContextMenuAttribute(string funcName = null, string menuName = null)
        {
            FuncName = RuntimeUtil.ParseCallback(funcName).content;
            (MenuName, MenuNameIsCallback) = RuntimeUtil.ParseCallback(menuName);
        }
    }
}
