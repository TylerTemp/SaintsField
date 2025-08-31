using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.Utils;

namespace SaintsField.Wwise
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class GetWwiseAttribute: GetByXPathAttribute
    {
        public GetWwiseAttribute(params string[] resourcePath)
        {
            ParseOptions(EXP.NoAutoResignToNull | EXP.NoPicker);
            ParseXPaths(GetPath(resourcePath));
        }

        public GetWwiseAttribute(EXP config, params string[] resourcePath)
        {
            ParseOptions(config);
            ParseXPaths(GetPath(resourcePath));
        }

        private static string[] GetPath(string[] paths)
        {
            return paths.Length == 0
                ? new[] { "//*" }
                : paths.Select(TrasformPath).ToArray();
        }

        private static string TrasformPath(string path)
        {
            if (RuntimeUtil.ParseCallback(path).isCallback)
            {
                return path;
            }
            // return path.Contains('/')? path: $"//{path}";
            return path.StartsWith('/')? path: $"//{path}";
        }
    }
}
