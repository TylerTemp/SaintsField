using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Interfaces;
using SaintsField.SaintsXPathParser;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Wwise
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class GetBankAttribute: GetByXPathAttribute
    {
        public GetBankAttribute(params string[] bank)
        {
            ParseOptions(EXP.NoAutoResignToNull | EXP.NoPicker);
            ParseXPaths(bank.Select(TrasformPath).ToArray());
        }

        public GetBankAttribute(EXP config, params string[] bank)
        {
            ParseOptions(config);
            ParseXPaths(bank.Select(TrasformPath).ToArray());
        }

        private static string TrasformPath(string path)
        {
            return path.Contains('/')? path: $"//{path}";
        }
    }
}
