using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class GetComponentByPathAttribute: GetByXPathAttribute
    {

        public GetComponentByPathAttribute(string path, params string[] paths)
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentByPathExp(EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker));
            ParseXPaths(paths.Prepend(path).Select(TranslatePath).ToArray());
        }

        public GetComponentByPathAttribute(EXP config, string path, params string[] paths)
        {
            ParseOptions(config);
            ParseXPaths(paths.Prepend(path).Select(TranslatePath).ToArray());
        }

        public GetComponentByPathAttribute(EGetComp config, string path, params string[] paths): this(TranslateConfig(config), path, paths)
        {
        }

        private static string TranslatePath(string path)
        {
            if (path.StartsWith("///"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return $"scene:://{path.Substring(3)}";
            }

            if (path.StartsWith("//"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return $"scene::/{path.Substring(2)}";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (path.StartsWith("/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return $"scene::/{path.Substring(1)}";
            }

            return path;
        }

        private static EXP TranslateConfig(EGetComp config)
        {
            EXP exp = EXP.NoPicker;
            if (config.HasFlag(EGetComp.ForceResign))
            {
                // do nothing
            }
            else
            {
                exp |= EXP.NoAutoResignToValue;
                exp |= EXP.NoAutoResignToNull;
            }

            if (config.HasFlag(EGetComp.NoResignButton))
            {
                exp |= EXP.NoResignButton;
            }

            return exp;
        }
    }
}
