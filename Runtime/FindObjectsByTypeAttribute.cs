using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.SaintsXPathParser.Optimization;
using SaintsField.Utils;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    // public class GetComponentInSceneAttribute: GetByXPathAttribute
    public class FindObjectsByTypeAttribute: GetByXPathAttribute
    {
        // ReSharper disable once InconsistentNaming
        public new const EXP DefaultEXP = EXP.NoPicker | EXP.NoAutoResignToNull;
        public override string GroupBy { get; }

        // ReSharper disable once NotAccessedField.Global
        public readonly Type Type;
        // ReSharper disable once NotAccessedField.Global
        public readonly bool FindObjectsInactive;

        public FindObjectsByTypeAttribute(Type type = null, bool findObjectsInactive = false, string groupBy = ""): this(DefaultEXP, type, findObjectsInactive, groupBy)
        {
        }

        public FindObjectsByTypeAttribute(EXP exp, Type type = null, bool findObjectsInactive = false, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentInSceneExp(exp));
            ParseArguments(type, findObjectsInactive);
            GroupBy = groupBy;

            Type = type;
            FindObjectsInactive = findObjectsInactive;

            OptimizationPayload = new GetComponentInScenePayload(findObjectsInactive, type);
        }

        private void ParseArguments(Type compType, bool includeInactive)
        {
            string compFilter = GetComponentFilter(compType);
            string activeFilter = includeInactive ? "" : "[@{gameObject.activeInHierarchy}]";
            string allFilter = $"{activeFilter}{compFilter}";
            // string sepFilter = allFilter == ""? "": $"/{allFilter}";

            string ePath = $"scene:://*{allFilter}";
            // UnityEngine.Debug.Log(ePath);


            XPathInfoAndList = new[]
            {
                new[]
                {
                    new XPathInfo
                    {
                        IsCallback = false,
                        Callback = "",
#if UNITY_EDITOR
                        XPathSteps = XPathParser.Parse(ePath).ToArray(),
#endif
                    },
                },
            };
        }
    }
}
